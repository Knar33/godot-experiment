# Wave System

## Structure

Waves are continuous — there is no downtime between them. As one wave's enemies are being dealt with, the next wave's enemies begin spawning. This creates a feeling of relentless escalation rather than discrete rounds.

## Wave Progression

### Early Game (Waves 1-5): Learning the Basics

- **Wave 1**: Crawlers only (small group of 5)
- **Wave 2**: More Crawlers (8-10)
- **Wave 3**: Crawlers + first Spitters (2)
- **Wave 4**: Crawlers + Spitters + first Charger
- **Wave 5**: Mixed Crawlers, Spitters, Charger, first Drone cluster

The early waves teach each enemy type in relative isolation before combining them.

### Mid Game (Waves 6-12): Layering Pressure

- Enemy counts increase significantly
- Bloaters, Shades, and Burrowers begin appearing
- Sentinels and Howlers are introduced one at a time
- The player must begin prioritizing targets and managing space

### Late Game (Waves 13-20): Full Pressure

- All enemy types are in play
- Multiple Sentinels and Howlers can be active simultaneously
- First Titan appears (around wave 15)
- Crawler and Drone counts are high enough to be overwhelming without active thinning

### Endless (Waves 21+): Escalation Loop

- Wave composition repeats a cycle of varied combinations, but enemy counts continue scaling upward
- Additional Titans begin appearing
- Spawn rate accelerates
- This is where the game becomes a pure endurance and skill test

## Spawn Behavior

- Enemies spawn at the edges of the arena, spread across multiple entry points
- Spawn points are randomized but weighted to avoid spawning directly behind the player (brief grace window)
- Larger enemies (Titan, Sentinel) have a brief spawn-in animation where they are visible but not yet active, giving the player a moment to register the new threat
- Spawn timing within a wave is staggered — enemies trickle in over the wave's duration rather than all appearing at once

## Difficulty Scaling

Difficulty increases through three axes:

1. **Enemy Count** - More enemies per wave
2. **Enemy Composition** - More dangerous enemy type combinations
3. **Spawn Speed** - Shorter intervals between spawn groups within a wave and shorter gaps between waves

Enemy stats (health, speed, damage) do NOT scale. The enemies themselves are always the same — the difficulty comes purely from quantity and composition. This ensures that player skill transfers between early and late runs without hidden stat inflation.

## Playtest Tuning (Temporary)

While iterating on early-game readability and threat telegraphs, enemy spawns can be temporarily forced to a fixed interval to make targeted testing easier.

- **Spawn interval**: currently ~0.7 seconds between individual enemy spawns (across waves)

This will be revisited once the early combat loop feels stable.
