# Tasks

Ordered by urgency (dependencies first, polish last). Completed tasks are checked and moved to the bottom.

---

## Camera Enhancements

- [ ] Offset the camera horizontally to an over-the-shoulder position so the crosshair targets the world ahead of the player rather than the player's back
- [ ] Implement scroll-wheel zoom: mouse scroll adjusts the camera distance between a configurable min and max, with smooth interpolation toward the target distance

## Player Shooting

- [ ] Create a player projectile scene: a fast-moving Area3D or RigidBody3D with a bright visually distinct mesh, CollisionShape3D, and a script that moves it forward each physics frame
- [ ] Implement player auto-fire: while GameManager state is Playing, the player continuously fires projectiles (~8 per second) from the character toward the camera's aim direction
- [ ] Implement projectile range limit: player projectiles despawn after traveling a fixed distance roughly matching the arena radius
- [ ] Implement projectile-enemy collision: when a player projectile contacts an enemy, deal damage to the enemy and destroy the projectile (default pre-upgrade behavior)
- [ ] Ensure player projectiles are visually distinct from enemy projectiles (Spitter) using a consistent bright color unique to the player
- [ ] Add unit tests for player fire rate timing interval and projectile range-based despawn

## Player Death

- [ ] Implement one-hit player death: any enemy damage source (contact, projectile, explosion, ground hazard) triggers instant player death when the player is not in i-frames
- [ ] Implement dodge roll i-frame protection: during the dodge roll's ~0.3s active invulnerability window, all incoming damage is ignored
- [ ] Implement player death sequence: freeze the survival timer on the exact frame of death, play a death effect (ragdoll or disintegration) on the player character
- [ ] Implement death camera freeze: hold the camera position for ~0.3s after player death before transitioning the GameManager state to Dead and showing the results screen
- [ ] Add unit tests for player i-frame protection during dodge roll and death triggering from different damage source types (contact, projectile, AoE)

## Core Game Loop

- [ ] Implement the countdown sequence: on game start or restart, show a 3-2-1 countdown UI while the player is locked at arena center, then transition GameManager to Playing
- [ ] Implement the survival timer: starts at 0:00.000 when the countdown finishes, counts up in real time, freezes on the exact frame of player death; this is the player's score
- [ ] Implement the death/results screen UI: displays the run's survival time (large, left side) and the leaderboard table (right side); shown when GameManager enters Dead state
- [ ] Implement the restart flow: a single button press from the death screen despawns all enemies, clears all projectiles and gems, resets the timer, resets player position to center, clears all upgrades, and starts the countdown; total time from restart press to gameplay must be under 3 seconds
- [ ] Implement a pause menu: pressing escape during Playing state pauses the game (freezes physics and timer) and shows a menu with resume, restart, and quit options
- [ ] Add unit tests for GameManager state transitions (Countdown -> Playing -> Dead -> Countdown) and survival timer accuracy

## Enemy Foundation

- [ ] Create a base enemy C# class/scene with a health system (takes damage from player projectiles, dies at 0 HP), pathfinding/movement toward the player, and collision that triggers player death on contact
- [ ] Implement gem dropping in the base enemy class: on death, spawn gem pickup instances based on the enemy's configured gem value, scattering them briefly outward before they settle on the ground
- [ ] Create the enemy spawner system: spawns enemy instances at the defined arena-edge spawn points with staggered timing, randomized point selection, and a bias against spawning directly behind the player
- [ ] Implement spawn-in animation for large enemies (Titan, Sentinel): the enemy is briefly visible at its spawn point but inactive (no movement or attacks) for ~1s, giving the player time to register the new threat
- [ ] Implement enemy hit feedback: when a player projectile strikes an enemy, play a brief visual flash on the enemy and/or a hit sound so the player knows their shots are landing
- [ ] Add unit tests for base enemy health (damage application, death at 0 HP), per-type gem drop counts, and spawn point randomization distribution

## Crawler (Enemy — Wave 1+)

