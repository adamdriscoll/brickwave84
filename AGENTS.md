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
  - The prototype currently supports a runtime main menu, run setup flow, in-game HUD, pause / restart / return-to-menu actions, one paddle, one or more balls, lives, serve/reset flow between ball losses, seeded procedural multi-level progression, deterministic between-level `pick 1 of 3` run-upgrade drafts, temporary timed pickups, and brick-driven power-up / power-down drops
- Brick content is now data-driven through ScriptableObject assets, including multi-strength breakable bricks, spin-reactive breakable bricks, unbreakable obstacle bricks, and optional per-level moving-brick rules
  - Power-up content is now data-driven through ScriptableObject assets, with timed paddle-size, ball-speed, sticky, laser, phase, chain-lightning, reverse-control, split-paddle, gravity-well, fog-of-war, and lag-spike modifiers plus instant multi-ball burst and shield-wall effects
  - Runtime visuals now support data-driven theme selection, with palette-based themes for background, walls, paddle, bricks, power-up pickups, and ball plus sprite hooks reserved for future art passes
- Runtime visuals now also support SVG-backed gameplay sprites plus a resource-backed background image, with URP bloom driving additive glow for the ball and pickups while paddle and bricks stay crisp on unlit sprite materials; the backdrop path now layers a smoked-glass haze and scanline treatment behind gameplay pieces for better contrast
  - The desired presentation direction is now documented in `Plan/STYLE.md`: a readability-first synthwave arcade look with dark indigo backgrounds, magenta/cyan neon accents, controlled CRT glow, and giant-cabinet framing cues
  - The current groundwork already includes runtime OnGUI-based main menu, run setup, HUD, pause, end-state flow, persisted run-setup choices, seed entry, difficulty presets, modifier validation, deterministic gameplay rolls, and seeded procedural level generation
  - There are custom C# scripts now, still no `.asmdef` files and no prefabs, and there is now a first small layer of automated Unity Edit Mode coverage under `Assets/Tests/Editor/`
- Input System is enabled and has a starter action asset at `Assets/InputSystem_Actions.inputactions`.

## What Exists

