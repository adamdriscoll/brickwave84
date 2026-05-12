from __future__ import annotations

import ctypes
import json
import os
import time
import uuid
from pathlib import Path
from typing import Any


def is_unity_editor_open(project_root: Path) -> bool:
    instance_path = project_root / "Library" / "EditorInstance.json"
    if not instance_path.is_file():
        return False

    try:
        data = json.loads(instance_path.read_text(encoding="utf-8"))
        process_id = int(data.get("process_id", 0))
    except (OSError, TypeError, ValueError, json.JSONDecodeError):
        return False

    return process_id > 0 and _is_process_running(process_id)


def request_unity_editor(
    project_root: Path,
    command: str,
    *,
    platform: str = "",
    test_filter: str | None = None,
    results_path: Path | None = None,
    timeout_seconds: int = 180,
) -> dict[str, Any]:
    bridge_root = project_root / "Temp" / "CodexUnityBridge"
    request_dir = bridge_root / "requests"
    response_dir = bridge_root / "responses"
    request_dir.mkdir(parents=True, exist_ok=True)
    response_dir.mkdir(parents=True, exist_ok=True)

    request_id = uuid.uuid4().hex
    response_path = response_dir / f"{request_id}.json"
    request_path = request_dir / f"{request_id}.json"
    payload = {
        "id": request_id,
        "command": command,
        "platform": platform,
        "testFilter": test_filter or "",
        "resultsPath": str(results_path.resolve()) if results_path else "",
    }

    request_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")

    deadline = time.monotonic() + timeout_seconds
    while time.monotonic() < deadline:
        if response_path.is_file():
            try:
                return json.loads(response_path.read_text(encoding="utf-8"))
            finally:
                try:
                    response_path.unlink()
                except OSError:
                    pass

        time.sleep(0.25)

    try:
        request_path.unlink()
    except OSError:
        pass

    raise TimeoutError(
        f"The open Unity editor did not answer the Codex {command} request within {timeout_seconds} seconds. "
        "Wait for Unity to finish importing/compiling and try again."
    )


def _is_process_running(process_id: int) -> bool:
    if os.name == "nt":
        return _is_windows_process_running(process_id)

    try:
        os.kill(process_id, 0)
    except OSError:
        return False
    return True


def _is_windows_process_running(process_id: int) -> bool:
    kernel32 = ctypes.windll.kernel32
    process_query_limited_information = 0x1000
    still_active = 259

    handle = kernel32.OpenProcess(process_query_limited_information, False, process_id)
    if not handle:
        return False

    try:
        exit_code = ctypes.c_ulong()
        if not kernel32.GetExitCodeProcess(handle, ctypes.byref(exit_code)):
            return False
        return exit_code.value == still_active
    finally:
        kernel32.CloseHandle(handle)
