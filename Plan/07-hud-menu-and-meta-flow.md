# Chunk 07: HUD Menu And Meta Flow

## Goal

Give players a clean way to configure, understand, and move through the game.

## Systems In Scope

- Main menu
- Run setup or options screen
- In-game HUD
- Pause flow
- Win and loss screens
- Basic settings persistence

## HUD Candidates

- Score
- Lives
- Level number
- Balls in play
- Active effects
- Seed

## Deliverables

- A start flow that lets players choose core settings before gameplay
- A readable in-game HUD for the main status items
- Pause, restart, and return-to-menu actions
- End-of-level and game-over feedback

## Implementation Notes

- Keep UI functional and readable first; style can mature later.
- Plan the run setup screen around future modifiers and multiplayer options.
- If settings are persisted, keep the save format simple and versionable.

## Done When

- A player can launch the game, configure a run, play, pause, finish, and restart without editor interaction.

## Explicitly Deferred

- Final art direction
- Accessibility deep pass
- Full onboarding tutorial
