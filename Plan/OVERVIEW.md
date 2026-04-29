# Get Bricked Overview

## Product Reframe

`Get Bricked` should no longer be planned as a straight brick-breaker that gradually adds more levels, more bricks, and eventually multiplayer. The current prototype has already moved past that starting line.

The stronger direction is:

- a `run-based arcade roguelite`
- built on readable Breakout fundamentals
- driven by `seeded replayability`
- elevated by `upgrade drafts`, `synergies`, and `controlled chaos`

The central player question shifts from:

`Can I clear the next level?`

to:

`What kind of broken, risky, or stylish build can I assemble this run?`

## What The Current Prototype Already Proves

The current build already gives us a real foundation for this direction:

- runtime run setup and main-menu flow
- deterministic seed handling
- procedural multi-level progression
- data-driven bricks, levels, power-ups, and themes
- temporary modifier effects and drop logic
- a playable life-loss, serve, pause, restart, and game-over loop

That means the roadmap should stop treating the game as a blank-slate prototype and instead focus on `run identity`, `build depth`, and `replay structure`.

## Core Experience

Each run should feel like a fast arcade climb that keeps asking the player to make bolder choices.

The intended loop is:

1. Start a run from a chosen or generated seed
2. Clear a board or encounter
3. Pick `1 of 3` upgrades
4. Stack synergies, risks, and rule changes across the run
5. Survive escalating boards, hazards, or boss moments
6. End the run with a score, summary, and future challenge hook

The run should become progressively less stable in a fun way. Early play is readable and controlled. Mid-run play becomes expressive. Late-run play can become gloriously chaotic as long as readability survives.

## Design Pillars

### 1. Reliable Arcade Feel

The paddle, ball, bounce logic, and collision readability remain the non-negotiable base. No amount of upgrades or spectacle can compensate for weak feel.

### 2. Buildcraft Over Content Volume

The game gets depth primarily from `interactions`, not from shipping a huge count of isolated features. A smaller pool of well-connected upgrades is better than a larger pool of disconnected gimmicks.

### 3. Readable Chaos

Runs can become wild, but the player should still understand:

- what their build is doing
- why the run became harder
- which rewards are helping
- which risks they chose on purpose

### 4. Deterministic Replayability

Seeds are a design asset, not just a debugging convenience. The same seed should make challenge runs, daily runs, and comparison play possible.

### 5. Expandable Social Energy

Social or multiplayer modes are still valuable, but they should grow out of a strong single-run game instead of defining the project too early.

## System Layers

### Run Layer

This is the backbone of the new direction:

- seeded run start
- encounter progression
- between-level upgrade drafts
- active build tracking
- run summary and outcome tracking

### Upgrade Layer

This is where build identity lives:

- persistent run upgrades
- limited-slot `Core Mods`
- stackable `Run Mods`
- temporary pickup effects
- synergy tags, conflicts, and combo hooks

### Encounter Layer

This is how the boards push back:

- strategic brick behaviors
- weird ball behaviors
- environmental pressure
- phase-based escalation
- boss-style encounters

### Mastery Layer

This is what keeps runs replayable:

- score expression
- opt-in risk for reward
- daily seeds
- challenge presets
- local bests and unlockable variety

## Content Direction

The next content should prioritize mechanics that change decisions.

High-value examples:

- bricks that pull, redirect, grow, link, shield, or explode
- balls that phase, drill, curve, split, or overclock
- upgrades that intentionally create synergy stories
- hazards that change the board without requiring huge art scope
- score systems that reward speed, danger, precision, and clutch recovery

## Naming And Identity

Upgrade names, hazard names, and boss names should support the arcade-roguelite fantasy. Even simple mechanics will feel more memorable if they are framed as part of a coherent build language.

Examples of the desired tone:

- `Overclock`
- `Chain Reactor`
- `Singularity Core`
- `Ghostline`
- `Glass Cannon`

## Near-Term Priorities

If we prioritize only a handful of features, these should lead:

1. Between-level upgrade drafts
2. Synergy-aware modifier rules
3. A few standout ball and brick behaviors
4. Escalating run phases and endless continuation
5. Daily seeded challenge structure

That combination best matches the current codebase and gives the game a stronger identity without requiring a massive art or multiplayer scope jump.

## Near-Term Non-Goals

For now, do not let the roadmap drift toward:

- a giant authored level campaign
- permanent stat grinding
- full online infrastructure
- social modes before the single-player run loop sings
- content quantity that outpaces system clarity
