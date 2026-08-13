; Inno Setup 7 script for Processing 5 - Creative Coding Station.
;
; Build the self-contained publish output first, then compile with ISCC,
; passing the architecture and its publish directory:
;
;   dotnet publish src/P5CCS.App/P5CCS.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:Platform=x64
;   ISCC.exe /DAppArch=x64 /DPublishDir=..\..\src\P5CCS.App\bin\x64\Release\net8.0-windows\win-x64\publish installers\windows\setup.iss
;
; For ARM64, substitute win-arm64 / ARM64 / bin\ARM64\... throughout.

#ifndef AppArch
  #define AppArch "x64"
#endif

#ifndef PublishDir
  #define PublishDir "..\..\src\P5CCS.App\bin\" + AppArch + "\Release\net8.0-windows\win-" + AppArch + "\publish"
#endif

#define MyAppName "Processing 5 - Creative Coding Station"
#define MyAppExeName "P5CCS.App.exe"

; Read the version straight off the published binary's own file version
; resource (set from Directory.Build.props at build time) instead of
; duplicating it here by hand, where it would silently drift out of sync.
#define MyAppVersion GetVersionNumbersString(PublishDir + "\" + MyAppExeName)

#define MyAppPublisher "Patrick JAILLET"
#define MyAppURL "https://patrickjaillet.github.io/p5ccs"

; A bundled WebView2 Fixed Version Runtime is optional: if present at this
; path (populated separately, see docs/KNOWN-LIMITATIONS.md), it's deployed
; alongside the app and preferred automatically by WebView2RuntimeLocator;
; otherwise the app falls back to the system's Evergreen WebView2 Runtime.
#define WebView2RuntimeSourceDir "..\..\resources\webview2runtime\win-" + AppArch

[Setup]
AppId={{5C9F3B7A-6E2D-4A1F-9B0C-8D2E7F4A1C6B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
; Per-user install (no admin/UAC elevation required), matching the pattern
; used by e.g. Visual Studio Code's "User Installer" — appropriate here
; since P5CCS is a single-user creative tool with no system-wide component
; (no services, no shared program data, no all-users registration needed).
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\..\dist
OutputBaseFilename=P5CCS-Setup-{#MyAppVersion}-{#AppArch}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\..\resources\icons\app.ico
WizardImageFile=..\..\resources\icons\wizard-image.bmp
WizardSmallImageFile=..\..\resources\icons\wizard-small.bmp
LicenseFile=..\..\LICENSE
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed={#AppArch=="arm64" ? "arm64" : "x64compatible"}
ArchitecturesInstallIn64BitMode={#AppArch=="arm64" ? "arm64" : "x64compatible"}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
#ifexist WebView2RuntimeSourceDir
Source: "{#WebView2RuntimeSourceDir}\*"; DestDir: "{app}\WebView2Runtime"; Flags: ignoreversion recursesubdirs createallsubdirs
#endif

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
