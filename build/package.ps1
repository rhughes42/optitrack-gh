param(
    [string]$Version = "1.11.0",
    [string]$Configuration = "Release",
    [switch]$IncludeBundledNatNet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$distRoot = Join-Path $repoRoot "dist"
$packageName = "OptiTrack-GH-v$Version"
$packageRoot = Join-Path $distRoot $packageName
$pluginOutDir = Join-Path $repoRoot "src/Tracker/bin/$Configuration"
$zipPath = Join-Path $distRoot "$packageName.zip"

if (Test-Path $packageRoot) { Remove-Item -LiteralPath $packageRoot -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
New-Item -ItemType Directory -Path $packageRoot | Out-Null

# Core plugin payload.
New-Item -ItemType Directory -Path (Join-Path $packageRoot "plugin") | Out-Null
Copy-Item -LiteralPath (Join-Path $pluginOutDir "Tracker.gha") -Destination (Join-Path $packageRoot "plugin/Tracker.gha")

if ($IncludeBundledNatNet) {
    Copy-Item -LiteralPath (Join-Path $repoRoot "lib/NatNet/NatNetML.dll") -Destination (Join-Path $packageRoot "plugin/NatNetML.dll")
    Copy-Item -LiteralPath (Join-Path $repoRoot "lib/NatNet/NatNetLib.dll") -Destination (Join-Path $packageRoot "plugin/NatNetLib.dll")
} else {
    New-Item -ItemType Directory -Path (Join-Path $packageRoot "plugin/sdk-manual-install") | Out-Null
    Set-Content -LiteralPath (Join-Path $packageRoot "plugin/sdk-manual-install/README.txt") -Value @"
NatNet runtime files are not bundled by default in this package.

Install NatNet SDK 4.0 from OptiTrack and place:
- NatNetML.dll
- NatNetLib.dll

next to Tracker.gha in your Grasshopper Libraries folder.
"@
}

# Package documentation and examples.
Copy-Item -LiteralPath (Join-Path $repoRoot "LICENSE") -Destination (Join-Path $packageRoot "LICENSE")
Copy-Item -LiteralPath (Join-Path $repoRoot "docs") -Destination (Join-Path $packageRoot "docs") -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot "examples") -Destination (Join-Path $packageRoot "examples") -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot "build/release-README.txt") -Destination (Join-Path $packageRoot "README.txt")

if (-not (Test-Path $distRoot)) { New-Item -ItemType Directory -Path $distRoot | Out-Null }
Compress-Archive -Path $packageRoot -DestinationPath $zipPath

Write-Host "Created package: $zipPath"

