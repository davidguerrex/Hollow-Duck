[CmdletBinding()]
param(
    [string]$DotnetExe = "dotnet",
    [string]$HollowKnightInstall = $env:HOLLOW_KNIGHT_PATH,
    [string]$ModdingApiPath = $env:HOLLOW_KNIGHT_API_PATH
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$sheetDirectory = Join-Path $projectRoot "sheets"
$sheetFiles = Get-ChildItem $sheetDirectory -File -Filter "*.json"
$sheets = @{}
$issues = [System.Collections.Generic.List[string]]::new()

foreach ($file in $sheetFiles) {
    try {
        $sheet = Get-Content $file.FullName -Raw | ConvertFrom-Json
        $sheets[$file.Name] = $sheet
        foreach ($row in $sheet.rows) {
            if ([string]::IsNullOrWhiteSpace($row.id) -or $row.verified -ne $true) {
                $issues.Add("$($file.Name)#$($row.id): missing id or verification")
            }

            foreach ($property in $row.PSObject.Properties) {
                if ($null -eq $property.Value -or ($property.Value -is [string] -and [string]::IsNullOrWhiteSpace($property.Value))) {
                    $issues.Add("$($file.Name)#$($row.id): empty $($property.Name)")
                }
            }
        }
    }
    catch {
        $issues.Add("$($file.Name): invalid JSON: $($_.Exception.Message)")
    }
}

foreach ($file in $sheetFiles) {
    $content = Get-Content $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, '([A-Za-z0-9_-]+\.json)#([A-Za-z0-9_-]+)')) {
        $targetSheet = $match.Groups[1].Value
        $targetId = $match.Groups[2].Value
        if (-not $sheets.ContainsKey($targetSheet) -or -not ($sheets[$targetSheet].rows | Where-Object id -eq $targetId)) {
            $issues.Add("$($file.Name): unresolved reference $targetSheet#$targetId")
        }
    }
}

if ($issues.Count -gt 0) {
    $issues | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "Preflight clean: $($sheetFiles.Count) sheets, verified rows, resolved references."

if ([string]::IsNullOrWhiteSpace($HollowKnightInstall) -or -not (Test-Path $HollowKnightInstall)) {
    throw "Pass -HollowKnightInstall or set HOLLOW_KNIGHT_PATH to the local game folder."
}
if ([string]::IsNullOrWhiteSpace($ModdingApiPath) -or -not (Test-Path (Join-Path $ModdingApiPath "Assembly-CSharp.dll"))) {
    throw "Pass -ModdingApiPath or set HOLLOW_KNIGHT_API_PATH to the local Modding API references folder."
}

$projectFile = Join-Path $projectRoot "src\HollowDuck.csproj"
& $DotnetExe build $projectFile -c Release `
    "-p:HollowKnightInstall=$HollowKnightInstall" `
    "-p:ModdingApiPath=$ModdingApiPath"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}