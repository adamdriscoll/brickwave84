# Chunk 03: Brick Content And Level Pipeline

## Goal

Make bricks and levels data-driven so content can scale cleanly from a prototype to a 50-level release.

## Systems In Scope

- Brick definitions
- Multi-strength brick behavior
- Unbreakable brick behavior
- Level data model
- Level loading or spawning pipeline
- Early content authoring workflow

## Deliverables

- Generic brick system with variable hit strength
- Unbreakable obstacle brick
- A level format that can define layout, difficulty tuning, and completion rules
- At least 3 to 5 test levels that demonstrate variety

## Implementation Notes

- Use ScriptableObjects or similar authoring data to avoid hard-coded brick rules.
- Keep brick visuals and brick behavior configurable separately where practical.
- Delay large-scale level authoring until the workflow is comfortable.
- Consider a hybrid level approach:
  - authored layout template
  - seed-driven variation pass for selected elements

## Done When

- New brick strengths or brick types can be added without rewriting the core hit logic.
- Designers can add a new level by editing data rather than code.

## Explicitly Deferred

- Full 50-level content pass
- DLC packaging
- Theme-specific art polish
