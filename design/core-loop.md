# Core Game Loop

## Loop Summary

1. **Spawn** - Player spawns at the center of the arena. A brief countdown (3-2-1) gives them a moment to orient.
2. **Survive** - Waves of enemies spawn at the arena edges. The player moves, dodges, and shoots to stay alive. A survival timer counts up from 0.
3. **Die** - Any enemy attack kills the player instantly. The run ends.
4. **Results** - The death screen displays the survival time and the local leaderboard. If the player's time is in the top 10, they can enter their name.
5. **Restart** - The player presses a button to immediately start a new run.

## Survival Timer

- Begins counting at 0:00.000 when the countdown finishes and the first wave spawns
- Displays in MM:SS.mmm format (minutes, seconds, milliseconds)
- Visible on the HUD during gameplay
- Frozen on the exact frame of player death
- This is the player's score — the sole measure of performance

## Death

- Triggered by any contact with an enemy attack (melee hit, projectile, explosion, etc.)
- The player character ragdolls or disintegrates on death
- All audio cuts abruptly on death. A brief silence followed by a low death sting (see `audio.md`)
- A brief camera freeze (0.3s) emphasizes the moment of death before transitioning to the results screen
- No health, no shields, no extra lives — one hit and the run is over

## Restart Flow

- From the results screen, the player presses a single button to restart
- The arena resets completely: all enemies despawn, all projectiles clear, all gems clear, the timer resets
- **Enemy despawn**: Every living enemy must be immediately removed from the scene tree on restart. No enemies from a previous run should persist into the countdown or the next run. This includes enemies that are mid-spawn, mid-attack, or in any special state (burrowed, charging, etc.).
- The player spawns at center and the countdown begins again
- Total time from death to gameplay should be under 3 seconds (excluding the countdown)

## Run Statistics

The death screen shows a summary of the run alongside the survival time and leaderboard. Stats serve two purposes: they give the player a sense of accomplishment even on short runs, and they help diagnose what went well or poorly.

- **Survival Time**: The primary score, displayed large and prominent.
- **Personal Best Notification**: If the run's time is a new personal best, a "NEW BEST" callout appears next to the time. This moment should feel special — brief fanfare sound, highlighted text.
- **Enemies Killed**: Total enemies killed during the run, with a per-type breakdown available on hover or secondary view.
- **Gems Collected**: Total gems collected.
- **Upgrades Chosen**: Icons of the upgrades selected during the run, displayed in order of acquisition.
- **Longest Bhop Chain**: The longest consecutive bhop chain achieved during the run. Serves as a secondary skill metric alongside survival time.
- **Wave Reached**: The highest wave number reached.

Stats are shown alongside the leaderboard on the death screen (see `leaderboard.md`). They are not persisted between sessions — only the survival time is saved to the leaderboard. This keeps the data lightweight and focused.

## Session Flow

There is no meta-progression, no menus between runs (unless the player presses escape), and no loading screens. The game is a tight loop of play-die-replay. The main menu is accessible via pause but the default state is always the arena.