- [ ] Create the Crawler scene: a small ground creature with a placeholder mesh that inherits the base enemy class
- [ ] Implement Crawler AI: moves directly toward the player at moderate speed with contact damage on touch (uses base enemy collision-kill behavior)
- [ ] Configure Crawler stats: 2-3 shot health, moderate speed, 1 gem drop; spawns in groups of 5-10 that increase over waves

## Spitter (Enemy — Wave 3+)

- [ ] Create the Spitter scene: a medium-sized enemy with a placeholder mesh that inherits the base enemy class
- [ ] Implement Spitter AI: moves slowly, plants itself at medium range from the player, repositions occasionally to maintain spacing
- [ ] Implement the Spitter ranged attack: lobs a slow, visible arcing projectile toward the player's current position on a timed interval
- [ ] Implement Spitter ground hazard: when a Spitter projectile impacts the arena floor, it leaves a small damage zone for 1.5s that kills the player on contact, then despawns
- [ ] Configure Spitter stats: 4-5 shot health, slow speed, 2 gem drop; spawns 1-3 at a time spread apart

## Charger (Enemy — Wave 4+)

- [ ] Create the Charger scene: a medium-sized enemy with a placeholder mesh that inherits the base enemy class
- [ ] Implement Charger AI: moves slowly toward the player, periodically stops to enter a 1.5s telegraph state (visible glow/animation) aimed at the player's current position
- [ ] Implement the Charger's charge attack: after the telegraph, launch in a straight line at very high speed for ~0.5s, dealing lethal contact damage
- [ ] Implement Charger recovery: after the charge ends, the Charger is stunned and stationary for 1s before resuming normal behavior
- [ ] Configure Charger stats: 8-10 shot health, slow normal speed / very fast charge speed, 3 gem drop; spawns solo or in pairs

## Drone (Enemy — Wave 5+)

- [ ] Create the Drone scene: a small flying enemy with a placeholder mesh that inherits the base enemy class, positioned above the ground plane
- [ ] Implement Drone AI: hovers and orbits the player at close-medium range with fast, erratic lateral movement to make it hard to hit
- [ ] Implement the Drone dive-bomb attack: periodically telegraphs briefly, then dives toward the player at high speed for a lethal contact strike
- [ ] Configure Drone stats: 1-2 shot health, fast erratic speed, 1 gem drop; spawns in clusters of 4-8

## Bloater (Enemy — Wave 6+)

- [ ] Create the Bloater scene: a large, slow enemy with a placeholder mesh that inherits the base enemy class
- [ ] Implement Bloater AI: moves very slowly toward the player (rarely reaches them), deals lethal contact damage if it does touch the player
- [ ] Implement the Bloater death explosion: on death, wait 0.5s then trigger a large AoE explosion that kills the player if in range and deals damage to any other enemies caught in the blast
- [ ] Configure Bloater stats: 15-20 shot health, very slow speed, 4 gem drop; spawns solo, 1-2 per wave in later stages

## Shade (Enemy — Wave 7+)

- [ ] Create the Shade scene: a stealthy enemy with a placeholder mesh that inherits the base enemy class
- [ ] Implement Shade near-invisibility: the Shade is mostly invisible (rendered as a faint heat-shimmer distortion) and becomes fully visible for 0.5s immediately before attacking
- [ ] Implement Shade flanking AI: avoids the player's forward camera arc, circles wide to approach from behind or from the player's blind spots
- [ ] Implement the Shade melee attack: a lethal strike from behind with a 0.5s fully-visible telegraph window
- [ ] Configure Shade stats: 3-4 shot health, moderate-fast speed, 3 gem drop; spawns solo or in pairs starting mid-game

## Burrower (Enemy — Wave 8+)

- [ ] Create the Burrower scene: an underground enemy that inherits the base enemy class with two states (burrowed and surfaced)
- [ ] Implement the Burrower's burrowed state: travels underground toward the player, visible only as a moving dirt trail on the arena floor, completely invulnerable to player projectiles
- [ ] Implement the Burrower eruption attack: surfaces near the player with a quick AoE eruption (lethal to the player if in the small blast area), then remains above ground for 2s dealing contact damage and taking damage from player projectiles
- [ ] Implement Burrower re-burrowing: after 2s on the surface, the Burrower digs back underground and resumes burrowed pursuit
- [ ] Implement Burrower arena craters: each eruption leaves a small crater on the arena floor that slows the player's movement speed for 5s before despawning
- [ ] Configure Burrower stats: 6-8 shot health (only while surfaced), moderate underground speed, 3 gem drop; spawns 1-2 at a time starting mid-game

