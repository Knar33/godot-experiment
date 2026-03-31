# Tasks

Ordered by priority: core mechanics first, then combat loop, content, scoring, game feel, audio, atmosphere, settings, and final polish. Completed tasks are checked and moved to the bottom.

---

## 4. Core Game Loop

- [ ] Implement the countdown sequence: on game start or restart, show a 3-2-1 countdown UI with percussive audio beats while the player is locked at arena center, then transition GameManager to Playing
- [ ] Implement the survival timer: starts at 0:00.000 when the countdown finishes, counts up in real time, freezes on the exact frame of player death; this is the player's score
- [ ] Implement the restart flow: a single button press from the death screen despawns all enemies, clears all projectiles and gems, resets the timer, resets player position to center, clears all upgrades, and starts the countdown; total time from restart press to gameplay must be under 3 seconds
- [ ] Implement a pause menu: pressing escape during Playing state pauses the game (freezes physics and timer) and shows a menu with resume, restart, quit, and settings options
- [ ] Add unit tests for GameManager state transitions (Countdown -> Playing -> Dead -> Countdown) and survival timer accuracy

## 5. HUD Foundation

- [ ] Create the HUD as a CanvasLayer scene with all gameplay UI elements layered over the 3D viewport
- [ ] Display the survival timer on the HUD in MM:SS.mmm format, visible during gameplay, frozen on death
- [ ] Display the crosshair at screen center on the HUD, visible during gameplay
- [ ] Display the gem count and upgrade meter progress on the HUD as a progress bar showing gems collected toward the next upgrade threshold
- [ ] Display the countdown text (3-2-1) on the HUD at the start of each run, then hide it when gameplay begins
- [ ] Ensure all HUD elements are small and unobtrusive so they don't obscure enemies or compete with gameplay readability

## 6. Death Screen & Run Statistics

- [ ] Implement the death/results screen UI: displays the run's survival time (large, left side) and the leaderboard table (right side); shown when GameManager enters Dead state
- [ ] Track and display run statistics on the death screen: enemies killed, gems collected, wave reached, longest bhop chain, upgrades chosen (displayed as icons in acquisition order)
- [ ] Implement personal best tracking: if the run's time is a new personal best, show a "NEW BEST" callout with a brief fanfare sound
- [ ] Add a clearly labeled restart button/key prompt on the death screen

## 7. Enemy Foundation

- [ ] Create a base enemy C# class/scene with a health system (takes damage from player projectiles, dies at 0 HP), pathfinding/movement toward the player, and collision that triggers player death on contact
- [ ] Add per-enemy audio nodes to the base scene: `AmbientAudio`, `TelegraphAudio`, `DeathAudio` (all `AudioStreamPlayer3D`, bus: EnemySFX)
- [ ] Implement enemy damage flash: `ShaderMaterial` with `flash_intensity` uniform, set to 1.0 on hit, decays over 2 frames
- [ ] Implement enemy low-health indicator: when health < 25%, `flash_intensity` oscillates at 2 Hz between 0.0 and 0.3
- [ ] Implement enemy death effects: on death, spawn a detached `GpuParticles3D` with per-type color/count/explosiveness, free the enemy node immediately
- [ ] Implement gem dropping in the base enemy class: on death, spawn gem pickup instances based on the enemy's configured gem value, scattering them briefly outward before they settle
- [ ] Create the enemy spawner system: spawns enemy instances at the defined arena-edge spawn points with staggered timing, randomized point selection, and a bias against spawning directly behind the player
- [ ] Implement spawn-in animation for large enemies (Titan, Sentinel): the enemy is briefly visible at its spawn point but inactive for ~1s
- [ ] Add unit tests for base enemy health (damage application, death at 0 HP), per-type gem drop counts, and spawn point randomization distribution

## 8. Crawler (Enemy — Wave 1+)

