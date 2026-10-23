# Windows PowerShell 7+ / .NET 10 SDK. Stops on any failed check.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    function Invoke-DotNet([string[]]$Arguments) {
        & dotnet @Arguments
        if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE" }
    }
    Invoke-DotNet @('restore','AshkanSocialSphere.slnx','--force-evaluate')
    & (Join-Path $PSScriptRoot 'audit-nuget.ps1')
    if (-not $?) { throw 'NuGet audit failed' }
    Invoke-DotNet @('build','AshkanSocialSphere.slnx','-c','Release','--no-restore')
    Invoke-DotNet @('run','--project','tests/AshkanSocialSphere.DomainChecks','-c','Release','--no-build')
    Invoke-DotNet @('run','--project','tests/AshkanSocialSphere.IntegrationChecks','-c','Release','--no-build')
    Write-Host 'All release checks passed on this machine.'
}
finally { Pop-Location }
