# Architecture

## Current Scope

This repository is a Godot 4.6 C# third-person prototype foundation.

## Project Structure

- `project.godot` - project settings, startup scene, and input action declarations.
- `godot-experiment.csproj` - C# project definition for Godot .NET.
- `scenes/main/Main.tscn` - playable test map with environment, platforms, and UI layer.
- `scenes/player/Player.tscn` - player character with collision, camera rig, and controller script.
- `scenes/projectiles/OrbProjectile.tscn` - reusable projectile scene.
- `scripts/core/InputActions.cs` - centralized input action constants, defaults, and rebind API.
- `scripts/player/PlayerController.cs` - movement, camera orbit, zoom, crouch, and firing behavior.
- `scripts/projectiles/OrbProjectile.cs` - projectile movement and lifetime.

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