- [ ] Create the Crawler scene: a small ground creature with a distinct placeholder mesh and unique silhouette, inheriting the base enemy class
- [ ] Implement Crawler AI: moves directly toward the player at moderate speed with contact damage on touch
- [ ] Configure Crawler stats: 2-3 shot health, moderate speed, 1 gem drop; spawns in groups of 5-10 that increase over waves
- [ ] Add Crawler audio: quiet skittering ambient sound, small crunch/pop death sound

## 9. Gem Pickups

- [ ] Create the gem pickup scene: a bright, easy-to-spot small mesh with a CollisionShape3D trigger area for player collection
- [ ] Implement gem collection on the player: when the player's collection area (~2 unit radius) overlaps a gem, the gem enters a magnetism snap (lerp toward player over ~0.1s with slight curve), then triggers collection
- [ ] Add gem collection audio: short bright chime via `AudioStreamPlayer3D` on the player, with ascending pitch on rapid successive pickups (+0.05 per pickup within 0.3s, capped at +0.5), reset after 0.3s gap
- [ ] Add gem collection particle effect: small bright burst at player position matching gem color
- [ ] Ensure gems persist on the arena floor indefinitely within a run and do not despawn until the run is restarted
- [ ] Implement the upgrade meter: tracks total gems collected toward a threshold that increases with each upgrade earned (1st at 10, 2nd at 15, 3rd at 20, etc.); pulse the HUD meter element when the threshold is reached
- [ ] Display the gem count and upgrade meter progress on the HUD as a progress bar or numeric indicator
- [ ] Reset all dropped gems, collected gem count, and the upgrade meter threshold on player death/restart

## 10. Wave System

- [ ] Create a WaveManager node that tracks the current wave number, controls spawn timing, and interfaces with the enemy spawner
- [ ] Define wave composition as data: for each wave number, specify which enemy types spawn and in what quantities (resource file or dictionary)
- [ ] Implement continuous wave flow: the next wave's enemies begin spawning while the current wave's enemies are still alive (no downtime)
- [ ] Implement staggered spawning within each wave: enemies trickle in over the wave's duration at intervals rather than all appearing simultaneously
- [ ] Implement early-game wave compositions (waves 1-5): wave 1 Crawlers only, then progressively introduce Spitters, Chargers, and Drones
- [ ] Enforce the difficulty scaling rule: difficulty increases only through enemy count, composition, and spawn speed — enemy stats are never modified
- [ ] Add unit tests for wave composition data correctness and spawn timing intervals

## 11. Spitter (Enemy — Wave 3+)

- [ ] Create the Spitter scene: a medium-sized enemy with a distinct placeholder mesh, inheriting the base enemy class
- [ ] Implement Spitter AI: moves slowly, plants itself at medium range, repositions occasionally to maintain spacing
- [ ] Implement the Spitter ranged attack: lobs a slow, visible arcing projectile toward the player's current position on a timed interval
- [ ] Implement Spitter ground hazard: projectile impact leaves a small damage zone for 1.5s that kills on contact, then despawns
- [ ] Configure Spitter stats: 4-5 shot health, slow speed, 2 gem drop; spawns 1-3 at a time spread apart
- [ ] Add Spitter audio: low gurgling ambient, brief spit/hiss telegraph, wet burst death sound

## 12. Charger (Enemy — Wave 4+)

- [ ] Create the Charger scene: a medium-sized enemy with a distinct placeholder mesh, inheriting the base enemy class
- [ ] Implement Charger AI: moves slowly toward the player, periodically stops to enter a 1.5s telegraph state (visible glow/animation)
- [ ] Implement the Charger's charge attack: after telegraph, launch in a straight line at very high speed for ~0.5s, dealing lethal contact damage
- [ ] Implement Charger recovery: stunned and stationary for 1s after charge ends before resuming
- [ ] Configure Charger stats: 8-10 shot health, slow normal / very fast charge speed, 3 gem drop; spawns solo or in pairs
- [ ] Add Charger audio: heavy footsteps/snorting ambient, loud scraping/revving 1.5s telegraph (high priority mix), heavy crash death sound

