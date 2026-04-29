---
name: repo-maintenance
description: Review recent repository work, extract durable project knowledge, and update repo guidance. Use when Codex needs to refresh `AGENTS.md`, maintain repo-local skills under `.codex/skills/`, or capture repeatable workflows after agents add or change systems, assets, packages, scenes, scripts, tests, tooling, or conventions.
---

# Repo Maintenance

Maintain durable repo guidance after real work lands. Treat `AGENTS.md` as a living onboarding brief, not a changelog. Capture facts future agents need to succeed, and promote repeated repo-specific work into reusable local skills.

## Quick Start

1. Re-read `AGENTS.md`.
2. Review the actual work that happened:
   - Prefer `git status --short`, `git diff --stat`, `git diff -- <paths>`, and recent commits.
   - If git history is unavailable or incomplete, inspect the changed files directly and compare them to `AGENTS.md`.
3. Run the snapshot helper:

```powershell
python .codex/skills/repo-maintenance/scripts/repo_snapshot.py
```

4. Update `AGENTS.md` with durable new facts and remove stale statements.
5. Create or update repo-local skills under `.codex/skills/` when the work introduced a reusable workflow or project-specific procedure.
6. Validate touched skills with:

```powershell
python C:\Users\adamr\.codex\skills\.system\skill-creator\scripts\quick_validate.py .codex/skills/repo-maintenance
```

Run the validator for any other edited local skill too.

## Maintenance Workflow

### 1. Build Context From Source Artifacts

Read only what is needed to understand the durable outcome of the work:

- `AGENTS.md`
- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `ProjectSettings/EditorBuildSettings.asset`
- Changed scenes, prefabs, scripts, tests, input assets, packages, and tooling files
- Any existing repo-local skill touched by the work

Use the helper script output to spot gaps, but verify important claims against the source files before writing them into `AGENTS.md`.

### 2. Decide What Belongs In `AGENTS.md`

Add or revise facts that will still help a future agent after the current task is forgotten:

- Current Unity/editor version
- Repo status assumptions that changed, such as git now existing
- New gameplay systems, runtime folders, prefabs, scenes, input assets, asmdefs, tests, or tooling
- New conventions that future agents should follow
- Validation steps that became important because the project structure changed
- Good-first-read files that are now central to understanding the repo

Do not add:

- Temporary task notes
- One-off bug descriptions without reusable lessons
- Personal preferences that are not repo conventions
- Ephemeral branch state or worktree noise
- Narratives about how the change was made

When a prior `AGENTS.md` statement is now false, replace it instead of layering contradictory notes on top.

### 3. Decide Whether The Work Warrants A Skill Update

Create or update a repo-local skill when the work added one of these:

- A repeatable repo-specific workflow that future agents should follow
- A specialized tool/script/template that is easier to reuse than to rediscover
- A project-specific architecture or content pipeline with non-obvious rules
- A maintenance routine that spans several files and benefits from a checklist

Prefer updating an existing skill when the workflow fits its current purpose. Create a new skill only when the task meaningfully differs.

### 4. Keep Skill Metadata Fresh

For each touched skill:

- Keep `SKILL.md` concise and imperative
- Put trigger conditions in the frontmatter `description`
- Keep durable helper code in `scripts/`
- Regenerate or update `agents/openai.yaml` if the skill prompt or purpose changed
- Validate with `quick_validate.py`

If a skill grows variants or long documentation, split detail into `references/` instead of bloating `SKILL.md`.

## Unity-Specific Review Checklist

For this repository, pay extra attention to:

- New or renamed assets under `Assets/`
- Matching `.meta` files for manually added Unity assets
- Scene/build-setting changes in `ProjectSettings/EditorBuildSettings.asset`
- New C# scripts, `.asmdef` files, prefabs, ScriptableObjects, and tests
- Input System asset changes in `Assets/*.inputactions`
- Package changes in `Packages/manifest.json`
- Any new validation requirement that should be called out for future agents

Do not edit generated Unity folders for durable repo maintenance: `Library/`, `Logs/`, `Temp/`, or `UserSettings/`.

## Expected Output

After using this skill, the repo should have:

- An `AGENTS.md` file that matches the current durable repo state
- Updated or newly created repo-local skills for reusable workflows
- Validated skill metadata for each touched local skill
- A short handoff note that mentions what was updated and any validation that could not be run