- `README.md`: human-readable project overview and implemented-content inventory; keep this aligned with the actual playable state
- `Assets/Scenes/SampleScene.unity`: current playable scene and only scene in build settings
- `Assets/InputSystem_Actions.inputactions`: starter input maps for `Player` and `UI`
- `Assets/Scripts/Core/BreakoutBootstrap.cs`: runtime entry point that ensures the prototype controller exists after scene load
- `Assets/Scripts/Gameplay/BreakoutGameController.cs`: top-level runtime coordinator for scene bootstrapping, run flow, serve/life transitions, and cross-system orchestration
- `Assets/Scripts/Gameplay/BreakoutLevelPlanner.cs`: deterministic procedural level-planning service that builds seeded layout plans and moving-brick assignments from level templates and brick definitions
- `Assets/Scripts/Gameplay/BreakoutRunSetupState.cs`: mutable run-setup state model for seed text, difficulty/modifier choices, and validated `RunSettings` construction
- `Assets/Scripts/Gameplay/BreakoutRunSetupPersistence.cs`: `PlayerPrefs` persistence helper for saved run-setup choices and theme selection
- `Assets/Scripts/Gameplay/BreakoutRunState.cs`: run-layer state for cleared encounters, chosen permanent upgrades, pending draft offers, and aggregated persistent modifiers
- `Assets/Scripts/Gameplay/BreakoutThemeService.cs`: runtime theme resolver/applicator for camera, walls, paddle, balls, bricks, and pickups
- `Assets/Scripts/Gameplay/BreakoutPowerUpService.cs`: pickup spawning, timed-effect tracking, banner state, and effect-modifier calculations for movement, control, visibility, laser, phase, and chain behaviors
- `Assets/Scripts/Gameplay/BreakoutUiRenderer.cs`: runtime OnGUI presentation service for main menu, run setup, HUD, pause, upgrade draft, and end-state overlays
- `Assets/Scripts/Gameplay/BreakoutUpgradeDraftService.cs`: deterministic `1 of 3` run-upgrade offer generator using the run seed, cleared-level count, and prior picks
- `Assets/Scripts/Gameplay/BreakoutGlowRenderer.cs`: legacy runtime helper for layered neon halo sprites that still exists for optional use, but is no longer the default glow path for core gameplay pieces
- `Assets/Scripts/Gameplay/PaddleController.cs`: keyboard-driven paddle movement with clamped horizontal bounds and runtime width modifiers
- `Assets/Scripts/Gameplay/BallController.cs`: launch, bounce shaping, spin-biased brick ricochet response, per-level speed tuning, speed clamping, and single/multi-ball loss detection support
- `Assets/Scripts/Gameplay/Brick.cs`: definition-driven brick behavior with variable durability, optional anchored spin response, optional moving-body motion, and unbreakable support
- `Assets/Scripts/Gameplay/PowerUpPickup.cs`: falling pickup behavior and paddle catch detection
- `Assets/Scripts/Gameplay/DeterministicRandomService.cs`: seed-driven random helper used for gameplay-critical procedural choices
- `Assets/Scripts/Gameplay/Data/BrickDefinition.cs`: ScriptableObject data for brick durability, scoring, completion contribution, colors, and weighted drop tables
- `Assets/Scripts/Gameplay/Data/LevelDefinition.cs`: ScriptableObject data for progression-profile tuning, legacy layout references, completion rules, and optional authored motion hints that now serve mainly as template data
- `Assets/Scripts/Gameplay/Data/PowerUpDefinition.cs`: ScriptableObject data for pickup effect type, duration, magnitude, and HUD labeling
- `Assets/Scripts/Gameplay/Data/RunSettings.cs`: runtime run configuration model for seed, difficulty, balls-per-serve, modifier multipliers, and drop-pool restrictions
- `Assets/Scripts/Gameplay/Data/RunUpgradeDefinition.cs`: ScriptableObject data for permanent run modifiers, draft weighting, stack caps, exclusions, and upgrade presentation
- `Assets/Scripts/Gameplay/Data/ThemeDefinition.cs`: ScriptableObject theme data for semantic visual slots, palette colors, and future sprite overrides
- `Assets/Tests/Editor/BreakoutGameControllerPowerUpTests.cs`: first Unity Edit Mode regression coverage for immediate power-up modifier application on the paddle
- `Assets/Tests/Editor/BreakoutSpinningBrickTests.cs`: Unity Edit Mode regression coverage for spinning-brick rigidbody setup and spin-biased bounce math
- `Assets/Resources/Bricks/*`: authored brick definition assets loaded at runtime
- `Assets/Resources/Levels/*`: authored level definition assets loaded at runtime
- `Assets/Resources/PowerUps/*`: authored power-up definition assets referenced by brick drop tables
- `Assets/Resources/Upgrades/*`: authored permanent run-upgrade assets loaded for between-level draft offers
- `Assets/Resources/Themes/*`: authored runtime theme assets loaded by run setup and applied across gameplay visuals
- `Assets/Resources/Sprites/*`: default SVG gameplay sprites loaded at runtime as fallback art for the ball, bricks, paddle, and power-up pickups, with `spinning-brick.svg` available for spin-reactive brick definitions through per-brick sprite resource overrides
- `Assets/Resources/Backgrounds/*`: backdrop images available for runtime background presentation; the runtime now rotates them by level index using the sorted contents of this folder and supports assets imported as either `Sprite` or plain `Texture2D`
- `Assets/Shaders/SpriteAdditive.shader`: custom URP-compatible additive sprite shader used by the runtime ball and pickup presentation path
- `Plan/OVERVIEW.md`: current high-level product reframe toward a run-based arcade roguelite structure
- `Plan/PLAN.md`: current roadmap built on the run-based roguelite direction
- `Assets/Settings/*`: URP / 2D renderer assets and template scene assets
- `Plan/STYLE.md`: durable visual direction guide for the target synthwave arcade presentation and future theme / VFX / UI work
- `Plan/VOICE.md`: durable naming, tone, slang, and UI-copy guide for the game's synthwave arcade personality
- `.codex/skills/breakout-svg-art/*`: repo-local skill for generating and wiring new SVG gameplay art into the runtime theme pipeline
- `.codex/skills/repo-maintenance/*`: repo-local maintenance skill and snapshot helper for refreshing `AGENTS.md` and local skills after agent work
- `.codex/skills/unity-compile/*`: repo-local Unity batchmode compile-check skill and helper script for reproducing script compilation failures from the terminal
- `.codex/skills/unity-tests/*`: repo-local Unity Test Framework skill and batchmode runner for Edit Mode and Play Mode suites
- `Packages/manifest.json`: Unity package dependencies
- `ProjectSettings/ProjectVersion.txt`: authoritative Unity version
- `Get Bricked.sln`: may stay sparse until Unity regenerates project files after script import

