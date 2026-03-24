# Design

## Prototype Goal

Establish a clean, playable 3D baseline for a multiplayer third-person action game.

## Included Gameplay Features

- Third-person controller:
  - Move with `WASD` and arrow keys.
  - Hold `Shift` to sprint.
  - Press `Space` to jump.
  - Hold `C` to crouch.
- Camera:
  - Mouse movement orbits around the player (yaw on player, pitch on camera pivot).
  - Camera pitch is clamped to prevent inversion/extreme angles.
  - Mouse wheel zoom adjusts camera distance within configurable bounds.
  - `Esc` toggles mouse capture for cursor release/debug use.
- Combat interaction:
  - Left mouse button fires a white orb in camera-forward direction.
  - Projectile spawn point is positioned slightly in front of player body.
- Environment:
  - Blue/green sky background.
  - Green ground plane.
  - Brown stepped boxes sized for jump traversal.
- UI:
  - Small, thin dark gray center crosshair (slightly enlarged for visibility).

## Tunable Prototype Parameters

- Player movement tuning:
  - `WalkSpeed`, `SprintSpeed`, `CrouchSpeed`, `JumpVelocity`
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
  - Sprint: `Shift`
  - Jump: `Space`
  - Crouch: `C`
  - Fire: left mouse button
  - Zoom in/out: mouse wheel up/down
- Input architecture is intentionally ready for future rebind UI through `InputActions.RebindAction(...)`.

## Known Setup Requirement

- Open with the Godot .NET editor build to run C# scripts.

## Next Planned Expansions

- Add keybinding UI to call `InputActions.RebindAction(...)`.
- Add multiplayer-aware projectile authority and replication.
- Add collision/damage interactions for projectiles.
