# Hollow Duck Modding Plan

- Host install: `D:\SteamLibrary\steamapps\common\Hollow Knight` (Steam app 367520, build 22529139).
- Host engine: Unity Mono 6000.0.61f1; game constant 1.5.12620; 64-bit.
- Host loader: Hollow Knight Modding API 1.5.78.11833-77 (MIT); Melty lists it as auto-installed, but it targets a different game build than this install.
- Guest install: discovered through Steam libraries; `D:\SteamLibrary\steamapps\common\Duck Game` (Steam app 312530, build 8782838).
- Guest engine: XNA/FNA/MonoGame-style managed .NET; DuckGame.exe file version 1.0.0.0; no anti-cheat detected by the toolkit.
- Play mode: one player, offline. Do not run, hook, or connect to Duck Game; read its installed content only.
- Route: C# Hollow Knight Modding API mod. Keep the original Hallownest scenes, collisions, enemies, and saves. Adapt Duck movement, the duck sprite, and selected firearms inside that real host game.
- Guest assets: runtime-only reads of `Content/duck.xnb`, `Content/pistol.xnb`, and `Content/shotgun.xnb` from the player's Duck Game Steam install. No game content or decompiled code is copied into this repository or release.
- Build target: `net472`, matching the official API example. Host/game references stay outside the repository and are marked `Private=false`.
- Saves: progression in `{localappdata}/HollowDuck/progress.json`; no edits to either game's save files.
- Safety: keep the project offline/single-player. Hollow Knight scan found no anti-cheat. Duck Game supports online PvP, but this mod never alters or launches its online client.
- Compatibility blocker: installed Hollow Knight reports `Constants.GAME_VERSION = 1.5.12620`; API v77 reports `1.5.78.11833`. The isolated API-enabled copy exits during `MonoManager.ReloadAssembly`. The vanilla copy exits with `Application was not launched through Steam`. Do not bypass the Steam check or claim in-game validation; Melty's current v77 loader is not verified against this installed build.
- Lab: in-game validation needs the Melty-installed loader or user permission to install API v77 in the local Hollow Knight folder. Do not install it into the game folder without asking.
- Known first-slice limit: the armory is an overlay in the current saved room; it does not force a Dirtmouth teleport or replace map scenes.