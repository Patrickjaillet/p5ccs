# Compilation Guide

## Prerequisites

- Windows 10 (1809+) or Windows 11, x64 or ARM64
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS)
- Visual Studio 2022 (17.8+) with the ".NET desktop development" workload, or
  the `dotnet` CLI alone
- Git, with [Git LFS](https://git-lfs.com/) installed and run once via
  `git lfs install` — the embedded `ffmpeg.exe` binaries are tracked via LFS
- [Inno Setup 7](https://jrsoftware.org/isinfo.php) (only needed to build the
  Windows installer, not for `dotnet build`/`test`/`publish`)

The exact SDK version is pinned in [`global.json`](../global.json).

## Restoring Dependencies

NuGet packages are restored automatically on build. To restore explicitly and
populate the local cache for an offline-reproducible build:

```powershell
dotnet restore P5CCS.sln
```

## Building

```powershell
dotnet build P5CCS.sln --configuration Release -p:Platform=x64
```

For ARM64:

```powershell
dotnet build P5CCS.sln --configuration Release -p:Platform=ARM64
```

## Running Tests

```powershell
dotnet test P5CCS.sln --configuration Release
```

## Publishing (Self-Contained)

```powershell
dotnet publish src/P5CCS.App/P5CCS.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:Platform=x64
```

For ARM64 (produces a genuinely native ARM64 build — including the bundled
`ffmpeg.exe`, not an x64 binary running under emulation):

```powershell
dotnet publish src/P5CCS.App/P5CCS.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=false -p:Platform=ARM64
```

Output lands in
`src/P5CCS.App/bin/<Platform>/Release/net8.0-windows/<RID>/publish/`.

## Building the Windows Installer

The Inno Setup 7 script is at `installers/windows/setup.iss`. Publish first
(see above), then compile with `ISCC.exe`, passing the architecture:

```powershell
& "C:\Program Files\Inno Setup 7\ISCC.exe" /DAppArch=x64 installers\windows\setup.iss
```

```powershell
& "C:\Program Files\Inno Setup 7\ISCC.exe" /DAppArch=arm64 installers\windows\setup.iss
```

Both produce `dist\P5CCS-Setup-<version>-<arch>.exe`, where `<version>` is
read directly off the published `P5CCS.App.exe`'s own file version resource
(set from `Directory.Build.props`) rather than duplicated by hand in the
script. The installer bundles a WebView2 Fixed Version Runtime automatically
if one is present at `resources/webview2runtime/win-<arch>/` at compile time
(see [`docs/KNOWN-LIMITATIONS.md`](KNOWN-LIMITATIONS.md) — none is vendored
yet, so installs currently fall back to the system's Evergreen WebView2
Runtime, which `WebView2RuntimeLocator` detects and prefers automatically
once a Fixed Version Runtime folder is added).

The installer performs a per-user install (`PrivilegesRequired=lowest`,
installing to `%LocalAppData%\Programs\...`) and never requires
administrator elevation or a UAC prompt, the same pattern Visual Studio
Code's "User Installer" uses — appropriate since P5CCS has no system-wide
component to register.

## Notes

- The project is offline-first: no build step requires network access beyond
  the initial NuGet restore.
- Solution targets `net8.0-windows` exclusively; Linux/macOS are not
  supported (WPF/WPF-UI are Windows-only technologies).
