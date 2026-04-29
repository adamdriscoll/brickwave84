# Chunk 05: Multiplayer Ruleset

## Goal

Add the defining twist of Get Bricked: two platforms, multiple players, and multi-ball pressure.

## Systems In Scope

- Two-player input mapping
- Second platform setup and movement boundaries
- Default multi-ball support
- Shared or separate scoring and life rules
- Multiplayer-ready round flow
- Optional special mechanics such as platform switching

## Key Design Decisions To Resolve

- Cooperative or competitive by default
- Shared life pool or per-player life pool
- Whether a ball lost past the top paddle can be recovered by the lower paddle, or vice versa
- Whether power-up effects apply globally, per player, or by catch owner

## Deliverables

- Two controllable platforms in the same playfield
- Stable multi-ball behavior
- Clear failure and victory rules for multiplayer rounds
- A small set of multiplayer test scenarios

## Implementation Notes

- Add multiplayer after the single-player loop is solid so bugs are easier to isolate.
- Revisit camera framing and playfield proportions once two paddles are in place.
- Keep input abstraction flexible enough for keyboard plus controller or controller plus controller setups later.

## Done When

- Two players can play a full level together under clear and understandable rules.
- Multiplayer feels like a designed mode, not a hacked-on duplicate of single-player.

## Explicitly Deferred

- Online networking
- Advanced matchmaking or lobby flow
