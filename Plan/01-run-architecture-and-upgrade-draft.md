# Chunk 01: Run Architecture And Upgrade Draft

## Goal

Turn the current seed-driven level flow into a real `run` structure with post-level upgrade drafts, persistent build state, and deterministic offer generation.

## Why This Comes First

The current prototype already proves that a seeded run can start, progress through levels, and end cleanly. The next identity-defining step is to make every cleared board feed into buildcraft instead of only into another harder board.

## Systems In Scope

- `RunState` or equivalent model layered on top of `RunSettings`
- Between-level reward flow
- Draft generation rules for `pick 1 of 3`
- Upgrade persistence for the length of a run
- Upgrade rarity, tags, exclusions, and weighting
- Run-summary/debug visibility for active upgrades and history

## Deliverables

- Clearing a level opens a deterministic upgrade draft instead of immediately jumping forward
- The player can choose `1 of 3` upgrade offers, and the choice persists for the rest of the run
- Upgrade offers are generated from seeded rules so the same run state can be reproduced for testing
- Active upgrades are tracked separately from pre-run settings and temporary pickup effects
- The HUD or overlay can show the current build in a readable compact form
- Designers can add new upgrade entries through data without rewriting the draft flow

## Implementation Notes

- Keep `RunSettings` focused on pre-run configuration such as seed, difficulty, and opted-in challenge modifiers
- Introduce a separate run-layer model for:
  - chosen upgrades
  - encounter index
  - score state
  - unlock flags or future challenge hooks
- Start with a small curated pool before attempting large content breadth
- Use deterministic generation inputs such as:
  - run seed
  - level index
  - previous picks
  - filtered upgrade pool state
- Distinguish between:
  - `Core Mods`: limited-slot build-defining upgrades
  - `Run Mods`: standard stackable run upgrades
  - `Temporary Effects`: short-lived pickups already handled by gameplay systems

## Done When

- A full run includes at least one meaningful between-level draft choice
- Replaying the same seed and making the same decisions produces the same upgrade offers
- The current codebase has a clean conceptual split between setup-time settings, run-time build state, and moment-to-moment temporary effects

## Explicitly Deferred

- Meta-progression unlocks
- Full rarity economy balancing
- Final UI polish for the draft presentation
