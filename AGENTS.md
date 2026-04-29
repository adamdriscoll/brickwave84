# AGENTS.md

## Project Snapshot

- This repository is a very early-stage Unity project named `Get Bricked`.
- This folder is now tracked in git.
- Unity editor version: `6000.3.6f1`.
- Render pipeline: Universal Render Pipeline with 2D renderer assets already configured.
- Current gameplay state is a first playable runtime prototype:
  - One enabled build scene: `Assets/Scenes/SampleScene.unity`
  - The serialized scene asset is still close to the template and only contains the default `Main Camera` and `Global Light 2D`
  - A runtime bootstrap now injects the playable prototype into the scene on load
  - The prototype currently supports a runtime main menu, run setup flow, in-game HUD, pause / restart / return-to-menu actions, one paddle, one or more balls, lives, serve/reset flow between ball losses, authored multi-level progression, temporary level-complete / game-over states, and brick-driven power-up / power-down drops
  - Brick content is now data-driven through ScriptableObject assets, including multi-strength breakable bricks and unbreakable obstacle bricks
  - Power-up content is now data-driven through ScriptableObject assets, with timed paddle-size and ball-speed modifiers plus an instant multi-ball burst effect
  - Runtime visuals now support data-driven theme selection, with palette-based themes for background, walls, paddle, bricks, power-up pickups, and ball plus sprite hooks reserved for future art passes
  - Chunk 06 and chunk 07 groundwork are now in place through runtime OnGUI overlays for main menu, run setup, HUD, pause, end-state flow, persisted run-setup choices, seed entry, difficulty presets, modifier validation, deterministic gameplay rolls, and seeded authored-level transforms
  - There are custom C# scripts now, but still no `.asmdef` files, no prefabs, and no automated tests yet
- Input System is enabled and has a starter action asset at `Assets/InputSystem_Actions.inputactions`.

## What Exists

- `Assets/Scenes/SampleScene.unity`: current playable scene and only scene in build settings
- `Assets/InputSystem_Actions.inputactions`: starter input maps for `Player` and `UI`
- `Assets/Scripts/Core/BreakoutBootstrap.cs`: runtime entry point that ensures the prototype controller exists after scene load
- `Assets/Scripts/Gameplay/BreakoutGameController.cs`: builds the prototype playfield, runtime main menu/setup/HUD overlays, run-setup persistence, pause flow, runtime brick layout, and multi-level run-state flow
- `Assets/Scripts/Gameplay/PaddleController.cs`: keyboard-driven paddle movement with clamped horizontal bounds and runtime width modifiers
- `Assets/Scripts/Gameplay/BallController.cs`: launch, bounce shaping, per-level speed tuning, speed clamping, and single/multi-ball loss detection support
- `Assets/Scripts/Gameplay/Brick.cs`: definition-driven brick behavior with variable durability and unbreakable support
- `Assets/Scripts/Gameplay/PowerUpPickup.cs`: falling pickup behavior and paddle catch detection
- `Assets/Scripts/Gameplay/DeterministicRandomService.cs`: seed-driven random helper used for gameplay-critical procedural choices
- `Assets/Scripts/Gameplay/Data/BrickDefinition.cs`: ScriptableObject data for brick durability, scoring, completion contribution, colors, and weighted drop tables
- `Assets/Scripts/Gameplay/Data/LevelDefinition.cs`: ScriptableObject data for layout rows, legend mapping, completion rules, and per-level tuning
- `Assets/Scripts/Gameplay/Data/PowerUpDefinition.cs`: ScriptableObject data for pickup effect type, duration, magnitude, and HUD labeling
- `Assets/Scripts/Gameplay/Data/RunSettings.cs`: runtime run configuration model for seed, difficulty, balls-per-serve, modifier multipliers, and drop-pool restrictions
- `Assets/Scripts/Gameplay/Data/ThemeDefinition.cs`: ScriptableObject theme data for semantic visual slots, palette colors, and future sprite overrides
- `Assets/Resources/Bricks/*`: authored brick definition assets loaded at runtime
- `Assets/Resources/Levels/*`: authored level definition assets loaded at runtime
- `Assets/Resources/PowerUps/*`: authored power-up definition assets referenced by brick drop tables
- `Assets/Resources/Themes/*`: authored runtime theme assets loaded by run setup and applied across gameplay visuals
- `Assets/Settings/*`: URP / 2D renderer assets and template scene assets
- `.codex/skills/repo-maintenance/*`: repo-local maintenance skill and snapshot helper for refreshing `AGENTS.md` and local skills after agent work
- `.codex/skills/unity-compile/*`: repo-local Unity batchmode compile-check skill and helper script for reproducing script compilation failures from the terminal
- `Packages/manifest.json`: Unity package dependencies
- `ProjectSettings/ProjectVersion.txt`: authoritative Unity version
- `Get Bricked.sln`: may stay sparse until Unity regenerates project files after script import

## Key Technical Notes

