# AGENTS.md

## Project Snapshot

- `Get Bricked` is an early-stage Unity 6 brick-breaker / arcade roguelite prototype.
- Unity editor version: `6000.3.6f1`.
- Render pipeline: URP with 2D renderer assets.
- One enabled build scene: `Assets/Scenes/SampleScene.unity`.
- The scene stays intentionally light: `Assets/Scripts/Core/BreakoutBootstrap.cs` injects the playable runtime prototype on load.
- The project is tracked in git. Do not rewrite history or revert unrelated user changes.
- There are no `.asmdef` files and no prefabs yet; runtime objects are currently created from code.

## Current Gameplay Shape

- Runtime flow includes main menu, run setup, in-game HUD, pause, game-over flow, level clear, and deterministic `pick 1 of 3` upgrade drafts.
- Runs support seeded procedural levels, lives, high-score mode, one or more balls per serve, power-up drops, timed effects, permanent run upgrades, theme selection, and persisted setup choices through `PlayerPrefs`.
- Bricks, levels, power-ups, run upgrades, and themes are data-driven ScriptableObjects under `Assets/Resources/`.
- Gameplay art uses SVG sprites from `Assets/Resources/Sprites/`, theme palettes from `Assets/Resources/Themes/`, and level-rotated backgrounds from `Assets/Resources/Backgrounds/`.
- The runtime presentation path uses URP bloom plus additive sprites for the ball and pickups; paddle, bricks, walls, and backdrop stay on unlit sprite materials for readability.
- Input currently reads directly from `UnityEngine.InputSystem.Keyboard`; `Assets/InputSystem_Actions.inputactions` exists but is not yet the runtime input layer.

## Architecture Map

- `Assets/Scripts/Gameplay/BreakoutGameController.cs` is the orchestration root for run flow, serve/life transitions, scene wiring, and cross-system coordination. Keep shrinking it when a responsibility becomes coherent enough to extract.
- `Assets/Scripts/Gameplay/Actors/` contains MonoBehaviours for paddle, balls, bricks, and pickups.
- `Assets/Scripts/Gameplay/Data/` contains ScriptableObject definitions and runtime settings models.
- `Assets/Scripts/Gameplay/Levels/BreakoutLevelPlanner.cs` owns deterministic procedural layout planning.
- `Assets/Scripts/Gameplay/PowerUps/` owns pickup spawning, timed effects, direct-damage targeting, and power-up modifier calculations.
- `Assets/Scripts/Gameplay/RunFlow/` owns run setup state, persistence, active run upgrade state, and draft generation.
- `Assets/Scripts/Gameplay/Scoring/` owns scoring rules and floating score popup state behind `IBreakoutScoreService`.
- `Assets/Scripts/Gameplay/Presentation/` owns theme application, OnGUI rendering, glow helpers, background presentation, runtime visual factories, and UI view models.
- `Assets/Tests/Editor/` contains the current Edit Mode regression suite.

## Refactoring Rules

- Prefer small, focused collaborators over growing `BreakoutGameController` or other large files.
- Keep MonoBehaviours focused on Unity callbacks, serialized tuning, scene object lifetime, and component wiring. Put deterministic rules and calculations in plain C# services where practical.
- Add interfaces only when they create a real seam for multiple implementations, strategy swapping, or meaningful test isolation.
- Preserve Unity `.meta` files for every manually added asset, folder, or script.
- Do not introduce `.asmdef` files casually. When assemblies become worthwhile, move a coherent folder slice and its tests together.
- Use ScriptableObjects for authored configuration. Keep run/session state in runtime models or services, not written back to assets.
- Use Unity 6 / modern 2D physics APIs such as `Rigidbody2D.linearVelocity`.
- For C# style, follow the codebase first: clear names, small cohesive methods, narrow dependencies, explicit null checks where useful, and no unnecessary framework ceremony.

## Visual And Copy Rules

