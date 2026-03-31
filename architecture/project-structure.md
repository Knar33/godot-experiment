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
  - `scenes/enemies/` — Base enemy scene and per-type enemy scenes (Crawler, etc.)
  - `scenes/pickups/` — Gem pickup scene
  - `scenes/player/` — Player and camera scenes
  - `scenes/Game.tscn` — Root game scene (main scene)
- `scripts/` — Godot-specific C# scripts (nodes, autoloads) that reference Core types, organized by domain
  - `scripts/arena/` — Arena node scripts
  - `scripts/enemies/` — Enemy scripts (BaseEnemy, Crawler, EnemySpawner)
  - `scripts/managers/` — Autoload and manager scripts (GameManager, WaveManager, HitStopManager, MusicManager, SettingsManager)
  - `scripts/pickups/` — Pickup scripts (GemPickup)
  - `scripts/player/` — Player, camera, and projectile scripts (ScreenShake)
  - `scripts/ui/` — HUD and UI element scripts (Crosshair, CountdownUI, SurvivalTimerUI, GemCounterUI, DeathScreen, PauseMenu, HitMarker, SpeedLines, BhopCounter, ThreatIndicator, SettingsMenu)
- `src/GodotExperiment.Core/` — Pure C# classes: enums, state machines, data models, calculations
  - `PlayerMovement/` — Player movement state (BhopState, DodgeRollState) — namespace `GodotExperiment.PlayerMovement`
  - `Combat/` — Combat mechanics (AutoFireState, ProjectileState, DamageSource, PlayerHealthState) — namespace `GodotExperiment.Combat`
  - `Enemies/` — Enemy logic (EnemyHealthState, SpawnPointSelector, SpitterAIState, SeparationState) — namespace `GodotExperiment.Enemies`
  - `GameLoop/` — Game state management (GameState, GameStateMachine, SurvivalTimerState, CountdownState, UpgradeMeterState, RunStatistics, PersonalBestState) — namespace `GodotExperiment.GameLoop`
  - `Waves/` — Wave system (WaveEnemyGroup, WaveDefinition, WaveCompositions, WaveManagerState) — namespace `GodotExperiment.Waves`
  - `GameFeel/` — Screen shake and hit stop state (ScreenShakeState, HitStopState) — namespace `GodotExperiment.GameFeel`
  - `Settings/` — Settings data model (SettingsData) — namespace `GodotExperiment.Settings`
- `tests/GodotExperiment.Tests/` — xUnit tests for Core classes
- `assets/audio/` — Audio assets organized by category (music/, player/, enemies/, ui/, ambience/)
  - `assets/audio/enemies/` — Per-enemy-type audio (ambient loops, death sounds, telegraphs)
- `assets/shaders/` — Shader files (enemy_flash.gdshader)
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

`GameManager` (scripts/managers/) is a Godot `Node` autoload (`ProcessMode = Always`) that wraps `GameStateMachine`, `CountdownState`, `SurvivalTimerState`, `UpgradeMeterState`, `RunStatistics`, and `PersonalBestState`. It orchestrates:

- **Countdown**: on `_Ready()` and on restart, starts a 3-second countdown via `CountdownState`. Player movement is locked during countdown. Emits `CountdownTick(int)` and `CountdownFinished` signals.
- **Playing**: on countdown finish, transitions to Playing, starts the survival timer, and connects bhop tracking from the player's `BhopState` to `RunStatistics`. Emits `SurvivalTimeUpdated(string)` each frame.
- **Death**: `TriggerPlayerDeath()` freezes the timer on the exact death frame and checks `PersonalBestState` to determine if the run is a new personal best (`LastRunWasPersonalBest`). After the camera freeze (0.3s), `PlayerCamera` transitions to Dead.
- **Pause**: Escape during Playing pauses the tree (`GetTree().Paused = true`) and shows the PauseMenu. Resume unpauses and re-captures the mouse.
- **Restart**: R key from Dead state (or restart button from pause menu) clears all enemies/projectiles/gems, resets player position/state, resets upgrade meter and run statistics, and starts a new countdown.
- **Gems / Upgrade Meter**: `AddGems(int)` feeds both `UpgradeMeterState` and `RunStatistics`. The meter tracks gems toward an escalating threshold (10, 15, 20, …). Emits `GemCountChanged(int, int)` on collection and fires `ThresholdReached` when full (future upgrade selection will consume the upgrade via `ConsumeUpgrade()`).
- **Run Statistics**: `RunStatistics` tracks enemies killed, gems collected, wave reached, longest bhop chain, and upgrades chosen during a run. Bhop events are wired from the player's `BhopState` via `BhopLanded` and `ChainBroken` events. Stats are exposed for the death screen and reset on restart.
- **Personal Best**: `PersonalBestState` tracks the best survival time across sessions. Checked on death; `LastRunWasPersonalBest` flag is set for the death screen UI to show a "NEW BEST" callout.

