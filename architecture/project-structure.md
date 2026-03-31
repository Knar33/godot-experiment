# Project Structure

## Solution Layout

```
GodotExperiment.sln
├── GodotExperiment.csproj          (Godot.NET.Sdk — main game project)
├── src/GodotExperiment.Core/       (Microsoft.NET.Sdk — pure C# logic, no Godot dependency)
└── tests/GodotExperiment.Tests/    (xUnit — unit tests, references Core only)
```

## Why Three Projects

The Godot project (`GodotExperiment.csproj`) uses `Godot.NET.Sdk` which requires the Godot editor to build fully. Testable game logic (state machines, leaderboard data, upgrade calculations, etc.) lives in `GodotExperiment.Core` as a standard .NET class library with zero Godot dependency. The test project references only Core, so `dotnet test` works without Godot installed.

The main Godot project references Core via `<ProjectReference>` and excludes `src/` and `tests/` from its own compilation (Godot auto-includes all `.cs` files in subdirectories by default).

## Key Directories

- `scenes/` — Godot `.tscn` scene files, organized by domain
  - `scenes/arena/` — Arena scene
  - `scenes/player/` — Player and camera scenes
  - `scenes/Game.tscn` — Root game scene (main scene)
- `scripts/` — Godot-specific C# scripts (nodes, autoloads) that reference Core types, organized by domain
  - `scripts/arena/` — Arena node scripts
  - `scripts/managers/` — Autoload and manager scripts (GameManager, HitStopManager, MusicManager, SettingsManager)
  - `scripts/player/` — Player, camera, and projectile scripts (ScreenShake)
  - `scripts/ui/` — HUD and UI element scripts (Crosshair, HitMarker, SpeedLines, BhopCounter, ThreatIndicator, SettingsMenu)
- `src/GodotExperiment.Core/` — Pure C# classes: enums, state machines, data models, calculations
  - `PlayerMovement/` — Player movement state (BhopState, DodgeRollState) — namespace `GodotExperiment.PlayerMovement`
  - `Combat/` — Combat mechanics (AutoFireState, ProjectileState, DamageSource, PlayerHealthState) — namespace `GodotExperiment.Combat`
  - `GameLoop/` — Game state management (GameState, GameStateMachine) — namespace `GodotExperiment.GameLoop`
  - `GameFeel/` — Screen shake and hit stop state (ScreenShakeState, HitStopState) — namespace `GodotExperiment.GameFeel`
  - `Settings/` — Settings data model (SettingsData) — namespace `GodotExperiment.Settings`
- `tests/GodotExperiment.Tests/` — xUnit tests for Core classes
- `assets/audio/` — Audio assets organized by category (music/, player/, enemies/, ui/, ambience/)
- `design/` — Game design documents (source of truth for gameplay intent)
- `architecture/` — Technical implementation documents
- `tasks/` — Task tracking

## Game State Management

`GameStateMachine` (Core, `GodotExperiment.GameLoop`) is a pure C# state machine with four states:

```
Countdown → Playing → Dead → Countdown (restart)
                  ↕
               Paused
```

Valid transitions are enforced; invalid transitions return `false` and leave state unchanged. The machine fires a `StateChanged` event on valid transitions and on `Reset()`.

`GameManager` (scripts/managers/) is a Godot `Node` autoload that wraps `GameStateMachine` and re-emits state changes as Godot signals so other nodes can connect via the editor or code.

## Input Actions

Defined in `project.godot` under `[input]`:

| Action | Key | Usage |
|--------|-----|-------|
| `move_forward` | W | Player movement |
| `move_backward` | S | Player movement |
| `move_left` | A | Player movement |
| `move_right` | D | Player movement |
| `jump` | Space | Jump / bunny hop |
| `dodge_roll` | Left Shift | Dodge roll |
| `shoot` | Left Mouse Button | Hold to fire projectiles |
| `pause` | Escape | Pause menu |
| `restart` | R | Restart from death screen |

Mouse look is handled directly in code (Input.MouseMode capture), not via input actions. Gamepad bindings: `shoot` on right trigger, movement on left stick, aim on right stick.

## Godot SDK Version

The `GodotExperiment.csproj` specifies `Godot.NET.Sdk/4.6.1`. This version must match the installed Godot editor version exactly. Update the SDK version in the `.csproj` if using a different Godot version.