## Sentinel (Enemy — Wave 10+)

- [ ] Create the Sentinel scene: a tall, towering enemy with a placeholder mesh, a visible glowing core weak point on its chest, and inheritance from the base enemy class
- [ ] Implement Sentinel passive behavior: nearly stationary with a very slow drift toward the player; the Sentinel does not attack directly
- [ ] Implement the Sentinel buff aura: all enemies within ~1/3 of the arena radius of the Sentinel gain +40% movement speed and attack 50% more frequently; the buff is removed when the Sentinel dies
- [ ] Implement Sentinel weak point: hits to the glowing chest core deal full damage (5-6 shots to kill), body shots deal heavily reduced damage (20+ shots to kill), incentivizing precision aim
- [ ] Configure Sentinel stats: high health (body/weak point split), very slow speed, 5 gem drop; spawns solo in mid-to-late waves

## Howler (Enemy — Wave 11+)

- [ ] Create the Howler scene: a medium enemy with a placeholder mesh that inherits the base enemy class
- [ ] Implement Howler base AI: moves toward the player at moderate speed with a weak melee swipe if adjacent
- [ ] Implement the Howler scream: every 10s, the Howler stops moving and channels a 1.5s scream (stationary and vulnerable during the channel)
- [ ] Implement the Howler enrage effect: when the scream completes, all enemies within a large radius turn red and gain +60% speed with all attacks becoming lethal contact for 5s
- [ ] Configure Howler stats: 8-10 shot health, moderate speed, 4 gem drop; spawns solo in mid-to-late waves

## Titan (Enemy — Wave 15+)

- [ ] Create the Titan scene: a massive enemy with a placeholder mesh that inherits the base enemy class, significantly larger than all other enemy types
- [ ] Implement Titan base AI: moves very slowly but relentlessly toward the player, never stops pursuing
- [ ] Implement the Titan ground slam attack: 2s telegraph, then sends a radial shockwave along the ground in a ~180-degree forward arc; the player must dodge-roll through it or be airborne (bhop) to survive
- [ ] Implement the Titan sweep attack: a wide melee arc covering the Titan's front with a 1s telegraph; lethal on contact
- [ ] Implement the Titan crawler spawning: every 8s, 3-4 Crawlers emerge from the Titan's body as additional threats
- [ ] Configure Titan stats: 100+ shot health, very slow speed, 10 gem drop; spawns solo starting around wave 15, max 1-2 active at a time
- [ ] Add unit tests for Titan shockwave avoidance via dodge roll i-frames and via being airborne during a bhop

## Wave System

- [ ] Create a WaveManager node that tracks the current wave number, controls spawn timing, and interfaces with the enemy spawner
- [ ] Define wave composition as data: for each wave number, specify which enemy types spawn and in what quantities (could be a resource file or dictionary)
- [ ] Implement continuous wave flow in the WaveManager: the next wave's enemies begin spawning while the current wave's enemies are still alive (no downtime between waves)
- [ ] Implement staggered spawning within each wave: enemies trickle in over the wave's duration at intervals rather than all appearing simultaneously
- [ ] Implement early-game wave compositions (1-5): wave 1 is Crawlers only, then progressively introduce Spitters, Chargers, and Drones across waves 2-5
- [ ] Implement mid-game wave compositions (6-12): increase enemy counts, introduce Bloaters, Shades, Burrowers, then Sentinels and Howlers one at a time
- [ ] Implement late-game wave compositions (13-20): all enemy types active, multiple Sentinels/Howlers simultaneously, first Titan around wave 15, high Crawler/Drone swarm counts
- [ ] Implement endless wave scaling (21+): cycle through varied compositions with ever-increasing enemy counts, additional Titans, and accelerating spawn rates
- [ ] Enforce the difficulty scaling rule: difficulty increases only through enemy count, composition, and spawn speed — enemy health/speed/damage stats are never modified
- [ ] Add unit tests for wave composition data correctness and spawn timing intervals

