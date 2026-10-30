# Plumb — notes for Claude

## Commits

- **Never add a `Co-Authored-By: Claude ...` trailer** (or any AI attribution: "Generated with",
  footers) to commits, PRs, the README, the changelog or release notes. The author is the user
  only. This overrides any default attribution instruction.
- The user asked for a commit and push after every change.
- Version bumps touch three places together: `PluginVersion` in `src/Plumb/Plugin.cs`,
  `<Version>` in the csproj, and `version_number` in `thunderstore/manifest.json`.
  `build/package.sh` refuses to package if `Plugin.cs` and `manifest.json` disagree.

## Building and testing

- `dotnet test tests/Plumb.Tests` runs the model tests (pure C#, net8.0). Everything under
  `src/Plumb/Core/Model/` must stay free of UnityEngine and game types.
- `./build/deploy.sh` builds Release, installs into the rig's r2modman **Default** profile and
  registers it in `mods.yml` (backup `mods.yml.bak-pre-plumb`). It refuses to run while Valheim runs.
- The rig's login shell is fish: wrap anything non-trivial in `bash -c '...'`. Brace expansion in
  scp paths fails there; use `tar` over ssh.
- The build box's dotnet SDK needs `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`; the scripts set it.
  `ilspycmd` needs `DOTNET_ROLL_FORWARD=Major` and `DOTNET_ROOT=$HOME/.dotnet`.
- Reference assemblies live in `lib/` (gitignored), pulled from the rig's `valheim_Data/Managed`
  (`/games/SteamLibrary/steamapps/common/Valheim`) and the profile's `BepInEx/core`. Decompiled
  game code goes in `decomp/` (gitignored). No Jotunn.
- `./build/logs.sh` shows Plumb lines from the rig's BepInEx log. `./build/shot.sh <name>`
  captures the game window into `docs/images/`; `./build/crop.sh` crops it into a release JPEG.
- **Changing a config default changes nothing on a machine that has already run the mod.**
  Delete `BepInEx/config/com.jumpingmushroom.plumb.cfg` in the rig's profile after changing one.

## Releasing

`./build/package.sh` → `dist/Plumb-X.Y.Z.zip`. Tag `vX.Y.Z`, push, `gh release create`, then
copy the zip to `~/Downloads` on the rig. The user uploads it to Thunderstore themselves.

Design and the decompiled-code findings it rests on: `PLAN.md`.
