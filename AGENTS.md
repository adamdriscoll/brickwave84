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
  - The prototype currently supports one paddle, one or more balls, a score HUD, lives, serve/reset flow between ball losses, authored multi-level progression, temporary level-complete / game-over states, and brick-driven power-up / power-down drops
  - Brick content is now data-driven through ScriptableObject assets, including multi-strength breakable bricks and unbreakable obstacle bricks
  - Power-up content is now data-driven through ScriptableObject assets, with timed paddle-size and ball-speed modifiers plus an instant multi-ball burst effect
  - Chunk 06 groundwork is now in place through a runtime run-setup overlay with seed entry, difficulty presets, modifier validation, deterministic gameplay rolls, and seeded authored-level transforms
  - There are custom C# scripts now, but still no `.asmdef` files, no prefabs, and no automated tests yet
- Input System is enabled and has a starter action asset at `Assets/InputSystem_Actions.inputactions`.

## What Exists

- `Assets/Scenes/SampleScene.unity`: current playable scene and only scene in build settings
- `Assets/InputSystem_Actions.inputactions`: starter input maps for `Player` and `UI`
- `Assets/Scripts/Core/BreakoutBootstrap.cs`: runtime entry point that ensures the prototype controller exists after scene load
- `Assets/Scripts/Gameplay/BreakoutGameController.cs`: builds the prototype playfield, score/lives HUD, runtime brick layout, and multi-level run-state flow
- `Assets/Scripts/Gameplay/PaddleController.cs`: keyboard-driven paddle movement with clamped horizontal bounds and runtime width modifiers
- `Assets/Scripts/Gameplay/BallController.cs`: launch, bounce shaping, per-level speed tuning, speed clamping, and single/multi-ball loss detection support
- `Assets/Scripts/Gameplay/Brick.cs`: definition-driven brick behavior with variable durability and unbreakable support
- `Assets/Scripts/Gameplay/PowerUpPickup.cs`: falling pickup behavior and paddle catch detection
- `Assets/Scripts/Gameplay/DeterministicRandomService.cs`: seed-driven random helper used for gameplay-critical procedural choices
- `Assets/Scripts/Gameplay/Data/BrickDefinition.cs`: ScriptableObject data for brick durability, scoring, completion contribution, colors, and weighted drop tables
- `Assets/Scripts/Gameplay/Data/LevelDefinition.cs`: ScriptableObject data for layout rows, legend mapping, completion rules, and per-level tuning
- `Assets/Scripts/Gameplay/Data/PowerUpDefinition.cs`: ScriptableObject data for pickup effect type, duration, magnitude, and HUD labeling
- `Assets/Scripts/Gameplay/Data/RunSettings.cs`: runtime run configuration model for seed, difficulty, balls-per-serve, modifier multipliers, and drop-pool restrictions
- `Assets/Resources/Bricks/*`: authored brick definition assets loaded at runtime
- `Assets/Resources/Levels/*`: authored level definition assets loaded at runtime
- `Assets/Resources/PowerUps/*`: authored power-up definition assets referenced by brick drop tables
- `Assets/Settings/*`: URP / 2D renderer assets and template scene assets
- `.codex/skills/repo-maintenance/*`: repo-local maintenance skill and snapshot helper for refreshing `AGENTS.md` and local skills after agent work
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
- Level layouts are currently encoded as row strings plus a symbol-to-brick legend, so adding a new level is a data-editing task rather than a code change.
- The seeded variation layer currently transforms authored levels instead of replacing them: per-level plans can mirror layouts and rotate row strings deterministically from the selected run seed.
- Run setup currently lives inside `BreakoutGameController` as a temporary OnGUI overlay instead of a separate menu scene or prefab-based UI.
- Difficulty presets and player-selected modifiers are normalized into `RunSettings`, so future tuning should usually flow through that model instead of adding one-off conditionals.
- Deterministic gameplay randomness currently covers serve launch direction, drop chance / weighted pickup selection, and seeded level-layout transforms.
- `R` now returns to the run-setup overlay; `Space` still launches serves and advances/restarts runs once a configuration has been started.
- Timed pickup effects currently refresh by extending the same effect's duration, while opposing effects coexist and combine multiplicatively.
- Life loss now depends on all active balls leaving play, so multi-ball changes should be reviewed against `BreakoutGameController.HandleBallLost`.
- The chunk-06 `Balls Per Serve` modifier spawns extra balls at every serve, so future ball-loss or serve-flow changes should be checked against `BreakoutGameController.SpawnConfiguredServeBalls`.
- Prototype input is currently read directly from `UnityEngine.InputSystem.Keyboard` rather than being wired through `PlayerInput` or the existing action asset.
- Ball and paddle behavior use Unity 6-era 2D physics APIs such as `Rigidbody2D.linearVelocity`.
- Run flow is still runtime-authored inside `BreakoutGameController`, including lives, serve states, level transitions, level completion, game over, pickup spawning, effect timers, and multi-ball cleanup.
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
- For the current prototype, verify:
  - On boot or after pressing `R`, the run-setup overlay appears with seed, preset, and modifier controls
  - Typing digits changes the run seed, `T` randomizes it, and `Space` starts a run with the shown configuration
  - `Breakout Prototype` appears in the runtime hierarchy automatically
  - `A/D` or left/right arrows move the paddle
  - `Space` launches the ball
  - Replaying the same seed reproduces the same layout mirror/row-shift pattern and deterministic drop/launch rolls
  - Losing the ball removes one life and re-serves from the paddle until lives reach zero
  - Breakable bricks respect their configured hit strength and unbreakable bricks stay in play
  - Run modifiers affect serve ball count, paddle width, ball speed, brick durability, and drop filtering as shown in the setup preview
  - Destroyed breakable bricks can spawn falling pickups, and the paddle can catch or miss them naturally
  - Timed paddle-width and ball-speed effects appear in the HUD and clean themselves up when their timers expire
  - Multi-ball does not consume a life until the last active ball is lost
  - Clearing the current objective reaches the temporary level-complete state and `Space` advances to the next authored level
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
12. `Assets/Resources/Levels/Level01.asset`
13. `Assets/Resources/Bricks/BasicBrick.asset`
14. `Assets/Scenes/SampleScene.unity`
15. `.codex/skills/repo-maintenance/SKILL.md`

## Current Reality Check

- There is now a minimal implemented game loop for a single-screen brick-breaker prototype with authored level data, pickup-driven rule changes, and first-pass chunk-06 seeded-run support.
- The current custom systems are small, but they are real and worth extending deliberately instead of replacing by default.
- Most near-term work will still be greenfield, but it should now build on the existing runtime prototype and folder structure.
- If a user asks for game features, you will likely be extending the current scripts and authored content assets first, then deciding when to promote runtime-generated objects into authored scene or prefab assets.
- If a user asks for more chunk-06 work, the most likely follow-ups are refining fairness validation, promoting the setup overlay into a dedicated UI flow, and expanding the set of deterministic authored-content transforms.
