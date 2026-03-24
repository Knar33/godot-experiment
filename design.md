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
  - Mouse movement orbits around the player.
  - Mouse wheel zoom adjusts camera distance within configurable bounds.
- Combat interaction:
  - Left mouse button fires a white orb in camera-forward direction.
- Environment:
  - Blue/green sky background.
  - Green ground plane.
  - Brown stepped boxes sized for jump traversal.
- UI:
  - Minimal dark gray center crosshair.

## Next Planned Expansions

- Add keybinding UI to call `InputActions.RebindAction(...)`.
- Add multiplayer-aware projectile authority and replication.
- Add collision/damage interactions for projectiles.
