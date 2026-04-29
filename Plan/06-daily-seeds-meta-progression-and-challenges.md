# Chunk 06: Daily Seeds, Meta Progression, And Challenges

## Goal

Create lightweight long-term retention systems that reward replay and experimentation without turning the game into a stat-grind treadmill.

## Why This Matters

The current deterministic seed work is already a strong foundation. This chunk turns that technical capability into player-facing reasons to come back.

## Systems In Scope

- Daily or rotating challenge seed rules
- Local best tracking
- Unlocks focused on `variety`, not raw permanent power
- Seed mutators and challenge presets
- Run history or summary archive hooks

## Deliverables

- A documented daily-seed generation rule set
- Local tracking for best score, best survival, or other key metrics per challenge context
- Unlock paths that expand content pools such as:
  - new upgrade buckets
  - new starting loadouts
  - new mutator sets
- Challenge presets that intentionally reshape runs, such as explosive-heavy or speed-heavy rule sets
- Clear separation between:
  - standard runs
  - daily challenge runs
  - chaos or custom runs

## Implementation Notes

- Do not front-load this with online service dependency
- Favor unlocks that widen possibility space instead of invalidating early balance
- Keep seed reproducibility central so challenge runs can still be discussed, tested, and compared
- If some challenge modifiers are too wild for score legitimacy, define that rule explicitly instead of leaving it ambiguous

## Done When

- A player has at least one reason to replay beyond simply seeing another random board
- The game can support `same seed, same challenge, compare results` conversations
- Progression adds new toys and rules more than it adds raw stats

## Explicitly Deferred

- Full backend services
- Monetization systems
- Seasonal live-ops structure
