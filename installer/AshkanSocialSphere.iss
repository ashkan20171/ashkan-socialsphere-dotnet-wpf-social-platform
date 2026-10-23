; Compile on Windows using Inno Setup 6 after scripts/publish-windows.ps1.
; Usage: ISCC.exe /DAppVersion=0.42.0 installer\AshkanSocialSphere.iss
#ifndef AppVersion
#define AppVersion "0.51.0"
#endif
#define AppName "Ashkan SocialSphere"
#define AppExe "AshkanSocialSphere.Desktop.exe"
[Setup]
AppId={{C63AF1F8-A65A-4C41-8C2D-9A44E67B1E51}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Ashkan
DefaultDirName={localappdata}\Programs\AshkanSocialSphere
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
OutputDir=..\artifacts\installer
OutputBaseFilename=AshkanSocialSphere-{#AppVersion}-win-x64-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\AshkanSocialSphere.Desktop\Assets\SocialSphere.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName={#AppName}
[Files]
Source: "..\artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon
[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"
[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
; User database lives in LocalAppData\AshkanSocialSphere, not the install folder.
; Intentionally do not remove user data on uninstall.
