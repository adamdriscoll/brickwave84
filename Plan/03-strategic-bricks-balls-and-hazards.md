# Chunk 03: Strategic Bricks, Balls, And Hazards

## Goal

Expand the board space with brick, ball, and environmental behaviors that force different decisions instead of only increasing durability numbers.

## Why This Matters

The run layer needs content worth building around. New modifiers are strongest when the board itself can answer them with interesting targets and threats.

## Systems In Scope

- New brick behavior families
- New ball behavior variants
- Environmental rule overlays and hazards
- Data hooks for special targeting or influence fields
- Interaction rules between upgrades and board behaviors

## Recommended First Content Families

- `Control Bricks`
  - gravity pull
  - wind influence
  - portal routing
- `Risk / Reward Bricks`
  - score multiplier bricks
  - chain-reaction cores
  - speed-up bricks
- `Tactical Bricks`
  - shielded angle bricks
  - linked response bricks
  - growth or self-repair bricks
- `Ball Variants`
  - phase ball
  - ricochet ball
  - drill ball
  - orbit or curve ball
- `Environmental Modifiers`
  - shifting gravity
  - moving walls
  - shrinking arena
  - fog or visibility pressure

## Deliverables

- At least `3` new brick behavior types beyond basic durability and unbreakable obstacles
- At least `2` meaningful ball behavior variants that can be granted by upgrades or encounters
- At least `1` environment-level rule modifier that changes how a board is played
- The new content hooks into the same data-driven authoring path as the rest of the game where practical
- The team can combine a board behavior and a player build without bespoke per-pair scripting every time

## Implementation Notes

- Favor authored behavior modules or strategy-like handlers over expanding a single giant brick script indefinitely
- When a new behavior is hard to communicate, solve that with stronger presentation before deciding it is a bad mechanic
- Use the theme system to keep hazard and reward language readable
- Prioritize mechanics that create `decision pressure`, not just visual novelty

## Done When

- Later boards ask the player to adapt strategy, not only execution speed
- Upgrades, brick behaviors, and hazards produce recognizable interaction patterns
- New content can be added through a repeatable authoring workflow instead of gameplay-script surgery

## Explicitly Deferred

- Large enemy rosters
- Full boss behavior set
- Highly cinematic set pieces
