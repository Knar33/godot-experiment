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
- A brief camera freeze (0.3s) emphasizes the moment of death before transitioning to the results screen
- No health, no shields, no extra lives — one hit and the run is over

## Restart Flow

- From the results screen, the player presses a single button to restart
- The arena resets completely: all enemies despawn, all projectiles clear, the timer resets
- The player spawns at center and the countdown begins again
- Total time from death to gameplay should be under 3 seconds (excluding the countdown)

## Session Flow

There is no meta-progression, no menus between runs (unless the player presses escape), and no loading screens. The game is a tight loop of play-die-replay. The main menu is accessible via pause but the default state is always the arena.
