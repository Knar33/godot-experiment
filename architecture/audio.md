# Audio Architecture

## Audio Bus Layout

Godot's AudioServer is configured with the following bus hierarchy:

```
Master
├── Music
├── SFX
│   ├── PlayerSFX
│   ├── EnemySFX
│   └── UISFX
└── Ambience
```

Each bus has independent volume control mapped to the settings sliders (Master, Music, SFX). The sub-buses under SFX allow per-category volume adjustments at code level for mix priority.

## Adaptive Music System

### Scene Structure

```
MusicManager (Node) [scripts/managers/MusicManager.cs] — Autoload
├── BaseLayer (AudioStreamPlayer) — Ambient drone, always playing
├── MidLayer (AudioStreamPlayer) — Percussion/pulse, fades in at mid intensity
└── HighLayer (AudioStreamPlayer) — Full driving track, fades in at high intensity
```

All streams are set to loop. Each layer outputs to the `Music` bus.

### Intensity Metric

`MusicManager` calculates an intensity value (0.0-1.0) each frame based on:

- Active enemy count (normalized against a baseline, e.g. 30 enemies = 0.5)
- Current wave number (normalized against late-game, e.g. wave 15 = 0.5)
- Enemy proximity (count of enemies within 8 units of the player)

The three factors are weighted and combined. The intensity value drives layer volumes:

| Intensity Range | BaseLayer | MidLayer | HighLayer |
|-----------------|-----------|----------|-----------|
| 0.0 - 0.3 | 1.0 | 0.0 | 0.0 |
| 0.3 - 0.6 | 1.0 | 0.0 → 1.0 | 0.0 |
| 0.6 - 1.0 | 1.0 | 1.0 | 0.0 → 1.0 |

Volume transitions use exponential interpolation (smooth, no jarring cuts).

### Death & Restart

- On player death: all layers fade to 0 over 0.2s, then stop. After the 0.3s camera freeze, a one-shot death sting plays.
- On restart: `BaseLayer` fades back in during the countdown.

## Player Audio

### Fire Sound

An `AudioStreamPlayer3D` on the player node, output to `PlayerSFX` bus. On each projectile spawn:

- Plays one of 3-4 firing sound variations (loaded as an array of `AudioStream`).
- `PitchScale` randomized ±5% per shot.
- `MaxDb` set slightly below enemy telegraph sounds so firing doesn't drown out warnings.

### Impact Sound

Each `PlayerProjectile` has an `AudioStreamPlayer3D` child, output to `PlayerSFX` bus:

- On collision, plays before the projectile is freed.
- Two sound sets: `SurfaceImpactSounds` (for arena geometry) and `EnemyHitSounds` (for enemy hits).
- The `AudioStreamPlayer3D` is reparented to a temporary node on collision so it can finish playing after the projectile is freed.

### Hit / Kill Confirmation

- **Hit sound**: Small thud/click played on each successful enemy hit. Played via the impact sound on the projectile.
- **Kill sound**: A distinct, punchier one-shot played from a dedicated `AudioStreamPlayer3D` on the player when an enemy dies from the player's projectile. Different from the per-hit sound.
- **Weak point hit** (Sentinel): A higher-pitched variant of the hit sound, selected when the projectile hits the Sentinel's core collider.

### Movement Audio

- **Bhop landing**: `AudioStreamPlayer3D` on the player. Plays a short, bright impact sound on successful bhop (BhopState reports success). Normal/failed landings play a subtly duller variant.
- **Dodge roll**: `AudioStreamPlayer3D` on the player. Quick whoosh on roll start.
- **Footsteps**: Not implemented. Player is airborne most of the time; footsteps would be inconsistent.

## Enemy Audio

### Per-Enemy Audio Nodes

Each enemy scene includes:

```
Enemy (CharacterBody3D)
├── AmbientAudio (AudioStreamPlayer3D) — Looping ambient sound, bus: EnemySFX
├── TelegraphAudio (AudioStreamPlayer3D) — Attack telegraph, bus: EnemySFX
└── DeathAudio (AudioStreamPlayer3D) — Death sound, bus: EnemySFX
```

Audio streams are configured per enemy type (see `design/audio.md` for the full identity table).

### Spatial Audio Configuration

All enemy `AudioStreamPlayer3D` nodes use:

- `AttenuationModel`: Inverse distance.
- `MaxDistance`: 40 units (full arena diameter). Ensures distant enemies are faint, nearby are loud.
- `UnitSize`: Tuned per sound type — telegraphs have a larger unit size (louder at distance) to ensure they cut through.

### Swarm Audio Optimization

When more than ~15 enemies of the same type are active, individual ambient sounds become cacophonous. Mitigation:

- Each enemy's `AmbientAudio` has a `MaxPolyphony` of 1.
- A spatial audio culling check skips playback for enemies beyond 20 units from the player.
- The combined effect of nearby ambient sounds creates a generalized "swarm noise" naturally.

### Telegraph Priority

Telegraph sounds (Charger scrape, Howler scream, Burrower rumble) use a higher `UnitSize` and are assigned to the `EnemySFX` bus with a slight volume boost. They must be audible at any distance as critical survival information.

## UI & System Audio

Played via non-positional `AudioStreamPlayer` nodes (2D audio), output to `UISFX` bus:

- **Gem pickup**: Short bright chime. Pitch scaling handled by `Player.cs` (see `game-feel.md` architecture).
- **Upgrade meter full**: Distinct "level up" chime.
- **Upgrade card hover/select**: Subtle UI tick / heavier confirmation sound.
- **Countdown beats**: Percussive beats for 3-2-1, sharp hit on "GO".
- **Leaderboard placement**: Celebratory sting (only if top 10).
- **Menu navigation**: Minimal clean ticks.

These are played from a `UIAudio` node in the HUD scene or from the `GameManager` autoload.

## Mix Priorities

Implemented via bus volume levels and per-source `MaxDb` settings:

1. Enemy attack telegraphs (`EnemySFX`, boosted `UnitSize`)
2. Player hit/kill confirmation (`PlayerSFX`)
3. Death sting (`Music` bus, one-shot)
4. Music intensity layers (`Music` bus)
5. Player firing (`PlayerSFX`, slightly attenuated)
6. Gem/UI sounds (`UISFX`)
7. Enemy ambient (`EnemySFX`, standard attenuation)
8. Environmental ambience (`Ambience` bus)

## Audio Assets

All audio assets live in `assets/audio/`, organized by category:

```
assets/audio/
├── music/          — Adaptive music layers (base, mid, high), death sting
├── player/         — Firing variations, impact sounds, hit/kill confirms, bhop, roll
├── enemies/        — Per-type ambient, telegraph, death sounds
├── ui/             — Gem pickup, upgrade chimes, countdown, menu ticks
└── ambience/       — Arena ambient loop
```

Placeholder assets (generated or sourced from free libraries) are used initially and replaced with final assets in the polish phase.