## 13. Drone (Enemy — Wave 5+)

- [ ] Create the Drone scene: a small flying enemy with a distinct placeholder mesh, positioned above ground, inheriting the base enemy class
- [ ] Implement Drone AI: hovers and orbits the player at close-medium range with fast, erratic lateral movement
- [ ] Implement the Drone dive-bomb attack: periodically telegraphs briefly, then dives toward the player at high speed for a lethal contact strike
- [ ] Configure Drone stats: 1-2 shot health, fast erratic speed, 1 gem drop; spawns in clusters of 4-8
- [ ] Add Drone audio: high-pitched buzzing ambient, quick dive whistle telegraph, small electric crackle death sound

## 14. Bloater (Enemy — Wave 6+)

- [ ] Create the Bloater scene: a large, slow enemy with a distinct placeholder mesh, inheriting the base enemy class
- [ ] Implement Bloater AI: moves very slowly toward the player, deals lethal contact damage if it touches the player
- [ ] Implement the Bloater death explosion: on death, wait 0.5s then trigger a large AoE explosion (lethal to player, damages other enemies)
- [ ] Configure Bloater stats: 15-20 shot health, very slow speed, 4 gem drop; spawns solo, 1-2 per wave
- [ ] Add Bloater audio: deep labored breathing ambient, 0.5s ticking/swelling pre-explosion telegraph, massive boom death sound (loudest non-music sound)

## 15. Mid/Late Wave Compositions

- [ ] Implement mid-game wave compositions (6-12): increase enemy counts, introduce Bloaters, Shades, Burrowers, then Sentinels and Howlers one at a time
- [ ] Implement late-game wave compositions (13-20): all enemy types active, multiple Sentinels/Howlers simultaneously, first Titan around wave 15, high Crawler/Drone swarm counts
- [ ] Implement endless wave scaling (21+): cycle through varied compositions with ever-increasing enemy counts, additional Titans, and accelerating spawn rates

## 16. Shade (Enemy — Wave 7+)

- [ ] Create the Shade scene: a stealthy enemy with a distinct placeholder mesh, inheriting the base enemy class
- [ ] Implement Shade near-invisibility: rendered as a faint heat-shimmer distortion, becomes fully visible for 0.5s before attacking
- [ ] Implement Shade flanking AI: avoids the player's forward camera arc, circles wide to approach from behind or blind spots
- [ ] Implement the Shade melee attack: a lethal strike from behind with a 0.5s fully-visible telegraph window
- [ ] Configure Shade stats: 3-4 shot health, moderate-fast speed, 3 gem drop; spawns solo or in pairs
- [ ] Add Shade audio: near-silent faint whisper ambient, sharp blade-draw reveal telegraph, ghostly dissipation death sound

## 17. Burrower (Enemy — Wave 8+)

- [ ] Create the Burrower scene: an underground enemy inheriting the base enemy class with burrowed/surfaced states
- [ ] Implement the Burrower's burrowed state: travels underground toward the player, visible only as a moving dirt trail, invulnerable to projectiles
- [ ] Implement the Burrower eruption attack: surfaces near the player with a quick AoE eruption (lethal), then remains above ground for 2s
- [ ] Implement Burrower re-burrowing: after 2s on the surface, digs back underground and resumes pursuit
- [ ] Implement Burrower arena craters: each eruption leaves a small crater that slows player movement for 5s before despawning
- [ ] Configure Burrower stats: 6-8 shot health (only while surfaced), moderate underground speed, 3 gem drop; spawns 1-2 at a time
- [ ] Add Burrower audio: muffled rumbling ambient while underground, rising rumble eruption telegraph, ground crack death sound

## 18. Sentinel (Enemy — Wave 10+)

