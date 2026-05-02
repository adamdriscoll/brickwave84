# Chunk 09: Gameplay Mode Shell And Settings

## Goal

Give the cabinet a stable top-level menu structure before every mode is implemented, so future work can plug into clear user-facing lanes instead of piling options into one generic start flow.

## Home Menu Taxonomy

### Singleplayer

- `Rogue`
  - New primary run style for progression, unlocks, intensities, heat, and long-term arcade goals.
  - See [Chunk 10](./10-rogue-mode-gameplay-plan.md) for the concrete 10-stage run, boss, intensity, and paddle progression plan.
- `Custom Game`
  - Current playable seeded/configurable run setup.
  - Owns Tape ID editing, score mode, balls per serve, modifier bias, drop pool, Capsule Party, and theme selection.

### Multiplayer

- `Dual Sticks`
  - Side-by-side versus action.
  - Players race to clear their bricks first, with future negative drops and extra bricks sent to the opponent.
  - Placeholder shell for now.
- `Co-op`
  - Side-by-side cooperative play with two paddles, two balls, and one keyboard.
  - Similar to Custom Game, but tuned for two local players.
  - Placeholder shell for now.
- `Turn-Based`
  - Custom Game-style run where players switch on lives and level transitions.
  - Placeholder shell for now.

### Settings

- `Sound`
  - Future home for master, music, and sound effect volume.
  - Placeholder shell for now.
- `Graphics`
  - Future home for display, bloom/glow, scanline, readability, and other presentation settings.
  - Placeholder shell for now.

## Implementation Notes

- Keep `Custom Game` as the current functional gameplay path until `Rogue` has a real meta-progression model.
- Mode placeholders should still have descriptive previews so the menu teaches the intended product shape.
- Avoid letting multiplayer branch the core controller too early; prefer shared services and mode-specific orchestration once the first social prototype starts.
- Settings shells should be real menu destinations before sliders exist, so audio and graphics work can land without rearranging the home menu again.

## Scene Direction

The project currently uses one intentionally light Unity scene and creates the playable runtime through `BreakoutBootstrap`. That is still a good baseline while the game is mostly runtime-generated.

Start thinking about scenes when one of these becomes true:

- Menu, settings, or progression screens need authored UI objects, animation timelines, or persistent visual setup that is awkward in `OnGUI`.
- Gameplay modes need materially different camera, lighting, input, or object lifecycle roots.
- Local multiplayer needs stable authored split-screen or side-by-side layout anchors.
- Boot/loading/persistence work needs a durable app root that survives mode changes.

Recommended future shape:

- `Boot` scene: persistent app services, audio mixer, save/profile services, and scene loading.
- `MainMenu` scene: authored home menu, settings shell, mode previews, and progression surface.
- `GameplaySingle` scene: current one-paddle runtime game for Custom Game and early Rogue.
- `GameplayLocalMulti` scene: future side-by-side or shared-screen multiplayer prototypes.

Do not split scenes immediately just to organize code. First extract runtime responsibilities into mode/session services, then move authored presentation into scenes when the lifecycle boundaries are clear.

## Done When

- The home menu clearly communicates the Singleplayer, Multiplayer, and Settings lanes.
- `Custom Game` remains fully playable through the existing run setup.
- Placeholder selections make their future purpose explicit without pretending the modes are implemented.
- Future sessions can pick a mode lane and add behavior behind it without renaming or reshaping the menu.