- This is a Unity 6 project, so future edits should assume modern Unity APIs and URP defaults.
- The package list shows a 2D-focused setup plus the new Input System, UGUI, Timeline, and Visual Scripting.
- The input action asset already includes common starter actions like `Move`, `Look`, `Attack`, `Interact`, `Jump`, `Sprint`, `Previous`, and `Next`.
- The current prototype is scene-light and code-heavy: the gameplay board, bounds, ball, paddle, bricks, and temporary HUD are created at runtime instead of being serialized into `SampleScene`.
- Bricks and levels are now authored as ScriptableObjects under `Assets/Resources/` and loaded by `BreakoutGameController` at runtime.
- Brick definitions now own drop chance plus weighted pickup references, so most drop-table tuning is an asset edit rather than a controller edit.
- Theme definitions now own semantic visual-slot palettes under `Assets/Resources/Themes/`; the current implementation uses colors, but the theme pipeline already supports per-slot sprite overrides for future art passes.
- Level layouts are currently encoded as row strings plus a symbol-to-brick legend, so adding a new level is a data-editing task rather than a code change.
- The seeded variation layer currently transforms authored levels instead of replacing them: per-level plans can mirror layouts and rotate row strings deterministically from the selected run seed.
- The game now boots into a runtime main menu instead of straight into gameplay or setup, and the last run-setup selections are persisted through `PlayerPrefs`.
- The selected theme is part of the persisted run setup, so quick-starting from the main menu reuses the most recently chosen palette.
- Main menu, run setup, HUD, pause, and end-of-run flow currently live inside `BreakoutGameController` as temporary OnGUI overlays instead of a separate menu scene or prefab-based UI.
- Difficulty presets and player-selected modifiers are normalized into `RunSettings`, so future tuning should usually flow through that model instead of adding one-off conditionals.
- Deterministic gameplay randomness currently covers serve launch direction, drop chance / weighted pickup selection, and seeded level-layout transforms.
- `R` returns to the run-setup overlay, `Esc` / `P` pauses active gameplay, and `Space` launches serves or confirms overlay actions once a configuration has been started.
- Timed pickup effects currently refresh by extending the same effect's duration, while opposing effects coexist and combine multiplicatively.
- Life loss now depends on all active balls leaving play, so multi-ball changes should be reviewed against `BreakoutGameController.HandleBallLost`.
- The chunk-06 `Balls Per Serve` modifier spawns extra balls at every serve, so future ball-loss or serve-flow changes should be checked against `BreakoutGameController.SpawnConfiguredServeBalls`.
- Prototype input is currently read directly from `UnityEngine.InputSystem.Keyboard` rather than being wired through `PlayerInput` or the existing action asset.
- Ball and paddle behavior use Unity 6-era 2D physics APIs such as `Rigidbody2D.linearVelocity`.
- Run flow is still runtime-authored inside `BreakoutGameController`, including lives, serve states, level transitions, level completion, game over, pickup spawning, effect timers, and multi-ball cleanup.
- Brick and power-up definition assets can optionally override their semantic theme slot, but default routing already maps brick durability tiers plus beneficial/harmful/burst pickups onto the shared theme palette automatically.
- Terminal-side compile validation can now be done with `python .codex/skills/unity-compile/scripts/run_unity_compile.py`, which reads `ProjectVersion.txt`, locates the matching Unity Hub editor, runs batchmode, and summarizes build-blocking script errors from the generated log.
- After adding or renaming scripts, let Unity regenerate project files instead of hand-maintaining the `.sln`.

## Working Rules For Future Agents

- Prefer adding gameplay code under `Assets/Scripts/` unless the user asks for a different layout.
- Preserve Unity `.meta` pairings for every manually added script and folder under `Assets/Scripts/`.
- After substantial project work, use `.codex/skills/repo-maintenance/` to refresh `AGENTS.md` and repo-local skills with durable new repo knowledge.
- Keep Unity `.meta` files intact. If you add an asset or script manually, ensure the matching `.meta` file exists and stays paired with it.
- Do not edit or rely on generated folders for durable changes:
  - `Library/`
  - `Logs/`
  - `Temp/`
  - `UserSettings/`
- Be cautious editing `.unity`, `.prefab`, or other YAML asset files by hand. Small targeted edits are fine, but large structural changes are safer in the Unity Editor because GUID/reference breakage is easy.
- If you move or rename assets manually, remember that Unity references them by GUID from the `.meta` files.
- This folder now has a git repository, so prefer `git status`, `git diff`, and recent commit history to understand agent work. Do not rewrite history or revert unrelated user changes unless explicitly asked.
- If you extend the current prototype, check whether the change belongs in the runtime bootstrap path or whether it is time to migrate pieces into authored scene objects or prefabs.

## Suggested Conventions

- Create new runtime code in `Assets/Scripts/`.
- If the project grows, split code by domain early, for example:
  - `Assets/Scripts/Gameplay/`
  - `Assets/Scripts/Input/`
  - `Assets/Scripts/UI/`
  - `Assets/Scripts/Core/`