- [ ] Create the Sentinel scene: a tall, towering enemy with a distinct placeholder mesh, visible glowing core weak point on chest, inheriting the base enemy class
- [ ] Implement Sentinel passive behavior: nearly stationary with a very slow drift toward the player; does not attack directly
- [ ] Implement the Sentinel buff aura: enemies within ~1/3 of arena radius gain +40% movement speed and attack 50% more frequently; removed on death
- [ ] Implement Sentinel weak point: hits to chest core deal full damage (5-6 shots to kill), body shots deal heavily reduced damage (20+ shots)
- [ ] Configure Sentinel stats: high health (body/weak point split), very slow speed, 5 gem drop; spawns solo
- [ ] Add Sentinel audio: deep resonant humming ambient (constant while alive), low structural collapse death sound with audible "depressurize" on buff removal
- [ ] Add Sentinel weak point hit sound: higher-pitched / more resonant variant of the player hit confirmation sound

## 19. Howler (Enemy — Wave 11+)

- [ ] Create the Howler scene: a medium enemy with a distinct placeholder mesh, inheriting the base enemy class
- [ ] Implement Howler base AI: moves toward the player at moderate speed with a weak melee swipe if adjacent
- [ ] Implement the Howler scream: every 10s, stops and channels a 1.5s scream (stationary and vulnerable during channel)
- [ ] Implement the Howler enrage effect: on scream completion, all enemies within a large radius turn red and gain +60% speed with all attacks becoming lethal contact for 5s
- [ ] Configure Howler stats: 8-10 shot health, moderate speed, 4 gem drop; spawns solo
- [ ] Add Howler audio: low growl ambient, rising scream 1.5s telegraph (loud, distinctive, unmistakable — high priority mix), abrupt silence choke-off death sound

## 20. Titan (Enemy — Wave 15+)

- [ ] Create the Titan scene: a massive enemy with a distinct placeholder mesh, significantly larger than all others, inheriting the base enemy class
- [ ] Implement Titan base AI: moves very slowly but relentlessly toward the player, never stops pursuing
- [ ] Implement the Titan ground slam attack: 2s telegraph, then radial shockwave along the ground in a ~180-degree forward arc; player must dodge-roll through or be airborne
- [ ] Implement the Titan sweep attack: a wide melee arc covering the front with a 1s telegraph; lethal on contact
- [ ] Implement the Titan crawler spawning: every 8s, 3-4 Crawlers emerge from the Titan's body
- [ ] Configure Titan stats: 100+ shot health, very slow speed, 10 gem drop; spawns solo, max 1-2 active
- [ ] Add Titan audio: heavy rhythmic boom footsteps, rising seismic hum slam telegraph, whoosh sweep telegraph, prolonged collapse/groan death sound (biggest death sound in the game)
- [ ] Add unit tests for Titan shockwave avoidance via dodge roll i-frames and via being airborne during a bhop

## 21. Upgrade Selection

- [ ] When the upgrade meter fills, play a "level up" chime, freeze the game completely (pause all enemies, projectiles, physics, survival timer), and show the upgrade selection UI
- [ ] Display 3 upgrade cards on screen, each showing the upgrade name, a short description, and an icon; draw from the full pool with no duplicates in a single offering
- [ ] Allow already-owned stackable upgrades to appear again (up to their stack limit); fully-stacked upgrades are excluded
- [ ] Implement weighted rarity so that Piercing Rounds appears less frequently
- [ ] Implement upgrade selection input: player picks one of the 3 cards via mouse click or keyboard (1/2/3 keys); add hover tick and selection confirmation sounds
- [ ] On selection, play a confirmation animation (card flies toward player, brief flash), apply the upgrade immediately, unpause, and reset the upgrade meter with an increased threshold
- [ ] Clear all active upgrades from the player when the run ends (death/restart)

## 22. Weapon Upgrades

