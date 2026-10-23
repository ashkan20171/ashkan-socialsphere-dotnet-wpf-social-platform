[CmdletBinding()]
param([string]$Version='0.51.0', [string]$IsccPath='', [switch]$SkipVerification)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=Split-Path -Parent $PSScriptRoot
if ($Version -notmatch '^\d+\.\d+\.\d+([-.][A-Za-z0-9.-]+)?$') { throw 'Invalid version.' }
& (Join-Path $PSScriptRoot 'publish-windows.ps1') -Version $Version -SkipVerification:$SkipVerification
if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw 'Publish failed.' }
if (-not $IsccPath) {
 $candidate=Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'
 if (Test-Path $candidate) { $IsccPath=$candidate }
 else { $cmd=Get-Command ISCC.exe -ErrorAction SilentlyContinue; if ($cmd) { $IsccPath=$cmd.Source } }
}
if (-not $IsccPath -or -not (Test-Path $IsccPath)) { throw 'Inno Setup 6 ISCC.exe not found. Install it or specify -IsccPath.' }
Push-Location $root
try {
 & $IsccPath "/DAppVersion=$Version" 'installer/AshkanSocialSphere.iss'
 if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
 $output=Join-Path $root "artifacts/installer/AshkanSocialSphere-$Version-win-x64-setup.exe"
 if (-not (Test-Path $output)) { throw "Installer missing: $output" }
 $hash=(Get-FileHash $output -Algorithm SHA256).Hash
 "$hash  $(Split-Path $output -Leaf)" | Set-Content -Encoding ascii "$output.sha256"
 Write-Host "Installer created: $output"
} finally { Pop-Location }
