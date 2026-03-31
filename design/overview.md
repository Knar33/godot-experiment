# Game Overview

## Concept

A single-player third-person shooter where the player survives against endless waves of increasingly difficult enemies in a confined arena. Inspired by Devil Daggers, the game distills the shooter genre down to its purest form: shoot, survive, die, try again.

## Target Audience

Players who enjoy high-skill-ceiling action games where improvement is measured in seconds. Fans of Devil Daggers, Vampire Survivors (the combat intensity, not the idle aspect), DOOM Eternal's flow state, and Quake's movement mechanics. The game rewards players who want to master systems rather than be guided through them.

## Pillars

- **Skill Meets Chaos** - Every run starts the same, but randomized gem upgrades make each one play differently. Mastery comes from adapting your playstyle to the upgrades you're given while executing precise movement and aim under pressure.
- **One More Run** - Runs are short and death is instant. The friction between dying and trying again is near zero. The randomized upgrades ensure the next run will feel different from the last.
- **Readable Chaos** - Even when dozens of enemies flood the arena, every enemy type is visually and behaviorally distinct enough that skilled players can read and react to the situation.
- **Movement Is A Weapon** - Bunny hopping rewards skilled, rhythmic movement with increased speed. The gap between a player who can bhop and one who can't is enormous — mastering movement is as important as mastering aim.
- **Everything Feels Good** - Every shot fired, every enemy killed, every gem collected, every bhop landed should produce satisfying feedback. Audio, visual effects, and camera responses work together to make the moment-to-moment action feel punchy and rewarding (see `game-feel.md` and `audio.md`).

## Core Fantasy

The player is alone in a hostile arena. Every second survived is earned. The leaderboard is the only measure of mastery.

## Camera

Third-person camera positioned behind and above the player character. The camera should provide clear visibility of the surrounding arena to let the player read incoming threats from all directions.

## Target Feel

- Fast, responsive movement with no acceleration curve — the player moves at full speed immediately
- Bunny hopping builds speed beyond the base — skilled players are always in the air, always moving fast
- Shooting is constant and fluid; the player should almost always be firing
- Every hit lands with a satisfying thud. Kill confirmations are immediate and clear — audio, visual, and crosshair all agree
- Gems rain from dead enemies, filling the upgrade meter — the player grows stronger as the run progresses
- Death is abrupt — no health bar, no second chances (one-hit kill from any enemy attack). The music cuts, the world freezes, silence
- Respawn is instant — press a button on the death screen to restart immediately
- The game never talks down to the player. No tutorials, no hand-holding. Mechanics are learned by doing
