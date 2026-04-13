# Waves Architecture

## Overview

Waves are driven by a Core-only state machine (`WaveManagerState`) and orchestrated by a Godot scene node (`WaveManager`) that delegates actual instantiation to `EnemySpawner`.

## Code Split

- `src/GodotExperiment.Core/Waves/WaveCompositions.cs`
  - Defines the enemy composition per wave (hand-authored for waves 1–5, formula beyond that).
  - Defines the *intended* per-wave spawn interval (`WaveDefinition.SpawnInterval`).
- `src/GodotExperiment.Core/Waves/WaveManagerState.cs`
  - Maintains the current wave number and a shuffled spawn queue.
  - Emits `WaveStarted` on each wave advance.
  - Returns the next enemy type to spawn when the spawn timer elapses.
- `scripts/managers/WaveManager.cs`
  - Updates `WaveManagerState` only while `GameManager` is in `Playing`.
  - Translates returned enemy type strings into `PackedScene` instances.
  - Calls `EnemySpawner.SpawnEnemyOfType(scene)` to spawn.

## Scene Wiring

`scenes/Game.tscn` contains:

- `EnemySpawner` (`scripts/enemies/EnemySpawner.cs`)
  - `WaveManaged = true` so it does not self-spawn on its own timer.
- `WaveManager` (`scripts/managers/WaveManager.cs`)
  - `EnemySpawnerPath` points at `EnemySpawner`.

## Playtest Spawn Interval Override (Temporary)

To support early playtesting (readability/telegraph iteration), the wave system supports forcing a fixed delay between individual spawns:

- **Core knob**: `WaveManagerState.SpawnIntervalOverrideSeconds`
  - If `> 0`, it overrides `WaveDefinition.SpawnInterval` for all waves.
- **Godot knob**: `WaveManager.SpawnIntervalOverrideSeconds` (`[Export]`)
  - Assigned to the Core state in `_Ready()`.
  - `scenes/Game.tscn` currently sets this to `5.0` seconds.

## Tests

`tests/GodotExperiment.Tests/WaveManagerStateTests.cs` includes coverage for:

- Spawn timing behavior and continuous flow
- Spawn interval override behavior (`Update_RespectsSpawnIntervalOverride`)

