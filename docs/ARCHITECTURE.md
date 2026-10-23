# Architecture overview

Ashkan SocialSphere is a Windows desktop social-network prototype built with C# and .NET 10 WPF.

| Project | Responsibility |
| --- | --- |
| `AshkanSocialSphere.Domain` | Core domain types and invariants |
| `AshkanSocialSphere.Application` | Repository contracts and application-facing models |
| `AshkanSocialSphere.Infrastructure` | SQLite persistence and data access |
| `AshkanSocialSphere.Desktop` | WPF views, UI state, commands and localization |
| `tests/*Checks` | Executable domain and SQLite regression checks |

## Message flow

`WPF View -> MainViewModel -> Application repository interface -> SQLite repository -> local SQLite database`

The messaging UI refreshes periodically and is not backed by a server push channel. Read status is stored in SQLite; this is not a cross-device delivery acknowledgment.

## Engineering notes

- The app uses local accounts and local storage. It is a portfolio prototype, not a production multi-tenant service.
- Persian is RTL and English is LTR; verify both language modes in every new UI screen.
- SQLite integrity checks and regression checks help detect errors, but do not replace backups or a security audit.
- Windows-only WPF code must be compiled and smoke-tested on Windows before publishing.
