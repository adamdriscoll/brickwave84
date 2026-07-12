param(
    [switch]$BuildPlayer,

    [string]$GameBuildDir,

    [string]$UnityPath,

    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$installerRoot = Split-Path -Parent $scriptRoot
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $installerRoot '..')).Path
$versionProps = [System.IO.Path]::Combine($installerRoot, 'ProductVersion.props')
$wixProject = [System.IO.Path]::Combine($installerRoot, 'Brickwave84.wixproj')
$outputDir = [System.IO.Path]::Combine($repositoryRoot, 'Builds', 'Installer')

if (-not $GameBuildDir) {
    $GameBuildDir = [System.IO.Path]::Combine($repositoryRoot, 'Builds', 'Release', 'Brickwave84')
}

$gameBuildPath = [System.IO.Path]::GetFullPath($GameBuildDir)
$versionDocument = [xml](Get-Content -Raw -LiteralPath $versionProps)
$productVersion = $versionDocument.Project.PropertyGroup.ProductVersion

if (-not $productVersion) {
    throw "ProductVersion is missing from $versionProps."
}

function Resolve-UnityPath {
    param([string]$RequestedUnityPath)

    if ($RequestedUnityPath) {
        $resolved = (Resolve-Path -LiteralPath $RequestedUnityPath).Path
        if (Test-Path -LiteralPath $resolved -PathType Leaf) {
            return $resolved
        }
    }

    if ($env:UNITY_EXE -and (Test-Path -LiteralPath $env:UNITY_EXE -PathType Leaf)) {
        return $env:UNITY_EXE
    }

    $projectVersion = Get-Content -Raw -LiteralPath ([System.IO.Path]::Combine($repositoryRoot, 'ProjectSettings', 'ProjectVersion.txt'))
    $editorVersion = if ($projectVersion -match 'm_EditorVersion:\s*(\S+)') { $Matches[1] } else { $null }
    if ($editorVersion) {
        $hubPath = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
        if (Test-Path -LiteralPath $hubPath -PathType Leaf) {
            return $hubPath
        }
    }

    $command = Get-Command Unity.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "Unity.exe was not found. Pass -UnityPath or set UNITY_EXE."
}

function Clear-PlayerBuildDirectory {
    param([string]$TargetPath)

    $buildsRoot = [System.IO.Path]::GetFullPath(([System.IO.Path]::Combine($repositoryRoot, 'Builds')))
    if (-not $TargetPath.StartsWith($buildsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean '$TargetPath' because it is outside '$buildsRoot'."
    }

    if (Test-Path -LiteralPath $TargetPath) {
        Remove-Item -LiteralPath $TargetPath -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null
}

function Remove-UnityNonShippingDirectories {
    param([string]$TargetPath)

    $buildsRoot = [System.IO.Path]::GetFullPath(([System.IO.Path]::Combine($repositoryRoot, 'Builds')))
    if (-not $TargetPath.StartsWith($buildsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean Unity non-shipping folders in '$TargetPath' because it is outside '$buildsRoot'."
    }

    Get-ChildItem -LiteralPath $TargetPath -Directory -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -like '*_BackUpThisFolder_ButDontShipItWithYourGame' -or
            $_.Name -like '*_BurstDebugInformation_DoNotShip'
        } |
        Remove-Item -Recurse -Force
}

function Join-ProcessArguments {
    param([string[]]$Arguments)

    ($Arguments | ForEach-Object {
        '"' + ($_ -replace '"', '\"') + '"'
    }) -join ' '
}

if ($BuildPlayer) {
    $unityExe = Resolve-UnityPath -RequestedUnityPath $UnityPath
    Clear-PlayerBuildDirectory -TargetPath $gameBuildPath

    $unityLog = Join-Path $outputDir 'unity-player-build.log'
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    $unityArguments = @(
        '-batchmode',
        '-quit',
        '-projectPath', $repositoryRoot,
        '-executeMethod', 'GetBricked.EditorTools.VsCodeBuildTargets.BuildWindowsPlayer',
        '-vscodeBuildPath', ([System.IO.Path]::Combine($gameBuildPath, 'Brickwave84.exe')),
        '-logFile', $unityLog
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $unityExe
    $startInfo.Arguments = Join-ProcessArguments -Arguments $unityArguments
    $startInfo.UseShellExecute = $false

    $unityProcess = [System.Diagnostics.Process]::Start($startInfo)
    $unityProcess.WaitForExit()

    if ($unityProcess.ExitCode -ne 0) {
        throw "Unity player build failed with exit code $($unityProcess.ExitCode). See $unityLog."
    }
}

$gameExe = [System.IO.Path]::Combine($gameBuildPath, 'Brickwave84.exe')
if (-not (Test-Path -LiteralPath $gameExe -PathType Leaf)) {
    throw "Brickwave player build not found at $gameExe. Re-run with -BuildPlayer or pass -GameBuildDir."
}

Remove-UnityNonShippingDirectories -TargetPath $gameBuildPath

$doNotShipFolders = Get-ChildItem -LiteralPath $gameBuildPath -Directory -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -like '*_BackUpThisFolder_ButDontShipItWithYourGame' -or
        $_.Name -like '*_BurstDebugInformation_DoNotShip'
    }

if ($doNotShipFolders) {
    $folderList = ($doNotShipFolders | ForEach-Object { $_.FullName }) -join ', '
    throw "Refusing to package Unity debug folders: $folderList. Build a clean player with -BuildPlayer."
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$outputPathArgument = $outputDir
if (-not $outputPathArgument.EndsWith([System.IO.Path]::DirectorySeparatorChar.ToString())) {
    $outputPathArgument += [System.IO.Path]::DirectorySeparatorChar
}

dotnet build $wixProject `
    -c $Configuration `
    -p:GameBuildDir="$gameBuildPath" `
    -p:OutputPath="$outputPathArgument"

if ($LASTEXITCODE -ne 0) {
    throw "MSI build failed with exit code $LASTEXITCODE."
}

$msiPath = [System.IO.Path]::Combine($outputDir, "Brickwave84-$productVersion.msi")
if (-not (Test-Path -LiteralPath $msiPath -PathType Leaf)) {
    throw "Expected MSI was not created at $msiPath."
}

Write-Host "Built $msiPath"
