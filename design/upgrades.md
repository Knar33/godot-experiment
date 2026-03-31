# Upgrades

## Overview

When the player collects enough gems to fill the upgrade meter, the game briefly pauses and presents 3 random upgrades to choose from. The player picks one, the game resumes, and the meter resets (with a higher threshold for the next upgrade). Upgrades are permanent for the duration of the run and lost on death.

## Upgrade Selection

- **Pause**: The game freezes (not slow-mo — full stop) when the meter fills. Enemies and projectiles halt in place.
- **Presentation**: 3 upgrade cards appear on screen. Each shows the upgrade name, a short description, and an icon.
- **Selection**: The player picks one using mouse click or keyboard (1/2/3 keys). There is no time limit — the player can read and decide.
- **Resume**: After selecting, the game immediately unpauses. No transition animation.
- **Pool**: Upgrades are drawn from the full pool with no duplicates within a single offering. An upgrade already owned can appear again as a stack (upgrades that stack say so in their description).

## Upgrade Pool

### Weapon Upgrades

**Rapid Fire**
Increases fire rate by 25%. Stacks up to 3 times.
*More bullets, more pressure. Each stack makes sustained fire noticeably more intense.*

**Piercing Rounds**
Projectiles pass through enemies instead of stopping on hit. Does not stack.
*Transforms crowd control — a line of Crawlers becomes one shot instead of ten.*

**Spread Shot**
Each shot fires 2 additional projectiles in a narrow cone (±10 degrees). Each additional stack adds 1 more projectile. Stacks up to 2 times (max 5 projectiles per shot at full stacks).
*Coverage over precision. Excellent for swarms, less efficient against single targets.*

**Explosive Rounds**
Projectiles deal a small AoE burst on hit (damage falls off with distance). Does not stack.
*Adds splash damage. Synergizes with Spread Shot for devastating area coverage.*

**Heavy Shots**
Projectiles deal 50% more damage. Stacks up to 2 times.
*Simple and effective — everything dies faster.*

**Extended Range**
Projectile travel distance increased by 40%. Does not stack.
*Reach across the arena. Lets the player engage Sentinels and Spitters from safer distances.*

### Movement Upgrades

**Momentum**
Bunny hop speed cap increased by 20%. Stacks up to 2 times.
*Skilled bhoppers become even faster. The speed ceiling goes from dangerous to untouchable.*

**Forgiving Timing**
Bunny hop timing window widened by 50% (~150ms instead of ~100ms). Does not stack.
*Makes bhop chains easier to maintain under pressure. Consistency over ceiling.*

**Quick Roll**
Dodge roll cooldown reduced by 30%. Stacks up to 2 times.
*More defensive options. At max stacks, the player can roll every ~0.7s.*

**Air Control**
Air strafe influence doubled. Does not stack.
*Sharper air turns. Lets the player weave through tight gaps between enemies while bhopping.*

### Utility Upgrades

**Gem Magnet**
Gem collection radius tripled (~6 units). Does not stack.
*Less time chasing gems, more time shooting. Especially valuable in chaotic late-game arenas.*

**Aftershock**
Dodge roll emits a small damaging pulse at the end of the roll (enough to kill Crawlers). Does not stack.
*Turns the defensive tool into an offensive one. Rewards aggressive rolling into swarms.*

**Last Stand**
Survive one lethal hit, becoming invulnerable for 1s. Consumed on use — removed from upgrades after triggering. Does not stack.
*A single second chance. The player still feels the pressure of one-hit death, but gets one reprieve.*

**Chain Lightning**
Killing an enemy sends a bolt of damage to the nearest enemy within range (deals 50% of a normal shot). Does not stack.
*Passive crowd thinning. Most effective against clumps of low-health enemies.*

## Design Intent

- **No Bad Choices**: Every upgrade should feel meaningful. The worst upgrade in any offering of 3 should still be worth taking.
- **Synergies**: Some upgrades combine powerfully (Spread Shot + Explosive Rounds, Momentum + Air Control). Recognizing and building toward synergies is part of the skill expression.
- **Run Identity**: The randomized selection means each run develops a different character. One run might become a speed demon with Momentum + Air Control, another might become a walking turret with Rapid Fire + Spread Shot + Explosive Rounds.
- **Pacing**: Early upgrades come quickly (low gem threshold), giving the player a power bump that makes the mid-game feel rewarding. Later upgrades require more investment, matching the escalating difficulty.

## Balance Considerations

- Upgrades should make the player stronger but never trivialize the game. Enemy scaling through wave count and composition outpaces upgrade acquisition — the player is always falling behind, just more slowly.
- Last Stand is intentionally consumable to preserve the tension of one-hit death while giving a memorable "save" moment.
- Piercing Rounds is the strongest single upgrade against swarms. It should appear slightly less frequently in the random pool (weighted rarity) to prevent it from dominating every run.