- Read `Plan/STYLE.md` before UI, theme, VFX, shader, environment, art, or presentation changes.
- Read `Plan/VOICE.md` before adding or revising UI labels, names, callouts, pickup text, stage text, or result text.
- Visual target: readability-first 1980s synthwave arcade cabinet, with dark indigo/violet bases, magenta/cyan neon accents, warm highlights, restrained CRT/cabinet cues, and selective glow.
- Gameplay readability wins over decoration. The ball, paddle, brick durability, pickup polarity, and active/paused state must remain easy to read.
- Voice target: lost 1984 arcade cabinet. Keep labels short, punchy, readable, and arcade-first; pair stylized names with plain descriptions when needed.

## Content And Folder Conventions

- Runtime code belongs under `Assets/Scripts/`, grouped by responsibility.
- Tests belong under `Assets/Tests/Editor/` for Edit Mode and `Assets/Tests/PlayMode/` if Play Mode coverage is introduced.
- Authored gameplay content currently belongs under:
  - `Assets/Resources/Bricks/`
  - `Assets/Resources/Levels/`
  - `Assets/Resources/PowerUps/`
  - `Assets/Resources/Upgrades/`
  - `Assets/Resources/Themes/`
  - `Assets/Resources/Sprites/`
  - `Assets/Resources/Backgrounds/`
- If prefabs, art, or audio are introduced later, prefer `Assets/Prefabs/`, `Assets/Art/`, and `Assets/Audio/`.
- Do not edit generated folders for durable changes: `Library/`, `Logs/`, `Temp/`, or `UserSettings/`.
- Be cautious with large manual edits to `.unity`, `.prefab`, or other YAML assets because Unity references assets by GUID.

## Validation

- After code or behavior changes, run the Unity compile helper when the environment allows:

```powershell
python .codex/skills/unity-compile/scripts/run_unity_compile.py
```

- After adding or changing gameplay rules or tests, run the relevant Unity Test Framework suite:

```powershell
python .codex/skills/unity-tests/scripts/run_unity_tests.py --platform editmode
```

- Use Play Mode validation for scene lifecycle, collisions, physics timing, spawned runtime objects, input flow, and visual integration.
- If Unity cannot run in the environment, report that clearly and include the closest completed validation.
- Let Unity regenerate project files after adding or renaming scripts instead of hand-maintaining `Get Bricked.sln`.

## Local Skills

- `.codex/skills/unity-compile/`: terminal Unity compile checks.
- `.codex/skills/unity-tests/`: Unity Test Framework batchmode runs.
- `.codex/skills/breakout-svg-art/`: SVG gameplay art workflow for this project.
- `.codex/skills/repo-maintenance/`: refresh durable repo guidance and local skills after substantial work.

## Good First Reads

1. `AGENTS.md`
2. `README.md`
3. `ProjectSettings/ProjectVersion.txt`
4. `Packages/manifest.json`
5. `ProjectSettings/EditorBuildSettings.asset`
6. `Plan/STYLE.md`
7. `Plan/VOICE.md`
8. `Assets/Scripts/Core/BreakoutBootstrap.cs`
9. `Assets/Scripts/Gameplay/BreakoutGameController.cs`
10. `Assets/Scripts/Gameplay/Levels/BreakoutLevelPlanner.cs`
11. `Assets/Scripts/Gameplay/PowerUps/BreakoutPowerUpService.cs`
12. `Assets/Scripts/Gameplay/Presentation/BreakoutUiRenderer.cs`
13. `Assets/Scripts/Gameplay/Presentation/BreakoutThemeService.cs`
14. `Assets/Scripts/Gameplay/RunFlow/BreakoutRunSetupState.cs`
15. `Assets/Scripts/Gameplay/RunFlow/BreakoutRunState.cs`
16. `Assets/Scripts/Gameplay/Scoring/BreakoutScoreService.cs`
17. `Assets/Tests/Editor/`

## Current Reality Check

- This is a playable runtime prototype, not a blank Unity template anymore.
- Test coverage exists but is still sparse; add focused regression coverage when changing bug-prone gameplay logic.
- Most near-term work should extend the current runtime prototype and extract responsibilities incrementally, then promote runtime-generated pieces into authored scene objects or prefabs only when the project is ready.
