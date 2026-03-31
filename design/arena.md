# Arena

## Layout

The arena is a single, enclosed, flat space. There are no corridors, no rooms, no multi-level terrain. The entire arena is visible from any point within it, ensuring the player always has full information about enemy positions. The player introduces verticality through jumping and bunny hopping, but the arena floor itself is flat.

## Shape

Circular. A circular arena has no corners to get trapped in and no edges that favor specific strategies. Every direction is equal, reinforcing the pure skill focus.

## Size

Medium — large enough that the player has room to maneuver and kite enemies, but small enough that they can never fully escape pressure. Approximate diameter: 40-50 units (tunable). The player should be able to sprint across the arena in roughly 4-5 seconds.

## Boundaries

The arena is bounded by a visible wall or void edge. The player cannot leave. Enemies spawn at or just beyond the boundary and enter the play space.

- The boundary should be visually obvious — the player should never accidentally run into it
- Contact with the boundary does not damage the player, but being pinned against it with enemies closing in is effectively a death sentence

## Floor

Flat and uniform with subtle grid or texture for movement readability. The player should be able to gauge their speed and position relative to the center at a glance.

Burrower craters are the only terrain modification — temporary rough patches that slow the player. These despawn after several seconds, so the arena always returns to its clean state.

## Visual Style

- Dark, minimal, atmospheric — the arena should feel hostile and alien
- Lighting is functional: the player and enemies are well-lit and readable; the background is dark and undistracting
- The center of the arena may have a subtle landmark (glow, marking) so the player can orient at a glance
- Enemy spawn points at the edges should have brief visual/audio cues (flash, rumble) to alert the player