## Key Technical Notes

- This is a Unity 6 project, so future edits should assume modern Unity APIs and URP defaults.
- The package list shows a 2D-focused setup plus the new Input System, UGUI, Timeline, and Visual Scripting.
- `com.unity.vectorgraphics` is now installed so Unity imports the repo's SVG gameplay art through the Vector Graphics package.
- `com.unity.test-framework` is installed and the project now has a starter Edit Mode test layer under `Assets/Tests/Editor/`.
- The input action asset already includes common starter actions like `Move`, `Look`, `Attack`, `Interact`, `Jump`, `Sprint`, `Previous`, and `Next`.
- The current prototype is scene-light and code-heavy: the gameplay board, bounds, ball, paddle, bricks, and temporary HUD are created at runtime instead of being serialized into `SampleScene`.
- Bricks and levels are now authored as ScriptableObjects under `Assets/Resources/` and loaded at runtime by the controller plus its leaf services.
- Brick definitions now own drop chance plus weighted pickup references, so most drop-table tuning is an asset edit rather than a controller edit.
- Brick definitions can now also opt into anchored spin behavior, tune torque/bounce response, and point at a per-definition fallback sprite resource path without changing controller code.
- Level definitions can now optionally assign motion rules per layout symbol, including a base direction, a speed, and modifiers such as row/column alternation, checkerboard reversal, and center-relative motion.
- Theme definitions now own semantic visual-slot palettes under `Assets/Resources/Themes/`; the current implementation uses colors, drives both runtime object tinting and derived UI chrome accents, and still supports per-slot sprite overrides for future art passes.
- The runtime controller now loads default fallback gameplay art from `Assets/Resources/Sprites/ball.svg`, `brick.svg`, `paddle.svg`, and `powerup.svg`; brick definitions can also request a specific fallback sprite such as `Sprites/spinning-brick`, and theme slot sprite overrides still win when they are present in a `ThemeDefinition`.
- The runtime background layer now rotates through the sorted contents of `Assets/Resources/Backgrounds/` as levels advance, with support for both Sprite-imported images and runtime-created sprites from plain textures, then overlays a hazy smoked-glass pass plus procedural scanlines behind the playfield to keep gameplay silhouettes legible.
- Level definitions now act primarily as procedural progression profiles: the controller uses the selected run seed plus level index to build reproducible layouts, brick mixes, drop exposure, and motion patterns at runtime.
- Procedural generation currently ramps difficulty by increasing row/column density, unlocking tougher brick definitions over time, escalating moving-brick frequency, and occasionally switching later levels to score-target completion.
- Spin-reactive bricks currently enter the procedural pool after the opening levels and deliberately skip the separate moving-brick assignment path so their hinge-based rotation can stay centered and physically constrained by neighbors.
- Moving bricks still use dynamic `Rigidbody2D` bodies with bounce material and keep a constant authored speed after collisions, but motion pressure is now assigned procedurally per generated cell rather than only coming from authored symbol maps.
- The game now boots into a runtime main menu instead of straight into gameplay or setup, and the last run-setup selections are persisted through `PlayerPrefs`.
- The selected theme is part of the persisted run setup, so quick-starting from the main menu reuses the most recently chosen palette.
- Main menu, run setup, HUD, pause, diagnostics, and end-of-run flow currently still use temporary runtime OnGUI UI, but that layer now includes theme-aware cabinet styling with marquee/bezel framing, smoked-glass panels, subtle perspective-grid treatment, and restrained scanlines; it is still a likely future candidate for authored UI/prefab migration.
- The runtime OnGUI and overlay text should now follow `Plan/VOICE.md`, which defines the game's 80s arcade naming, slang-density, and UI-labeling rules for menus, callouts, pickups, bricks, stages, and result screens.
- Runtime presentation now enables URP bloom from code, uses a custom additive sprite shader for the ball and pickups, and uses URP sprite-unlit materials for the paddle, bricks, walls, and backdrop; the older `BreakoutGlowRenderer` helper still exists but is no longer the default glow path for core gameplay pieces.
- Difficulty presets and player-selected modifiers are normalized into `RunSettings`, so future tuning should usually flow through that model instead of adding one-off conditionals.
- Permanent build mods now live in a separate run layer instead of mutating `RunSettings`; `BreakoutRunState` tracks chosen upgrades while `BreakoutPowerUpService` still owns temporary pickup effects.
- Clearing a level now attempts to open a deterministic upgrade draft before the next board loads; the current implementation sources offers from `Assets/Resources/Upgrades/`, applies one selected modifier permanently for the run, and then advances to the next level.
- Run-upgrade offers are currently deterministic from the run seed, cleared-level count, and previously chosen upgrades, so reproducing the same seed plus the same pick order should reproduce the same draft sequence.
- The first curated permanent-upgrade pool currently covers paddle width, ball speed, drop chance, extra lives, extra balls per serve, and a persistent mild wavy-paddle modifier.
- Deterministic gameplay randomness currently covers serve launch direction, drop chance / weighted pickup selection, and seeded procedural level plans keyed off the run seed plus level index.
- `R` returns to the run-setup overlay, `Esc` / `P` pauses active gameplay, `Space` launches serves or confirms overlay actions, active sticky catches relaunch on `Space`, active laser volleys also fire on `Space` during gameplay, and between-level upgrade drafts use left/right plus `Space` with `R` still acting as an abandon-to-setup shortcut.
- Timed pickup effects currently refresh by extending the same effect's duration, while opposing effects coexist and combine multiplicatively.
- The advanced pickup set now works through runtime-authored effect state rather than bespoke per-asset branches: sticky paddle can trap one ball for a manual relaunch, laser paddle fires with `Space` during active play, phase ball keeps launched balls moving through breakable bricks, chain lightning arcs from destroyed bricks into nearby targets, shield wall stores one or more bottom-edge rescues, reverse controls flips paddle input, split paddle opens a temporary center gap, gravity well bends balls toward the arena midpoint, fog of war dims bricks and pickups, and lag spike intermittently stalls paddle movement.
- Life loss now depends on all active balls leaving play, so multi-ball changes should be reviewed against `BreakoutGameController.HandleBallLost`.
- The current `Balls Per Serve` run modifier spawns extra balls at every serve, so future ball-loss or serve-flow changes should be checked against `BreakoutGameController.SpawnConfiguredServeBalls`.
- Prototype input is currently read directly from `UnityEngine.InputSystem.Keyboard` rather than being wired through `PlayerInput` or the existing action asset.
- Ball and paddle behavior use Unity 6-era 2D physics APIs such as `Rigidbody2D.linearVelocity`.
- Run flow is still runtime-authored and coordinated by `BreakoutGameController`, but setup persistence, procedural planning, theme application, pickup/effect state, and OnGUI rendering now live in dedicated gameplay services.
- Brick and power-up definition assets can optionally override their semantic theme slot, but default routing already maps brick durability tiers plus beneficial/harmful/burst pickups onto the shared theme palette automatically.
- Laser shots, chain-lightning arcs, and phase-ball passthrough all route back through brick runtime damage helpers instead of bypassing `BrickDefinition` durability and score rules, so future direct-damage effects should usually build on `Brick.ApplyEffectHit`.
- The current default visuals now reach a first-pass synthwave cabinet presentation through theme palettes, theme-derived UI chrome, playfield framing, and a softer glowing ball silhouette, but there is still room for authored art, material, VFX, and post-processing polish.
- Terminal-side compile validation can now be done with `python .codex/skills/unity-compile/scripts/run_unity_compile.py`, which reads `ProjectVersion.txt`, locates the matching Unity Hub editor, runs batchmode, and summarizes build-blocking script errors from the generated log.
- Terminal-side test validation can now be done with `python .codex/skills/unity-tests/scripts/run_unity_tests.py`, which locates the matching Unity editor, runs Unity Test Framework suites in batchmode, and summarizes XML result totals plus failing test names.
- The first automated regression coverage currently checks that caught power-up modifiers immediately reapply to the paddle, including wavy and width-modifier behavior.
- After adding or renaming scripts, let Unity regenerate project files instead of hand-maintaining the `.sln`.