State changes are re-emitted as Godot signals via `StateChanged(int, int)` so UI nodes can react.

## Enemy System

### Core Types (pure C#, `GodotExperiment.Enemies`)

`EnemyHealthState` tracks per-enemy HP with damage, death events, and a low-health flag (≤25% max HP). All enemies use integer health; player projectiles deal 1 damage by default.

`SpawnPointSelector` provides weighted random spawn point selection. `ComputeWeights()` biases toward points the player is facing (dot product mapping: front ≈ 1.0, behind ≈ 0.2). `SelectWeighted()` performs weighted random index selection from the computed weights.

### Godot Types

`BaseEnemy` (`scripts/enemies/BaseEnemy.cs`) is a `CharacterBody3D` on collision layer 4 (mask 1 for arena geometry). It composes `EnemyHealthState` and provides:

- **Movement**: Direct steering toward the player each physics frame (flat arena, no nav mesh needed). Override `MoveTowardPlayer()` in subclasses for specialized AI.
- **Contact damage**: `ContactArea` (child `Area3D`, mask 1) detects player via group check and calls `Player.TakeDamage(DamageSource.Contact)`.
- **Damage flash**: `ShaderMaterial` (`assets/shaders/enemy_flash.gdshader`) with `flash_intensity` uniform. Set to 1.0 on hit, decays linearly over 2 frames (~33ms). When health ≤ 25%, oscillates at 2 Hz between 0.0–0.3.
- **Death effects**: Spawns a detached one-shot `GpuParticles3D` with configurable color/count/explosiveness, then `QueueFree()` immediately. Calls `GameManager.RecordEnemyKill()`.
- **Gem dropping**: Spawns `GemPickup` instances (count from `GemDropCount`) scattered outward from the death position.
- **Spawn-in**: Large enemies (`IsLargeEnemy = true`) are visible but inactive for `SpawnInDuration` seconds before moving/dealing damage.

All enemies are added to the `"enemy"` group. `PlayerProjectile.OnBodyEntered` calls `BaseEnemy.TakeDamage(1)` on hit.

### Audio Playback

`BaseEnemy` handles ambient and death audio for all enemy types:

- **Ambient**: On `_Ready()`, if the `AmbientAudio` node has a stream assigned, it begins playing and auto-loops via the `Finished` signal. Stopped on death.
- **Death**: On death, the `DeathAudio` node is reparented to a temporary detached `Node3D` so it outlives the enemy's `QueueFree()`. The temp node self-frees when playback finishes.

Audio streams are configured per-type in each enemy's scene file (e.g. `Crawler.tscn` assigns `crawler_ambient.wav` and `crawler_death.wav`). Enemies without audio streams assigned simply skip playback.

### Crawler

`Crawler` (`scripts/enemies/Crawler.cs`) extends `BaseEnemy` with no overrides — the default move-toward-player and contact-damage behavior is exactly the Crawler's AI. Stats configured in `scenes/enemies/Crawler.tscn`:

- **Health**: 3 (dies in 3 shots)
- **Speed**: 7 units/s (moderate)
- **Gems**: 1 per kill
- **Mesh**: Flattened sphere (green, scale 1.3×0.6×1.3) — distinct low/wide silhouette vs. the base capsule
- **Audio**: Quiet skittering ambient (looping, -12 dB), small crunch/pop death sound

### Spitter

`Spitter` (`scripts/enemies/Spitter.cs`) extends `BaseEnemy` with a ranged AI driven by `SpitterAIState` (Core, `GodotExperiment.Enemies`). The AI has three phases:

1. **Approaching**: Moves toward the player until within preferred range (14 ± 4 units).
2. **Planted**: Stops moving and fires arcing projectiles on a 3-second interval. First shot fires at half-interval after planting.
3. **Repositioning**: If the player moves out of range and the Spitter has been planted for ≥5 seconds, it resumes movement to close/widen distance.

Stats configured in `scenes/enemies/Spitter.tscn`:

- **Health**: 5 (dies in 5 shots)
- **Speed**: 3 units/s (slow)
- **Gems**: 2 per kill
- **Mesh**: Tapered cylinder (purple, wider at base) — distinct upright silhouette vs. Crawler's low sphere
- **Audio**: Low gurgling ambient (-10 dB), spit/hiss telegraph on fire, wet burst death sound

