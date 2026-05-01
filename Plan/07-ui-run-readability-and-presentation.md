# Chunk 07: UI, Run Readability, And Presentation

## Goal

Make the new run systems understandable and exciting through better UX, stronger status communication, and presentation that matches the synthwave arcade direction.

## Why This Matters

The roadmap is moving toward richer builds, score rules, and escalation states. If players cannot read the current run at a glance, the new depth will feel noisy instead of compelling.

## Systems In Scope

- Upgrade draft presentation
- Build summary and synergy callouts
- HUD expansion for combo, score pressure, and phase state
- Boss warnings and special-event messaging
- Run-end recap screens
- Longer-term path away from temporary debug-feeling overlays

## Deliverables

- A readable between-level draft screen
- HUD support for the most important new run systems:
  - active build identity
  - combo or score pressure
  - phase or encounter state
  - important temporary effects
- Stronger run-end summary screens that explain the build, the seed, and the score
- UI direction that follows `Plan/STYLE.md` instead of staying in placeholder mode
- A plan for when to keep iterating the current runtime UI path versus migrating pieces into authored Unity UI assets or prefabs

## Implementation Notes

- Readability outranks spectacle during active play
- Avoid forcing all system detail onto the HUD at once
- Use the theme slot system and style guide to keep reward, danger, and neutral language consistent
- Build around concise player questions:
  - What build am I running?
  - Why did this upgrade light up as a good pick?
  - Why is the board harder right now?
  - Where did this score come from?

## Done When

- A new player can understand the state of a run without reading design notes
- The game feels closer to a polished release and less like an internal debug build
- The upgraded UX can support more content without collapsing into clutter

## Explicitly Deferred

- Final trailer-quality presentation polish
- Full accessibility feature pass
- Platform-specific certification work
