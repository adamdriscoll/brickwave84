# Chunk 04: Scoring, Risk, And Reward Systems

## Goal

Reward skill expression and daring choices so score becomes part of the fun loop rather than a passive counter.

## Why This Matters

If the game is going to lean into seeds, daily runs, and replayability, then scoring has to measure more than survival. It should reward precision, speed, and self-imposed danger.

## Systems In Scope

- Combo rules
- Speed-tier bonuses
- Angle or trick-shot bonuses
- Near-miss or clutch-save bonuses
- Opt-in risk modifiers with score upside
- Run-end score breakdown

## Deliverables

- A combo system that rewards sustained offensive play, such as extended sequences without a paddle touch
- Score multipliers tied to ball speed tiers or other visible danger states
- At least one risk-forward rule the player can deliberately opt into for extra points
- A results breakdown that explains where the score came from
- Score rules designed to remain deterministic for repeatable seeds and future leaderboard comparisons

## Implementation Notes

- Keep the base score readable even if advanced systems are ignored by new players
- Make every major bonus source legible through HUD feedback, not only end-screen math
- Avoid rewarding degenerate stalling or safe farming patterns
- When difficulty and score incentives are linked, communicate the contract clearly before the player commits

## Done When

- Higher scores reflect observable better play or braver decisions
- Players can intentionally chase score strategies during a run
- The run summary can explain the scoring outcome without hidden rules

## Explicitly Deferred

- Online leaderboards
- Spectator or replay systems
- Deep stat history screens
