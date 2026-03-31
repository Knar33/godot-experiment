# Design

## Prototype Goal

Establish a clean, playable 3D baseline for a multiplayer third-person action game.

## Included Gameplay Features

- Third-person controller:
  - Move with `WASD` and arrow keys.
  - Hold `Shift` or `E` to sprint (`E` avoids known Godot quirks with modifier polling; both are merged into the sprint action).
  - Press `Space` to jump.
  - Hold `C` (or `Ctrl`) to crouch.
- Camera:
  - Mouse movement orbits around the player (yaw on player, pitch on camera pivot).
  - Camera pitch is clamped to prevent inversion/extreme angles.
  - Mouse wheel zoom adjusts camera distance within configurable bounds.
  - `Esc` toggles mouse capture for cursor release/debug use.
- Combat interaction:
  - Left mouse button fires a white orb in camera-forward direction.
  - Projectile spawn point is positioned slightly in front of player body.
- Environment:
  - Red sky background (procedural sky in `Main.tscn`).
  - Green ground plane.
  - Brown stepped boxes sized for jump traversal.
- UI:
  - Small, thin dark gray center crosshair (slightly enlarged for visibility).
- Character animation (GLB + skeleton):
  - `godot_platformer_player.glb` — rigged robot from the Godot “3D Platformer” demo (MIT); imported as a scene with `Skeleton3D` and embedded clips.
  - **Default:** locomotion is driven by **`AnimationPlayer`** (idle / walk / run / jump / fall) with crossfades; imported clips are forced to **loop** at runtime so they do not freeze after one cycle. **`AnimationTree`** is optional via **`UseAnimationTreeForLocomotion`** (then idle/walk/run + jump/fall + filtered `shooting_standing` blend as in the original demo).
  - Sprint run speed and crouch walk rate use **`AnimationPlayer.SpeedScale`**. Crouch shortens the capsule and moves its center; the mesh gets a small **`CrouchMeshLocalYOffset`** (no Y-scale squash). No crouch clip — toggling crouch re-triggers `Play()` when needed so walk/idle still refresh at low speed.

## Tunable Prototype Parameters

- Player movement tuning:
  - `WalkSpeed`, `SprintSpeed`, `CrouchSpeed`, `JumpVelocity`
  - `UseAnimationTreeForLocomotion` (off = direct `AnimationPlayer`), `LocomotionCrossFadeSeconds`, `CrouchMeshLocalYOffset`
- Camera tuning:
  - `MouseSensitivity`
  - `CameraMinPitch`, `CameraMaxPitch`
  - `CameraMinDistance`, `CameraMaxDistance`, `CameraZoomStep`
- Projectile tuning:
  - `OrbSpeed`, `LifetimeSeconds`
  - `OrbSpawnForwardOffset`, `OrbSpawnHeightOffset`

## Controls and Input Mapping Strategy

- All gameplay actions use named input actions via `InputActions`.
- Default bindings currently include:
  - Movement: `WASD` and arrow keys
  - Sprint: `Shift` or `E` (both map to the same action; controller also polls physical keys to work around engine modifier quirks)
  - Jump: `Space`
  - Crouch: `C` or `Ctrl` (alternate binding added at runtime if not already mapped)
  - Fire: left mouse button
  - Zoom in/out: mouse wheel up/down
- Input architecture is intentionally ready for future rebind UI through `InputActions.RebindAction(...)`.

## Known Setup Requirement

- Open with the Godot .NET editor build to run C# scripts.

## Next Planned Expansions

- Add keybinding UI to call `InputActions.RebindAction(...)`.
- Swap in a different `.glb` with more locomotion clips (strafe, backward, crouch) or retarget Mixamo clips to this skeleton; extend the `AnimationTree` (e.g. `BlendSpace2D`) for direction.
- Add multiplayer-aware projectile authority and replication.
- Add collision/damage interactions for projectiles.
