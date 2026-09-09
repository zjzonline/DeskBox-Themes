[CmdletBinding()]
param(
    [ValidateSet('Debug')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $repository 'src\DeskBox\DeskBox.csproj'
$sdkWorkingDirectory = Split-Path $repository -Parent
$runtimeIdentifier = 'win-x64'
$framework = 'net10.0-windows10.0.22621.0'
$outputDirectory = Join-Path $repository (
    "src\DeskBox\bin\x64\$Configuration\$framework\$runtimeIdentifier")

$previousToolchain = [Environment]::GetEnvironmentVariable('RUSTUP_TOOLCHAIN', 'Process')
try {
    # The repository toolchain also declares ARM64 components. Pinning the installed
    # x64 toolchain prevents a desktop build from waiting on unrelated downloads.
    $env:RUSTUP_TOOLCHAIN = '1.96.0-x86_64-pc-windows-msvc'

    Push-Location $sdkWorkingDirectory
    try {
        & dotnet build $project `
            -c $Configuration `
            --no-restore `
            -p:Platform=x64 `
            -p:RuntimeIdentifier=$runtimeIdentifier `
            -p:WindowsAppSDKSelfContained=true `
            -p:DeskBoxRustNative=true `
            -p:DeskBoxShellThumbnailProxy=true `
            -p:NuGetAudit=false
        if ($LASTEXITCODE -ne 0) {
            throw "DeskBox build failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($null -eq $previousToolchain) {
        Remove-Item Env:RUSTUP_TOOLCHAIN -ErrorAction SilentlyContinue
    }
    else {
        $env:RUSTUP_TOOLCHAIN = $previousToolchain
    }
}

$requiredRuntimeFiles = @(
    (Join-Path $outputDirectory 'DeskBox.exe'),
    (Join-Path $outputDirectory 'deskbox_native.dll'),
    (Join-Path $outputDirectory 'DeskBox.ThumbnailProxy.exe')
)
$missingRuntimeFiles = @(
    $requiredRuntimeFiles | Where-Object { -not (Test-Path -LiteralPath $_) }
)
if ($missingRuntimeFiles.Count -gt 0) {
    $missingNames = ($missingRuntimeFiles | ForEach-Object { Split-Path $_ -Leaf }) -join ', '
    throw "DeskBox build is incomplete; missing runtime files: $missingNames"
}

Write-Host ''
Write-Host 'DeskBox theme desktop build is complete.' -ForegroundColor Green
Write-Host "Output: $outputDirectory"
Write-Host 'Verified: DeskBox.exe, deskbox_native.dll, DeskBox.ThumbnailProxy.exe'