## Visual Direction

- Read `Plan/STYLE.md` before making UI, theme, VFX, shader, environment, or presentation changes.
- The target look is `1980s synthwave retro-futurism`: dark indigo or violet bases, focused magenta/cyan neon accents, warm gold/coral highlights, and restrained CRT or cabinet cues.
- The visual tone should feel like a game running inside a giant arcade machine: backlit marquee energy, bezel framing, smoked-glass panels, and selective glow.
- Gameplay readability wins over decoration. The ball path, paddle silhouette, brick durability, pickup polarity, and paused-vs-active state should remain legible at a glance.
- Use the semantic theme-slot system first for palette mapping, then layer future glow, grid, scanline, or material work on top instead of hard-coding colors inside gameplay logic.

## Voice And Naming

- Read `Plan/VOICE.md` before adding or revising UI copy, labels, power-up names, brick names, stage names, achievement text, callouts, or result-screen text.
- The target voice is a lost 1984 arcade cabinet: fast, punchy, readable, cheerfully cocky, and arcade-first with 80s slang used sparingly.
- Keep most display labels to `1-3` words, prefer mechanical clarity over cleverness, and pair stylized names with plain-English descriptions or tooltips.
- Avoid parody-overload slang stacks, insensitive dated slang, and direct references to real films, bands, brands, or existing game IP.
- For procedural run terminology, prefer the cassette-flavored defaults from `Plan/VOICE.md`, including `Mixtape` for runs, `Tape ID` for seeds, `Stage` for levels, and `Capsule` or `Token` for pickups unless a feature needs a stronger reason to differ.