`SpitterProjectile` (`scripts/enemies/SpitterProjectile.cs`) is an `Area3D` that follows a parabolic arc computed from spawn position to the player's position at fire time. Flight time is ~1.2 seconds. On collision with arena geometry or the player (mask 1), it spawns a `SpitterGroundHazard` and frees itself. Direct player hits deal `DamageSource.Projectile`. A Y-position failsafe triggers ground impact if the projectile falls below floor level.

`SpitterGroundHazard` (`scripts/enemies/SpitterGroundHazard.cs`) is an `Area3D` with a flat glowing disc mesh (yellow-green, transparent). Lasts 1.5 seconds, fading out over its lifetime. Contact with the player deals `DamageSource.GroundHazard`. Self-frees on expiry.

`EnemySpawner` (`scripts/enemies/EnemySpawner.cs`) handles the mechanics of placing enemies in the arena. Uses `SpawnPointSelector` for behind-player bias. Exposes `SpawnEnemyOfType(PackedScene)` for wave-managed spawning and `SpawnEnemyAt(PackedScene, Vector3)` for direct placement. When `WaveManaged = true` (default in `Game.tscn`), EnemySpawner's internal timer loop is disabled and spawning is driven entirely by `WaveManager`.

`GemPickup` (`scripts/pickups/GemPickup.cs`) is an `Area3D` in the `"gems"` group (layer 5, mask 0, monitorable) with a scatter animation on spawn. When the player's `GemCollectionArea` (Area3D, mask 16, radius 2 units) detects a gem, the gem enters magnetism mode: it accelerates toward the player over ~0.12s with a slight upward arc, then emits `Collected` and frees itself. On collection, `Player.cs` calls `GameManager.AddGems(1)`, plays a chime via `CollectAudio` (`AudioStreamPlayer3D`) with ascending pitch on rapid pickups (+0.05 per pickup within 0.3s, capped at +0.5, reset after 0.3s gap), and spawns a one-shot green `GpuParticles3D` burst at player height.

## Wave System

### Core Types (pure C#, `GodotExperiment.Waves`)

`WaveEnemyGroup` pairs an enemy type name (string) with a spawn count. `WaveDefinition` groups multiple `WaveEnemyGroup`s with a wave number and spawn interval. `TotalEnemyCount` sums all groups.

`WaveCompositions` is a static provider of wave definitions. Waves 1–5 are hand-authored per `design/waves.md`:

| Wave | Composition | Spawn Interval |
|------|-------------|----------------|
| 1 | 5 Crawlers | 2.0s |
| 2 | 10 Crawlers | 1.5s |
| 3 | 10 Crawlers, 2 Spitters | 1.2s |
| 4 | 12 Crawlers, 3 Spitters, 1 Charger | 1.0s |
| 5 | 14 Crawlers, 3 Spitters, 1 Charger, 6 Drones | 0.8s |

Waves 6+ use a scaling formula (Crawler-only placeholder) until mid/late compositions are implemented (task 15). Enemy types are identified by string constants (`WaveCompositions.Crawler`, etc.) which the Godot layer maps to scene paths.

`WaveManagerState` drives wave progression. On `Start()`, it fills a spawn queue from wave 1's definition (shuffled for variety). `Update(dt)` ticks a spawn timer; when it expires, it dequeues the next enemy type and returns it. When the queue empties, it immediately advances to the next wave (no downtime). Accepts an optional `seed` for deterministic testing.

### Godot Types

`WaveManager` (`scripts/managers/WaveManager.cs`) is a scene node in `Game.tscn` that composes `WaveManagerState`. It:

- Connects to `GameManager.CountdownFinished` to start waves after each countdown
- Connects to `GameManager.StateChanged` to reset on death/restart
- In `_PhysicsProcess`, calls `WaveManagerState.Update()` and spawns the returned enemy type via `EnemySpawner.SpawnEnemyOfType()`
- Maps enemy type strings to `PackedScene` paths; gracefully skips types whose scenes don't exist yet
- Reports wave number to `GameManager.RecordWaveReached()` on each wave advance
- Emits `WaveStarted(int)` signal for UI consumption

### Collision Layers

| Layer | Bit Value | Usage |
|-------|-----------|-------|
| 1 | 1 | Arena geometry, Player (default) |
| 3 | 4 | Player projectiles |
| 4 | 8 | Enemies |
| 5 | 16 | Gem pickups |

`PlayerProjectile`: layer 3, mask 1+4 (hits arena geometry and enemies).
`BaseEnemy`: layer 4, mask 1 (collides with arena geometry for movement).
`GemPickup`: layer 5, mask 0 (detected by player collection area, not self-monitoring).

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
