param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

$repositoryRootPath = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$outputPath = if (Test-Path -LiteralPath $OutputDirectory) {
    (Resolve-Path -LiteralPath $OutputDirectory).Path
} else {
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    (Resolve-Path -LiteralPath $OutputDirectory).Path
}

function Test-IsWindows {
    [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)
}

function Resolve-ImageMagick {
    $magickCommand = Get-Command magick -ErrorAction SilentlyContinue
    if ($magickCommand) {
        return $magickCommand.Source
    }

    $convertCommand = Get-Command convert -ErrorAction SilentlyContinue
    if ($convertCommand -and -not (Test-IsWindows)) {
        return $convertCommand.Source
    }

    throw "ImageMagick was not found. Install ImageMagick so the installer artwork can be generated."
}

function Join-RepositoryPath {
    param([string[]]$Parts)

    $path = $repositoryRootPath
    foreach ($part in $Parts) {
        $path = Join-Path $path $part
    }

    $path
}

$magick = Resolve-ImageMagick
$runningOnWindows = Test-IsWindows
$titleFont = if ($runningOnWindows) { 'Arial-Bold' } else { 'DejaVu-Sans-Bold' }
$bodyFont = if ($runningOnWindows) { 'Arial' } else { 'DejaVu-Sans' }
$backgroundCandidates = @(
    @('Assets', 'Resources', 'Backgrounds', 'synthwave.jpg'),
    @('Assets', 'Resources', 'Backgrounds', 'city.png'),
    @('Assets', 'Resources', 'Backgrounds', 'tunnel.png')
)

$background = $backgroundCandidates |
    ForEach-Object { Join-RepositoryPath -Parts $_ } |
    Where-Object { Test-Path -LiteralPath $_ } |
    Select-Object -First 1

if (-not $background) {
    throw "No installer background asset found under Assets\Resources\Backgrounds."
}

$dialog = Join-Path $outputPath 'Dialog.bmp'
$banner = Join-Path $outputPath 'Banner.bmp'
$iconPng = Join-Path $outputPath 'Product.png'
$icon = Join-Path $outputPath 'Product.ico'

& $magick @(
    $background,
    '-resize', '493x312^',
    '-gravity', 'center',
    '-extent', '493x312',
    '-fill', '#120914B8',
    '-draw', 'rectangle 0,0 493,312',
    '-fill', '#03EDF9',
    '-draw', 'rectangle 0,0 8,312',
    '-fill', '#FC28A8',
    '-draw', 'rectangle 8,0 15,312',
    '-fill', '#FDFDFD',
    '-font', $titleFont,
    '-pointsize', '34',
    '-gravity', 'northwest',
    '-annotate', '+34+32', "BRICKWAVE '84",
    '-fill', '#FEDE5D',
    '-font', $bodyFont,
    '-pointsize', '17',
    '-annotate', '+36+78', 'PRESS START. BREAK THE GRID.',
    $dialog
)

& $magick @(
    $background,
    '-resize', '493x58^',
    '-gravity', 'center',
    '-extent', '493x58',
    '-fill', '#120914CC',
    '-draw', 'rectangle 0,0 493,58',
    '-fill', '#03EDF9',
    '-draw', 'rectangle 0,52 493,58',
    '-fill', '#FF7EDB',
    '-font', $titleFont,
    '-pointsize', '20',
    '-gravity', 'west',
    '-annotate', '+18+0', "BRICKWAVE '84",
    $banner
)

& $magick @(
    $background,
    '-resize', '256x256^',
    '-gravity', 'center',
    '-extent', '256x256',
    '-fill', '#120914A8',
    '-draw', 'rectangle 0,0 256,256',
    '-fill', '#03EDF9',
    '-draw', 'roundrectangle 34,91 222,139 8,8',
    '-fill', '#FF7EDB',
    '-draw', 'roundrectangle 58,153 198,189 8,8',
    '-fill', '#FEDE5D',
    '-draw', 'circle 128,62 128,82',
    $iconPng
)

& $magick @(
    $iconPng,
    '-define', 'icon:auto-resize=256,128,64,48,32,16',
    $icon
)
