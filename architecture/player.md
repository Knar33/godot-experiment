# Player Architecture

## Scene Structure

`scenes/Player.tscn` is a standalone scene instanced into `Game.tscn`.

```
Player (CharacterBody3D) [scripts/Player.cs]
├── CollisionShape3D — CapsuleShape3D (radius 0.4, height 1.8), centered at y=0.9
└── MeshInstance3D — CapsuleMesh placeholder (same dimensions), light blue material
```

The capsule bottom sits at y=0 so the player stands flush on the arena floor.

## Code Split

Player mechanics are split across two layers:

- **`src/GodotExperiment.Core/BhopState.cs`** — Pure C# bhop timing, speed stacking, decay logic. No Godot dependency, fully unit-testable.
- **`src/GodotExperiment.Core/DodgeRollState.cs`** — Pure C# dodge roll state machine (rolling, i-frames, cooldown). No Godot dependency, fully unit-testable.
- **`scripts/Player.cs`** — Godot `CharacterBody3D` script. Reads input, queries camera for direction, delegates to `BhopState`/`DodgeRollState` for logic, applies physics via `MoveAndSlide()`.

## Movement System

### Ground Movement
- 8-directional via WASD, direction relative to the active `Camera3D`'s facing.
- Instant full-speed with no acceleration ramp. Releasing input stops immediately.
- Speed = `BaseSpeed * BhopState.SpeedMultiplier` (default base: 10 units/s).

### Jump & Bunny Hopping
- Jump velocity: 8 units/s upward, gravity: 20 units/s². Jump height ≈ 1.6 units, air time ≈ 0.8s.
- **Jump buffering**: Jump input is buffered. If jump was pressed within the bhop timing window before landing, it triggers immediately on contact.
- **Bhop timing window**: 100ms. If the player jumps within 100ms of landing (before or after), `BhopState.TryBhop()` succeeds.
- **Speed stacking**: Each successful bhop adds +0.12 to the speed multiplier (12% of base speed per bhop). Caps at 1.8x.
- **Grace period**: Speed decay does not begin until the player has been grounded for longer than the timing window (100ms), giving the full window to hit the bhop without penalty.
- **Speed decay**: After the grace period, speed decays linearly at 3.0 multiplier units/s. From max (1.8x), full decay takes ~0.27s.

### Air Strafing
- Only left/right (A/D) input applies force while airborne, using the camera's right vector.
- Influence rate: 8 units/s². Over a 0.8s jump, this adds ~6.4 units/s of lateral velocity.
- Horizontal speed is capped at `BaseSpeed * SpeedMultiplier` so strafing changes direction without increasing speed.

### Dodge Roll
- Ground-only. Cannot activate while airborne or already rolling.
- Duration: 0.5s. Player moves at 18 units/s in the input direction (or camera forward if no input).
- I-frames: First 0.3s of the roll grants invulnerability.
- Cooldown: 1.5s after the roll ends before the next roll is available.

## Exported Properties

| Property | Default | Description |
|----------|---------|-------------|
| `BaseSpeed` | 10 | Ground movement speed (units/s) |
| `JumpVelocity` | 8 | Upward velocity on jump (units/s) |
| `Gravity` | 20 | Downward acceleration (units/s²) |
| `AirStrafeInfluence` | 8 | Lateral force while airborne (units/s²) |
| `DodgeRollSpeed` | 18 | Movement speed during dodge roll (units/s) |

## Tests

`tests/GodotExperiment.Tests/BhopStateTests.cs` — 20 tests covering timing window edge cases, speed stacking, cap enforcement, decay rate, chain tracking, and reset.

`tests/GodotExperiment.Tests/DodgeRollStateTests.cs` — 18 tests covering roll initiation, i-frame window, roll duration, cooldown lifecycle, full sequences, and reset.
