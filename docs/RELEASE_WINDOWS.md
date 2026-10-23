# Windows release instructions

## Install and run

1. Extract the entire `AshkanSocialSphere-<version>-win-x64.zip` archive into a folder you own (for example `%LOCALAPPDATA%\Programs\AshkanSocialSphere`).
2. Run `AshkanSocialSphere.Desktop.exe`. Do not move only the executable: companion files are required.
3. This is a Windows x64 desktop application. The package is self-contained and does not require a separately installed .NET runtime.
4. The package is **not code-signed**. Windows SmartScreen may display a warning; verify the SHA-256 checksum and download source before running. Never bypass organization security policy.

## Data and backups

Application data is stored locally using SQLite. Back up `%LOCALAPPDATA%\AshkanSocialSphere` before replacing or uninstalling a version. Publishing does not migrate data on a remote server and does not provide online messaging.

## Developer release procedure

Install Visual Studio with the .NET desktop development workload, .NET 10 SDK, and PowerShell 7. On Windows:

```powershell
pwsh -File scripts/verify-release.ps1
pwsh -File scripts/publish-windows.ps1 -Version 0.41.0
```

The publishing script reruns verification unless `-SkipVerification` is explicitly set. The zip and SHA-256 checksum are written to `artifacts/releases/`. Upload only after checking the CI run, the NuGet audit, and a manual smoke test on a clean Windows x64 machine.

## Smoke test checklist

- Start the executable, register/login, and sign out.
- Switch between Persian RTL and English LTR.
- Create a post, open an image, and navigate between screens.
- Send a message between two test accounts; verify unread badges, read receipts, and reopening conversations.
- Close/reopen the application and verify local data persistence.
- Confirm backups work before upgrading a previous installation.

## Known limitations

- Local SQLite prototype, not a hosted or real-time messaging service.
- No installer, code signing, automatic updates, or online synchronization.
- Vulnerability advisory NU1903 must be resolved from an actual NuGet audit before a public release is called secure.
