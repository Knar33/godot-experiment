# Game Feel Architecture

## Code Split

Game feel systems follow the same Core/Godot split as player mechanics.

- **`src/GodotExperiment.Core/GameFeel/ScreenShakeState.cs`** — Decaying-spring shake accumulator. Additive intensity with hard cap. No Godot dependency. Namespace: `GodotExperiment.GameFeel`.
- **`src/GodotExperiment.Core/GameFeel/HitStopState.cs`** — Frame-counted freeze timer. No Godot dependency. Namespace: `GodotExperiment.GameFeel`.
- **`scripts/player/ScreenShake.cs`** — Component on `PlayerCamera`. Reads `ScreenShakeState.CurrentOffset` and applies to camera transform each frame. Multiplied by user's screen shake intensity setting (0-1).
- **`scripts/managers/HitStopManager.cs`** — Autoload. Sets `Engine.TimeScale = 0` while `HitStopState.IsFrozen` is true. Uses `_Process` (unscaled delta) to count real frames.
- **`scripts/ui/HitMarker.cs`** — Control node sibling to `Crosshair`. Draws hit/kill markers via `_Draw()`.
- **`scripts/ui/SpeedLines.cs`** — Control node on UI CanvasLayer. Draws radial lines via `_Draw()` scaled by bhop speed.
- **`scripts/ui/BhopCounter.cs`** — Label node on HUD. Displays chain count, fades after timeout.
- **`scripts/ui/ThreatIndicator.cs`** — Control node on HUD. Draws red arcs for nearby off-screen enemies.

## Screen Shake

### ScreenShakeState (Core)

Uses a 2D decaying spring model (frequency 15 Hz, damping 0.5):

- `AddShake(float intensity)` — Adds energy to the spring.
- `Update(float dt)` — Steps the simulation, updates `CurrentOffset` (Vector2).
- `MaxIntensity` — Hard cap (default 8.0) prevents stacking beyond comfort.

### Shake Intensities

| Event | Intensity |
|-------|-----------|
| Player shot fired | 0.3 |
| Enemy hit confirmed | 0.5 |
| Crawler death | 0.2 |
| Mid-tier enemy death (Charger, Bloater, Howler) | 1.5 |
| Titan death | 4.0 |
| Bloater explosion | 3.5 |
| Player death | 5.0 |

### Integration

`ScreenShake.cs` on `PlayerCamera` applies `ScreenShakeState.CurrentOffset` as a displacement perpendicular to the aim direction each `_PhysicsProcess`. The offset is multiplied by the user's screen shake intensity setting (0.0-1.0, default 1.0).

## Hit Stop

### HitStopState (Core)

- `Trigger(int frames)` — Sets or extends freeze duration. Capped at 5 frames (~83ms).
- `Update()` — Decrements by 1. Returns `true` while frozen.
- `IsFrozen` — Whether hit stop is active.

Multiple triggers during an active freeze extend duration but do not stack multiplicatively.

### Hit Stop Values

| Event | Frames |
|-------|--------|
| Enemy kill (single) | 1-2 |
| Multi-kill (same frame) | 3-4 |
| Titan ground slam impact | 2-3 |
| Bloater explosion | 2-3 |

### Integration

`HitStopManager` autoload updates `HitStopState` in `_Process` with unscaled time. While frozen, `Engine.TimeScale = 0.0`. On unfreeze, restores to `1.0`. The hit stop intensity setting (0.0-1.0) scales the triggered frame count (at 0.0, all triggers become 0 frames).

## FOV Scaling

Handled in `PlayerCamera.cs`. Each frame:

```
effectiveFov = baseFov + (speedMultiplier - 1.0) * FovScalePerUnit
```

`FovScalePerUnit` default: 6°. At max bhop (1.8x), FOV widens by ~4.8°. Smoothly interpolated via exponential lerp to prevent jumps.

| Property | Default | Description |
|----------|---------|-------------|
| `BaseFov` | 75 | User-configurable base FOV (degrees) |
| `FovScalePerUnit` | 6 | FOV increase per 1.0x speed multiplier (degrees) |

## Speed Lines

`SpeedLines.cs` is a full-rect `Control` on the UI CanvasLayer.

- Only visible when `BhopState.SpeedMultiplier > 1.3`.
- Line count, length, and alpha scale linearly from 0 (at 1.3x) to max (at 1.8x).
- Lines radiate from the player's velocity direction projected to screen space.
- Respects the speed lines toggle in settings. Disabled when "Reduce Motion" is on.
- Drawn via `_Draw()` with `DrawLine()` — no scene tree overhead.

## Projectile Trail

