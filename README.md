# Hollow Duck

A solo, offline Hollow Knight mod concept: explore the original Hallownest as a Duck Game duck, find firearms, and unlock them in an armory. Hallownest and its enemies come from the player's Hollow Knight install. The duck and firearm textures are read at runtime from the player's Duck Game Steam install.

## Status

Prototype in development. Source and project data build locally, and the Duck Game install locator/XNB parser pass local tests. Gameplay is not yet tested: the installed Hollow Knight reports game version `1.5.12620`, while Melty's auto-installed Modding API v77 targets `1.5.78.11833`; the API-enabled isolated copy exits during Mono assembly reload. The vanilla copy also enforces Steam launch. Do not install or publish this source-only prototype as a finished game until a compatible Melty-supported loader is available and gameplay is tested in Steam.

## First playable scope

- Hollow Knight remains the host and supplies the actual Hallownest world, its enemies, physics collision, and save.
- Duck Game supplies its real duck/weapon assets and the movement/firearm values read from the installed game's classes.
- The first roster is the Pistol and Shotgun. Enemy drops unlock each weapon when picked up; the armory shows progression and selection.
- Single-player/offline only. No Duck Game process, online session, or game files are included.
- The armory overlay opens in the current Hollow Knight save room. The first slice does not teleport the player to Dirtmouth.

## Controls

- Move and jump with the current Hollow Knight bindings.
- Fire with Hollow Knight's current Attack binding.
- Press `F2` to open or close the armory, `1` for Pistol, `2` for Shotgun, and `R` to reload.

## Build

Requires the .NET SDK and a Hollow Knight install plus a matching Hollow Knight Modding API reference package. Set the local paths when building; these references are not copied into the project or release:

```powershell
dotnet build .\src\HollowDuck.csproj -c Release `
  -p:HollowKnightInstall="D:\SteamLibrary\steamapps\common\Hollow Knight" `
  -p:ModdingApiPath="C:\path\to\ModdingAPI-v77"
```

## Credits and licensing

Hollow Knight Modding API v77 is MIT-licensed and is installed by Melty for the host. Duck Game assets remain the player's own game files and are never redistributed. The project's source license and remix permission are intentionally not set until the creator chooses them.