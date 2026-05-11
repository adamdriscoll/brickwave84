#!/usr/bin/env python3
from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parents[2]))
from unity_editor_bridge import is_unity_editor_open, request_unity_editor


DEFAULT_LOG_NAME = "codex-unity-compile.log"
ERROR_PATTERNS = (
    re.compile(r"error CS\d+:", re.IGNORECASE),
    re.compile(r"Scripts have compiler errors\.", re.IGNORECASE),
    re.compile(r"\*\*\* Tundra build failed", re.IGNORECASE),
    re.compile(r"Script Compilation Error", re.IGNORECASE),
)


def parse_args() -> argparse.Namespace:
    repo_root = Path(__file__).resolve().parents[4]
    default_log = repo_root / "Logs" / DEFAULT_LOG_NAME

    parser = argparse.ArgumentParser(
        description="Run a Unity batchmode compile check for the current project."
    )
    parser.add_argument(
        "--project",
        type=Path,
        default=repo_root,
        help="Path to the Unity project root. Defaults to this repository.",
    )
    parser.add_argument(
        "--unity",
        type=Path,
        default=None,
        help="Optional path to Unity.exe when it is not in the standard Unity Hub install location.",
    )
    parser.add_argument(
        "--log",
        type=Path,
        default=default_log,
        help="Path to the Unity log file to generate.",
    )
    parser.add_argument(
        "--tail",
        type=int,
        default=25,
        help="Maximum number of relevant error lines to print on failure.",
    )
    parser.add_argument(
        "--editor-timeout",
        type=int,
        default=180,
        help="Seconds to wait for an already-open Unity editor to answer the compile request.",
    )
    parser.add_argument(
        "--force-batchmode",
        action="store_true",
        help="Run the legacy batchmode check even if the project is already open in Unity.",
    )
    return parser.parse_args()


def read_editor_version(project_root: Path) -> str:
    version_file = project_root / "ProjectSettings" / "ProjectVersion.txt"
    if not version_file.is_file():
        raise FileNotFoundError(f"Could not find Unity version file at {version_file}.")

    for line in version_file.read_text(encoding="utf-8").splitlines():
        if line.startswith("m_EditorVersion:"):
            return line.split(":", 1)[1].strip()

    raise RuntimeError(f"Could not parse m_EditorVersion from {version_file}.")


def find_unity_executable(editor_version: str, explicit_path: Path | None) -> Path:
    if explicit_path is not None:
        resolved = explicit_path.expanduser().resolve()
        if not resolved.is_file():
            raise FileNotFoundError(f"Unity executable not found at {resolved}.")
        return resolved

    base_paths = []
    for env_name in ("ProgramFiles", "ProgramW6432", "ProgramFiles(x86)"):
        value = os.environ.get(env_name)
        if value:
            base_paths.append(Path(value))

    for base_path in base_paths:
        candidate = base_path / "Unity" / "Hub" / "Editor" / editor_version / "Editor" / "Unity.exe"
        if candidate.is_file():
            return candidate.resolve()

    raise FileNotFoundError(
        f"Unity {editor_version} was not found in the standard Unity Hub install paths. "
        "Pass --unity with the full path to Unity.exe."
    )


def run_compile(unity_executable: Path, project_root: Path, log_path: Path) -> int:
    log_path.parent.mkdir(parents=True, exist_ok=True)
    command = [
        str(unity_executable),
        "-batchmode",
        "-quit",
        "-projectPath",
        str(project_root),
        "-logFile",
        str(log_path),
    ]

    completed = subprocess.run(command, check=False)
    return completed.returncode


def read_log(log_path: Path) -> list[str]:
    if not log_path.is_file():
        return []

    return log_path.read_text(encoding="utf-8", errors="replace").splitlines()


def collect_relevant_errors(lines: list[str], max_lines: int) -> list[str]:
    relevant: list[str] = []
    for line in lines:
        if any(pattern.search(line) for pattern in ERROR_PATTERNS):
            relevant.append(line.strip())

    deduped: list[str] = []
    seen = set()
    for line in relevant:
        if line in seen:
            continue
        seen.add(line)
        deduped.append(line)

    return deduped[-max_lines:]


def main() -> int:
    args = parse_args()
    project_root = args.project.expanduser().resolve()
    log_path = args.log.expanduser().resolve()

    editor_version = read_editor_version(project_root)
    unity_executable = find_unity_executable(editor_version, args.unity)

    print(f"Project: {project_root}")
    print(f"Unity version: {editor_version}")
    print(f"Unity editor: {unity_executable}")
    print(f"Log file: {log_path}")

    if not args.force_batchmode and is_unity_editor_open(project_root):
        print("Open Unity editor detected; requesting an in-editor compile check.")
        try:
            response = request_unity_editor(
                project_root,
                "compile",
                timeout_seconds=args.editor_timeout,
            )
        except TimeoutError as exception:
            print(str(exception))
            return 1

        if response.get("success"):
            print(response.get("message", "Unity editor compile check passed."))
            return 0

        print(response.get("message", "Unity editor compile check failed."))
        errors = response.get("errors", "")
        if errors:
            print("Relevant log lines:")
            for line in errors.splitlines()[-args.tail:]:
                print(line)
        return 1

    exit_code = run_compile(unity_executable, project_root, log_path)
    log_lines = read_log(log_path)
    relevant_errors = collect_relevant_errors(log_lines, args.tail)

    if exit_code != 0 or relevant_errors:
        print("Unity compile check failed.")
        if exit_code != 0:
            print(f"Unity exit code: {exit_code}")
        if relevant_errors:
            print("Relevant log lines:")
            for line in relevant_errors:
                print(line)
        else:
            print("No matching compiler patterns were found in the log. Inspect the full log manually.")
        return 1

    print("Unity compile check passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