## Working Rules For Future Agents

- Prefer adding gameplay code under `Assets/Scripts/` unless the user asks for a different layout.
- Treat `Assets/Scripts/Gameplay/BreakoutGameController.cs` as an orchestration root, not the default home for every new gameplay feature. If a task adds a distinct responsibility such as score rules, serve flow, overlay input, upgrade resolution, background presentation, or pickup effect coordination, prefer extracting that touched slice into a focused collaborator instead of growing the controller again.
- Preserve Unity `.meta` pairings for every manually added script and folder under `Assets/Scripts/`.
- After substantial project work, use `.codex/skills/repo-maintenance/` to refresh `AGENTS.md` and repo-local skills with durable new repo knowledge.
- Keep `README.md` up to date when implemented gameplay systems, controls, content pools, setup/build options, themes, or other player-visible features change.
- When behavior changes or a bug is fixed, add or update the closest relevant automated test when practical instead of leaving coverage behind.
- After code or behavior changes, run the relevant automated tests plus the Unity compile check before closing the task whenever the environment allows it.
- Refactor incrementally. When work touches an overcrowded class or service, improve only that local responsibility, keep behavior stable, and avoid repo-wide architecture rewrites unless the user explicitly asks for one.
- In Unity terms, keep `MonoBehaviour` classes focused on engine callbacks, serialized references, scene wiring, and `GameObject` lifetime. Prefer plain C# classes for gameplay rules, calculations, state transitions, deterministic selection logic, timers, and other behavior that does not need direct Unity API ownership.
- Use `ScriptableObject` assets for authored configuration and reusable content definitions. Keep mutable run/session state in runtime models or services rather than writing transient state back into assets.
- Add interfaces or abstract/base classes only when they create a real seam for multiple behaviors, strategy swapping, or easier testing around Unity-dependent code. Prefer composition over inheritance by default, and avoid one-interface-per-class ceremony.
- When extracting logic from a large runtime class, prefer narrow dependencies and adapters over handing the entire controller to helpers. New collaborators should have a short, single-purpose API and clear ownership.
- If a task naturally exposes a cleanup opportunity, leave the touched area slightly better structured than you found it, but stop once the local code is readable, testable, and appropriately scoped for the current task.
- Before touching visual style, read `Plan/STYLE.md` and keep the synthwave arcade direction consistent across gameplay, HUD, menus, and future theme assets.
- Before touching UI copy, naming, event callouts, or flavor text, read `Plan/VOICE.md` and keep the arcade-cabinet tone plus labeling rules consistent across gameplay, menus, pickups, stages, and result screens.
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
- If a feature slice keeps growing, prefer focused subfolders by responsibility such as `Assets/Scripts/Gameplay/RunFlow/`, `Assets/Scripts/Gameplay/Scoring/`, `Assets/Scripts/Gameplay/Presentation/`, or `Assets/Scripts/Gameplay/PowerUps/` instead of keeping every runtime class in one flat folder forever.
- Prefer small, responsibility-revealing collaborator names such as `*Service`, `*Coordinator`, `*Resolver`, `*Factory`, `*Presenter`, or `*Builder` over catch-all names like `Utils`, `Helpers`, or `Manager` when the role is more specific.
- Favor Edit Mode testable plain C# logic behind narrow seams, then let `MonoBehaviour` wrappers translate Unity callbacks and scene state into those services.
- Keep first-pass gameplay tuning values serialized on the controlling MonoBehaviour so feel can be adjusted quickly in the Inspector during playtesting.
- Add prefabs under `Assets/Prefabs/`, art under `Assets/Art/`, and audio under `Assets/Audio/` if those areas are created later.
- Prefer Unity Test Framework with clear separation between Edit Mode and Play Mode tests.
- Keep test code under `Assets/Tests/`, using `Editor/` for Edit Mode coverage and `PlayMode/` for runtime integration coverage if that folder is introduced later.
- Prefer narrow regression tests for gameplay bugs first, then broaden coverage when shared systems or runtime integration is at risk.
- Do not introduce `.asmdef` files casually. When the project is ready for assemblies, move a coherent folder slice and its corresponding test references together so compile boundaries remain intentional and easy to reason about.
- If you add more authored gameplay content, keep using `.asset` plus `.meta` pairings under `Assets/Resources/Bricks/`, `Assets/Resources/Levels/`, `Assets/Resources/PowerUps/`, and `Assets/Resources/Upgrades/` unless the project deliberately migrates to a different content-loading path.
- If you add or replace gameplay SVGs, keep them under `Assets/Resources/Sprites/` unless a request explicitly introduces a new content path, and let Unity generate or refresh the paired `.meta` files after import.
- If you add or change vector art workflow guidance, prefer updating `.codex/skills/breakout-svg-art/` instead of repeating the same SVG hookup instructions in task-specific notes.

