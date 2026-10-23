# Ashkan SocialSphere

**Bilingual desktop social networking platform · C# · .NET 10 · WPF · SQLite**

A portfolio-ready, local-first social application demonstrating desktop product engineering, layered architecture, persistence, secure ownership checks, and thoughtful UX. Designed with **English LTR / Persian RTL**, a polished dark interface, user profiles, media posts, social interactions and direct messaging.

> **Scope:** This is a local-first Windows desktop prototype. It is **not** a deployed cloud social network, and messages do not synchronize across devices. Build and security audit must pass before production distribution.

## Features

- Feed, following feed, reactions, comments, people discovery and follow relationships
- User profiles with avatar, cover, biography, personal posts and photo gallery
- Up to four images per post; full-size preview and owner-only post editing/deletion
- **Phase 51:** edit attached images (add/remove), character count and on-device text draft save/restore
- Direct messaging, unread indicators, notifications and periodic chat refresh
- Local SQLite database with EF Core, domain/application/infrastructure/desktop separation
- English/Persian UI with LTR/RTL switching; dark/light themes and responsive shell
- Windows packaging scripts, installer definition, integration checks and GitHub Actions validation

## Architecture

| Project | Responsibility |
| --- | --- |
| `AshkanSocialSphere.Domain` | Domain models and core rules |
| `AshkanSocialSphere.Application` | Repository contracts and application boundaries |
| `AshkanSocialSphere.Infrastructure` | EF Core + SQLite persistence and data operations |
| `AshkanSocialSphere.Desktop` | WPF UI, view models and presentation |
| `tests/` | Domain and integration verification |

## Getting started (Windows)

**Prerequisites:** Visual Studio with .NET desktop development workload, .NET 10 SDK.

```powershell
dotnet restore AshkanSocialSphere.slnx
dotnet build AshkanSocialSphere.slnx -c Release
dotnet run --project src/AshkanSocialSphere.Desktop
```

Run checks:

```powershell
dotnet run --project tests/AshkanSocialSphere.DomainChecks
dotnet run --project tests/AshkanSocialSphere.IntegrationChecks
pwsh -File scripts/verify-release.ps1
pwsh -File scripts/audit-nuget.ps1
```

Local user data is stored in `%LOCALAPPDATA%\AshkanSocialSphere`. Back up this folder before upgrades. Phase 51 drafts are text-only, saved in `post-draft.txt`; they do not include selected photo attachments. Removed photo references do **not** delete the source image files from disk.

## Quality and security notes

- Owner-only edits/deletes are checked in the SQLite repository, not only in the UI.
- Images are referenced from local disk. There is no cloud upload, distributed auth, or cross-device sync.
- **NuGet `NU1903` advisory remains unresolved/unverified.** Audit and update vulnerable packages before release.
- WPF compilation, visual testing and installer execution were not possible in the authoring environment; Windows CI is the required release gate.

## Recruiter snapshot | Für Recruiter | معرفی پروژه

**EN:** A C#/.NET desktop portfolio project showcasing layered design, EF Core/SQLite, bilingual WPF UX, media workflows and authorization boundaries.

**DE:** Zweisprachige WPF-Desktop-Anwendung mit C#/.NET, SQLite, mehrschichtiger Architektur, Profilen, Beiträgen, Nachrichten und rollenbezogenen Zugriffsprüfungen auf Datenebene.

**FA:** پروژه نمونه‌کار شبکه اجتماعی دسکتاپ با سی‌شارپ، WPF، معماری لایه‌ای، پایگاه داده SQLite و پشتیبانی فارسی و انگلیسی.

## License

No license has been selected. Contact the repository owner for reuse permissions; publishing source code does not automatically grant redistribution rights.

See `PHASE51_NOTES_FA.md` for this release and `docs/` for additional technical notes.
"# ashkan-socialsphere-dotnet-wpf-social-platform" 