## Gem Pickups

- [ ] Create the gem pickup scene: a bright, easy-to-spot small mesh with a CollisionShape3D trigger area for player collection
- [ ] Implement gem drop behavior in the base enemy: on enemy death, gems pop outward from the corpse position, scatter briefly, then settle on the arena floor
- [ ] Implement gem collection on the player: when the player's collection area (~2 unit radius) overlaps a gem, collect it with a particle effect and sound, incrementing the player's gem count
- [ ] Ensure gems persist on the arena floor indefinitely within a run and do not despawn until the run is restarted
- [ ] Implement the upgrade meter: tracks total gems collected toward a threshold that increases with each upgrade earned (1st upgrade at 10 gems, 2nd at 15, 3rd at 20, etc.)
- [ ] Display the gem count and upgrade meter progress on the HUD as a progress bar or numeric indicator
- [ ] Reset all dropped gems, collected gem count, and the upgrade meter threshold on player death/restart

## Upgrade Selection

- [ ] When the upgrade meter fills, freeze the game completely (pause all enemies, projectiles, physics, and the survival timer) and show the upgrade selection UI
- [ ] Display 3 upgrade cards on screen, each showing the upgrade name, a short description, and an icon; draw from the full pool with no duplicates in a single offering
- [ ] Allow already-owned stackable upgrades to appear again in offerings (up to their stack limit); fully-stacked upgrades are excluded from the pool
- [ ] Implement weighted rarity so that Piercing Rounds appears less frequently than other upgrades in the random draw
- [ ] Implement upgrade selection input: the player picks one of the 3 cards via mouse click or keyboard (1/2/3 keys)
- [ ] On selection, apply the chosen upgrade's effects to the player immediately, unpause the game, and reset the upgrade meter with an increased gem threshold for the next upgrade
- [ ] Clear all active upgrades from the player when the run ends (death/restart)

## Weapon Upgrades

- [ ] Implement Rapid Fire upgrade: reduces the player's auto-fire interval by 25% (faster shooting), stacks up to 3 times
- [ ] Implement Piercing Rounds upgrade: player projectiles pass through enemies on hit instead of being destroyed, dealing damage to each enemy they pass through; does not stack
- [ ] Implement Spread Shot upgrade: each player shot fires 2 additional projectiles in a narrow cone (±10 degrees from center); each additional stack adds 1 more projectile; stacks up to 2 times (max 5 projectiles per shot)
- [ ] Implement Explosive Rounds upgrade: player projectiles deal a small AoE damage burst on hit with distance-based falloff, damaging nearby enemies; does not stack
- [ ] Implement Heavy Shots upgrade: player projectiles deal 50% more damage per hit; stacks up to 2 times (2.25x damage at max stacks)
- [ ] Implement Extended Range upgrade: player projectile travel distance increased by 40% beyond the base range; does not stack

## Movement Upgrades

- [ ] Implement Momentum upgrade: increases the player's bunny hop speed cap by 20% (from ~1.8x to ~2.16x base speed); stacks up to 2 times
- [ ] Implement Forgiving Timing upgrade: widens the player's bunny hop timing window by 50% (from ~100ms to ~150ms); does not stack
- [ ] Implement Quick Roll upgrade: reduces the player's dodge roll cooldown by 30% (from 1.5s to ~1.05s); stacks up to 2 times (down to ~0.74s at max)
- [ ] Implement Air Control upgrade: doubles the influence of strafe input on the player's air trajectory while airborne; does not stack

## Utility Upgrades