## Validation Checklist

When making changes, validate with the Unity editor when possible:

- Open the project in Unity `6000.3.6f1` or the closest compatible version.
- Confirm the scene loads without missing scripts or broken references.
- Check the Console for compile errors after adding scripts.
- Add or update relevant automated tests when the change affects gameplay rules, bug-prone logic, or reusable runtime services.
- If gameplay changes are made, enter Play Mode in `Assets/Scenes/SampleScene.unity`.
- Before or after gameplay script edits, prefer running `python .codex/skills/unity-compile/scripts/run_unity_compile.py` from the repo root for a fast terminal compile check when Unity UI validation is not yet practical.
- After adding or updating tests, prefer running `python .codex/skills/unity-tests/scripts/run_unity_tests.py --platform editmode` for logic coverage and switch to `--platform playmode` or `--platform all` when the change depends on runtime integration.
- For the current prototype, verify:
  - On boot, the runtime main menu appears and can start a run or open run setup without editor interaction
  - From run setup, seed, preset, and modifier controls still respond correctly and `Esc` returns to the main menu
  - Typing digits changes the run seed, `T` randomizes it, and `Space` starts a run with the shown configuration
  - Returning to the main menu and reopening run setup preserves the last chosen setup values across that session, and restarting the app restores them
  - Run setup theme selection responds to left/right input, persists across sessions, and the selected palette recolors the background, paddle, bricks, pickups, and ball on the next run
  - The imported SVG ball, brick, paddle, and pickup art loads at runtime and the backdrop changes as levels advance without stretching or disappearing
  - The background remains visually subordinate to gameplay because the runtime haze and scanline overlays stay behind the walls, bricks, paddle, ball, and pickups
  - The OnGUI presentation layer renders the expected cabinet shell for menus and gameplay, including marquee/bezel framing, smoked-glass panels, and subtle grid/scanline cues without obscuring the ball or paddle
  - `Breakout Prototype` appears in the runtime hierarchy automatically
  - `A/D` or left/right arrows move the paddle
  - `Space` launches the ball
  - `Esc` or `P` pauses active gameplay and exposes resume, restart, setup, and main-menu actions
  - Replaying the same seed reproduces the same procedural layouts, brick/pickup progression, and deterministic drop/launch rolls for the same sequence of level clears
  - Losing the ball removes one life and re-serves from the paddle until lives reach zero
  - Breakable bricks respect their configured hit strength and unbreakable bricks stay in play
  - Early levels start mostly simple, later levels introduce more brick types plus broader pickup/drop exposure, and the same seed reproduces that progression order exactly
  - Generated moving bricks in later levels bounce off walls and other bricks without losing their damage behavior
  - Spinner bricks start rotating when struck, stay anchored around their center point, can have that rotation physically limited by nearby bricks, and can kick balls into visibly stranger bounce angles than standard bricks
  - Run modifiers affect serve ball count, paddle width, ball speed, brick durability, and drop filtering as shown in the setup preview
  - Destroyed breakable bricks can spawn falling pickups, and the paddle can catch or miss them naturally
  - Shield Wall catches the next ball that falls below the arena and bounces it back into play instead of immediately spending a life
  - Sticky Paddle can trap a ball on paddle contact and relaunch it with `Space`
  - Laser Paddle lets `Space` fire upward brick hits during active play without breaking normal pause or menu flow
  - Phase Ball keeps active balls moving through breakable bricks while still damaging them
  - Chain Lightning can jump from a destroyed brick into nearby breakable bricks
  - Reverse Controls, Split Paddle, Gravity Well, Fog of War, and Lag Spike each visibly change control feel or playfield readability when caught
  - Ball and pickup glow now comes from bloom on additive sprites instead of the old layered halo look, while paddle and bricks remain crisp and readable on unlit materials
  - Timed paddle-width and ball-speed effects appear in the HUD and clean themselves up when their timers expire
  - Multi-ball does not consume a life until the last active ball is lost
  - Clearing the current objective opens a deterministic `1 of 3` upgrade draft before the next level, and the selected upgrade persists for the rest of the run
  - Replaying the same seed and taking the same upgrade picks reproduces the same draft offers in the same order
  - Permanent upgrades that affect paddle width, ball speed, drop chance, lives, or serve ball count immediately update the current run and keep affecting later levels
  - The HUD and diagnostics show the active build summary once at least one permanent upgrade has been drafted
  - Game over exposes restart, setup, and main-menu actions
  - `R` returns to run setup without leaving orphaned runtime balls or pickups behind
