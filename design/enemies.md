# Enemies

## Design Philosophy

Enemies are designed as an ecosystem. Each type applies a different kind of pressure, and their danger comes from how they combine. Early waves introduce simple enemies in isolation. Later waves layer types together, creating situations that demand priority targeting, spatial awareness, and split-second decision-making.

All enemies spawn at the edges of the arena and move inward toward the player. All enemies drop gems on death (see `player.md` for gem values per enemy type).

## Visual & Audio Identity

Every enemy type must be instantly recognizable by both sight and sound. In a chaotic arena with 30+ enemies on screen, the player relies on silhouette, color, and audio cues to prioritize targets without consciously identifying each one.

- **Silhouette**: Each enemy has a unique shape and size that reads clearly even at distance or in peripheral vision. No two enemy types should share a similar silhouette.
- **Color Palette**: Each enemy type has a dominant color or accent that distinguishes it. Colors should remain readable under the arena's lighting and for players with color vision deficiencies (see `settings.md` for colorblind mode).
- **Audio Signature**: Each enemy has a distinct ambient sound and attack telegraph sound. The player should be able to identify off-screen threats by ear. Telegraph sounds (Charger wind-up, Howler scream) are mixed at high priority so they cut through combat noise (see `audio.md` for full audio identity table).

## Death Effects

Enemy deaths should feel satisfying and communicate clearly that the threat is eliminated.

- **Burst/dissolve**: On death, enemies burst apart or dissolve into particles matching their color palette. No ragdolls — corpses must not litter the arena, obscure gems, or create visual noise.
- **Speed**: Death effects are fast (under 0.3s). The battlefield should clear quickly so the player can focus on remaining threats.
- **Scaling**: Death effects scale with enemy importance. A Crawler pops quietly. A Titan has a prolonged collapse with heavy particles, screen shake, and a distinctive death sound.
- **Kill sound**: Each enemy type has a distinct death sound. The player should know what they killed by the audio alone (see `audio.md`).

---

## 1. Crawler

**Role**: Fodder / Pressure

The baseline enemy. Crawlers are small ground creatures that move directly toward the player at moderate speed. They attack by making contact. Individually trivial — they exist to create a constant floor of pressure that the player must manage while dealing with more dangerous threats.

- **Health**: Very Low (2-3 shots)
- **Speed**: Moderate
- **Attack**: Contact damage
- **Spawn Pattern**: Groups of 5-10, increasing over time
- **Threat Level**: Low individually, dangerous in swarms

---

## 2. Spitter

**Role**: Ranged Pressure / Area Denial

A stationary-ish enemy that plants itself at medium range and lobs slow, visible projectiles at the player. Spitters force the player to dodge while handling melee threats. Their projectiles linger briefly on the ground as small hazard zones on impact.

- **Health**: Low (4-5 shots)
- **Speed**: Slow (repositions occasionally)
- **Attack**: Arcing projectile that leaves a brief ground hazard (1.5s)
- **Spawn Pattern**: 1-3 at a time, positioned spread apart
- **Threat Level**: Low-Medium (their real danger is zoning the player into melee enemies)

---

## 3. Charger

**Role**: Burst Threat / Disruption

A medium-sized enemy that pauses, telegraphs with a visible wind-up (glows, scrapes the ground), then launches in a straight line at high speed. Deals heavy contact damage. After charging, it skids to a stop and is vulnerable for a brief window before it can charge again.