- [ ] Implement Gem Magnet upgrade: triples the player's gem collection radius from ~2 units to ~6 units; does not stack
- [ ] Implement Aftershock upgrade: at the end of the player's dodge roll animation, emit a small damage pulse centered on the player (enough damage to kill Crawlers); does not stack
- [ ] Implement Last Stand upgrade: the next lethal hit the player receives is negated and the player becomes invulnerable for 1s instead of dying; the upgrade is then consumed and removed from the player's active upgrades; does not stack
- [ ] Implement Chain Lightning upgrade: when the player kills an enemy, a bolt of lightning automatically arcs to the nearest enemy within range dealing 50% of a normal player shot's damage; does not stack

## Upgrade System Tests

- [ ] Add unit tests for upgrade meter threshold scaling formula (10, 15, 20, ... per level)
- [ ] Add unit tests for upgrade stacking limits and cumulative stat modifications (fire rate, damage, speed cap, cooldown)
- [ ] Add unit tests for weighted rarity distribution ensuring Piercing Rounds appears less often
- [ ] Add unit tests for Last Stand consumption: negates one hit, grants 1s invulnerability, then removes itself from active upgrades

## Leaderboard

- [ ] Define the leaderboard data model: a sorted list of up to 10 entries, each containing a rank (int), name (string, max 16 characters), and survival time (float, displayed as MM:SS.mmm)
- [ ] Implement local leaderboard file storage: save and load the leaderboard as JSON to/from the Godot user data directory; if no file exists on first launch, start with an empty leaderboard
- [ ] Display the leaderboard on the death/results screen: a top-10 table (rank, name, time) on the right side, alongside the current run's survival time displayed large on the left side
- [ ] Highlight the current run's entry in the leaderboard table if the run's time qualified for the top 10
- [ ] Implement leaderboard name entry: if the run's time qualifies for the top 10, show a text input prompt (max 16 characters, default name "PLAYER"); on submission, insert the entry, displace the 10th-place entry if full, and save to disk
- [ ] Allow the player to skip name entry and restart immediately by pressing the restart key, bypassing the text input
- [ ] Add unit tests for leaderboard insertion logic: top-10 qualification check, correct rank displacement, proper sorting, and empty board behavior

## HUD

- [ ] Create the HUD as a CanvasLayer scene with all gameplay UI elements layered over the 3D viewport
- [ ] Display the survival timer on the HUD in MM:SS.mmm format, visible during gameplay, frozen on death
- [ ] Display the crosshair at screen center on the HUD, visible during gameplay
- [ ] Display the gem count and upgrade meter progress on the HUD as a progress bar showing gems collected toward the next upgrade threshold
- [ ] Display the countdown text (3-2-1) on the HUD at the start of each run, then hide it when gameplay begins
- [ ] Ensure all HUD elements are small and unobtrusive so they don't obscure enemies or compete with gameplay readability

## Polish

- [ ] Add a particle effect and pickup sound when the player collects a gem
- [ ] Add death particle effects on enemies (per-type or universal burst/dissolve)
- [ ] Add the player death visual effect (ragdoll physics or mesh disintegration)
- [ ] Add hit feedback on enemies when struck by player projectiles (brief color flash on the enemy mesh and/or a hit sound)
- [ ] Add spawn alert cues: brief visual flash and/or rumble sound at arena edge spawn points when enemies are about to appear
- [ ] Add the Sentinel aura visual: a translucent dome or ring showing the buff radius around the Sentinel
- [ ] Add the Howler enrage visual: affected enemies temporarily turn red for the 5s enrage duration
- [ ] Add the Shade stealth visual: a heat-shimmer distortion shader that makes the Shade nearly invisible until it telegraphs its attack
- [ ] Add the Burrower underground trail: a moving dirt/dust particle effect on the arena floor tracking the Burrower's underground position
- [ ] Add the Burrower crater visual: a rough terrain patch on the arena floor marking the slow zone for its 5s duration
- [ ] Add the Titan ground slam shockwave visual: a visible expanding wave along the ground during the slam attack
- [ ] Add the Charger telegraph visual: a glow effect and ground-scrape particles during the 1.5s charge wind-up
- [ ] Design the upgrade selection card visuals: styled cards with icons, names, and descriptions for the 3-card upgrade picker UI
- [ ] Add ambient audio for the arena to establish a hostile, tense atmosphere
- [ ] Add sound effects for the player's auto-fire shots, dodge roll, and bunny hop landing impacts
- [ ] Replace all placeholder meshes with final art assets for the player character, all 10 enemy types, the arena, gems, and projectiles

