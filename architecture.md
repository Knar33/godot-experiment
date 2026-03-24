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
- `scripts/player/PlayerController.cs` - movement, camera orbit, zoom, crouch, and firing behavior.
- `scripts/projectiles/OrbProjectile.cs` - projectile movement and lifetime.

## Scene Composition

- `scenes/main/Main.tscn`
  - World environment with procedural blue/green sky and directional light.
  - Green ground plane with collision.
  - Brown stepped platform boxes for jump traversal tests.
  - UI canvas layer that renders a centered crosshair.
- `scenes/player/Player.tscn`
  - `CharacterBody3D` root with capsule collider and visual mesh.
  - Camera rig: `CameraPivot -> SpringArm3D -> Camera3D` for orbit + zoom.
  - Projectile packed scene wired to the player controller export.
- `scenes/projectiles/OrbProjectile.tscn`
  - `Area3D` with sphere mesh + collision and emissive white material.

## Technical Conventions

- Use scene-based composition:
  - Keep mechanics in focused scripts.
  - Keep geometry and visual setup in scene files.
- Use input actions only (never hardcode key checks in gameplay logic).
- Keep configurable gameplay values as `[Export]` fields.
- Keep systems extensible:
  - `InputActions.RebindAction(...)` exists as the entry point for future keybinding UI.

## Input Architecture

- Input action names are constants in `InputActions`.
- Default bindings are applied only if actions have no events, so custom mappings are preserved.
- Gameplay systems query actions by constant name, not literal strings.
- `InputActions` is autoloaded and ensures defaults at startup.
- `InputActions.RebindAction(...)` is the supported API for future keybinding UI.

## Implemented Systems

- `PlayerController`
  - Uses `CharacterBody3D` velocity-based locomotion for walk/sprint/crouch/jump.
  - Handles camera yaw on player root and camera pitch on pivot, clamped by exported pitch limits.
  - Captures mouse on startup; `Esc` toggles capture/visibility.
  - Handles scroll-wheel zoom by adjusting spring arm length within exported min/max values.
  - Fires projectiles toward camera-forward direction.
  - Spawns projectiles slightly in front of player using exported spawn offsets (`OrbSpawnForwardOffset`, `OrbSpawnHeightOffset`).
- `OrbProjectile`
  - Receives initial direction/speed through `Initialize(...)`.
  - Moves in `_PhysicsProcess` and self-despawns after configurable lifetime.

## Compatibility Notes

- Scene files use format-compatible text resource syntax for broad Godot 4.x interoperability.
- If stale dependency or parse errors occur after editor changes, delete `.godot` and reopen the project to rebuild import/cache metadata.
