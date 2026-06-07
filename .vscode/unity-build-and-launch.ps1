param(
    [switch]$BuildOnly,
    [switch]$LaunchOnly,
    [switch]$Release,
    [string]$UnityExe = $env:UNITY_EDITOR_PATH
)

$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$buildExe = Join-Path $projectRoot "Builds\Codex\Brickwave84.exe"
$logPath = Join-Path $projectRoot "Logs\VsCodeBuildWindowsPlayer.log"

function Resolve-UnityEditor {
    param([string]$ConfiguredPath)

    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($ConfiguredPath)) {
        $candidates += $ConfiguredPath
    }

    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $candidates += (Join-Path $env:ProgramFiles "Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe")
    }

    if (-not [string]::IsNullOrWhiteSpace(${env:ProgramFiles(x86)})) {
        $candidates += (Join-Path ${env:ProgramFiles(x86)} "Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe")
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    $unityCommand = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($unityCommand) {
        return $unityCommand.Source
    }

    throw "Unity 6000.3.6f1 was not found. Set UNITY_EDITOR_PATH to the full Unity.exe path and try again."
}

function Build-Player {
    $unity = Resolve-UnityEditor $UnityExe
    $method = "GetBricked.EditorTools.VsCodeBuildTargets.BuildWindowsDevelopmentPlayer"
    if ($Release) {
        $method = "GetBricked.EditorTools.VsCodeBuildTargets.BuildWindowsPlayer"
    }

    Stop-ExistingPlayer

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $logPath) | Out-Null
    if (Test-Path -LiteralPath $logPath) {
        Remove-Item -LiteralPath $logPath -Force
    }

    $unityArgs = @(
        "-batchmode",
        "-quit",
        "-projectPath",
        $projectRoot,
        "-executeMethod",
        $method,
        "-vscodeBuildPath",
        $buildExe,
        "-logFile",
        $logPath
    )

    Write-Host "Building Brickwave '84 with $unity"
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $unity
    $startInfo.Arguments = ($unityArgs | ForEach-Object { Convert-ToCommandLineArgument $_ }) -join " "
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    $unityProcess = [System.Diagnostics.Process]::Start($startInfo)
    $unityProcess.WaitForExit()

    if ($unityProcess.ExitCode -ne 0) {
        throw "Unity build failed with exit code $($unityProcess.ExitCode). See $logPath"
    }

    if (-not (Test-Path -LiteralPath $buildExe)) {
        throw "Unity reported success, but the built player was not found at $buildExe. See $logPath"
    }
}

function Stop-ExistingPlayer {
    $existingPlayers = Get-Process Brickwave84 -ErrorAction SilentlyContinue | Where-Object {
        try {
            $_.Path -eq $buildExe
        }
        catch {
            $false
        }
    }

    foreach ($player in $existingPlayers) {
        Write-Host "Stopping existing Brickwave '84 player process $($player.Id)"
        Stop-Process -Id $player.Id -Force
        Wait-Process -Id $player.Id -Timeout 10 -ErrorAction SilentlyContinue
    }
}

function Convert-ToCommandLineArgument {
    param([string]$Value)

    if ($null -eq $Value) {
        return '""'
    }

    $escaped = $Value -replace '(\\*)"', '$1$1\"'
    $escaped = $escaped -replace '(\\+)$', '$1$1'
    return '"' + $escaped + '"'
}

function Launch-Player {
    if (-not (Test-Path -LiteralPath $buildExe)) {
        throw "Built player was not found at $buildExe. Run the build target first."
    }

    Write-Host "Launching $buildExe"
    Start-Process -FilePath $buildExe -WorkingDirectory (Split-Path -Parent $buildExe)
}

if (-not $LaunchOnly) {
    Build-Player
}

if (-not $BuildOnly) {
    Launch-Player
}