- [ ] Implement Rapid Fire upgrade: reduces auto-fire interval by 25%, stacks up to 3 times
- [ ] Implement Piercing Rounds upgrade: projectiles pass through enemies instead of stopping on hit; does not stack
- [ ] Implement Spread Shot upgrade: each shot fires 2 additional projectiles in a narrow cone (±10°); each stack adds 1 more; stacks up to 2 times (max 5 per shot)
- [ ] Implement Explosive Rounds upgrade: projectiles deal a small AoE burst on hit with distance-based falloff; does not stack
- [ ] Implement Heavy Shots upgrade: projectiles deal 50% more damage per hit; stacks up to 2 times (2.25x at max)
- [ ] Implement Extended Range upgrade: projectile travel distance increased by 40%; does not stack

## 23. Movement Upgrades

- [ ] Implement Momentum upgrade: bhop speed cap increased by 20% (1.8x → 2.16x); stacks up to 2 times
- [ ] Implement Forgiving Timing upgrade: bhop timing window widened by 50% (100ms → 150ms); does not stack
- [ ] Implement Quick Roll upgrade: dodge roll cooldown reduced by 30% (1.5s → 1.05s); stacks up to 2 times (0.74s at max)
- [ ] Implement Air Control upgrade: air strafe influence doubled; does not stack

## 24. Utility Upgrades

- [ ] Implement Gem Magnet upgrade: gem collection radius tripled (2 → 6 units); does not stack
- [ ] Implement Aftershock upgrade: dodge roll emits a small damage pulse at roll end (kills Crawlers); does not stack
- [ ] Implement Last Stand upgrade: survive one lethal hit, become invulnerable for 1s, then consumed; does not stack
- [ ] Implement Chain Lightning upgrade: on enemy kill, bolt arcs to nearest enemy within range dealing 50% shot damage; does not stack

## 25. Upgrade System Tests

- [ ] Add unit tests for upgrade meter threshold scaling formula (10, 15, 20, ... per level)
- [ ] Add unit tests for upgrade stacking limits and cumulative stat modifications (fire rate, damage, speed cap, cooldown)
- [ ] Add unit tests for weighted rarity distribution ensuring Piercing Rounds appears less often
- [ ] Add unit tests for Last Stand consumption: negates one hit, grants 1s invulnerability, then removes itself

## 26. Leaderboard

- [ ] Define the leaderboard data model: a sorted list of up to 10 entries, each containing rank (int), name (string, max 16 chars), and survival time (float, MM:SS.mmm)
- [ ] Implement local leaderboard file storage: save/load as JSON to/from Godot user data directory; empty on first launch
- [ ] Display the leaderboard on the death/results screen: top-10 table (rank, name, time) on the right side
- [ ] Highlight the current run's entry in the leaderboard if it qualified for top 10
- [ ] Implement leaderboard name entry: if top 10, show a text input (max 16 chars, default "PLAYER"); on submit, insert and save
- [ ] Play a celebratory audio sting on leaderboard placement; absent if the run didn't qualify
- [ ] Allow the player to skip name entry and restart immediately by pressing the restart key
- [ ] Add unit tests for leaderboard insertion logic: top-10 qualification, rank displacement, sorting, empty board

## 27. Screen Shake System

- [ ] Implement `ScreenShakeState` in Core: decaying-spring model with additive intensity and hard cap (max 8.0)
- [ ] Implement `ScreenShake.cs` on `PlayerCamera`: reads `ScreenShakeState.CurrentOffset`, applies as camera displacement perpendicular to aim, multiplied by settings intensity
- [ ] Wire shake triggers: player shot fired (0.3), enemy hit (0.5), enemy kills (0.2–4.0 scaled by type), Bloater explosion (3.5), player death (5.0)
- [ ] Add unit tests for ScreenShakeState: additive intensity, hard cap, spring decay, zero-energy at rest

## 28. Hit Stop System

