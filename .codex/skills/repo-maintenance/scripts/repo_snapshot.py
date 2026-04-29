from __future__ import annotations

import json
import subprocess
from datetime import datetime, timezone
from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[4]


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def relpath(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def run_command(args: list[str]) -> str | None:
    try:
        result = subprocess.run(
            args,
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=True,
        )
    except (FileNotFoundError, subprocess.CalledProcessError):
        return None
    return result.stdout.strip()


def gather_git_info() -> dict[str, object]:
    if not (ROOT / ".git").exists():
        return {"present": False}

    branch = run_command(["git", "branch", "--show-current"]) or "(detached)"
    status_text = run_command(["git", "status", "--short"]) or ""
    last_commit = run_command(["git", "log", "-1", "--pretty=format:%h %cs %s"])
    changed_files = [line.strip() for line in status_text.splitlines() if line.strip()]

    return {
        "present": True,
        "branch": branch,
        "changed_files": changed_files,
        "clean": not changed_files,
        "last_commit": last_commit,
    }


def parse_unity_version() -> str | None:
    version_file = ROOT / "ProjectSettings" / "ProjectVersion.txt"
    if not version_file.exists():
        return None

    match = re.search(r"m_EditorVersion:\s*(.+)", read_text(version_file))
    return match.group(1).strip() if match else None


def parse_enabled_scenes() -> list[str]:
    build_settings = ROOT / "ProjectSettings" / "EditorBuildSettings.asset"
    if not build_settings.exists():
        return []

    text = read_text(build_settings)
    matches = re.findall(r"- enabled: 1\s+path: ([^\r\n]+)", text)
    return [match.strip() for match in matches]


def parse_manifest_packages() -> list[str]:
    manifest_path = ROOT / "Packages" / "manifest.json"
    if not manifest_path.exists():
        return []

    manifest = json.loads(read_text(manifest_path))
    dependencies = manifest.get("dependencies", {})
    filtered = [
        f"{name} {version}"
        for name, version in sorted(dependencies.items())
        if not name.startswith("com.unity.modules.")
    ]
    return filtered


def collect_paths(pattern: str) -> list[str]:
    return sorted(
        relpath(path)
        for path in ROOT.glob(pattern)
        if path.is_file()
    )


def collect_test_paths() -> list[str]:
    candidates = collect_paths("Assets/**/*.cs") + collect_paths("Packages/**/*.cs")
    tests: list[str] = []

    for candidate in candidates:
        lower = candidate.lower()
        if "/tests/" in lower or lower.endswith("test.cs") or lower.endswith("tests.cs"):
            tests.append(candidate)

    return sorted(set(tests))


def collect_local_skills() -> list[dict[str, object]]:
    skills_root = ROOT / ".codex" / "skills"
    if not skills_root.exists():
        return []

    skills: list[dict[str, object]] = []

    for skill_dir in sorted(path for path in skills_root.iterdir() if path.is_dir()):
        skill_file = skill_dir / "SKILL.md"
        if not skill_file.exists():
            continue

        resources = [
            name
            for name in ("scripts", "references", "assets", "agents")
            if (skill_dir / name).exists()
        ]
        skills.append(
            {
                "name": skill_dir.name,
                "path": relpath(skill_dir),
                "resources": resources,
            }
        )

    return skills


def print_section(title: str, lines: list[str]) -> None:
    print(f"## {title}")
    if lines:
        for line in lines:
            print(line)
    else:
        print("- None")
    print()


def main() -> int:
    generated_at = datetime.now(timezone.utc).astimezone().isoformat(timespec="seconds")
    git_info = gather_git_info()
    unity_version = parse_unity_version()
    enabled_scenes = parse_enabled_scenes()
    packages = parse_manifest_packages()
    scenes = collect_paths("Assets/**/*.unity")
    prefabs = collect_paths("Assets/**/*.prefab")
    scripts = collect_paths("Assets/**/*.cs")
    asmdefs = collect_paths("Assets/**/*.asmdef")
    input_actions = collect_paths("Assets/**/*.inputactions")
    tests = collect_test_paths()
    local_skills = collect_local_skills()

    print("# Repo Snapshot")
    print(f"- Root: {ROOT}")
    print(f"- Generated: {generated_at}")
    print()

    git_lines = [f"- Git repo present: {'yes' if git_info['present'] else 'no'}"]
    if git_info["present"]:
        git_lines.append(f"- Branch: {git_info['branch']}")
        git_lines.append(f"- Working tree clean: {'yes' if git_info['clean'] else 'no'}")
        if git_info["last_commit"]:
            git_lines.append(f"- Last commit: {git_info['last_commit']}")
        changed_files = git_info["changed_files"]
        if changed_files:
            git_lines.append("- Changed files:")
            git_lines.extend(f"  - {line}" for line in changed_files[:20])
            if len(changed_files) > 20:
                git_lines.append(f"  - ... ({len(changed_files) - 20} more)")
    print_section("Git", git_lines)

    unity_lines = []
    if unity_version:
        unity_lines.append(f"- Editor version: {unity_version}")
    unity_lines.append(f"- Enabled build scenes: {len(enabled_scenes)}")
    unity_lines.extend(f"  - {scene}" for scene in enabled_scenes)
    print_section("Unity", unity_lines)

    inventory_lines = [
        f"- Scene assets: {len(scenes)}",
        f"- Prefabs: {len(prefabs)}",
        f"- C# scripts: {len(scripts)}",
        f"- Assembly definitions: {len(asmdefs)}",
        f"- Input action assets: {len(input_actions)}",
        f"- Test scripts: {len(tests)}",
    ]
    if scripts:
        inventory_lines.append("- Script files:")
        inventory_lines.extend(f"  - {path}" for path in scripts[:20])
        if len(scripts) > 20:
            inventory_lines.append(f"  - ... ({len(scripts) - 20} more)")
    print_section("Project Inventory", inventory_lines)

    package_lines = [f"- Non-module packages: {len(packages)}"]
    package_lines.extend(f"  - {package}" for package in packages)
    print_section("Packages", package_lines)

    skill_lines = [f"- Repo-local skills: {len(local_skills)}"]
    for skill in local_skills:
        resources = ", ".join(skill["resources"]) if skill["resources"] else "none"
        skill_lines.append(f"  - {skill['name']} ({skill['path']}) [{resources}]")
    print_section("Local Skills", skill_lines)

    print("Use this snapshot to verify `AGENTS.md` and skill docs against the current repo state.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