- **Health**: Medium (8-10 shots)
- **Speed**: Slow normally, very fast during charge
- **Attack**: High-speed linear charge (1.5s telegraph, ~0.5s charge duration)
- **Recovery**: Stunned for 1s after charge ends
- **Spawn Pattern**: Solo or in pairs
- **Threat Level**: Medium (predictable but punishing if you're not watching)

---

## 4. Drone

**Role**: Aerial Swarm / Attention Split

Small flying enemies that hover above the ground plane. They orbit the player at close-medium range and periodically dive in to strike. Individually fragile, but they spawn in clusters and attack from angles the ground enemies don't cover, forcing the player to aim upward.

- **Health**: Very Low (1-2 shots)
- **Speed**: Fast, erratic movement
- **Attack**: Dive-bomb (short telegraph, quick strike)
- **Spawn Pattern**: Clusters of 4-8
- **Threat Level**: Low-Medium (force attention upward while ground threats close in)

---

## 5. Bloater

**Role**: Area Denial / Positioning Punishment

A large, slow, bloated enemy that absorbs significant damage. On death, it explodes in a wide radius, dealing lethal damage to the player (and other enemies caught in the blast). Bloaters force the player to manage *where* they kill things — detonating one in the middle of a crawler swarm clears the swarm, but detonating one at close range is death.

- **Health**: High (15-20 shots)
- **Speed**: Very Slow
- **Attack**: Contact damage (slow, rarely reaches player)
- **On Death**: Explodes after 0.5s delay, large AoE (lethal to player, damages other enemies)
- **Spawn Pattern**: Solo, 1-2 per wave in later stages
- **Threat Level**: Medium (a puzzle piece — dangerous if mismanaged, useful if played correctly)

---

## 6. Shade

**Role**: Flanker / Paranoia

A stealthy enemy that is mostly invisible — only faintly visible as a heat-shimmer distortion. Shades circle wide around the player and attack from behind. They become fully visible for a brief moment just before striking, giving the player a narrow reaction window.

- **Health**: Low (3-4 shots)
- **Speed**: Moderate-Fast
- **Attack**: Melee strike from behind (0.5s visible telegraph)
- **Behavior**: Avoids the player's forward arc, seeks blind spots
- **Spawn Pattern**: Solo or pairs, starting in mid-game waves
- **Threat Level**: Medium-High (punishes tunnel vision, rewards spatial awareness)

---

## 7. Sentinel

**Role**: Force Multiplier / Priority Target

A tall, towering enemy that does not attack directly. Instead, it projects an aura that buffs all enemies within a radius — increased speed and aggression. Sentinels stand at the back and let other enemies do the work. Killing the Sentinel removes the buff. They have high health and a small, hard-to-hit weak point (glowing core on their chest).

- **Health**: High (20+ shots to body, 5-6 shots to weak point)
- **Speed**: Very Slow (nearly stationary, slow drift toward player)
- **Attack**: None directly
- **Aura**: Enemies within range gain +40% movement speed and attack 50% more frequently
- **Aura Range**: Large (roughly 1/3 of the arena)
- **Spawn Pattern**: Solo, appears in mid-to-late waves
- **Threat Level**: High (makes everything around it more dangerous — top priority target)

---

## 8. Burrower

**Role**: Spatial Disruption / Surprise Threat

An enemy that travels underground, visible only as a moving dirt trail on the arena floor. It surfaces near the player with a quick eruption attack, then burrows again. While surfaced, it is briefly vulnerable. The eruption leaves a small crater that acts as rough terrain (slows the player) for several seconds.

- **Health**: Medium (6-8 shots, only damageable while surfaced)
- **Speed**: Moderate underground
- **Attack**: Eruption on surfacing (small AoE), contact damage while above ground
- **Surface Duration**: 2s before re-burrowing
- **Crater Duration**: 5s of slow terrain
- **Spawn Pattern**: 1-2 at a time, starting mid-game
- **Threat Level**: Medium-High (craters degrade the arena over time, limiting safe movement space)

---

## 9. Howler

**Role**: Enrage / Priority Target

A medium enemy that periodically stops moving and lets out a scream. The scream enrages all enemies within a large radius for a duration — enraged enemies turn red, move faster, and deal damage on contact even if they normally use ranged attacks. The Howler itself is vulnerable during the scream (it stands still with a long animation).

- **Health**: Medium (8-10 shots)
- **Speed**: Moderate
- **Attack**: Weak melee swipe
- **Howl**: 1.5s channel, enrages all enemies in range for 5s (+60% speed, all attacks become lethal contact)
- **Howl Cooldown**: 10s
- **Spawn Pattern**: Solo, appears in mid-to-late waves
- **Threat Level**: High (transforms manageable situations into lethal ones — must be killed during the howl window or before)

---

## 10. Titan

**Role**: Boss / Endurance Test

A massive enemy that appears in late waves. The Titan is slow but relentless. It has a massive health pool and multiple attack patterns: a ground slam that sends out a shockwave the player must dodge-roll over, a sweeping arm attack covering a wide frontal arc, and it periodically spawns Crawlers from its body. The Titan demands sustained focus-fire while the player continues managing all other enemies.

- **Health**: Very High (100+ shots)
- **Speed**: Very Slow
- **Attacks**:
  - **Ground Slam**: Telegraphed 2s, sends a radial shockwave along the ground (must dodge-roll or bunny hop over to avoid, ~180-degree forward arc)
  - **Sweep**: Wide melee arc in front, 1s telegraph
  - **Spawn Crawlers**: Every 8s, 3-4 Crawlers emerge from the Titan's body
- **Spawn Pattern**: Solo, first appears around wave 15+, max 1-2 on the field at a time
- **Threat Level**: Very High (an endurance check that pressures the player for extended periods)

---

## Enemy Interaction Matrix

The real difficulty comes from enemy combinations:

| Combination | Why It's Dangerous |
|---|---|
| Crawlers + Spitters | Spitter hazard zones funnel the player into crawler swarms |
| Charger + Drones | Drones pull aim upward; Charger punishes inattention to ground level |
| Bloater + Crawlers | Killing the Bloater in panic can chain into the player; ignoring it blocks a lane |
| Shade + Sentinel | Sentinel buffs make the already-sneaky Shade strike faster and harder |
| Howler + anything | Enraged swarms are lethal — the Howler must be prioritized |
| Burrower + Spitters | Craters limit dodging space; Spitter projectiles cover the shrinking safe zones |
| Titan + Howler | The Titan's crawlers become enraged, and the player can't easily focus-fire the Howler while dodging slams |
