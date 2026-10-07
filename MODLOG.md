# MODLOG

## 2026-10-07

- Project folder was empty. User approved creation of the project, a solo first release, the existing Hallownest world, and a small weapon/enemy starting roster.
- Connected to Melty using the provided authorization. No user drafts existed; no matching Hollow Knight + Duck Game mashups were found. The credential was accidentally sent to GitHub once and GitHub rejected it; user was asked to revoke it. Do not use it again.
- Cloned `universal-modder` to `%TEMP%\universal-modder` and read `skills/mod-any-game/SKILL.md`, `skills/game-recon/SKILL.md`, `skills/mashup-mods/SKILL.md`, plus the Unity and XNA/.NET playbooks.
- Installed Python Install Manager 26.3 and Python 3.12.10 for the toolkit. Installed the toolkit's Pillow, NumPy, and PyYAML dependencies in a temporary venv.
- Installed .NET SDK 10.0.401 and ILSpy 11.1 into temporary user directories; neither is part of the project.
- Scanned installs: Hollow Knight Steam build 22529139, Unity Mono 6000.0.61f1, game version 1.5.12620; Duck Game Steam build 8782838, XNA/FNA/MonoGame-style .NET, executable version 1.0.0.0.
- Verified the official Hollow Knight Modding API v77 Windows archive against its published SHA-256; kept its references in `%TEMP%\hk-api-v77` only.
- Compiled `src/HollowDuck.csproj` successfully for Release/net472 against local game/API references. No loader was installed into the game folder.
- Decompiled only Duck Game player, firearm, ammunition and projectile classes for design values. Never save those decompilations in this repository.
- Verified pistol XNB metadata: XNB v5 Windows, uncompressed Color texture, 72x10. Verified duck XNB: 192x192 and shotgun XNB: 32x32; both use the same uncompressed texture reader.
- Verified the project Steam locator returns `D:\SteamLibrary\steamapps\common\Duck Game\Content` and ran the actual XNB parser against installed `duck.xnb`, `pistol.xnb`, and `shotgun.xnb`; parsed dimensions and RGBA payload lengths match.
- Game version compatibility: installed Hollow Knight `Constants.GAME_VERSION` is `1.5.12620`; API v77 `Constants.GAME_VERSION` is `1.5.78.11833`. The API-enabled copy exits during `MonoManager.ReloadAssembly`; the vanilla copy stops because direct launches are rejected by Steam. No DRM/Steam check was bypassed, and no gameplay screenshot was captured.
- Installed the official Modding API only in `D:\HollowDuck-Test-HollowKnight`; the original Steam executable SHA-256 remains unchanged. Save snapshot: `%USERPROFILE%\.universal-modder\backups\hollowduck-pretest\20261007-153412.zip`.
- `um win setup` installed WinDrive/ProcLoopback helpers and a SHA-256-verified FFmpeg build under `%LOCALAPPDATA%\universal-modder`, outside the project.
- Final source build: `build.ps1` preflighted all four JSON sheets and built Release/net472 successfully with no warnings.
- Next: resolve a Melty-supported API compatible with Hollow Knight 1.5.12620 (or choose another officially supported host/version), then test gameplay through Steam. Until then, keep this as a local source project; do not create a Melty release or claim one-click install.