param(
    [string]$DataRoot = ''
)

$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$executable = Join-Path $repository 'src\DeskBox\bin\x64\Debug\net10.0-windows10.0.22621.0\win-x64\DeskBox.exe'

if (-not (Test-Path -LiteralPath $executable)) {
    throw "DeskBox Debug build was not found: $executable"
}

if ([string]::IsNullOrWhiteSpace($DataRoot)) {
    $DataRoot = Join-Path $repository '.local-preview\theme-desktop'
}
$resolvedDataRoot = [System.IO.Path]::GetFullPath($DataRoot)
[System.IO.Directory]::CreateDirectory($resolvedDataRoot) | Out-Null
$env:DESKBOX_DEV_DATA_ROOT = $resolvedDataRoot

Start-Process -FilePath $executable -WorkingDirectory (Split-Path $executable)
