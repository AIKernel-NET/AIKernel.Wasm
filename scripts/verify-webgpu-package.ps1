param(
    [string]$Configuration = "Release",
    [int]$LocalPackageBuildNumber = 1,
    [string]$OutputDirectory,
    [switch]$SkipPack
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src/Compute/WebGpuComputeProvider/WebGpuComputeProvider.csproj"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts/package-smoke/v0.1.3-dev$LocalPackageBuildNumber"
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

if (-not $SkipPack) {
    $localFeed = Join-Path $repoRoot "../artifacts/local-nuget/v0.1.3-dev$LocalPackageBuildNumber"
    dotnet pack $projectPath `
        --configuration $Configuration `
        --output $OutputDirectory `
        /p:UseLocalPackageVersion=true `
        /p:LocalPackageBuildNumber=$LocalPackageBuildNumber `
        --source $localFeed `
        --source "https://api.nuget.org/v3/index.json"
}

$packagePath = Get-ChildItem -Path $OutputDirectory -Filter "AIKernel.Wasm.WebGpuComputeProvider.*.nupkg" |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if ($null -eq $packagePath) {
    throw "AIKernel.Wasm.WebGpuComputeProvider package was not produced in $OutputDirectory"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath.FullName)
try {
    $requiredEntries = @(
        "providers/webgpu.provider.json",
        "runtime/browser/webgpu-rev3-envelope-bridge.js",
        "shaders/hud/hud-composite.rev3.wgsl",
        "shaders/aisthesis/aisthesis.rev3.wgsl",
        "shaders/spatial/spatial-reasoning.rev3.wgsl",
        "buffers/layouts/gpu-layouts.rev3.json",
        "samples/vector-add.wgsl"
    )

    foreach ($entryName in $requiredEntries) {
        $entry = $zip.GetEntry($entryName)
        if ($null -eq $entry) {
            throw "Missing package entry: $entryName"
        }

        Write-Host "package-entry: $entryName ok"
    }
}
finally {
    $zip.Dispose()
}

Write-Host "package: $($packagePath.FullName)"
Write-Host "AIKernel.Wasm WebGPU rev3 package smoke: ok"