- [ ] Implement `HitStopState` in Core: frame-counted freeze timer with max cap (5 frames)
- [ ] Implement `HitStopManager` autoload: sets `Engine.TimeScale = 0` while frozen, updates via `_Process` (unscaled), scales by settings intensity
- [ ] Wire hit stop triggers: enemy kill (1-2 frames), multi-kill (3-4 frames), Titan slam (2-3 frames), Bloater explosion (2-3 frames)
- [ ] Add unit tests for HitStopState: frame countdown, frozen state, multi-trigger extension, max cap

## 29. Projectile Feedback

- [ ] Add projectile trail to `PlayerProjectile.tscn`: `MeshInstance3D` child with `ImmediateMesh` trail, 2-3 projectile-lengths, fading cyan; persists 0.1s after destruction
- [ ] Add muzzle flash to player: `GpuParticles3D` at spawn point (0, 1.2, 0), one-shot burst per firing event, randomized rotation
- [ ] Add impact sparks: `GpuParticles3D` spawned at collision point on projectile destruction; different particles for arena surface vs. enemy hit
- [ ] Add floor scorch decals: on arena surface impact, spawn a small `Decal` that fades over 1.5s then self-frees; cap at ~30 active decals

## 30. Hit Marker & Kill Confirmation

- [ ] Implement `HitMarker.cs` as a `Control` sibling to `Crosshair` on the HUD: draws 4 white tick marks on enemy hit (fade over 0.1s), red X on kill (fade over 0.15s)
- [ ] Add kill confirmation audio: distinct punchier sound on the killing blow, played from a dedicated `AudioStreamPlayer3D` (`KillAudio`) on the player

## 31. Movement Feedback Effects

- [ ] Implement FOV scaling in `PlayerCamera.cs`: base FOV (default 75°) + `(speedMultiplier - 1.0) * 6°`, smoothly interpolated
- [ ] Implement `SpeedLines.cs` on the HUD CanvasLayer: radial lines via `_Draw()`, visible above 1.3x speed, intensity scales to 1.8x; disabled when Reduce Motion is on
- [ ] Implement `BhopCounter.cs` on the HUD: label showing "x{count}" after 2+ consecutive bhops, fades over 0.5s on chain break
- [ ] Add bhop landing audio: `AudioStreamPlayer3D` (`BhopAudio`) on the player; brighter sound on successful bhop, duller variant on normal landing
- [ ] Add dodge roll audio: `AudioStreamPlayer3D` (`RollAudio`) on the player; quick whoosh on roll start
- [ ] Add dodge roll afterimage: brief motion blur or ghosted trail during the roll duration; disabled when Reduce Motion is on
- [ ] Add landing dust particles: subtle `GpuParticles3D` at player feet on landing; brighter/sharper variant on successful bhop

## 32. Directional Threat Indicator

- [ ] Implement `ThreatIndicator.cs` on the HUD CanvasLayer: queries `"enemy"` group each frame, draws red arcs on screen edges for enemies within 5 units that are outside the camera frustum; alpha scales with proximity; limited to 3 closest off-screen threats

## 33. Timer Milestone Flash

- [ ] The survival timer on the HUD subtly pulses or changes color at round-number milestones (1:00, 2:00, 3:00, etc.)

## 34. Adaptive Music System

- [ ] Set up Godot AudioServer bus layout: Master → Music, SFX (PlayerSFX, EnemySFX, UISFX), Ambience
- [ ] Create `MusicManager` autoload with `BaseLayer`, `MidLayer`, `HighLayer` AudioStreamPlayers outputting to Music bus
- [ ] Implement intensity metric: combined score from active enemy count, wave number, and enemy proximity; drives layer volume crossfading
- [ ] Implement death transition: all layers fade to 0 over 0.2s, stop; death sting plays after 0.3s camera freeze
- [ ] Implement restart transition: base layer fades in during countdown
- [ ] Source or create placeholder adaptive music tracks (ambient drone, mid percussion, high driving) and death sting

