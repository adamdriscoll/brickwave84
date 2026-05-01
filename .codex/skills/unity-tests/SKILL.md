---
name: unity-tests
description: Run Unity Test Framework suites for this repository and summarize the results. Use when Codex needs to add, update, run, or debug automated tests for gameplay code, runtime systems, Edit Mode logic coverage, or Play Mode integration coverage in `Get Bricked`.
---

# Unity Tests

Use this skill to run targeted Unity tests without opening the editor UI and to keep test coverage moving alongside behavior changes.

## Quick Start

1. Choose the narrowest relevant test scope:
   - Use `EditMode` for pure gameplay logic, services, deterministic state, ScriptableObject-driven rules, and controller methods that can be exercised without loading a scene.
   - Use `PlayMode` for scene/runtime integration, spawned GameObjects, physics, timing, input flow, and anything that depends on frame progression.
2. Run:

```powershell
python .codex/skills/unity-tests/scripts/run_unity_tests.py --platform editmode
```

3. If only one fixture or test should run, narrow it:

```powershell
python .codex/skills/unity-tests/scripts/run_unity_tests.py --platform editmode --test-filter BreakoutGameControllerPowerUpTests
```

4. Read the summary first, then inspect the generated XML or log only when the summary is not enough.

Important: this project's Unity 6 batchmode test command must not pass `-quit`. When `-quit` is present with `-runTests`, Unity imports/compiles the project and exits with code `0` before the Test Runner writes the XML results file. Let `-runTests` control editor shutdown.

## Workflow

### 1. Pick The Right Test Type

- Prefer `EditMode` first when validating service logic or regression fixes in `Assets/Scripts/Gameplay/`.
- Move to `PlayMode` when the bug depends on real runtime behavior such as collisions, spawned pickups, pauses, serves, or scene lifecycle.
- Run `all` only when the change is broad enough to justify both suites.

### 2. Add Or Update Coverage With The Fix

- Do not treat tests as optional polish after behavior changes.
- When fixing a bug or changing gameplay rules, add or update the closest relevant test when practical.
- Keep tests targeted and readable. Prefer small fixtures that isolate one rule or regression.

### 3. Run The Smallest Useful Suite

- Start with a filtered run for the touched fixture when possible.
- Run the broader platform suite when the change affects shared systems or when a focused run passes but confidence is still low.
- Pair test execution with `python .codex/skills/unity-compile/scripts/run_unity_compile.py` for script changes.
- If Unity exits successfully but no XML file is produced, check the command and log for an accidental `-quit` argument before debugging tests.

## Resources

### scripts/

- `run_unity_tests.py`: Detect the project Unity version, locate `Unity.exe`, run Edit Mode and/or Play Mode tests in batchmode, and summarize pass/fail counts plus failing test names.
