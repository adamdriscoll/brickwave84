# Chunk 05: Endless Escalation And Boss Structure

## Goal

Give runs a stronger dramatic arc through escalation phases, special encounter spikes, and at least one boss-style board.

## Why This Matters

A roguelite run needs shape. The player should feel a run opening up, becoming unstable, and eventually tipping into either mastery or chaos.

## Systems In Scope

- Phase-based escalation rules
- Endless-mode framework
- Boss encounter structure
- Special wave or event boards
- Transitional rules between standard encounters and escalation moments

## Recommended Escalation Shape

1. `Opening`
   Standard readable boards that let the build take shape
2. `Pressure`
   Moving bricks, harsher drop mixes, or denser layouts
3. `Distortion`
   Environmental modifiers, board hazards, or stronger synergy asks
4. `Chaos`
   Rapid rule layering, high-speed scoring pressure, or forced adaptation

## Deliverables

- A reusable framework for encounter phases or tiers
- At least `3` escalation rules that meaningfully change how later encounters feel
- At least `1` boss implementation with:
  - a clear gimmick
  - readable attack windows or weak points
  - a reason to exist besides being a larger wall of health
- An endless or post-arc continuation mode that keeps stacking difficulty after the structured route is complete

## Implementation Notes

- Escalation should add new decision pressure before it adds raw stat inflation
- Bosses should reuse brick-breaker language where possible:
  - armor layers
  - exposed cores
  - reflected trajectories
  - hazard-spawning patterns
- Endless mode should feel like a distinct `how long can this build survive?` question, not just a missing game-over trigger

## Done When

- Runs have a clear mid-game and late-game identity
- Special encounters break up the rhythm without feeling like a different game
- Endless mode gives overpowered builds somewhere to go

## Explicitly Deferred

- Large boss roster
- Cutscene-heavy presentation
- Final campaign structure
