# Run with PowerShell 7+ and .NET 10 SDK. Fail closed if audit data cannot be read.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet restore AshkanSocialSphere.slnx --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw 'NuGet restore failed; audit is inconclusive.' }
    $output = & dotnet list AshkanSocialSphere.slnx package --vulnerable --include-transitive --format json 2>&1
    if ($LASTEXITCODE -ne 0) { throw "NuGet audit failed: $($output -join [Environment]::NewLine)" }
    $json = $output -join "`n"
    try { $report = $json | ConvertFrom-Json -ErrorAction Stop }
    catch { throw 'NuGet audit did not return valid JSON; cannot certify dependencies.' }
    if (-not $report.projects) { throw 'NuGet audit JSON contains no projects; result is inconclusive.' }
    $findings = @()
    $examinedFrameworks = 0
    foreach ($p in $report.projects) {
        foreach ($framework in @($p.frameworks)) {
            if ($null -eq $framework) { continue }
            $examinedFrameworks++
            foreach ($package in @($framework.topLevelPackages) + @($framework.transitivePackages)) {
                if ($null -eq $package) { continue }
                foreach ($v in @($package.vulnerabilities)) {
                    if ($null -ne $v) {
                        $findings += [pscustomobject]@{ Package=$package.id; Version=$package.resolvedVersion; Severity=$v.severity; Advisory=$v.advisoryurl }
                    }
                }
            }
        }
    }
    if ($examinedFrameworks -eq 0) { throw 'No target frameworks returned by NuGet audit; result is inconclusive.' }
    if ($findings.Count -gt 0) {
        $findings | Format-Table -AutoSize | Out-Host
        throw "$($findings.Count) vulnerable NuGet package advisory finding(s). Resolve before release."
    }
    Write-Host "Audit returned no vulnerable packages across $examinedFrameworks target framework(s). Re-run before release and check restore warnings."
}
finally { Pop-Location }
