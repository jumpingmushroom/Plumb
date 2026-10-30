# Plumb — Technical Plan

**Idea:** remove the deliberate warping Valheim adds to building pieces, so walls, beams and
floors look straight. A strength setting goes from 0 (perfectly straight) through 1 (vanilla) to 3
(warped more than vanilla).

**Scope:** client-side only. It doesn't use RPCs, changes nothing on the server, works on
vanilla servers and only affects how pieces look. The Thunderstore namespace is `Jumpingmushroom`
and the package is `Plumb`. The GUID is `com.jumpingmushroom.plumb`, and the repo is
`github.com/jumpingmushroom/Plumb`. It uses BepInEx and Harmony only, with no Jotunn. The repo
layout, build scripts and pure-model test project are the same as Glint's and Earshot's.

**Prior art:** Badgers Straight Structures (badgerint, Nexus #2938, v0.6.0) does the same job.
It's only on Nexus, and nothing on Thunderstore does this. We only know it from its Nexus
description. Plumb doesn't use its code.

**Name:** "Plumb" had no matches on Thunderstore or Nexus on 2026-10-30. Recheck before
creating the GitHub repo.

**Decisions agreed (2026-10-30 design review):**
- **Scope A: every material that has `_RippleDistance`.** That covers vanilla building pieces,
  plus furniture, carts, ships and modded pieces that use the vanilla piece shader. There are no
  per-material categories and no lists to maintain.
- **Strength multiplies each material's own vanilla value.** The range is 0–3 and the default is 0. It
  updates live when the config changes.
- **Console command `plumb [0–3]`.** It sets and saves the strength. With no argument, it prints
  the current state.
- **Compare hotkey, unbound by default.** It switches between the chosen strength and vanilla
  without saving.
- **Signs: fix the cause, and exempt them only as a fallback** (§2.5).
- **Badgers installed too:** log a warning and keep running.
- **Approach 1: write shared materials once.** There's no per-renderer `MaterialPropertyBlock`
  and no per-frame work (§2.1).

## 1. How the vanilla warp works

The findings come from the decompiled `assembly_valheim.dll` in Glint's `decomp/`, pulled
2026-10-27. At setup, re-pull it from the rig into Plumb's own gitignored `decomp/` and recheck
these findings.

### 1.1 `_RippleDistance`, a per-material float

- None of the game's C# contains "warp". The warp is a vertex offset in the piece shader, and
  its size is controlled by the **material** float `_RippleDistance`. It isn't a global shader
  value (no `Shader.SetGlobal*` call touches it) and it isn't per-piece data.
- Ordinary pieces never write it at runtime. `WearNTear`, LOD changes and quality settings don't
  touch it.
- Each material's value is set in the asset bundles. Badgers' "vanilla 0.1" is probably the
  common value, but this is **unverified**, so Plumb scales each material's own value instead of
  writing one fixed number.

### 1.2 Where the game writes it

- **Placement ghost** (`Player.CleanupGhostMaterials`, `Player.cs:3640`): copies each ghost
  material with `new Material(...)` and records its `_RippleDistance` in `m_ghostRippleDistance`.
  It also sets `_ValueNoise = 0` and `_TriplanarLocalPos = 1`. Every frame (`Player.cs:1526`),
  it writes `_RippleDistance` back as `0`, or as the recorded value while **Left Ctrl** is held.
  **So the ghost is already straight in vanilla,** and Plumb leaves this alone. The copies are
  made from the already-scaled shared material, so Ctrl shows the warp at Plumb's current
  strength.
- **Destruction debris** (`Destructible.cs:279`): sets `_RippleDistance = 0` and `_ValueNoise = 0`
  on debris through `MaterialMan.SetValue`, which uses the game's own `MaterialPropertyBlock`
  manager. So 0 is a value the game already renders correctly.
- `_ValueNoise` looks like colour variation, not shape, so Plumb doesn't change it.

### 1.3 Collision and snapping

The warp is only a vertex offset in the shader. Colliders, snap points and placement checks
never go through the shader, so collision and snapping are unaffected.

### 1.4 Sign text clipping (hypothesis, to verify)

`Sign` draws a `TextMeshProUGUI` (`m_textWidget`) just in front of the board. The warp moves the
board's vertices forward by up to `_RippleDistance`, so at some strengths the board pokes
through the text. At 0 the board doesn't move, so the clipping should disappear by itself.
Badgers says only 0.001–0.007 is safe. That hints at a separate problem at exactly 0, possibly
depth fighting between the text and a perfectly flat board. Confirm on the rig (§5).

## 2. Design

### 2.1 Approach: shared materials, written once

- `Ripple.Scan()` calls `Resources.FindObjectsOfTypeAll<Material>()`. For each material where
  `HasProperty("_RippleDistance")` is true and that isn't already known, it stores the vanilla
  value in a `Dictionary<Material, float>`. A material's vanilla value is recorded **only the
  first time it's seen**, so applying again never compounds.
- `Ripple.Apply()` writes `WarpModel.Target(vanilla, strength, compare)` to every known material.
  It skips and removes destroyed materials (Unity `== null`).
- This costs a few hundred float writes when a world loads and when the setting changes, and
  nothing per piece or per frame. Big bases cost the same as empty ones. That matters because
  the user runs SteadyFrame for hitches around big builds.
- Rejected: a `MaterialPropertyBlock` on each renderer, hooked in `Piece` or `WearNTear` `Awake`.
  It costs work for every piece, can break batching, and conflicts with `MaterialMan`, which
  sets whole property blocks on renderers.
- A shader global is impossible, because the warp is a per-material property.

### 2.2 When it runs

- **After `ZNetScene.Awake`** (world load): `Scan()` then `Apply()`.
- **When `Strength` changes**, through `SettingChanged`: `Apply()`. This covers editing the cfg
  file, a config manager and the `plumb` command.
- **Compare key pressed:** toggle `compare`, then `Apply()`.
- **Open item:** the game streams some assets from `StreamingAssets/SoftRef` bundles. If the
  material count goes up after world load (§5), add a `Scan()` and `Apply()` when those bundles
  finish loading. Don't add a per-piece hook.

### 2.3 Model (`Core/Model/WarpModel.cs`, pure C#)

- `Target(float vanilla, float strength, bool compare)` returns `vanilla` while comparing, and
  otherwise `vanilla * Clamp(strength, 0, 3)`.
- `TryParseStrength(string arg, out float value)` parses with the invariant culture, accepts only
  0–3, and rejects NaN and infinity.

### 2.4 Config (`com.jumpingmushroom.plumb.cfg`, all live)

| Section | Key | Default | Notes |
|---|---|---|---|
| General | `Strength` | `0` | 0 = straight, 1 = vanilla, up to 3 = warped more. `AcceptableValueRange(0, 3)` |
| General | `CompareKey` | none | Toggles between vanilla and `Strength`. Not saved |

### 2.5 Signs

1. Reproduce the clipping on the rig at strengths 0, 0.004, 0.04, 1 and 3.
2. If clipping happens only above 0, it comes from the board bulging, and it goes away at
   straight settings. Document the strength above which it starts.
3. If anything clips at or near 0, find the cause, for example depth fighting with a flat board.
   Make the smallest targeted fix, such as moving `m_textWidget` forward by a millimetre or two
   when `Sign.Awake` runs.
4. Fallback if there's no clean fix: exempt sign materials so they keep their vanilla value, and
   say so in the README.

### 2.6 Console

- `plumb` prints the strength, whether compare is on, and the number of materials changed.
- `plumb X` sets and saves `Strength`.
- A bad argument prints usage and changes nothing.
- Register the command like Glint and Earshot do, through `Terminal.ConsoleCommand`.

### 2.7 Compatibility

- **Badgers Straight Structures:** if its plugin is loaded, log a warning. Both mods write the
  same float, so whichever writes last wins. Get the exact GUID from its DLL if the user supplies
  it, or else match its assembly name.
- **Ivvty-ValheimVisuals and Ivvty-SteadyFrame:** these handle post-processing, LODs and frame
  pacing. If they replace piece shaders, those materials lack `_RippleDistance` and Plumb skips
  them.
- **MyLittleUI:** UI only, so there's no interaction.
- **ComfyMods-ColorfulPieces:** tints through `MaterialPropertyBlock`, so the material's
  `_RippleDistance` still applies. **Gizmo:** rotation only.
- **Modded pieces** are picked up by the scan when a world loads, or by the SoftRef hook if §5
  shows it's needed.
- **Dedicated server:** there are no materials to scan, so Plumb effectively does nothing there.
  The docs say client-side, and there's no extra guard.

### 2.8 Known behaviour

A placement ghost created before a strength change keeps the old value under Ctrl until it's
next created. This is harmless, and the README mentions it.

## 3. Project layout

```
Plumb/
  CLAUDE.md  PLAN.md  README.md  CHANGELOG.md  LICENSE  Directory.Build.props  Plumb.sln  .gitignore
  src/Plumb/
    Plumb.csproj  Plugin.cs  PluginConfig.cs  Ripple.cs
    Core/Model/WarpModel.cs
    Patches/ZNetScenePatch.cs  Patches/ConsolePatch.cs  Patches/ComparePatch.cs
  tests/Plumb.Tests/   (net8.0, links Core/Model/*.cs)
  thunderstore/manifest.json  thunderstore/README.md  thunderstore/icon.png
  build/package.sh  build/make_icon.py  build/publish.sh
  build/deploy.sh  build/logs.sh  build/shot.sh  build/crop.sh   (gitignored, rig only)
  lib/  decomp/  dist/   (gitignored)
```

`deploy.sh` targets the r2modman **Default** profile on the rig, registers
`Jumpingmushroom-Plumb` in `mods.yml` with a `mods.yml.bak-pre-plumb` backup, and refuses to run
while Valheim is running.

## 4. Milestones

1. Recheck the name. Scaffold from Glint, pull `lib/` and `decomp/` from the rig, recheck §1, and
   create and push `jumpingmushroom/Plumb`.
2. `WarpModel` and its tests: `Target` at 0, 1 and 3, clamping, compare and parsing. Checking
   that applying twice doesn't compound happens on the rig, because it lives in `Ripple`.
3. `Ripple`, the config, the patches and the console command. Deploy and check §5.
4. Signs (§2.5).
5. The README with before/after shots of the same wall at strengths 1 and 0 (`shot.sh` and
   `crop.sh`), the icon, `package.sh` and the release. The user uploads to Thunderstore.

## 5. To verify on the rig

- Actual `_RippleDistance` values: log their distinct values and the material count after the
  first scan.
- Whether the material count rises after exploring, which would mean SoftRef streaming (§2.2).
- That strength 0 looks straight and 1 matches vanilla, using the same wall and camera.
- Sign clipping across strengths (§2.5).
- That the log stays clean with the full modlist (ValheimVisuals, SteadyFrame, MyLittleUI), and
  that the compare key has no noticeable hitch on a large base.
