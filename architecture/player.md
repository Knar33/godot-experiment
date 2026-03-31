# Player Architecture

## Scene Structure

`scenes/player/Player.tscn` is a standalone scene instanced into `Game.tscn`.

```
Player (CharacterBody3D) [scripts/player/Player.cs]
├── CollisionShape3D — CapsuleShape3D (radius 0.4, height 1.8), centered at y=0.9
├── MeshInstance3D — CapsuleMesh placeholder (same dimensions), light blue material
├── MuzzleFlash (GpuParticles3D) — One-shot burst per firing event, position (0, 1.2, 0)
├── FireAudio (AudioStreamPlayer3D) — Firing sound, bus: PlayerSFX
├── KillAudio (AudioStreamPlayer3D) — Kill confirmation sound, bus: PlayerSFX
├── BhopAudio (AudioStreamPlayer3D) — Bhop landing sound, bus: PlayerSFX
└── RollAudio (AudioStreamPlayer3D) — Dodge roll whoosh, bus: PlayerSFX
```

The capsule bottom sits at y=0 so the player stands flush on the arena floor. The player is added to the `"player"` group at runtime for camera and other systems to locate it.

## Code Split

Player mechanics are split across two layers:

- **`src/GodotExperiment.Core/PlayerMovement/BhopState.cs`** — Pure C# bhop timing, speed stacking, decay logic. No Godot dependency, fully unit-testable. Namespace: `GodotExperiment.PlayerMovement`.
- **`src/GodotExperiment.Core/PlayerMovement/DodgeRollState.cs`** — Pure C# dodge roll state machine (rolling, i-frames, cooldown). No Godot dependency, fully unit-testable. Namespace: `GodotExperiment.PlayerMovement`.
- **`src/GodotExperiment.Core/Combat/AutoFireState.cs`** — Pure C# fire rate timing. Tracks elapsed time, determines when to fire. Namespace: `GodotExperiment.Combat`.
- **`src/GodotExperiment.Core/Combat/ProjectileState.cs`** — Pure C# projectile range tracking for despawn. Namespace: `GodotExperiment.Combat`.
- **`scripts/player/Player.cs`** — Godot `CharacterBody3D` script. Reads input, queries camera for direction, delegates to `BhopState`/`DodgeRollState`/`AutoFireState` for logic, applies physics via `MoveAndSlide()`, spawns projectiles.
- **`scripts/player/PlayerProjectile.cs`** — Godot `Area3D` script. Moves the projectile forward, delegates range tracking to `ProjectileState`, handles enemy collision via `BodyEntered`.

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

## Shooting

### Auto-Fire (Hold-to-Fire)

The player fires continuously while the fire input is held (`shoot` action — left mouse button / right trigger) and `GameManager.CurrentState == GameState.Playing`. Fire timing is managed by `AutoFireState` (Core, `GodotExperiment.Combat`).

- **Fire rate**: ~8 shots/sec (interval 0.125s).
- Each physics frame while the fire input is held, `AutoFireState.Update(dt)` accumulates time. When `CanFire` is true, `TryFire()` resets the timer and triggers a projectile spawn.
- When the fire input is released, `AutoFireState` stops accumulating time. The timer resets on the next press so the first shot fires immediately when the player starts holding.

### Projectile

`scenes/player/PlayerProjectile.tscn` is a standalone Area3D scene instanced at runtime.

```
PlayerProjectile (Area3D) [scripts/player/PlayerProjectile.cs]
├── CollisionShape3D — SphereShape3D (radius 0.15)
├── MeshInstance3D — SphereMesh (radius 0.15), bright cyan emissive material
├── Trail (MeshInstance3D) — ImmediateMesh trail, 2-3 projectile-lengths, fading cyan
└── ImpactAudio (AudioStreamPlayer3D) — Impact sound, bus: PlayerSFX
```

- **Collision layer**: 3 (player projectiles, bitmask 4). **Collision mask**: layers 1 (arena geometry, bitmask 1) and 4 (enemies, bitmask 8).
- **Speed**: 50 units/s. Covers the arena radius (25 units) in 0.5s.
- **Range**: Despawns after traveling 25 units (arena radius), tracked by `ProjectileState` (Core).
- **Arena collision**: On `BodyEntered`, if the body is arena geometry (layer 1), the projectile plays an impact effect and destroys itself.
- **Enemy collision**: On `BodyEntered`, if the body is in the `"enemy"` group, the projectile plays an impact effect and destroys itself. Damage application will be added when the enemy health system is implemented.
- Projectiles are spawned into the `Projectiles` Node3D container in `Game.tscn`.

### Audio

- **Fire sound**: An `AudioStreamPlayer3D` on the player plays a short, punchy firing sound each time a projectile is spawned. Uses a small pool of slightly pitch-randomized variations (±5%) to avoid repetitive machine-gun monotony at 8 shots/sec.
- **Impact sound**: Each projectile carries an `AudioStreamPlayer3D` that plays on collision (enemy or arena surface) at the impact position before the projectile is freed. Different sounds for enemy hits vs. surface hits.

### Aim Direction

Projectiles spawn at the player's position + (0, 1.2, 0) (upper chest height). The direction is `(AimPoint - spawnPosition).Normalized()`, using `PlayerCamera.AimPoint`. If the aim point is closer than 2 units, the camera's forward direction is used as fallback to prevent wild aim angles.

### Collision Layer Assignments

| Layer | Purpose | Bitmask |
|-------|---------|---------|
| 1 | Arena geometry, player | 1 |
| 3 | Player projectiles | 4 |
| 4 | Enemies (future) | 8 |

### Projectile Exported Properties

| Property | Default | Description |
|----------|---------|-------------|
| `Speed` | 50 | Travel speed (units/s) |
| `MaxRange` | 25 | Despawn distance (units) |

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

### Crosshair & Hit Marker
- A `Control` node (`scripts/ui/Crosshair.cs`) draws a small cross at screen center using `_Draw()`.
- The crosshair has 4 arms (default 8px length, 2px thick) separated by a small gap (3px) from center.
- Crosshair style (cross/dot/circle) and color are configurable via settings (see `architecture/settings.md`).
- A sibling `Control` node (`scripts/ui/HitMarker.cs`) draws hit/kill confirmation markers overlaid on the crosshair (see `architecture/game-feel.md`).
- Added as full-rect children of the UI CanvasLayer in `Game.tscn` with `mouse_filter = IGNORE`.

### FOV Scaling
- Camera FOV widens subtly as the player's bhop speed multiplier increases (`FovScalePerUnit` default 6° per 1.0x). Smoothly interpolated via exponential lerp.
- Base FOV is configurable via settings (default 75°). Speed-based scaling is additive on top.
- See `architecture/game-feel.md` for implementation details.

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

`tests/GodotExperiment.Tests/AutoFireStateTests.cs` — 14 tests covering fire interval timing, CanFire/TryFire behavior, timer reset, custom intervals, and shots-per-second accuracy.

`tests/GodotExperiment.Tests/ProjectileStateTests.cs` — 9 tests covering distance accumulation, max range expiry, small-increment simulation, and travel time accuracy.

Camera, crosshair, and projectile Godot scripts are Godot-dependent and are not covered by pure unit tests.
