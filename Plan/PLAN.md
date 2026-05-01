# Get Bricked Plan

## Planning Reset

This plan replaces the earlier roadmap that assumed the project still needed its first vertical slice. That is no longer true.

`Get Bricked` already has a real playable runtime with seeded run setup, procedural level progression, lives, drops, themes, temporary UI flow, and data-driven content hooks. The new plan starts from that baseline and pushes the game toward a stronger identity:

`a run-based arcade roguelite built on brick-breaker feel`

Read [OVERVIEW.md](./OVERVIEW.md) first for the product reframe.

## Current Baseline

The existing game already covers:

- one-paddle breakout gameplay with one or more balls
- seeded run setup and deterministic gameplay rolls
- procedural multi-level progression
- data-driven brick, level, power-up, and theme assets
- timed and instant pickup effects
- menu, setup, HUD, pause, win, and loss flow

That means the roadmap should focus less on `can we make breakout work?` and more on:

- `how does a run gain identity?`
- `how do upgrades create synergy stories?`
- `how do late runs escalate in interesting ways?`
- `what makes a seed worth replaying or sharing?`

## Product Pillars

1. `Arcade feel first`
   Paddle control, bounce behavior, and board readability remain the foundation.
2. `Buildcraft before content sprawl`
   Depth comes from interactions and run identity, not raw feature count.
3. `Deterministic replayability`
   Seeds should support testing, discussion, daily challenges, and future score comparison.
4. `Readable chaos`
   Wild builds are welcome as long as the player can still parse the run.
5. `Single-run strength before social scale`
   Co-op, versus, and party ideas should grow from a proven single-player run loop.

## Technical Direction

- Keep gameplay code under `Assets/Scripts/`
- Continue using ScriptableObjects and additive content data where practical
- Preserve a clear separation between:
  - `RunSettings` for pre-run choices
  - run-state models for active build and progression state
  - temporary effect systems for in-run pickups
- Keep deterministic systems authoritative for:
  - upgrade offers
  - encounter generation
  - score legitimacy
  - daily challenge reproducibility
- Expand data registries and content hooks instead of hard-coding special cases into one controller

## Scope Guardrails

- Do not pivot back toward a giant authored-level campaign as the primary differentiator
- Do not let multiplayer architecture dictate core single-run systems too early
- Do not add dozens of disconnected upgrades before the synergy model exists
- Do not sacrifice gameplay readability for spectacle
- Do not turn meta progression into permanent stat inflation

## Roadmap

- [01 Run Architecture And Upgrade Draft](./01-run-architecture-and-upgrade-draft.md)
- [02 Synergy Engine And Build Rules](./02-synergy-engine-and-build-rules.md)
- [03 Strategic Bricks, Balls, And Hazards](./03-strategic-bricks-balls-and-hazards.md)
- [04 Scoring, Risk, And Reward Systems](./04-scoring-risk-and-reward-systems.md)
- [05 Endless Escalation And Boss Structure](./05-endless-escalation-and-boss-structure.md)
- [06 Daily Seeds, Meta Progression, And Challenges](./06-daily-seeds-meta-progression-and-challenges.md)
- [07 UI, Run Readability, And Presentation](./07-ui-run-readability-and-presentation.md)
- [08 Social Modes, Content Pipeline, And Production Readiness](./08-social-modes-content-pipeline-and-production-readiness.md)

## Sequencing Logic

The new order is intentional:

1. First, define the `run layer` so the game has persistent build identity
2. Then, make upgrade interactions legible and worth chasing
3. Next, add encounter content that can answer those builds
4. After that, deepen mastery with score systems and risk contracts
5. Then, give runs stronger shape through phases, endless play, and bosses
6. Finally, wrap retention, presentation, and social expansion around a proven core

## Open Design Questions

- How many `Core Mod` slots should a run support before the build becomes too noisy?
- Which run systems remain score-valid for future daily challenges, and which become `chaos` or `custom` variants?
- Should bosses gate progress on fixed milestones, seed rules, or score thresholds?
- Is endless mode a branch from the main structured run or a separate start option?
- Which social mode should be the first real extension once single-player run depth is proven?

## Recommended Immediate Focus

If only one chunk is tackled next, it should be [Chunk 01](./01-run-architecture-and-upgrade-draft.md).

The current game already supports seeded progression and modifiers. The biggest missing step is the `between-level upgrade draft`, because it deepens the game from randomization into a run-builder with identity.