Each `PlayerProjectile` has a `TrailRenderer` child (`MeshInstance3D` with an `ImmediateMesh` or trail plugin):

- Trail length: 2-3 projectile-lengths behind the projectile.
- Same cyan emissive material as the projectile mesh, alpha fading from 1.0 to 0.0 along the trail.
- Trail persists for 0.1s after projectile destruction for a clean visual.

## Muzzle Flash

A `GpuParticles3D` node on the player at the projectile spawn point (0, 1.2, 0):

- One-shot burst per firing event (1-3 particles, lifetime 0.05s).
- Billboard mode, bright cyan/white material.
- Random rotation per emission to avoid repetition.
- Small enough not to obscure the crosshair.

## Hit Marker

`HitMarker.cs` is a `Control` node sibling to `Crosshair` on the HUD CanvasLayer.

- **Hit**: On receiving a hit signal, draws 4 small white tick marks around crosshair center (3px length). Fades over 0.1s.
- **Kill**: Draws a slightly larger red X that fades over 0.15s. Replaces the hit marker if both trigger on the same frame.
- Drawn via `_Draw()`, redrawn on signal.

## Bhop Chain Counter

`BhopCounter.cs` is a `Label` on the HUD CanvasLayer, positioned below the crosshair.

- Hidden by default.
- `BhopState` exposes `ConsecutiveBhops`. When >= 2, label shows "x{count}".
- On chain break (speed decay starts), label fades to alpha 0 over 0.5s via tween.

## Directional Threat Indicator

`ThreatIndicator.cs` is a full-rect `Control` on the HUD CanvasLayer.

- Each `_PhysicsProcess`, queries nodes in the `"enemy"` group.
- For enemies within close range (default 5 units) that are outside the camera frustum, calculates the angle from camera forward to the enemy.
- Draws a red arc segment on the screen edge at the corresponding angle. Alpha scales with proximity (closer = more opaque, max 0.6).
- Limited to the closest 3 off-screen threats to avoid clutter.

## Enemy Damage Flash

Handled in the base enemy script via a `ShaderMaterial` with a `flash_intensity` uniform:

- On damage: set `flash_intensity = 1.0`, decay to 0.0 over 2 frames (0.033s).
- Override material is shared per-type but instanced per-enemy to allow independent flashing.

## Enemy Attack Telegraph Flash

Attack telegraphs can contribute a high-visibility flash using the same `enemy_flash.gdshader` (which mixes the enemy's base albedo toward white based on `flash_intensity`).

- `scripts/enemies/BaseEnemy.cs` exposes `protected float TelegraphFlashIntensity` (0.0–1.0) for subclasses to drive.
- `BaseEnemy.UpdateFlash()` applies the final shader intensity as:
  - `max(damageFlash, lowHealthFlash, TelegraphFlashIntensity)`
- `scripts/enemies/Charger.cs` pulses `TelegraphFlashIntensity` during the Telegraph phase to provide an obvious flashing visual cue.

## Enemy Low-Health Indicator

When enemy health drops below 25%:

- `flash_intensity` oscillates at 2 Hz between 0.0 and 0.3 (a subtle flicker).
- Optional trailing spark `GpuParticles3D` emitting at low rate, attached to the enemy node.

## Enemy Death Effects

Each enemy type configures death particles on the base class:

- `DeathParticleColor` — Matches the enemy's color palette.
- `DeathParticleCount` — Scaled by importance (Crawler: 8, Titan: 60).
- `DeathParticleExplosiveness` — Instant for small enemies (1.0), slightly staggered for large (0.7).

On death, a detached `GpuParticles3D` is spawned at the enemy's position, emits once, then self-frees. The enemy node is freed immediately.

## Gem Collection Feedback

### Magnetism Snap

When a gem enters the player's collection `Area3D`, it enters a "snapping" state:

- Lerps toward the player over ~0.1s with a slight curved path (additive perpendicular velocity).
- On arrival, triggers the collection event (gem count increment, particle, sound).

### Sound Pitch Scaling

`Player.cs` tracks time since last gem pickup. If < 0.3s, the next pickup sound's `PitchScale` increases by +0.05 (additive, capped at base + 0.5). Resets after 0.3s gap.

### Upgrade Meter Pulse

When the gem count reaches the upgrade threshold, the HUD upgrade meter element plays a scale pulse tween (1.0 → 1.2 → 1.0) before the game pauses for upgrade selection.

## Tests

`tests/GodotExperiment.Tests/ScreenShakeStateTests.cs` — Additive intensity, hard cap, spring decay, zero-energy at rest.

`tests/GodotExperiment.Tests/HitStopStateTests.cs` — Frame countdown, frozen state, multi-trigger extension, max cap.
