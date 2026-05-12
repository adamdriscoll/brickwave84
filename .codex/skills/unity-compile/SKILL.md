---
name: unity-compile
description: Run Unity batchmode compile checks for this repository and surface compiler or build-blocking log errors quickly. Use when Codex needs to verify that gameplay script edits still compile, reproduce a Unity script-compilation failure, or sanity-check whether the project is ready for a later build/export step.
---

# Unity Compile

Use this skill to reproduce and diagnose Unity compile failures for `Get Bricked`. If this project is already open in Unity, the helper asks the open editor to run the check through `Assets/Editor/CodexUnityCommandBridge.cs`; otherwise it falls back to the legacy batchmode run.

## Quick Start

1. Run `python .codex/skills/unity-compile/scripts/run_unity_compile.py` from the repo root.
2. Read the script output first.
3. If the compile fails, inspect the referenced log excerpt and fix the reported script or batchmode issue.
4. Re-run the script after every fix until it reports success.

## Workflow

- Prefer the bundled script over retyping raw Unity batchmode commands.
- Let the script discover the required Unity editor version from `ProjectSettings/ProjectVersion.txt`.
- Use `--unity <path-to-Unity.exe>` only when the editor is installed outside the normal Unity Hub path.
- Use `--log <path>` if a task needs a separate log artifact.
- Leave Unity open when useful. The script detects the open editor through `Library/EditorInstance.json` and uses the editor bridge instead of launching a second Unity process.
- The open-editor bridge defaults to a 600-second wait, matching the test helper, because Unity imports and domain reloads can take longer than a normal script compile.
- Use `--force-batchmode` only when you intentionally want the old batchmode path and the project is not already open.
- Treat a clean compile check as a fast pre-build guard, not a full player build.

## Expected Output

- On success, the script prints the Unity version, editor path, log path, and a success message.
- On failure, the script prints the failing Unity exit code when present plus the most relevant compiler or build-error lines from the log.
- If Unity cannot be found, fix the editor path issue before debugging project code.

## Resources

### scripts/

- `run_unity_compile.py`: Detect the project Unity version, locate `Unity.exe`, use the open-editor bridge when available, run batchmode compile otherwise, and summarize any build-blocking log errors.
