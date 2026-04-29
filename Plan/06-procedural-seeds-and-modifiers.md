# Chunk 06: Procedural Seeds And Modifiers

## Goal

Make runs replayable and customizable without losing the structure of level-based progression.

## Systems In Scope

- Seed entry and seed display
- Deterministic random service
- Procedural variation rules
- Run modifiers
- Difficulty presets
- Validation for broken or unfair combinations

## Modifier Examples

- Starting ball count
- Paddle size
- Ball speed scaling
- Drop pool restrictions
- Brick durability scaling
- Extra hazard frequency

## Deliverables

- A user can start a run with a chosen seed or a random seed
- The same seed reproduces the same procedural choices
- Players can choose a set of modifiers before starting
- Difficulty options map cleanly to tuning parameters instead of hard-coded special cases

## Implementation Notes

- Use procedural variation to transform authored content, not necessarily replace it.
- Keep the seeded system deterministic for gameplay logic, but allow non-gameplay cosmetics to remain non-deterministic if needed.
- Add tooling or debug output so seeds can be tested and reproduced easily.

## Done When

- A seed can be shared and replayed with consistent gameplay results.
- Modifiers create meaningful variety without destabilizing the main loop.

## Explicitly Deferred

- Ranked leaderboards
- Daily challenge infrastructure
