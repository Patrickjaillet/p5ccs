# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0] - 2026-08-12

### Added

- Fluent visual theme (`ui:FluentWindow`, Mica backdrop, rounded corners,
  configurable accent color via `IThemeService`/`WpfThemeService`).
- Main window shell: custom `TitleBar`, full menu bar (File, Edit, View,
  Sketch, Export, Window, Help), toolbar (Run/Pause/Stop/Reset/Export) with
  Fluent `SymbolIcon`s.
- Dockable, resizable panel system via AvalonDock (`Dirkster.AvalonDock`)
  themed with `AvalonDock.Themes.WPFUI`: Explorer/Sliders/Console
  anchorable panels plus a multi-tab sketch document pane (open/close/reorder).
- Light/Dark/System theme selector wired to `ApplicationThemeManager`.
- Configurable global keyboard shortcuts (`IKeyBindingsService`) with
  persisted gesture mapping, applied dynamically to the main window.
- Native dialog service (`IDialogService`/`WpfDialogService`) using
  `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog`/`OpenFolderDialog`.
- Branded startup splash screen shown while the DI container and theme
  initialize.
- Status bar with engine status, FPS, and mouse position placeholders
  (wired for real data once the WebView2 engine lands).
- Unit tests for `KeyBindingsService`.

## [0.2.0] - 2026-08-12

### Added

- Layered application architecture (`App` UI / `Core` / `Engine` / `Editor` / `Export`).
- `App.xaml.cs` entry point bootstrapping a `Microsoft.Extensions.DependencyInjection`
  container, replacing the default `StartupUri` startup path.
- User configuration system (`IUserConfigurationService`) persisting arbitrary
  settings to `%AppData%\P5CCS\config.json`.
- Internal logging system (`Serilog`) with a rolling local file sink
  (`%AppData%\P5CCS\logs`) and an in-memory `IDebugLogSink` for a future
  in-app debug window, integrated with `Microsoft.Extensions.Logging`.
- SemVer version manager (`IVersionService`) reading `AssemblyInformationalVersion`,
  surfaced in the main window title text.
- Persistent user preferences (`IPreferencesService`): theme, UI language, last
  opened project, panel layout, stored in `%AppData%\P5CCS\preferences.json`.
- Project system (`IProjectService`) for the proprietary `.p5ccsproj` JSON format
  (create, open, save).
- Strict MVVM foundation via `CommunityToolkit.Mvvm` (`MainWindowViewModel`
  using `ObservableObject`/`[ObservableProperty]`), bound through DI-resolved
  constructor injection.
- Documented Core \<-\> UI service interfaces: `ISketchService`, `IProjectService`,
  `IExportService`, `IPreferencesService`, `IUserConfigurationService`, `IVersionService`.
- Unit test coverage (xUnit) for configuration, preferences, project, version,
  and logging services.

## [0.1.0] - 2026-08-12

### Added

- Initial project scaffolding: solution structure (`P5CCS.App`, `P5CCS.Core`,
  `P5CCS.Engine`, `P5CCS.Editor`, `P5CCS.Export`) and matching test projects.
- Centralized SemVer versioning via `Directory.Build.props`.
- `WPF-UI`, `Microsoft.Web.WebView2`, and `AvalonEdit` NuGet dependencies wired
  into their respective projects.
- GitHub Actions CI (Windows x64/ARM64 build and test).
- Repository governance files: `LICENSE` (MIT), `CONTRIBUTING.md`,
  `CODE_OF_CONDUCT.md`, issue and pull request templates.

[Unreleased]: https://github.com/Patrickjaillet/p5ccs/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/Patrickjaillet/p5ccs/releases/tag/v0.1.0
