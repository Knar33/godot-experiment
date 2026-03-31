# Player Architecture

## Scene Structure

`scenes/player/Player.tscn` is a standalone scene instanced into `Game.tscn`.

```
Player (CharacterBody3D) [scripts/player/Player.cs]
├── CollisionShape3D — CapsuleShape3D (radius 0.4, height 1.8), centered at y=0.9
└── MeshInstance3D — CapsuleMesh placeholder (same dimensions), light blue material
```

The capsule bottom sits at y=0 so the player stands flush on the arena floor. The player is added to the `"player"` group at runtime for camera and other systems to locate it.

## Code Split

Player mechanics are split across two layers:

- **`src/GodotExperiment.Core/PlayerMovement/BhopState.cs`** — Pure C# bhop timing, speed stacking, decay logic. No Godot dependency, fully unit-testable. Namespace: `GodotExperiment.PlayerMovement`.
- **`src/GodotExperiment.Core/PlayerMovement/DodgeRollState.cs`** — Pure C# dodge roll state machine (rolling, i-frames, cooldown). No Godot dependency, fully unit-testable. Namespace: `GodotExperiment.PlayerMovement`.
- **`scripts/player/Player.cs`** — Godot `CharacterBody3D` script. Reads input, queries camera for direction, delegates to `BhopState`/`DodgeRollState` for logic, applies physics via `MoveAndSlide()`.

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

## Camera

`scenes/player/PlayerCamera.tscn` is instanced into `Game.tscn` separately from the player.

```
PlayerCamera (Node3D) [scripts/player/PlayerCamera.cs]
└── Camera3D
```

### Orbit & Mouse-Look
- The camera orbits around an **orbit center** that smoothly follows the player's position plus a vertical offset (default 1.5 units, roughly head height).
- Mouse X input rotates **yaw** (unlimited horizontal rotation).
- Mouse Y input rotates **pitch** (clamped between -80° and +60° to prevent flipping).
- Mouse is captured on startup. Pressing Escape toggles captured/visible mode (placeholder until pause menu is implemented). Clicking while uncaptured re-captures.

### Positioning
- The camera faces the direction defined by (pitch, yaw) — this is the **aim direction**.
- The camera is positioned **behind** the aim direction at a configurable distance (default 8 units) from an **offset orbit center**.
- The offset center is displaced to the right of the player (default 0.6 units along the camera's right vector), creating an over-the-shoulder view. This shifts the player to the left of screen so the crosshair at screen center targets the world ahead rather than the player's back.

### Scroll-Wheel Zoom
- Mouse scroll adjusts a target distance between `MinDistance` (default 3) and `MaxDistance` (default 14) in discrete steps (`ZoomStep`, default 1 unit).
- Each physics frame, the actual `Distance` smoothly interpolates toward the target using exponential lerp (`ZoomSmoothing`, default 12).

### Smooth Follow
- Orbit center position uses framerate-independent exponential interpolation: `lerp(current, target, 1 - exp(-speed * dt))` with a default speed of 20.
- Initial orbit center snaps to the player on the first frame.

### Collision Avoidance
- Each physics frame, a raycast is cast from the orbit center toward the desired camera position.
- If the ray hits arena geometry (walls, floor), the camera is pulled forward to the hit point plus a small margin (0.3 units along the hit normal).
- The player's own collision is excluded from this raycast.

### Aim Point
- A raycast is cast from the camera center (screen center) into the world each physics frame.
- The hit point (or a far fallback point at 100 units) is stored as `AimPoint` for shooting systems to use.
- The player's collision is excluded so the aim ray passes through the character.

### Crosshair
- A `Control` node (`scripts/ui/Crosshair.cs`) draws a small cross at screen center using `_Draw()`.
- The crosshair has 4 arms (default 8px length, 2px thick) separated by a small gap (3px) from center.
- Added as a full-rect child of the UI CanvasLayer in `Game.tscn` with `mouse_filter = IGNORE`.

### Camera Exported Properties

| Property | Default | Description |
|----------|---------|-------------|
| `MouseSensitivity` | 0.002 | Radians per pixel of mouse movement |
| `Distance` | 8 | Distance from orbit center to camera (units) |
| `MinDistance` | 3 | Minimum zoom distance (units) |
| `MaxDistance` | 14 | Maximum zoom distance (units) |
| `ZoomStep` | 1 | Distance change per scroll tick (units) |
| `ZoomSmoothing` | 12 | Exponential interpolation rate for zoom |
| `MinPitch` | -80 | Minimum vertical angle (degrees) |
| `MaxPitch` | 60 | Maximum vertical angle (degrees) |
| `FollowSpeed` | 40 | Exponential follow rate (higher = snappier) |
| `VerticalOffset` | 1.5 | Height above player position for orbit center (units) |
| `HorizontalOffset` | 0.6 | Rightward offset from player for over-the-shoulder view (units) |
| `AimRayLength` | 100 | Maximum distance for aim raycast (units) |
| `ClipMargin` | 0.3 | Offset from geometry surface when camera clips (units) |

## Tests

`tests/GodotExperiment.Tests/BhopStateTests.cs` — 20 tests covering timing window edge cases, speed stacking, cap enforcement, decay rate, chain tracking, and reset.

`tests/GodotExperiment.Tests/DodgeRollStateTests.cs` — 18 tests covering roll initiation, i-frame window, roll duration, cooldown lifecycle, full sequences, and reset.

Camera and crosshair are Godot-dependent (Node3D, Camera3D, raycasts, Control drawing) and are not covered by pure unit tests.
