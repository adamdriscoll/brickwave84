# Chunk 02: Core Progression And Failure States

## Goal

Turn the prototype into a real round-based loop with lives, level completion, and reusable game state control.

## Systems In Scope

- Lives tracking
- Ball loss detection below the paddle
- Round reset behavior
- Level complete handling
- Basic score tracking
- Game state manager for play, win, lose, and transition states

## Deliverables

- Player loses a life when the ball falls below the playfield
- Ball and paddle reset cleanly between attempts
- Level complete state triggers when required bricks are cleared
- Game over triggers when lives reach zero
- Score increases from brick destruction

## Implementation Notes

- Separate per-ball state from overall run state early.
- Mark unbreakable bricks as non-required for level completion.
- Build the logic with future multi-ball in mind even if only one ball exists now.

## Done When

- A full round can be won or lost without manual resets in the editor.
- Lives, score, and completion rules are stable and testable.

## Explicitly Deferred

- Advanced scoring combos
- Multiplayer-specific life rules
- Save progression
