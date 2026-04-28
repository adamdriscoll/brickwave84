# AGENTS.md

## Project Snapshot

- This repository is a very early-stage Unity project named `Get Bricked`.
- Unity editor version: `6000.3.6f1`.
- Render pipeline: Universal Render Pipeline with 2D renderer assets already configured.
- Current gameplay state is essentially template-only:
  - One enabled build scene: `Assets/Scenes/SampleScene.unity`
  - Scene contents are just the default `Main Camera` and `Global Light 2D`
  - No custom C# scripts, no `.asmdef` files, no prefabs, and no automated tests yet
- Input System is enabled and has a starter action asset at `Assets/InputSystem_Actions.inputactions`.

## What Exists

- `Assets/Scenes/SampleScene.unity`: current playable scene and only scene in build settings
- `Assets/InputSystem_Actions.inputactions`: starter input maps for `Player` and `UI`
- `Assets/Settings/*`: URP / 2D renderer assets and template scene assets
- `Packages/manifest.json`: Unity package dependencies
- `ProjectSettings/ProjectVersion.txt`: authoritative Unity version
- `Get Bricked.sln`: currently a minimal Unity-generated solution shell with no populated project entries

## Key Technical Notes

- This is a Unity 6 project, so future edits should assume modern Unity APIs and URP defaults.
- The package list shows a 2D-focused setup plus the new Input System, UGUI, Timeline, and Visual Scripting.
- The input action asset already includes common starter actions like `Move`, `Look`, `Attack`, `Interact`, `Jump`, `Sprint`, `Previous`, and `Next`.
- Because there are no scripts yet, Unity has not generated a meaningful C# solution/project structure. After adding scripts, let Unity regenerate project files instead of hand-maintaining the `.sln`.

## Working Rules For Future Agents

- Prefer adding gameplay code under `Assets/Scripts/` unless the user asks for a different layout.
- Keep Unity `.meta` files intact. If you add an asset or script manually, ensure the matching `.meta` file exists and stays paired with it.
- Do not edit or rely on generated folders for durable changes:
  - `Library/`
  - `Logs/`
  - `Temp/`
  - `UserSettings/`
- Be cautious editing `.unity`, `.prefab`, or other YAML asset files by hand. Small targeted edits are fine, but large structural changes are safer in the Unity Editor because GUID/reference breakage is easy.
- If you move or rename assets manually, remember that Unity references them by GUID from the `.meta` files.
- There is currently no git repository in this folder, so you cannot rely on `git status`, history, or diffs unless the user initializes git later.

## Suggested Conventions

- Create new runtime code in `Assets/Scripts/`.
- If the project grows, split code by domain early, for example:
  - `Assets/Scripts/Gameplay/`
  - `Assets/Scripts/Input/`
  - `Assets/Scripts/UI/`
  - `Assets/Scripts/Core/`
- Add prefabs under `Assets/Prefabs/`, art under `Assets/Art/`, and audio under `Assets/Audio/` if those areas are created later.
- If tests are added, prefer Unity Test Framework with clear separation between Edit Mode and Play Mode tests.

## Validation Checklist

When making changes, validate with the Unity editor when possible:

- Open the project in Unity `6000.3.6f1` or the closest compatible version.
- Confirm the scene loads without missing scripts or broken references.
- Check the Console for compile errors after adding scripts.
- If gameplay changes are made, enter Play Mode in `Assets/Scenes/SampleScene.unity`.
- If build configuration changes are made, re-check `ProjectSettings/EditorBuildSettings.asset`.

## Good First Read Files

If you are a future agent starting work here, read these first:

1. `AGENTS.md`
2. `ProjectSettings/ProjectVersion.txt`
3. `Packages/manifest.json`
4. `ProjectSettings/EditorBuildSettings.asset`
5. `Assets/InputSystem_Actions.inputactions`
6. `Assets/Scenes/SampleScene.unity`

## Current Reality Check

- There is no implemented game loop yet.
- There are no custom systems to preserve.
- Most future work will probably be greenfield scaffolding rather than refactoring.
- If a user asks for game features, you will likely need to create the first scripts, scene objects, prefabs, and folder structure yourself.