- If build configuration changes are made, re-check `ProjectSettings/EditorBuildSettings.asset`.

## Good First Read Files

If you are a future agent starting work here, read these first:

1. `AGENTS.md`
2. `README.md`
3. `ProjectSettings/ProjectVersion.txt`
4. `Packages/manifest.json`
5. `ProjectSettings/EditorBuildSettings.asset`
6. `Assets/InputSystem_Actions.inputactions`
7. `Assets/Scripts/Core/BreakoutBootstrap.cs`
8. `Assets/Scripts/Gameplay/BreakoutGameController.cs`
9. `Assets/Scripts/Gameplay/BreakoutLevelPlanner.cs`
10. `Assets/Scripts/Gameplay/BreakoutRunSetupState.cs`
11. `Assets/Scripts/Gameplay/BreakoutThemeService.cs`
12. `Assets/Scripts/Gameplay/BreakoutPowerUpService.cs`
13. `Assets/Scripts/Gameplay/BreakoutRunState.cs`
14. `Assets/Scripts/Gameplay/BreakoutUpgradeDraftService.cs`
15. `Assets/Scripts/Gameplay/Data/RunSettings.cs`
16. `Assets/Scripts/Gameplay/Data/RunUpgradeDefinition.cs`
17. `Assets/Scripts/Gameplay/DeterministicRandomService.cs`
18. `Assets/Scripts/Gameplay/Data/LevelDefinition.cs`
19. `Assets/Scripts/Gameplay/Data/PowerUpDefinition.cs`
20. `Assets/Scripts/Gameplay/Data/ThemeDefinition.cs`
21. `Assets/Resources/Levels/Level01.asset`
22. `Assets/Resources/Bricks/BasicBrick.asset`
23. `Assets/Resources/Upgrades/WideLoader.asset`
24. `Assets/Resources/Themes/ClassicTheme.asset`
25. `Plan/OVERVIEW.md`
26. `Plan/PLAN.md`
27. `Plan/STYLE.md`
28. `Plan/VOICE.md`
29. `Assets/Scenes/SampleScene.unity`
30. `.codex/skills/breakout-svg-art/SKILL.md`
31. `.codex/skills/repo-maintenance/SKILL.md`
32. `.codex/skills/unity-tests/SKILL.md`
33. `Assets/Tests/Editor/BreakoutGameControllerPowerUpTests.cs`

## Current Reality Check

- There is now a minimal implemented game loop for a single-screen brick-breaker prototype with procedural seeded level generation, pickup-driven rule changes, deterministic between-level run-upgrade drafts, palette-based theme selection, SVG-backed gameplay art, a resource-backed synthwave background layer, and a first-pass player-facing menu / HUD / pause flow.
- There is now a first small layer of automated regression coverage for gameplay modifier application, but test coverage is still sparse and should grow alongside bug fixes and behavior changes.
- The current custom systems are small, but they are real and worth extending deliberately instead of replacing by default.
- Most near-term work will still be greenfield, but it should now build on the existing runtime prototype and folder structure.
- If a user asks for game features, you will likely be extending the current scripts and authored content assets first, then deciding when to promote runtime-generated objects into authored scene or prefab assets.
- If a user asks for more UI or meta-flow work, the most likely follow-ups are promoting the runtime OnGUI overlays into authored UI/prefabs, separating meta-flow concerns out of `BreakoutGameController`, broadening persisted settings beyond run setup, and applying `Plan/VOICE.md` consistently to menu text, prompts, callouts, and labeling.
