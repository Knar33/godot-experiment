# Leaderboard

## Overview

The leaderboard is a local, persistent top-10 ranking of the player's best survival times. It is the primary motivational hook — the reason to keep playing after each death.

## Display

### Death Screen Layout

After the player dies, the screen shows two elements side by side:

- **Left**: The player's survival time for the run that just ended, displayed large and prominent (MM:SS.mmm format)
- **Right**: The top-10 leaderboard, showing rank, name, and time for each entry

### Leaderboard Table Format

| Rank | Name | Time |
|------|------|------|
| 1 | AAA | 04:32.117 |
| 2 | BBB | 03:58.442 |
| ... | ... | ... |
| 10 | ZZZ | 00:45.223 |

- The current run's entry is highlighted if it placed on the board
- If the run did not place, the player's time is still shown on the left but is not added to the leaderboard

## Name Entry

- If the player's time qualifies for the top 10, a name entry prompt appears
- The player types a name (max 16 characters, alphanumeric and common symbols)
- Default name is "PLAYER" if they submit without typing
- After entering the name, the leaderboard updates to include the new entry and the displaced 10th-place entry is removed

## Data Storage

- Stored locally on disk (simple file — JSON or similar)
- No online/cloud component
- The leaderboard persists between game sessions
- If no leaderboard file exists (first launch), the leaderboard starts empty

## Initial State

On first launch with no saved data, the leaderboard is empty (no fake entries). The first death that ends a run creates the first entry. This makes the player's first placement feel earned rather than competing against placeholder data.

## Restart

- A clearly labeled restart button/key prompt is visible on the death screen
- Pressing the restart key bypasses name entry if the player doesn't want to save their score
- The flow is: Die -> See time + leaderboard -> (optional) enter name -> Press restart
