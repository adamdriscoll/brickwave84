# Get Bricked Plan

## Overview

Get Bricked starts from the classic brick-breaker loop, then expands it with seeded variation, modifiers, co-op competition, and long-term content growth. The immediate goal is not to build all 50 levels or every power-up at once. The goal is to create a stable, data-driven foundation so we can prove the game is fun early, then scale content without reworking the core systems.

## High-Level Vision

- Core loop: bounce one or more balls with player-controlled platforms to clear breakable bricks while protecting lives.
- Progression: clear a level to advance; difficulty rises through faster balls, tougher bricks, denser layouts, and trickier hazards.
- Identity: two-platform multiplayer, multi-ball pressure, seeded runs, and configurable modifiers make this more than a straight Breakout clone.
- Longevity: support expansion through new brick types, themes, power-ups, modifiers, and level packs or DLC.

## Product Pillars

1. The ball physics and paddle control must feel reliable and satisfying before anything else.
2. The rules should support both authored levels and seeded variation.
3. Content should be data-driven so new bricks, power-ups, and DLC are additive rather than invasive.
4. Multiplayer should be layered on top of a proven single-screen gameplay foundation instead of being the first thing we solve.

## Scope Guardrails For The First Milestones

- Start with local multiplayer only.
- Build a strong single-player vertical slice before full multiplayer rules.
- Use placeholder art and simple UI until the game loop is stable.
- Start with a small power-up and power-down pool, then expand after the drop system is proven.
- Do not author all 50 levels until the level pipeline, seeded variation, and balancing tools are in place.

## Recommended Technical Direction

- Runtime code lives under `Assets/Scripts/`.
- Use ScriptableObjects for data definitions such as brick types, power-up types, modifiers, themes, and level settings.
- Keep gameplay rules separate from content data:
  - `LevelDefinition` or `LevelTemplate` for authored layouts and difficulty settings
  - `RunSettings` for seed, lives, player count, selected modifiers, and ball count
  - `BrickDefinition` for health, score value, visuals, and special rules
  - `PowerUpDefinition` for drop behavior and effect application
- Use a deterministic random service seeded at run start so the same seed can reproduce procedural decisions.
- Plan for DLC by keeping content registries and asset references additive rather than hard-coded.

## Build Strategy

The safest path is to build the game in layers:

1. Prove the paddle, ball, brick collision, lives, and level clear loop.
2. Add data-driven brick health and authored level progression.
3. Add drops and effect systems.
4. Add multiplayer-specific rules and balancing.
5. Add seeded generation and user-facing modifiers.
6. Expand content, menus, polish, and DLC hooks.

## Roadmap

- [01 Foundation And Vertical Slice](./01-foundation-and-vertical-slice.md)
- [02 Core Progression And Failure States](./02-core-progression-and-failure-states.md)
- [03 Brick Content And Level Pipeline](./03-brick-content-and-level-pipeline.md)
- [04 Power-Ups And Power-Downs](./04-powerups-and-powerdowns.md)
- [05 Multiplayer Ruleset](./05-multiplayer-ruleset.md)
- [06 Procedural Seeds And Modifiers](./06-procedural-seeds-and-modifiers.md)
- [07 HUD Menu And Meta Flow](./07-hud-menu-and-meta-flow.md)
- [08 DLC Content Pipeline And Polish](./08-dlc-content-pipeline-and-polish.md)

## Open Design Questions

- Is multiplayer cooperative, competitive, or a mode that can swing between both?
- Do both players share lives, or does each platform own its own failures and recovery rules?
- Are the 50 starting levels fully authored, fully procedural, or hybrid templates with seeded variation?
- Should some modifiers disable leaderboard or progression tracking later?
- How chaotic should multi-ball become by default in multiplayer?

## Suggested Next Build Target

Start with Chunk 01 and build a playable single-screen prototype with:

- one controllable paddle
- one ball
- one simple brick grid
- brick destruction
- bounce tuning
- a temporary win/reset flow

That gives us a feel check quickly and keeps later decisions grounded in actual play rather than theory.
