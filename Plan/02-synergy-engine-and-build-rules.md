# Chunk 02: Synergy Engine And Build Rules

## Goal

Make upgrades interact intentionally so the fun comes from `builds`, not just from a pile of isolated modifiers.

## Why This Matters

The overview's strongest idea is not simply "more upgrades." It is that upgrades should create surprising combinations the player can recognize, chase, and talk about.

## Systems In Scope

- Upgrade tags and keywords
- Stack, refresh, replace, and conflict rules
- Synergy detection and surfacing
- Limited-slot `Core Mod` rules
- Build validation for degenerate or unreadable combinations
- First-pass balance matrix for high-risk interactions

## Deliverables

- Upgrade data supports reusable tags such as `Explosive`, `Speed`, `Split`, `Control`, `Risk`, `Pickup`, or `Score`
- The game can detect when a newly offered upgrade amplifies the player's current build
- Draft UI can call out synergy-positive choices instead of presenting them as flat text
- At least `10 to 15` intentionally designed upgrade interactions are implemented or documented for playtesting
- The run system supports both:
  - additive stacking
  - mutually exclusive upgrades
- A balance checklist exists for interactions that can spiral quickly, such as multi-ball plus split effects

## Implementation Notes

- Prefer composable effect application over one-off controller conditionals
- Separate `what an upgrade modifies` from `how it is presented`
- Define clear behavior for duplicate picks:
  - stack numerically
  - refresh duration
  - convert into a stronger tier
  - block as already owned
- Use synergy messaging to help new players discover depth without needing a wiki
- Guardrail rule: excitingly broken is good; unreadable or non-interactive is not

## Done When

- Players can describe a run by its build identity rather than only by its seed
- Upgrade interactions are visible in both rules and UI
- The team can add a new upgrade and know how it should stack, conflict, and advertise synergy

## Explicitly Deferred

- Huge content volume
- Permanent unlock economy
- Full telemetry-driven balance work