## 35. Enemy Audio Integration

- [ ] Configure per-enemy ambient sound streams in each enemy scene (see `design/audio.md` for full identity table)
- [ ] Configure per-enemy telegraph sound streams with boosted `UnitSize` for high priority mix
- [ ] Configure per-enemy death sound streams
- [ ] Implement swarm audio optimization: skip ambient playback for same-type enemies beyond 20 units from the player; `MaxPolyphony = 1` per enemy

## 36. UI & System Audio

- [ ] Add countdown audio: percussive beats for 3-2-1 and a sharp energizing hit on "GO"
- [ ] Add menu navigation sounds: minimal clean ticks on button hover/select
- [ ] Implement mute on focus loss: `SettingsManager` handles `NotificationApplicationFocusIn/Out`

## 37. Arena Atmosphere

- [ ] Add `AmbientParticles` (`GpuParticles3D`) to the Arena scene: faint dust/ember particles drifting above the floor; ~100-200 particles, slow drift, low alpha; disabled when Reduce Motion is on
- [ ] Extend the arena floor shader with an edge warning zone: the outer ~10% of arena radius has a subtle darker tint or pulsing edge line
- [ ] Add `AmbientAudio` (`AudioStreamPlayer`) to the Arena scene: looping ambient drone, output to Ambience bus
- [ ] Add arena edge glow: faint continuous glow or energy effect on the boundary, visible from center but not distracting

## 38. Settings System

- [ ] Implement `SettingsData` in Core: data class with all settings fields and defaults (controls, audio, video, game feel, accessibility)
- [ ] Implement `SettingsManager` autoload: loads/saves `SettingsData` as JSON to `user://settings.json`; applies settings to Godot systems (AudioServer, DisplayServer, Engine, InputMap)
- [ ] Add unit tests for SettingsData: serialization round-trip, default values, forward-compatibility with missing fields

## 39. Settings Menu UI

- [ ] Create the SettingsMenu scene: `TabContainer` with Controls, Audio, Video, Game Feel, and Accessibility tabs
- [ ] Implement Controls tab: mouse sensitivity slider, invert Y toggle, key rebinding buttons with conflict detection, gamepad deadzone slider
- [ ] Implement Audio tab: master/music/SFX volume sliders, mute on focus loss toggle
- [ ] Implement Video tab: resolution dropdown, display mode selector, VSync toggle, max FPS (visible when VSync off), FOV slider
- [ ] Implement Game Feel tab: screen shake intensity slider, hit stop intensity slider, speed lines toggle, crosshair style/color selector
- [ ] Implement Accessibility tab: colorblind mode dropdown, HUD scale slider, reduce motion toggle, high contrast outlines toggle
- [ ] Integrate the settings menu into the pause menu

## 40. Accessibility Implementation

- [ ] Implement colorblind palette system: `ColorPalette` resource with 4 variants (Default, Protanopia, Deuteranopia, Tritanopia); swap active palette on mode change; materials and UI reference the palette
- [ ] Implement reduce motion: force-disable speed lines, cap screen shake at 25%, disable dodge roll afterimage, reduce `GpuParticles3D` `AmountRatio` to 0.3
- [ ] Implement high contrast outlines: Sobel edge-detection post-process shader on enemy and projectile meshes, toggled via a `CanvasLayer`
- [ ] Implement HUD scaling: multiply HUD `CanvasLayer` scale by the HUD scale setting (0.75-1.5)

## 41. Spawn Alert Cues

- [ ] Add brief visual flash and/or rumble sound at arena edge spawn points when enemies are about to appear

## 42. Enemy-Specific Visuals

