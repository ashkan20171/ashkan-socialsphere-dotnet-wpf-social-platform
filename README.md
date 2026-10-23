<div align="center">

# ✦ Ashkan SocialSphere

### A thoughtfully engineered social experience for Windows.

**A bilingual, local-first social networking desktop application built with C# · .NET 10 · WPF · EF Core · SQLite**

[![C#](https://img.shields.io/badge/C%23-C%23-512BD4?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-0078D4)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Status](https://img.shields.io/badge/Status-Portfolio%20Prototype-F59E0B)](#project-status--limitations)

**[Explore Features](#what-you-can-do) · [Architecture](#architecture--engineering-decisions) · [Get Started](#getting-started) · [Engineering Highlights](#engineering-highlights) · [Deutsch](#für-recruiter--hiring-manager)**

</div>

---

## The idea

**What if a desktop social app felt as considered as a modern web product—without depending on a cloud backend?**

Ashkan SocialSphere explores that question through a Windows-native experience combining social feeds, personal profiles, media-rich posts, conversations and notifications. Beyond the interface, it is a practical demonstration of **C#/.NET application design, persistence, separation of concerns, ownership-aware data operations and multilingual UX**.

> **Project at a glance:** A local-first Windows desktop portfolio prototype—not a production social network or multi-device messaging service.

## Product preview

> 📸 **Screenshots coming soon.** Add real screenshots captured from a successful Windows build here before sharing with recruiters. Suggested captures: Feed, Profile & Gallery, Messaging, and Persian RTL mode.

<!-- Replace these placeholders with actual verified screenshots, e.g. docs/screenshots/feed.png. -->

## What you can do

| Experience | Capabilities |
| --- | --- |
| **Discover & connect** | Social feed, following feed, people discovery, follow relationships |
| **Create & interact** | Text/image posts, reactions, comments, and post previews |
| **Own your content** | Edit and delete your own posts; update attached images; ownership checks at the repository boundary |
| **Express yourself** | Profile avatar, cover image, biography, personal posts and photo gallery |
| **Stay in touch** | Direct messages, unread indicators, notifications and periodic chat refresh |
| **Work bilingually** | English **LTR** and Persian **RTL** interface support |
| **Make it yours** | Dark/light themes and a responsive desktop shell |
| **Keep a draft** | On-device text draft save/restore for unfinished posts |

**Media workflow:** Up to four images per post, with full-size preview. Removing an image from a post removes its reference, **not necessarily the original file on disk**. Drafts currently save text only.

## Architecture & engineering decisions

```text
AshkanSocialSphere.slnx
├── src/
│   ├── AshkanSocialSphere.Domain          # Domain models and rules
│   ├── AshkanSocialSphere.Application     # Contracts and application boundaries
│   ├── AshkanSocialSphere.Infrastructure  # EF Core / SQLite persistence
│   └── AshkanSocialSphere.Desktop         # WPF UI and view models
├── tests/                                # Domain and integration checks
├── scripts/                              # Build, release and audit helpers
├── installer/                            # Windows installer definition
└── .github/workflows/                    # Windows validation workflow
```

The solution separates **domain concepts**, **application contracts**, **data access** and **presentation**. This keeps UI concerns from becoming database concerns and makes individual layers easier to inspect and evolve.

### Engineering highlights

- **C# / .NET 10 / WPF:** Native Windows UI and view-model-driven presentation.
- **EF Core + SQLite:** Structured local persistence without requiring a separately hosted database server.
- **Authorization boundaries:** Post editing/deletion ownership is checked in repository operations, not only by hiding UI actions.
- **Internationalisation:** English LTR and Persian RTL require attention to flow direction, alignment and translated interface strings.
- **User-focused details:** Unread badges, empty states, media previews, editing feedback and draft recovery.
- **Delivery tooling:** Windows build workflow, validation scripts and installer definition included in the repository.

### Tech stack

| Area | Technology |
| --- | --- |
| Language | C# |
| Runtime | .NET 10 |
| Desktop UI | Windows Presentation Foundation (WPF), XAML |
| Persistence | Entity Framework Core, SQLite |
| Architecture | Layered solution with domain, application, infrastructure and desktop projects |
| Quality | Domain/integration check projects, Windows workflow and audit scripts |
| Distribution | Windows publishing scripts and Inno Setup definition |

## Getting started

**Requirements:** Windows 10/11, [.NET 10 SDK](https://dotnet.microsoft.com/download), and optionally Visual Studio with the **.NET desktop development** workload.

```powershell
# Clone your repository after publishing it to GitHub
# git clone https://github.com/<YOUR-USERNAME>/ashkan-socialsphere-dotnet-wpf-social-platform.git
# cd ashkan-socialsphere-dotnet-wpf-social-platform

dotnet restore AshkanSocialSphere.slnx
dotnet build AshkanSocialSphere.slnx -c Release
dotnet run --project src/AshkanSocialSphere.Desktop
```

### Verification commands

```powershell
dotnet run --project tests/AshkanSocialSphere.DomainChecks
dotnet run --project tests/AshkanSocialSphere.IntegrationChecks
pwsh -File scripts/verify-release.ps1
pwsh -File scripts/audit-nuget.ps1
```

> The check projects are executable verification projects; do not assume they are `dotnet test` test projects. Review command output before describing a build or check as passing.

### Local data & privacy

The application stores local user data under `%LOCALAPPDATA%\AshkanSocialSphere`. Back up this directory before significant upgrades. This prototype does **not** provide cloud synchronization, cross-device messaging or hosted user accounts.

## Project status & limitations

This repository is a **portfolio prototype under active development**. Transparency is part of the engineering story:

- **Windows compilation and interactive WPF testing have not yet been verified in the current authoring environment.** Run the commands above and confirm the workflow before publishing a release.
- A **NuGet `NU1903` security advisory** has been reported and requires dependency auditing/remediation before distribution.
- Messages and other data are **local-first**, not synchronized between devices.
- Text drafts do not persist selected image attachments.
- Removing image references does not automatically clean up source files on disk.
- Installer scripts are included, but a successful installer build/install must be independently verified on Windows.

**Roadmap:** strengthen Windows CI and dependency security, add automated UI coverage, improve media lifecycle cleanup, and evaluate an optional remote API/sync layer.

## Für Recruiter & Hiring Manager

**Ashkan SocialSphere** ist ein zweisprachiger Windows-Desktop-Prototyp, entwickelt mit **C#, .NET 10, WPF, Entity Framework Core und SQLite**. Das Projekt zeigt die praktische Umsetzung einer mehrschichtigen Softwarearchitektur, datenbankgestützter Funktionen, benutzerbezogener Berechtigungsprüfungen sowie einer Oberfläche mit **Englisch (LTR) und Persisch (RTL)**.

**Technische Schwerpunkte:** C#/.NET-Entwicklung · WPF/XAML · EF Core · SQLite · Softwarearchitektur · UI/UX · Internationalisierung · Wartbarkeit.

*Hinweis: Es handelt sich um ein lokal betriebenes Portfolio-Projekt, nicht um eine produktiv betriebene Cloud-Plattform.*

## About the developer

Created by **Ashkan Motaei** as a hands-on software engineering portfolio project, with a focus on desktop product development, maintainable architecture, and thoughtful bilingual interfaces.

**GitHub:** [ashkan20171](https://github.com/ashkan20171)

## License

No open-source license has been selected yet. **Source visibility does not automatically grant permission to reuse or redistribute the code.** Add a `LICENSE` file if you intend to allow third-party use.

---

<div align="center">

**Built to demonstrate how product thinking and software engineering meet.**

*C# · .NET · WPF · EF Core · SQLite · English/Persian UX*

</div>
