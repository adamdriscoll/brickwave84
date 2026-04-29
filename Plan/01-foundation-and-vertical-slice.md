# Chunk 01: Foundation And Vertical Slice

## Goal

Create the first playable prototype that proves the game feels good at the most basic level.

## Systems In Scope

- Project folder structure under `Assets/Scripts/`
- Basic gameplay scene setup
- Paddle movement
- Ball launch and bounce behavior
- Brick collision and destruction
- Temporary score or debug counters
- Temporary win/reset loop

## Deliverables

- A controllable paddle with movement limits
- A ball that can be launched and remains in play through stable bounces
- A small test wall of breakable bricks
- Bricks destroy on hit
- The level resets or reports success when all breakable bricks are cleared

## Implementation Notes

- Keep the first pass single-player and single-ball.
- Prefer simple, inspectable scripts over premature architecture.
- Separate tuning values into serialized fields so feel can be adjusted quickly.
- Record immediate questions about physics feel, bounce angles, and screen bounds.

## Done When

- A player can start the scene and clear a basic brick layout from start to finish.
- Control and bounce behavior feel predictable enough to tune rather than rewrite.

## Explicitly Deferred

- Multiplayer
- Power-ups
- Procedural generation
- Final UI
- Complex brick behaviors
