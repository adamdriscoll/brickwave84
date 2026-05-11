#!/usr/bin/env python3
from __future__ import annotations

import argparse
import os
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parents[2]))
from unity_editor_bridge import is_unity_editor_open, request_unity_editor


def parse_args() -> argparse.Namespace:
    repo_root = Path(__file__).resolve().parents[4]

    parser = argparse.ArgumentParser(
        description="Run Unity Test Framework suites for the current project."
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
        "--platform",
        choices=("editmode", "playmode", "all"),
        default="editmode",
        help="Which Unity test platform to run.",
    )
    parser.add_argument(
        "--test-filter",
        default=None,
        help="Optional Unity test filter string to narrow the run.",
    )
    parser.add_argument(
        "--logs-dir",
        type=Path,
        default=repo_root / "Logs",
        help="Directory where Unity test logs and XML results should be written.",
    )
    parser.add_argument(
        "--editor-timeout",
        type=int,
        default=600,
        help="Seconds to wait for an already-open Unity editor to answer each test request.",
    )
    parser.add_argument(
        "--force-batchmode",
        action="store_true",
        help="Run the legacy batchmode test command even if the project is already open in Unity.",
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


def resolve_platforms(requested_platform: str) -> list[str]:
    if requested_platform == "all":
        return ["editmode", "playmode"]

    return [requested_platform]


def run_platform_tests(
    unity_executable: Path,
    project_root: Path,
    logs_dir: Path,
    platform: str,
    test_filter: str | None,
) -> tuple[int, Path, Path]:
    logs_dir.mkdir(parents=True, exist_ok=True)
    log_path = logs_dir / f"{platform}-tests.log"
    results_path = logs_dir / f"{platform}-test-results.xml"

    if results_path.exists():
        results_path.unlink()

    unity_platform = "EditMode" if platform == "editmode" else "PlayMode"
    command = [
        str(unity_executable),
        "-batchmode",
        "-projectPath",
        str(project_root),
        "-runTests",
        "-testPlatform",
        unity_platform,
        "-testResults",
        str(results_path),
        "-logFile",
        str(log_path),
    ]

    if test_filter:
        command.extend(["-testFilter", test_filter])

    completed = subprocess.run(command, check=False)
    return completed.returncode, log_path, results_path


def run_platform_tests_in_editor(
    project_root: Path,
    logs_dir: Path,
    platform: str,
    test_filter: str | None,
    timeout_seconds: int,
) -> tuple[int, Path, Path]:
    logs_dir.mkdir(parents=True, exist_ok=True)
    log_path = logs_dir / f"{platform}-tests-editor.log"
    results_path = logs_dir / f"{platform}-test-results.xml"

    if results_path.exists():
        results_path.unlink()

    response = request_unity_editor(
        project_root,
        "tests",
        platform=platform,
        test_filter=test_filter,
        results_path=results_path,
        timeout_seconds=timeout_seconds,
    )

    log_path.write_text(response.get("message", ""), encoding="utf-8")
    return 0 if response.get("success") else 1, log_path, results_path


def parse_results(results_path: Path) -> dict[str, object] | None:
    if not results_path.is_file():
        return None

    root = ET.fromstring(results_path.read_text(encoding="utf-8", errors="replace"))
    failures: list[dict[str, str]] = []

    for test_case in root.findall(".//test-case[@result='Failed']"):
        failure_node = test_case.find("failure")
        message = ""
        if failure_node is not None:
            message_node = failure_node.find("message")
            if message_node is not None and message_node.text:
                message = " ".join(message_node.text.split())

        failures.append(
            {
                "name": test_case.attrib.get("fullname", test_case.attrib.get("name", "Unknown test")),
                "message": message,
            }
        )

    return {
        "result": root.attrib.get("result", "Unknown"),
        "total": int(root.attrib.get("total", "0")),
        "passed": int(root.attrib.get("passed", "0")),
        "failed": int(root.attrib.get("failed", "0")),
        "skipped": int(root.attrib.get("skipped", "0")),
        "inconclusive": int(root.attrib.get("inconclusive", "0")),
        "duration": root.attrib.get("duration", "0"),
        "failures": failures,
    }


def print_summary(
    project_root: Path,
    editor_version: str,
    unity_executable: Path,
    platform: str,
    log_path: Path,
    results_path: Path,
    results: dict[str, object] | None,
    exit_code: int,
) -> bool:
    print(f"Project: {project_root}")
    print(f"Unity version: {editor_version}")
    print(f"Unity editor: {unity_executable}")
    print(f"Platform: {platform}")
    print(f"Log file: {log_path}")
    print(f"Results file: {results_path}")

    if results is None:
        print("No Unity test results file was produced. Inspect the log manually.")
        return False

    print(
        "Summary: "
        f"result={results['result']} total={results['total']} passed={results['passed']} "
        f"failed={results['failed']} skipped={results['skipped']} "
        f"inconclusive={results['inconclusive']} duration={results['duration']}s"
    )

    if results["total"] == 0:
        print("No tests were discovered for this run.")

    if results["failures"]:
        print("Failing tests:")
        for failure in results["failures"]:
            print(f"- {failure['name']}")
            if failure["message"]:
                print(f"  {failure['message']}")

    return exit_code == 0 and results["failed"] == 0 and results["result"] != "Failed"


def main() -> int:
    args = parse_args()
    project_root = args.project.expanduser().resolve()
    logs_dir = args.logs_dir.expanduser().resolve()

    editor_version = read_editor_version(project_root)
    unity_executable = find_unity_executable(editor_version, args.unity)

    overall_success = True
    use_open_editor = not args.force_batchmode and is_unity_editor_open(project_root)
    if use_open_editor:
        print("Open Unity editor detected; requesting in-editor test runs.")

    for platform in resolve_platforms(args.platform):
        try:
            if use_open_editor:
                exit_code, log_path, results_path = run_platform_tests_in_editor(
                    project_root,
                    logs_dir,
                    platform,
                    args.test_filter,
                    args.editor_timeout,
                )
            else:
                exit_code, log_path, results_path = run_platform_tests(
                    unity_executable,
                    project_root,
                    logs_dir,
                    platform,
                    args.test_filter,
                )
        except TimeoutError as exception:
            print(str(exception))
            overall_success = False
            continue

        results = parse_results(results_path)
        platform_success = print_summary(
            project_root,
            editor_version,
            unity_executable,
            platform,
            log_path,
            results_path,
            results,
            exit_code,
        )

        if not platform_success:
            overall_success = False

    return 0 if overall_success else 1


if __name__ == "__main__":
    sys.exit(main())