- [ ] Add the Sentinel aura visual: translucent dome or ring showing the buff radius
- [ ] Add the Howler enrage visual: affected enemies temporarily turn red for the 5s enrage duration
- [ ] Add the Shade stealth visual: heat-shimmer distortion shader, fully visible during 0.5s telegraph
- [ ] Add the Burrower underground trail: moving dirt/dust particle effect on the arena floor
- [ ] Add the Burrower crater visual: rough terrain patch marking the slow zone for 5s
- [ ] Add the Titan ground slam shockwave visual: visible expanding wave along the ground
- [ ] Add the Charger telegraph visual: glow effect and ground-scrape particles during 1.5s wind-up

## 43. Final Art & Polish

- [ ] Design the upgrade selection card visuals: styled cards with icons, names, and descriptions for the 3-card picker UI
- [ ] Replace all placeholder meshes with final art assets for the player character, all 10 enemy types, the arena, gems, and projectiles
- [ ] Replace all placeholder audio with final sound assets
- [ ] Replace placeholder music tracks with final adaptive music

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

### Camera Enhancements

- [x] Offset the camera horizontally to an over-the-shoulder position so the crosshair targets the world ahead of the player rather than the player's back
- [x] Implement scroll-wheel zoom: mouse scroll adjusts the camera distance between a configurable min and max, with smooth interpolation toward the target distance

### Player Shooting

- [x] Create a player projectile scene: a fast-moving Area3D or RigidBody3D with a bright visually distinct mesh, CollisionShape3D, and a script that moves it forward each physics frame
- [x] Implement player auto-fire: while GameManager state is Playing, the player continuously fires projectiles (~8 per second) from the character toward the camera's aim direction
- [x] Implement projectile range limit: player projectiles despawn after traveling a fixed distance roughly matching the arena radius
- [x] Implement projectile-enemy collision: when a player projectile contacts an enemy, deal damage to the enemy and destroy the projectile (default pre-upgrade behavior)
- [x] Ensure player projectiles are visually distinct from enemy projectiles (Spitter) using a consistent bright color unique to the player
- [x] Add unit tests for player fire rate timing interval and projectile range-based despawn

### Shooting — Hold-to-Fire

- [x] Add a `shoot` input action mapped to left mouse button (and right trigger for gamepad) in the Godot project input map
- [x] Update `Player.cs` to only call `AutoFireState.Update(dt)` and spawn projectiles while the `shoot` action is held; stop firing when released
- [x] Reset the `AutoFireState` timer when the fire input is first pressed so the first shot fires immediately on click
- [x] Update `AutoFireState` unit tests to cover the hold-to-fire behavior (timer reset on press, no accumulation while not firing)

### Projectile Collision & Audio

- [x] Update `PlayerProjectile.tscn` collision mask to include layer 1 (arena geometry) so projectiles collide with walls and floor
- [x] Update `PlayerProjectile.cs` to detect arena geometry collisions in `BodyEntered` and destroy the projectile on impact
- [x] Add an `AudioStreamPlayer3D` (`FireAudio`) to the player for firing sounds; play a short punchy sound on each shot with slight pitch randomization (±5%)
- [x] Add an `AudioStreamPlayer3D` (`ImpactAudio`) to the projectile scene for impact sounds; reparent to a temp node on collision so it finishes playing after projectile is freed; distinct sounds for enemy hits vs. surface hits
- [x] Source or create placeholder audio assets for the firing sound and both impact sound types

### Player Death & I-Frames

- [x] Implement one-hit player death: any enemy damage source (contact, projectile, explosion, ground hazard) triggers instant player death when the player is not in i-frames
- [x] Implement dodge roll i-frame protection: during the dodge roll's ~0.3s active invulnerability window, all incoming damage is ignored
- [x] Implement player death sequence: freeze the survival timer on the exact frame of death, cut all audio abruptly, play a death effect (ragdoll or disintegration) on the player character
- [x] Implement death camera freeze: hold the camera position for ~0.3s after player death, then play the death sting sound and transition GameManager to Dead
- [x] Add unit tests for player i-frame protection during dodge roll and death triggering from different damage source types (contact, projectile, AoE)
