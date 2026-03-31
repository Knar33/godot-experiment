# Player

## Movement

- **Speed**: Fast and responsive. The player moves at full speed immediately with no acceleration ramp.
- **Direction**: 8-directional movement relative to the camera (WASD or left stick).
- **Dodge Roll**: A short, fast dodge roll on a brief cooldown (1.5s). The player is invulnerable during the roll's active frames (~0.3s of the ~0.5s animation). Cannot be performed while airborne.
- **Jump**: The player can jump. Jump height is modest — enough to clear ground-based attacks (Burrower eruptions, Titan shockwaves) but not enough to fly over enemies.

## Bunny Hopping

Bunny hopping is a core skill mechanic that rewards precise jump timing with increased movement speed.

- **Mechanic**: If the player jumps within a tight timing window on landing (~100ms before or after touching the ground), they retain their current speed and gain a small speed boost.
- **Speed Buildup**: Each successive well-timed bhop adds speed, stacking up to a cap of roughly 1.8x base movement speed.
- **Speed Decay**: Missing the timing window (jumping too late after landing, or simply running on the ground) causes speed to rapidly decay back to base.
- **Air Strafing**: While airborne, the player can subtly curve their trajectory by strafing. This allows skilled players to weave through enemy formations while maintaining bhop speed.
- **Skill Ceiling**: Bunny hopping under pressure — while dodging enemies, collecting gems, and aiming — is the highest expression of player skill. A player who can maintain a bhop chain survives significantly longer than one who runs on the ground.

### Bunny Hop Interactions

- Dodge roll is ground-only and cannot be performed mid-air
- The player can shoot while airborne with no penalty
- Ground-based attacks (Titan shockwave, Burrower eruption) can be avoided by being airborne at the right moment
- Landing in a Burrower crater still applies the slow effect, but a well-timed bhop off the crater can escape it quickly

## Shooting

- **Auto-Fire**: Holding the fire input (left mouse button / right trigger) continuously fires projectiles at the fire rate. Releasing the input stops firing.
- **Aim Direction**: The player aims toward the camera's look direction (mouse or right stick). A visible crosshair indicates the exact aim point.
- **Projectile Type**: Fast-moving projectiles that travel in a straight line. No hitscan — projectiles have travel time, rewarding leading targets.
- **Projectile Collision**: Projectiles collide with arena geometry (walls, floor). On impact they are destroyed and play a small hit effect.
- **Fire Rate**: Rapid fire (~8 shots per second). Individual shots deal low damage; sustained fire on a target is what kills.
- **Range**: Projectiles despawn after a fixed distance roughly matching the arena radius, preventing infinite-range sniping.
- **Sound**: Each shot produces a short, punchy firing sound. Projectiles play a distinct impact sound on hitting an enemy or arena surface.

## Gems

Enemies drop gems on death. Gems are automatically collected when the player moves near them (small magnetism radius). Collected gems fill an upgrade meter displayed on the HUD. When the meter fills, the player is offered a choice of randomized upgrades (see `upgrades.md`).

- **Drop Behavior**: Gems pop out of enemies on death and scatter briefly before settling. They persist indefinitely and do not despawn.
- **Collection Radius**: Small (~2 units). The player must actively move through gems to collect them — they don't fly across the arena.
- **Gem Value**: Different enemies drop different amounts of gems. Tougher enemies drop more.
  - Crawler: 1 gem
  - Spitter: 2 gems
  - Charger: 3 gems
  - Drone: 1 gem
  - Bloater: 4 gems
  - Shade: 3 gems
  - Sentinel: 5 gems
  - Burrower: 3 gems
  - Howler: 4 gems
  - Titan: 10 gems
- **Upgrade Meter**: The number of gems required to fill the meter increases with each upgrade earned (e.g., 10 for the first, 15 for the second, 20 for the third, etc.). This paces upgrades so early ones come quickly and later ones require sustained survival.
- **Visual**: Gems should be bright, easy to spot, and satisfying to collect (small particle effect + sound on pickup).

## Health

- **One Hit Kill**: The player dies to any single enemy attack. No health bar, no damage numbers, no healing.
- **Invulnerability Frames**: Only during the dodge roll's active window.

## Movement Feedback

- **Bhop Chain Counter**: After the 2nd consecutive successful bhop, a small counter appears on the HUD (e.g., "x3", "x4"). Fades out 0.5s after the chain breaks. Purely cosmetic — gives the player confirmation they're timing correctly without cluttering the screen.
- **Speed Trail**: At bhop speeds above ~1.3x, faint radial speed lines appear at the screen edges. Intensity scales with speed. Combined with FOV widening (see `game-feel.md`), this makes high-speed bhopping feel viscerally fast.
- **Landing Particles**: Successful bhop landings produce a brighter, sharper dust puff than normal landings. A visual "yes, you nailed it" cue that reinforces the timing window.
- **Dodge Roll Afterimage**: During the dodge roll, a brief motion blur or ghosted afterimage trail follows the player, selling the speed and invulnerability of the move.

## Threat Awareness

- **Directional Threat Indicator**: When an enemy enters a close range directly behind the player (outside camera view), a subtle red arc flashes on the corresponding screen edge. This is a "danger close" warning, not a radar — it only triggers for immediate, nearby threats. Helps counteract the third-person camera's inherent blind spot without trivializing flanking enemies like the Shade.
- **Proximity Audio**: Enemy sounds are spatially positioned (3D audio). The player should be able to hear a Charger wind-up or Howler scream and know its direction by ear alone (see `audio.md` for per-enemy audio identity).

## Visual Design

- The player character should be simple and readable — a humanoid silhouette that doesn't compete with enemy visibility.
- Projectiles from the player should be visually distinct (bright, consistent color) so the player can distinguish their fire from enemy projectiles.
- Each projectile leaves a short, bright trail (2-3 projectile-lengths) that fades quickly after impact. The trail makes the stream of fire visible at a glance and reinforces aim direction.
- A brief muzzle flash at the projectile spawn point (upper chest) on each shot, randomized rotation per shot to avoid visual repetition.
- The gem count and upgrade meter should be visible but unobtrusive on the HUD.
