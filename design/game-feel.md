# Game Feel

The difference between a functional game and a satisfying game lives in "juice" — the small feedback systems that make every action feel impactful. This game fires 8 rounds per second, kills hundreds of enemies per run, and demands constant movement. Every one of those moments needs to feel good.

## Principles

- **Responsiveness over animation**: Gameplay actions should never wait for an animation to finish. Visual flourishes layer on top of instant mechanical responses.
- **Proportional feedback**: Small events (single bullet hit) get subtle feedback. Big events (Bloater explosion, Titan death) get dramatic feedback. The player should be able to feel the "weight" of what just happened without looking at the screen.
- **Never obscure gameplay**: No amount of juice should make it harder to read the battlefield. Effects should enhance clarity, not compete with it.

## Screen Shake

Screen shake communicates the force of events through the camera.

- **Player firing**: Very subtle per-shot shake (0.5-1px). Barely perceptible individually, but contributes to the feeling of sustained fire. No shake when not firing — the contrast matters.
- **Enemy hit confirmed**: Tiny additional shake on successful hit to reinforce "I'm hitting something."
- **Enemy death**: Small shake scaled by enemy importance. Crawler death is nearly imperceptible. Titan death is a heavy, slow rumble.
- **Player death**: A single sharp jolt followed by the 0.3s camera freeze.
- **Bloater explosion**: Medium-heavy shake with a longer decay — the biggest non-death shake in the game.
- **Implementation**: Shake should be additive (multiple sources stack) with a hard cap to prevent seizure-inducing amounts during chaotic moments. Use a decaying spring model, not random jitter.

## Hit Stop

Hit stop (a brief frame pause on impact) adds weight to combat. Used sparingly to avoid interrupting the flow of a fast-paced game.

- **Enemy kill**: 1-2 frame pause (~16-33ms) on the killing blow. Not on every hit — only the kill. This creates a satisfying "crunch" without disrupting aim tracking.
- **Titan ground slam impact**: 2-3 frame pause when the shockwave spawns, selling the power of the attack.
- **Bloater explosion**: 2-3 frame pause at the moment of detonation.
- **Multi-kill**: If multiple enemies die within the same frame (Explosive Rounds, Bloater chain), the hit stop is slightly longer (3-4 frames) but does not stack per kill — one combined pause.

## Speed & Movement Effects

The player should *feel* fast when bhopping and *feel* the difference between base speed and max bhop speed.

- **FOV scaling**: Camera FOV widens subtly as the player's bhop speed multiplier increases. At 1.0x (base), FOV is default. At 1.8x (cap), FOV is ~5-8% wider. This creates a visceral sense of acceleration without distorting gameplay. FOV returns to default as speed decays.
- **Speed lines**: Faint radial lines appear at screen edges when above ~1.3x speed. Intensity increases with speed. Lines originate from the direction of movement, reinforcing the sense of velocity.
- **Bhop chain counter**: A small, unobtrusive counter appears on the HUD after the 2nd consecutive bhop (e.g., "x3", "x4"). Fades out 0.5s after the chain breaks. Purely cosmetic — no gameplay effect. Serves as a skill feedback tool so the player knows they're timing correctly.
- **Landing impact**: A subtle dust puff at the player's feet on landing, scaled by fall speed. Successful bhop landings have a slightly different (brighter/sharper) particle to visually confirm the bhop.
- **Dodge roll trail**: A brief motion blur or afterimage trail during the dodge roll to sell the speed and invulnerability of the move.

## Projectile Feedback

Every shot fired should feel punchy and every hit should feel confirmed.

- **Muzzle flash**: A brief bright flash at the projectile spawn point (player's upper chest). Randomized rotation per shot to avoid visual repetition. Should be small enough not to obscure the crosshair.
- **Projectile trail**: Each projectile leaves a short, bright trail (2-3 projectile-lengths behind it). The trail reinforces projectile direction and speed, and makes the stream of fire visible at a glance. Trail fades quickly after the projectile is destroyed.
- **Hit marker**: When a projectile connects with an enemy, a brief hit marker flashes on the crosshair (small "X" or tick marks). This confirms hits without requiring the player to visually track individual projectiles.
- **Kill confirmation**: On enemy death, the hit marker is replaced by a slightly larger, different-colored kill marker (e.g., red X instead of white). Accompanied by a distinct kill sound (see `audio.md`).
- **Impact sparks**: Projectiles hitting arena surfaces produce small spark/ricochet particles at the impact point. Projectiles hitting enemies produce a different effect (enemy-colored burst or flash).

## Enemy Feedback

Enemies should communicate their state clearly through visual cues.

- **Damage flash**: When an enemy takes damage, its mesh flashes white for 1-2 frames. This is the primary "I'm hitting it" signal and must be reliable and visible even in chaos.
- **Death effect**: Enemies burst or dissolve on death, scattering particles in the enemy's color palette. No ragdolls — corpses should not litter the arena or obscure gems. Death effects should be fast (under 0.3s) so the battlefield clears quickly.
- **Stagger/flinch**: No mechanical stagger (enemies don't slow down when hit). The damage flash alone communicates hits. This keeps enemy movement predictable and learnable.
- **Low-health indicator**: Enemies below 25% health show visible damage (cracks, flickering, or trailing sparks). This helps the player decide whether to finish off a target or switch to a higher-priority threat.

## Gem Collection

Collecting gems should be one of the most satisfying micro-interactions in the game.

- **Magnetism snap**: When a gem enters the collection radius, it accelerates toward the player with a slight curve (not an instant teleport). The snap takes ~0.1s, just long enough to be visible.
- **Collection burst**: On collection, a small bright particle pops at the player's position. Color matches the gem.
- **Sound pitch scaling**: Rapid gem collection (running through a pile from a big kill) produces ascending pitch on each successive pickup, creating an improvisational musical run. Pitch resets after a short gap.
- **Upgrade meter pulse**: When the meter fills, the HUD element pulses/flashes before the game pauses for upgrade selection. The pulse gives the player a split-second to register that an upgrade is coming.

## UI Feedback

- **Damage direction indicator**: When an enemy enters a close threat range directly behind the player (outside their camera view), a subtle red arc flashes on the corresponding edge of the screen. This helps counteract the third-person camera's blind spot without trivializing flanking enemies like the Shade. Only triggers for very close threats — it's a "danger close" warning, not a radar.
- **Upgrade selection whoosh**: Selecting an upgrade card plays a satisfying confirmation animation (card flies toward the player, brief full-screen flash) before the game resumes.
- **Timer milestone flash**: The survival timer subtly pulses or changes color at round-number milestones (1:00, 2:00, 3:00, etc.) to give the player a sense of progress during the run.
