# Chunk 08: Social Modes, Content Pipeline, And Production Readiness

## Goal

Prepare the game to scale into social modes and broader content without undermining the single-player run game that defines the current direction.

## Why This Comes Last

The overview includes strong social ideas, but they will land better once the core run loop already generates interesting choices and stories on its own.

## Systems In Scope

- Co-op and versus mode direction
- Shared or drafted modifier structures for multiple players
- Content registration and expansion workflow
- Balance and regression checklist for run-heavy content
- Release-readiness planning for a wider feature set

## Recommended Social Mode Priorities

1. `Shared-screen co-op`
   Two paddles, cooperative survival, and shared build decisions
2. `Draft pressure mode`
   Players take turns choosing upgrades that help one side and complicate another
3. `Versus disruption mode`
   Junk bricks, hazards, or rule injections sent between players

## Deliverables

- A clear social mode order of operations for continued production
- Design rules for how upgrade drafts and pickups behave with more than one player
- A content pipeline that cleanly supports additive upgrades, bricks, bosses, themes, and challenge presets
- A production checklist for validating:
  - seed determinism
  - run-state integrity
  - score legitimacy
  - UI readability
  - content regressions

## Implementation Notes

- Do not let multiplayer dictate the base run architecture retroactively
- Social modes should reuse the same content language as single-player wherever possible
- Favor additive registries and authoring pipelines over hard-coded branching by mode
- If a mode changes score legitimacy or determinism, label it clearly

## Done When

- The project has a credible path from strong single-player runs to compelling social play
- Future content additions do not require core-loop rewrites
- The repo has a repeatable validation story for increasingly chaotic combinations

## Explicitly Deferred

- Online netcode
- Account systems
- Live service obligations
