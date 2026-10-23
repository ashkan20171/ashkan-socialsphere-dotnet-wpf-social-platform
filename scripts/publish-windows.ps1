# Build a self-contained Windows x64 desktop release after passing all checks.
[CmdletBinding()]
param([string]$Version = '0.51.0', [switch]$SkipVerification)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
if ($Version -notmatch '^\d+\.\d+\.\d+([-.][A-Za-z0-9.-]+)?$') { throw 'Version must be a safe SemVer string.' }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw '.NET 10 SDK is required.' }
if (-not (Get-Command Compress-Archive -ErrorAction SilentlyContinue)) { throw 'Compress-Archive is required.' }
Push-Location $root
try {
  if (-not $SkipVerification) {
    & (Join-Path $PSScriptRoot 'verify-release.ps1')
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw 'Release verification failed.' }
  }
  $publish = Join-Path $root 'artifacts/publish/win-x64'
  $archiveDir = Join-Path $root 'artifacts/releases'
  $archive = Join-Path $archiveDir "AshkanSocialSphere-$Version-win-x64.zip"
  if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
  New-Item -ItemType Directory -Force -Path $publish, $archiveDir | Out-Null
  & dotnet publish 'src/AshkanSocialSphere.Desktop/AshkanSocialSphere.Desktop.csproj' -c Release -r win-x64 --self-contained true -p:Version=$Version -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o $publish
  if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }
  if (-not (Test-Path (Join-Path $publish 'AshkanSocialSphere.Desktop.exe'))) { throw 'Expected executable was not generated.' }
  Copy-Item (Join-Path $root 'docs/RELEASE_WINDOWS.md') (Join-Path $publish 'README_RELEASE.md')
  if (Test-Path $archive) { Remove-Item $archive -Force }
  Compress-Archive -Path (Join-Path $publish '*') -DestinationPath $archive -CompressionLevel Optimal
  $hash = (Get-FileHash $archive -Algorithm SHA256).Hash
  "$hash  $(Split-Path $archive -Leaf)" | Set-Content -Encoding ascii "$archive.sha256"
  Write-Host "Release created: $archive"
  Write-Host "SHA256: $hash"
}
finally { Pop-Location }
