# Audio

Audio is half of game feel. In a fast, chaotic arena shooter, sound does critical work: it confirms hits the player can't visually track, warns of threats outside the camera view, and drives the emotional intensity of each run.

## Principles

- **Spatial clarity**: Enemy audio cues must be spatially accurate (3D positional audio). The player should be able to hear a Charger winding up behind them before they see it.
- **Priority mixing**: Not every sound can play at full volume simultaneously. The mix should prioritize player-critical information (threat warnings, hit confirmation) over ambient noise (distant footsteps, background fire).
- **Variation**: Any sound that plays frequently (firing, gem pickup, Crawler death) needs multiple variations or pitch randomization to avoid ear fatigue over a multi-minute run.

## Music

### Adaptive Music System

The soundtrack should react to gameplay intensity rather than playing on a fixed loop.

- **Ambient base layer**: A low, droning ambient track plays continuously. Dark, tense, minimalist. Sets the hostile atmosphere of the arena.
- **Intensity layers**: Additional musical layers (percussion, synth pulses, bass hits) fade in as the wave count and enemy density increase. By late-game, the music should feel urgent and driving.
- **Intensity metric**: Based on a combination of active enemy count, current wave number, and number of enemies in close proximity to the player. Updates smoothly (no jarring transitions).
- **Death sting**: On player death, the music cuts abruptly to silence, followed by a single low-impact sound (a resonant thud or bass drop). The sudden silence emphasizes the finality of death.
- **Restart transition**: On restart, the ambient base layer fades back in during the countdown.

### Style Reference

Dark electronic/industrial with minimal melodic content. Think Devil Daggers, DOOM (2016) at lower intensity, or Quake's ambient dread. The music should feel like the arena itself is hostile — not heroic orchestral swells.

## Player Audio

### Firing

- Short, punchy projectile sound with slight pitch randomization (±5%) per shot.
- At 8 shots/sec, the individual sounds blend into a rhythmic stream. The firing sound should be designed to sound good both as single shots and as a sustained burst.
- Volume is slightly lower than enemy threat cues so it doesn't drown out important warnings.

### Movement

- **Footsteps**: Minimal or absent. In a game where the player is airborne most of the time (bhopping), footstep sounds would be inconsistent and distracting.
- **Bhop landing**: A short, tight impact sound on each successful bhop landing. Subtly different from a failed/normal landing — the bhop sound is slightly "brighter" or has a higher pitch component. This is audio feedback that the player timed the bhop correctly.
- **Dodge roll**: A quick whoosh sound during the roll. Short, sharp, and clean.
- **Jump**: A very subtle sound or none at all. The bhop landing sound is the important one.

### Hit Confirmation

- **Enemy hit**: A small, satisfying thud/click on each projectile connection. Must cut through the mix clearly — this is the player's primary feedback for whether their aim is on target.
- **Enemy kill**: A distinct, punchier sound on the killing blow. Slightly louder and different enough from the hit sound that kills are identifiable by audio alone.
- **Critical/weak point hit** (Sentinel): A higher-pitched or more resonant hit sound when striking a weak point, confirming that the player is aiming correctly.

### Death

- All player audio stops abruptly on death. Brief silence before the death sting plays.

## Enemy Audio

Each enemy type has a distinct audio identity so the player can recognize threats by sound, even off-screen.

### Audio Cues by Enemy

| Enemy | Ambient Sound | Attack Telegraph | Death Sound |
|-------|--------------|-----------------|-------------|
| Crawler | Skittering/chittering (quiet, blends into swarm noise) | None (too fast/simple) | Small crunch/pop |
| Spitter | Low gurgling | Brief spit/hiss before projectile launch | Wet burst |
| Charger | Heavy footsteps/snorting | Loud scraping/revving during 1.5s telegraph (critical warning) | Heavy crash/thud |
| Drone | High-pitched buzzing/whirring | Quick dive whistle before dive-bomb | Small electric crackle |
| Bloater | Deep, labored breathing | 0.5s ticking/swelling sound before explosion | Massive boom (loudest non-music sound) |
| Shade | Near-silent (faint whisper if very close) | Sharp reveal sound (blade draw/hiss) during 0.5s visible telegraph | Ghostly dissipation |
| Sentinel | Deep, resonant humming (constant while alive) | N/A (passive) | Low structural collapse sound, buff removal has audible "depressurize" |
| Burrower | Muffled rumbling while underground | Rising rumble before eruption | Ground crack/crumble |
| Howler | Low growl while moving | Rising scream during 1.5s channel (loud, distinctive, unmistakable) | Abrupt silence (choke-off mid-growl) |
| Titan | Heavy, rhythmic booms (footsteps felt more than heard) | Slam telegraph: rising seismic hum. Sweep telegraph: whoosh wind-up | Prolonged collapse/groan (the biggest death sound in the game) |

### Spatial Audio Rules

- All enemy sounds use 3D positional audio. The player should be able to identify threat direction by ear.
- Attenuation is tuned so that distant enemies produce only faint ambient noise, while nearby threats are loud and clear.
- Telegraph sounds (Charger scraping, Howler screaming, Burrower rumbling) should cut through the mix at any distance — these are critical survival information. They receive higher priority in the audio mix.
- When many enemies of the same type are present (20+ Crawlers), their ambient sounds blend into a generalized "swarm noise" rather than playing 20 individual clips.

## UI & System Audio

- **Gem pickup**: Short, bright chime. Pitch rises slightly with rapid successive pickups (see `game-feel.md`).
- **Upgrade meter full**: A distinct "level up" chime when the meter fills, immediately before the game pauses.
- **Upgrade card hover**: Subtle UI tick when highlighting a card.
- **Upgrade selected**: A satisfying confirmation sound (heavier than the hover tick).
- **Countdown numbers**: Each countdown number (3, 2, 1) has a low, percussive beat. The "GO" moment is a sharp, energizing hit that transitions into gameplay.
- **Leaderboard placement**: A celebratory sting if the player's time makes the top 10. Absent if it doesn't.
- **Menu navigation**: Minimal, clean UI sounds. No annoyance factor on repeated interaction.

## Mix Priorities (Highest to Lowest)

1. Enemy attack telegraphs (Charger scrape, Howler scream, Burrower rumble)
2. Player hit/kill confirmation
3. Player death sting
4. Music intensity layers
5. Player firing sound
6. Gem pickup / UI sounds
7. Enemy ambient sounds (footsteps, buzzing, growling)
8. Environmental ambience
