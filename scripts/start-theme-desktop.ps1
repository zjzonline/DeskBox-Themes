param(
    [string]$DataRoot = ''
)

$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$executable = [System.IO.Path]::GetFullPath((
    Join-Path $repository 'src\DeskBox\bin\x64\Debug\net10.0-windows10.0.22621.0\win-x64\DeskBox.exe'))

if (-not (Test-Path -LiteralPath $executable)) {
    throw "DeskBox Debug build was not found: $executable"
}

$runtimeDirectory = Split-Path $executable
$requiredRuntimeFiles = @(
    (Join-Path $runtimeDirectory 'deskbox_native.dll'),
    (Join-Path $runtimeDirectory 'DeskBox.ThumbnailProxy.exe')
)
$missingRuntimeFiles = @(
    $requiredRuntimeFiles | Where-Object { -not (Test-Path -LiteralPath $_) }
)
if ($missingRuntimeFiles.Count -gt 0) {
    $missingNames = ($missingRuntimeFiles | ForEach-Object { Split-Path $_ -Leaf }) -join ', '
    throw "DeskBox Debug build is incomplete; missing native files: $missingNames. Run scripts\build-theme-desktop.ps1."
}

if ([string]::IsNullOrWhiteSpace($DataRoot)) {
    $DataRoot = Join-Path $repository '.local-preview\theme-desktop'
}
$resolvedDataRoot = [System.IO.Path]::GetFullPath($DataRoot)
[System.IO.Directory]::CreateDirectory($resolvedDataRoot) | Out-Null
$env:DESKBOX_DEV_DATA_ROOT = $resolvedDataRoot

$launcherLog = Join-Path $resolvedDataRoot 'DeskBox.ThemeLauncher.log'
function Write-LauncherLog([string]$message) {
    try {
        Add-Content -LiteralPath $launcherLog -Value (
            '[{0:yyyy-MM-dd HH:mm:ss.fff}] {1}' -f [DateTime]::Now, $message)
    }
    catch {
        # Launcher diagnostics must never prevent DeskBox from opening.
    }
}

$legacyRepository = Join-Path (Split-Path $repository -Parent) 'DeskBox-main'
$legacyExecutable = [System.IO.Path]::GetFullPath((
    Join-Path $legacyRepository 'src\DeskBox\bin\x64\Debug\net10.0-windows10.0.22621.0\win-x64\DeskBox.exe'))
$newInstanceRunning = $false

try {
    $runningInstances = @(Get-CimInstance Win32_Process -Filter "Name='DeskBox.exe'" -ErrorAction Stop)
    foreach ($instance in $runningInstances) {
        if ([string]::IsNullOrWhiteSpace($instance.ExecutablePath)) {
            continue
        }

        $runningPath = [System.IO.Path]::GetFullPath($instance.ExecutablePath)
        if ($runningPath -eq $executable) {
            $newInstanceRunning = $true
            Write-LauncherLog "New DeskBox is already running (PID $($instance.ProcessId))."
            continue
        }

        if ($runningPath -eq $legacyExecutable) {
            Write-LauncherLog "Stopping legacy DeskBox (PID $($instance.ProcessId)): $runningPath"
            Stop-Process -Id $instance.ProcessId -ErrorAction Stop
        }
    }
}
catch {
    Write-LauncherLog "Process check failed; continuing with explicit new executable: $($_.Exception.Message)"
}

if ($newInstanceRunning) {
    return
}

$started = Start-Process `
    -FilePath $executable `
    -WorkingDirectory (Split-Path $executable) `
    -PassThru
Write-LauncherLog "Started new DeskBox (PID $($started.Id)): $executable"
