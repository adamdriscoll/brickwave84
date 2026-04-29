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
  - The prototype currently supports one paddle, one ball, a brick wall, score HUD, lives, serve/reset flow between ball losses, and temporary level-complete / game-over states
  - There are custom C# scripts now, but still no `.asmdef` files, no prefabs, and no automated tests yet
- Input System is enabled and has a starter action asset at `Assets/InputSystem_Actions.inputactions`.

## What Exists

- `Assets/Scenes/SampleScene.unity`: current playable scene and only scene in build settings
- `Assets/InputSystem_Actions.inputactions`: starter input maps for `Player` and `UI`
- `Assets/Scripts/Core/BreakoutBootstrap.cs`: runtime entry point that ensures the prototype controller exists after scene load
- `Assets/Scripts/Gameplay/BreakoutGameController.cs`: builds the prototype playfield, score/lives HUD, brick wall, and run-state flow at runtime
- `Assets/Scripts/Gameplay/PaddleController.cs`: keyboard-driven paddle movement with clamped horizontal bounds
- `Assets/Scripts/Gameplay/BallController.cs`: launch, bounce shaping, speed clamping, and loss detection for the prototype ball
- `Assets/Scripts/Gameplay/Brick.cs`: simple breakable brick behavior
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
- Prototype input is currently read directly from `UnityEngine.InputSystem.Keyboard` rather than being wired through `PlayerInput` or the existing action asset.
- Ball and paddle behavior use Unity 6-era 2D physics APIs such as `Rigidbody2D.linearVelocity`.
- Run flow is still runtime-authored inside `BreakoutGameController`, including lives, serve states, level completion, and game over.
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

## Validation Checklist

When making changes, validate with the Unity editor when possible:

- Open the project in Unity `6000.3.6f1` or the closest compatible version.
- Confirm the scene loads without missing scripts or broken references.
- Check the Console for compile errors after adding scripts.
- If gameplay changes are made, enter Play Mode in `Assets/Scenes/SampleScene.unity`.
- For the current prototype, verify:
  - `Breakout Prototype` appears in the runtime hierarchy automatically
  - `A/D` or left/right arrows move the paddle
  - `Space` launches the ball
  - Losing the ball removes one life and re-serves from the paddle until lives reach zero
  - Bricks are destroyed on hit
  - Clearing all required bricks reaches the temporary level-complete state
  - `R` restarts the full run
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
8. `Assets/Scenes/SampleScene.unity`
9. `.codex/skills/repo-maintenance/SKILL.md`

## Current Reality Check

- There is now a minimal implemented game loop for a single-screen brick-breaker prototype.
- The current custom systems are small, but they are real and worth extending deliberately instead of replacing by default.
- Most near-term work will still be greenfield, but it should now build on the existing runtime prototype and folder structure.
- If a user asks for game features, you will likely be extending the current scripts first, then deciding when to promote runtime-generated objects into authored scene or prefab assets.
