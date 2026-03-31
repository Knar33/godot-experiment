# Arena

## Layout

The arena is a single, enclosed, flat space. There are no corridors, no rooms, no multi-level terrain. The entire arena is visible from any point within it, ensuring the player always has full information about enemy positions. The player introduces verticality through jumping and bunny hopping, but the arena floor itself is flat.

## Shape

Circular. A circular arena has no corners to get trapped in and no edges that favor specific strategies. Every direction is equal, reinforcing the pure skill focus.

## Size

Medium — large enough that the player has room to maneuver and kite enemies, but small enough that they can never fully escape pressure. Approximate diameter: 40-50 units (tunable). The player should be able to sprint across the arena in roughly 4-5 seconds.

## Boundaries

The arena is an open platform floating over a void. There are no walls — the player and enemies can walk off the edge and fall. Falling off the arena is lethal: a killbox below the platform kills the player instantly.

- The edge should be visually obvious — the floor shader's edge glow and the visible drop-off into darkness make the boundary unmistakable
- The open edge adds a real environmental hazard: aggressive bhopping near the rim risks falling off, and enemies can be kited off the edge
- Enemies that fall off the arena are killed by the same killbox (freeing them without gem drops)
- Spawn points are positioned just inside the arena edge so enemies appear at the perimeter and move inward

## Floor

Flat and uniform with subtle grid or texture for movement readability. The player should be able to gauge their speed and position relative to the center at a glance.

Burrower craters are the only terrain modification — temporary rough patches that slow the player. These despawn after several seconds, so the arena always returns to its clean state.

## Visual Style

- Dark, minimal, atmospheric — the arena should feel hostile and alien
- Lighting is functional: the player and enemies are well-lit and readable; the background is dark and undistracting
- The center of the arena may have a subtle landmark (glow, marking) so the player can orient at a glance
- Enemy spawn points at the edges should have brief visual/audio cues (flash, rumble) to alert the player

## Environmental Atmosphere

The arena is not just a gameplay container — it should feel like a place, even if that place is minimal and hostile.

- **Ambient Particles**: Faint dust motes or embers drift through the air above the arena floor. Subtle enough not to interfere with gameplay readability, but present enough to give the space depth and atmosphere. Particle density may increase slightly as the wave count climbs, reinforcing the sense of escalating chaos.
- **Edge Glow**: The arena boundary has a faint, continuous glow or energy effect so the player always knows where the edge is at a glance. The glow should be visible from the arena center but not bright enough to distract.
- **Edge Warning Zone**: The outer ~10% of the arena radius has a subtle visual treatment on the floor (slightly darker, faint pulsing edge line, or different texture tint). This is a "you're getting close to the wall" signal — being pinned at the edge is dangerous, and the player should feel uneasy about spending time there.
- **Floor Reactivity**: Projectile impacts on the arena floor leave brief scorch marks or decals that fade after 1-2 seconds. These are purely cosmetic but add texture to the battlefield and help sell the physicality of combat. Burrower craters remain the only gameplay-affecting floor modification.
- **Ambient Audio**: A low, droning ambient soundscape underlies the arena at all times. It should feel oppressive and alien — the sonic equivalent of the dark, hostile visual style (see `audio.md`).
