# Architecture

## Current Scope

This repository is a Godot 4.6 .NET (C#) third-person prototype foundation.

## Runtime Requirement

- The project must be opened with the Godot .NET editor build (not the standard non-.NET build), because gameplay scripts are implemented in C#.
- The root `.godot` directory contains generated editor/import/cache data and can be deleted safely to force a clean reimport after editor/version issues.

## Project Structure

- `project.godot` - project settings, startup scene, and input action declarations.
- `godot-experiment.csproj` - C# project definition for Godot .NET.
- `scenes/main/Main.tscn` - playable test map with environment, platforms, and UI layer.
- `scenes/player/Player.tscn` - player character with collision, camera rig, and controller script.
- `scenes/projectiles/OrbProjectile.tscn` - reusable projectile scene.
- `scripts/core/InputActions.cs` - centralized input action constants, defaults, and rebind API.
- `scripts/player/PlayerController.cs` - movement, camera orbit, zoom, crouch, firing, and `AnimationTree` parameter updates.
- `scripts/projectiles/OrbProjectile.cs` - projectile movement and lifetime.
- `assets/characters/godot_platformer_player.glb` - rigged GLTF binary imported as a scene (skeleton + clips). The asset already applies `0.3` scale on the `Skeleton` node; do **not** scale the instanced root again or the character becomes tiny. See `assets/characters/CREDITS.txt` for source and license.

## Scene Composition

- `scenes/main/Main.tscn`
  - World environment with procedural red sky and directional light.
  - Green ground plane with collision.
  - Brown stepped platform boxes for jump traversal tests.
  - UI canvas layer that renders a centered crosshair.
- `scenes/player/Player.tscn`
  - `CharacterBody3D` root with capsule collider.
  - `Character` - instanced `godot_platformer_player.glb` (skinned mesh, `Skeleton3D`, `AnimationPlayer` with clips embedded in the GLB).
  - `AnimationTree` - blend tree adapted from the Godot “3D Platformer” demo (idle/walk/run, jump/fall, sprint time scale, filtered `shooting_standing`). Script **`platformer_animation_tree.gd`** drives parameters from C# via `set_blend_param`.
  - Camera rig: `CameraPivot -> SpringArm3D -> Camera3D` for orbit + zoom.
  - Projectile packed scene wired to the player controller export.
- `scenes/projectiles/OrbProjectile.tscn`
  - `Area3D` with sphere mesh + collision and emissive white material.

## Technical Conventions

- Use scene-based composition:
  - Keep mechanics in focused scripts.
  - Keep geometry and visual setup in scene files.
- Prefer input actions for rebinding; sprint/crouch also poll physical keys as a fallback (see `PlayerController`, `InputActions`).
- Keep configurable gameplay values as `[Export]` fields.
- Keep systems extensible:
  - `InputActions.RebindAction(...)` exists as the entry point for future keybinding UI.

## Input Architecture

- Input action names are constants in `InputActions`.
- Default physical keys and mouse buttons are **merged** into each action (added only if missing), so existing custom mappings are kept and broken/partial editor maps still get usable defaults.
- Gameplay systems query actions by constant name, not literal strings.
- `InputActions` is autoloaded and **merges** default keys/buttons into each action at startup (adds a binding if that physical key or mouse button is missing). This avoids a half-empty `project.godot` input map blocking movement. Sprint also binds **`E`** as a non-modifier alternate because **`IsActionPressed` can mis-report `Shift`**; `PlayerController` uses **`IsPhysicalKeyPressed`** for `Shift` / `C` / `Ctrl` as a fallback alongside actions.
- `InputActions.RebindAction(...)` is the supported API for future keybinding UI.

## Animation Architecture

- Character visuals use a **single** imported `.glb` with a **skeleton** and clips baked into the asset (`idle`, `walk`, `run`, `jump`, `falling`, `shooting_standing`).
- **`AnimationTree`** (not hand-authored `Animation` keyframes in C#) drives blending. Parameters mirror the Godot 3D Platformer demo:
  - `parameters/run/blend_amount` — idle vs moving (horizontal speed vs `SprintSpeed`, with input assist when on the floor).
  - `parameters/speed/blend_amount` — walk vs run clips: **run** while **sprint** is held and there is move intent (or meaningful horizontal speed), not from `(velocity − walkThreshold)` (small dips were forcing **walk** after a few steps). Crouch uses walk-heavy blend + slower `parameters/scale/scale` (no dedicated crouch clip in the GLB); crouch locomotion is not gated on `horizontalSpeed` alone so brief `IsOnFloor()` flicker does not zero the blend.
  - `parameters/state/blend_amount` — on ground vs in air.
  - `parameters/air_dir/blend_amount` — jump vs falling.
  - `parameters/scale/scale` — `AnimationNodeTimeScale` on the locomotion branch: higher while sprinting, reduced while crouched and moving.
  - `parameters/gun/blend_amount` — upper-body shooting pose; the tree uses a **bone filter** on the shooting branch so legs can keep locomotion while arms/torso blend toward `shooting_standing`.
- **Locomotion (default):** `PlayerController.UseAnimationTreeForLocomotion` is **`false`**. Locomotion uses **`AnimationPlayer.Play`** with crossfade (`LocomotionCrossFadeSeconds`); **`AnimationTree` is inactive** so it does not fight `Play()`. On startup, **`EnsureImportedAnimationsLoop`** sets **`Animation.LoopModeEnum.Linear`** on imported GLTF clips (except **`jump`**, which stays one-shot). Clips that imported with **no loop** otherwise stop after a single cycle (~a few seconds of motion), which often looks like “animation broke.”
- **Optional blend tree:** Set **`UseAnimationTreeForLocomotion = true`** on the player to use the platformer **`AnimationTree`** again; **`platformer_animation_tree.gd`** exposes `set_blend_param` (GDScript `set()`). The script’s `_ready()` forces float init on blend params per [GH-105275](https://github.com/godotengine/godot/issues/105275).
- Crouch shortens the capsule height and **repositions** `CollisionShape3D` so the capsule **bottom** stays at the same local Y (feet planted). **`CrouchMeshLocalYOffset`** lowers the `Character` mesh slightly (no Y-scale squash). Optional exports `CrouchCameraPivotYOffset` and `CrouchOrbHeightOffset` lower camera and projectile spawn while crouched. Direct `AnimationPlayer` locomotion: crouch toggling forces **`Play()`** even when the clip stays `walk`/`idle`, and crouch uses lower **`SpeedScale`** (including crouch idle). The GLB does not ship a dedicated crouch clip.
- **C# note:** do **not** rely on `Set(..., Variant.From(float))` for blend parameters; use the **`platformer_animation_tree.gd`** helper via **`Call("set_blend_param", path, value)`** from `PlayerController`.
- **CharacterBody3D:** `FloorSnapLength` is set in `_Ready` to reduce `IsOnFloor()` flicker on edges (helps ground vs air and locomotion blends).

## Implemented Systems

- `PlayerController`
  - Uses `CharacterBody3D` velocity-based locomotion for walk/sprint/crouch/jump.
  - Handles camera yaw on player root and camera pitch on pivot, clamped by exported pitch limits.
  - Captures mouse on startup; `Esc` toggles capture/visibility.
  - Handles scroll-wheel zoom by adjusting spring arm length within exported min/max values.
  - Fires projectiles toward camera-forward direction.
  - Spawns projectiles slightly in front of player using exported spawn offsets (`OrbSpawnForwardOffset`, `OrbSpawnHeightOffset`). The instance is **`AddChild`’d to `CurrentScene` before `GlobalPosition` is set**; setting global transform on a node that is not in the tree errors (`!is_inside_tree()`).
  - Drives `AnimationTree` parameters from velocity, floor state, sprint, and hold-to-aim on the fire action.
- `OrbProjectile`
  - Receives initial direction/speed through `Initialize(...)`.
  - Moves in `_PhysicsProcess` and self-despawns after configurable lifetime.

## Compatibility Notes

- Scene files use format-compatible text resource syntax for broad Godot 4.x interoperability.
- If stale dependency or parse errors occur after editor changes, delete `.godot` and reopen the project to rebuild import/cache metadata.
