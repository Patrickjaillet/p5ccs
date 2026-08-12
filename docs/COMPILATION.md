# Compilation Guide

## Prerequisites

- Windows 10 (1809+) or Windows 11, x64 or ARM64
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS)
- Visual Studio 2022 (17.8+) with the ".NET desktop development" workload, or
  the `dotnet` CLI alone
- Git

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
dotnet publish src/P5CCS.App/P5CCS.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

Replace `win-x64` with `win-arm64` for the ARM64 build.

## Building the Windows Installer

The Inno Setup 7 script for the Windows installer is located at
`installers/windows/setup.iss` (added in a later development phase). It
packages the self-contained publish output together with the embedded
WebView2 Fixed Version Runtime.

## Notes

- The project is offline-first: no build step requires network access beyond
  the initial NuGet restore.
- Solution targets `net8.0-windows` exclusively; Linux/macOS are not
  supported (WPF/WPF-UI are Windows-only technologies).