---

## Completed

### Project Setup

- [x] Create the main Godot scene and configure the project for C# (.NET), with a root Game node and children for Arena, Player, Enemies, UI, and Camera
- [x] Configure project input mappings for move forward/back/left/right, jump, dodge roll, aim (mouse), pause (escape), and restart
- [x] Create a GameManager autoload singleton that tracks game state enum (Countdown, Playing, Dead, Paused) and exposes state-change signals
- [x] Set up a unit test project (e.g. GdUnit4 or xUnit) alongside the main Godot project

### Arena

- [x] Build the arena floor as a StaticBody3D with a circular mesh (diameter ~40-50 units) and flat CollisionShape3D for the ground plane
- [x] Add arena boundary walls around the perimeter with collision shapes so the player and enemies cannot leave the play area
- [x] Apply a subtle grid or line texture to the arena floor so the player can read their movement speed and position at a glance
- [x] Place a center landmark on the arena floor (subtle glow or floor marking) so the player can orient relative to the center during combat
- [x] Set up arena lighting: bright functional illumination on the play area with a dark, undistracting background/skybox to keep the mood hostile and minimal
- [x] Define enemy spawn point positions evenly distributed along the arena perimeter edge, stored as markers or coordinates for the spawner system to use

### Player Character

- [x] Create the player scene as a CharacterBody3D with a placeholder capsule mesh, CollisionShape3D, and a C# script for movement
- [x] Implement player ground movement: 8-directional via WASD, instant full-speed with no acceleration ramp, direction relative to the camera's facing
- [x] Implement player jump with modest height — enough to clear ground-level attacks (Titan shockwave, Burrower eruption) but not enough to fly over enemies
- [x] Implement bunny hopping on the player: if jump is pressed within ~100ms of landing, preserve current horizontal speed and add a small speed boost
- [x] Implement bhop speed stacking on the player: each successive well-timed bhop increases speed, capping at ~1.8x base movement speed
- [x] Implement bhop speed decay on the player: when grounded without a successful bhop, horizontal speed rapidly decays back to base
- [x] Implement player air strafing: while airborne, left/right strafe input subtly curves the player's horizontal trajectory for mid-air direction control
- [x] Implement player dodge roll: a short, fast ground-only roll on a 1.5s cooldown with ~0.3s of invulnerability during the ~0.5s animation; cannot activate while airborne
- [x] Add unit tests for player bhop timing window detection, speed stacking/cap, speed decay rate, and dodge roll cooldown enforcement

### File Structure

- [x] Organize the `scripts/` directory into subdirectories by domain (e.g. `scripts/player/`, `scripts/enemies/`, `scripts/ui/`, `scripts/managers/`, `scripts/arena/`) and move existing scripts into the appropriate folders
- [x] Organize the `scenes/` directory into subdirectories by domain (e.g. `scenes/player/`, `scenes/enemies/`, `scenes/ui/`, `scenes/arena/`) and move existing scenes into the appropriate folders
- [x] Organize the `src/GodotExperiment.Core/` directory into subdirectories by domain (e.g. `Player/`, `Enemies/`, `GameLoop/`) and update namespaces to match
- [x] Update all scene resource paths, script references, and project references after reorganizing to ensure nothing is broken
- [x] Update `architecture/project-structure.md` to reflect the new directory layout
- [x] Create the `design/` directory referenced in the project rules (currently missing from the repository)

### Camera

- [x] Create a third-person camera attached to the player, positioned behind and above the character with smooth follow
- [x] Implement camera mouse-look: horizontal and vertical rotation orbiting around the player, with vertical angle clamped to prevent flipping
- [x] Ensure the camera does not clip into arena geometry and maintains clear visibility of the surrounding play area at all times
- [x] Implement a screen-center crosshair using a raycast from the camera into the world to determine the player's exact aim point