- Keep first-pass gameplay tuning values serialized on the controlling MonoBehaviour so feel can be adjusted quickly in the Inspector during playtesting.
- Add prefabs under `Assets/Prefabs/`, art under `Assets/Art/`, and audio under `Assets/Audio/` if those areas are created later.
- If tests are added, prefer Unity Test Framework with clear separation between Edit Mode and Play Mode tests.
- If you add more authored gameplay content, keep using `.asset` plus `.meta` pairings under `Assets/Resources/Bricks/`, `Assets/Resources/Levels/`, and `Assets/Resources/PowerUps/` unless the project deliberately migrates to a different content-loading path.

## Validation Checklist

When making changes, validate with the Unity editor when possible:

- Open the project in Unity `6000.3.6f1` or the closest compatible version.
- Confirm the scene loads without missing scripts or broken references.
- Check the Console for compile errors after adding scripts.
- If gameplay changes are made, enter Play Mode in `Assets/Scenes/SampleScene.unity`.
- Before or after gameplay script edits, prefer running `python .codex/skills/unity-compile/scripts/run_unity_compile.py` from the repo root for a fast terminal compile check when Unity UI validation is not yet practical.
- For the current prototype, verify:
  - On boot, the runtime main menu appears and can start a run or open run setup without editor interaction
  - From run setup, seed, preset, and modifier controls still respond correctly and `Esc` returns to the main menu
  - Typing digits changes the run seed, `T` randomizes it, and `Space` starts a run with the shown configuration
  - Returning to the main menu and reopening run setup preserves the last chosen setup values across that session, and restarting the app restores them
  - Run setup theme selection responds to left/right input, persists across sessions, and the selected palette recolors the background, paddle, bricks, pickups, and ball on the next run
  - `Breakout Prototype` appears in the runtime hierarchy automatically
  - `A/D` or left/right arrows move the paddle
  - `Space` launches the ball
  - `Esc` or `P` pauses active gameplay and exposes resume, restart, setup, and main-menu actions
  - Replaying the same seed reproduces the same layout mirror/row-shift pattern and deterministic drop/launch rolls
  - Losing the ball removes one life and re-serves from the paddle until lives reach zero
  - Breakable bricks respect their configured hit strength and unbreakable bricks stay in play
  - Run modifiers affect serve ball count, paddle width, ball speed, brick durability, and drop filtering as shown in the setup preview
  - Destroyed breakable bricks can spawn falling pickups, and the paddle can catch or miss them naturally
  - Timed paddle-width and ball-speed effects appear in the HUD and clean themselves up when their timers expire
  - Multi-ball does not consume a life until the last active ball is lost
  - Clearing the current objective reaches the temporary level-complete state and the overlay menu can advance, restart, or return to the main menu
  - Game over exposes restart, setup, and main-menu actions
  - `R` returns to run setup without leaving orphaned runtime balls or pickups behind
- If build configuration changes are made, re-check `ProjectSettings/EditorBuildSettings.asset`.

## Good First Read Files

If you are a future agent starting work here, read these first:

1. `AGENTS.md`
2. `ProjectSettings/ProjectVersion.txt`
3. `Packages/manifest.json`
4. `ProjectSettings/EditorBuildSettings.asset`
5. `Assets/InputSystem_Actions.inputactions`
6. `Assets/Scripts/Core/BreakoutBootstrap.cs`
7. `Assets/Scripts/Gameplay/BreakoutGameController.cs`
8. `Assets/Scripts/Gameplay/Data/RunSettings.cs`
9. `Assets/Scripts/Gameplay/DeterministicRandomService.cs`
10. `Assets/Scripts/Gameplay/Data/LevelDefinition.cs`
11. `Assets/Scripts/Gameplay/Data/PowerUpDefinition.cs`
12. `Assets/Scripts/Gameplay/Data/ThemeDefinition.cs`
13. `Assets/Resources/Levels/Level01.asset`
14. `Assets/Resources/Bricks/BasicBrick.asset`
15. `Assets/Resources/Themes/ClassicTheme.asset`
16. `Assets/Scenes/SampleScene.unity`
17. `.codex/skills/repo-maintenance/SKILL.md`

## Current Reality Check

- There is now a minimal implemented game loop for a single-screen brick-breaker prototype with authored level data, pickup-driven rule changes, seeded-run support, palette-based theme selection, and a first-pass chunk-07 player-facing menu / HUD / pause flow.
- The current custom systems are small, but they are real and worth extending deliberately instead of replacing by default.
- Most near-term work will still be greenfield, but it should now build on the existing runtime prototype and folder structure.
- If a user asks for game features, you will likely be extending the current scripts and authored content assets first, then deciding when to promote runtime-generated objects into authored scene or prefab assets.
- If a user asks for more chunk-07 work, the most likely follow-ups are promoting the runtime OnGUI overlays into authored UI/prefabs, separating meta-flow concerns out of `BreakoutGameController`, and broadening persisted settings beyond run setup.
