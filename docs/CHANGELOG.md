# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.0] - 2026-08-12

### Added

- `SketchSourceAnalyzer` (`P5CCS.Core`): lightweight static analysis of the
  sketch source detecting top-level numeric/boolean variable declarations
  (brace-depth tracked, so nested/function-local declarations are correctly
  excluded), `fill()`/`stroke()`/`background()` literal RGB calls, and
  `// @slider min max [step]` / `// @slider enum a, b, c` annotations that
  override inferred bounds or force an enum control. Contextual bound
  inference (angle/opacity name heuristics, symmetric/positive-value
  fallback).
- Dynamically generated sliders panel (`SlidersPanelViewModel`,
  `SliderItemViewModel`) with per-kind controls: `Slider` for numbers,
  `ui:ToggleSwitch` for booleans, an R/G/B slider triplet with a live color
  swatch for `fill`/`stroke`/`background` calls, and a `ComboBox` for
  `@slider enum`-annotated variables. Sliders are grouped by detected
  category (a preceding `//` comment, or `Variables`/`Colors`/`Flags`) in
  collapsible `Expander` sections.
- Live bidirectional binding: dragging a slider rewrites the exact source
  span in the sketch (debounced) and hot-reloads the running engine; editing
  the code in AvalonEdit re-analyzes and refreshes the sliders, preserving
  per-slider bound overrides and animation state where the same
  name/kind still exists.
- Manual bounds override per slider (min/max fields + "Set" button).
- Programmable slider animation (`SliderAnimator`: oscillate/ramp) driven by
  a dedicated `DispatcherTimer`, pushing live values straight into the
  running p5.js sketch via a new JS bridge `setVariable` command
  (`IP5jsEngineHost.SetGlobalNumber`) — no page reload per animation frame.
- Named preset snapshots (save/apply/delete) plus JSON export/import via
  native file dialogs (`SliderPreset`, `SliderPresetSerializer`).
- 21 new unit tests covering the analyzer, the animator, and preset
  JSON round-tripping (49 tests solution-wide).

### Fixed

- `SketchViewport` could crash with `ObjectDisposedException` on its
  `HttpListener` when AvalonDock's layout system triggered a spurious
  Unload→Load cycle on the WebView2 host; the local HTTP server is now only
  disposed on an explicit tab close (`IP5jsEngineHost` is now `IDisposable`).
- The dedicated Sliders/Console side panels were moved from AvalonDock
  `LayoutAnchorable` panes into the sketch tab's own split view
  (`SketchTabView`): the anchorable content area was never receiving a
  layout pass in this environment (confirmed zero `ActualWidth`/`ActualHeight`
  and no `SizeChanged` regardless of theme, `CanAutoHide`, or tab-selection
  state), so dynamic content placed there — and even the static Explorer
  placeholder — never rendered.

### Known Issues

- The Explorer `LayoutAnchorable` (left dock) is still affected by the
  AvalonDock anchorable content-sizing issue described above. Its content is
  currently just a placeholder with no functional impact, but the real
  Explorer implementation (a future phase) will need the same
  SketchTabView-hosting workaround, or a proper fix/replacement for
  AvalonDock anchorable content hosting.

## [0.5.0] - 2026-08-12

### Added

- `SketchCodeEditor` (`P5CCS.Editor`): full AvalonEdit-based code editor —
  native line numbers, real code folding (`JsBraceFoldingStrategy`, brace/
  bracket-aware), auto-indentation and bracket handling
  (`CSharpIndentationStrategy`), unlimited per-session undo/redo, and
  rectangular/box selection (all native to AvalonEdit).
- Custom JavaScript/p5.js syntax highlighting (`P5JavaScriptDark.xshd` /
  `P5JavaScriptLight.xshd`): keywords, p5.js API functions, constants,
  numbers, strings, and generic call expressions, switched automatically
  with the application theme.
- Contextual p5.js API autocompletion (`CompletionWindow`, curated
  `P5ApiCatalog` of ~80 core functions/constants with signature and
  description) and hover documentation tooltips
  (`TextView.MouseHover`).
- Real-time error indication: squiggly underlines
  (`SquigglyUnderlineRenderer`, a custom `IBackgroundRenderer`) and a
  clickable error margin (`ErrorMargin`) that jumps the caret to the
  offending line — fed by actual runtime errors reported from the WebView2
  engine via the JS bridge (`window.onerror`, `sketch.js:<line>` parsing),
  not a fake/simulated linter.
- Find/replace with regex, case, and whole-word options via AvalonEdit's
  built-in `SearchPanel`.
- Editor font zoom (Ctrl+MouseWheel) and light/dark editor color theme,
  linked live to the application's Fluent theme
  (`ApplicationThemeManager.Changed`).
- Auto-save (configurable interval/enabled in preferences) with crash
  recovery: unsaved sketch content is periodically written to
  `%AppData%\P5CCS\recovery`, and orphaned recovery files from an
  unclean shutdown are automatically reopened as tabs on next startup.
- Debounced hot-reload: edits automatically reload the sketch in the
  Viewport shortly after typing stops (configurable), without needing to
  press Run.
- Runtime console panel wired to real `console.log`/`console.error`/
  uncaught-exception messages streamed from the engine, per sketch tab.
- Split editor + Viewport layout (`SketchTabView`, `GridSplitter`) replacing
  the Viewport-only tab content from Phase 4.
- New `P5CCS.Editor.Tests` project with real logic tests for the folding
  strategy and the p5.js API catalog (32 tests solution-wide).

### Fixed

- The p5.js API completion window now properly commits or dismisses on
  non-identifier characters (missing `TextArea.TextEntering` handler),
  preventing it from getting stuck open and swallowing further input.

## [0.4.0] - 2026-08-12

### Added

- `SketchViewport` (`P5CCS.Engine`): `WebView2`-hosted rendering surface with
  a fixed native 800x450 canvas, Ctrl+MouseWheel zoom via `ScaleTransform`
  (render resolution unaffected), and an optional alignment grid overlay.
- Local, loopback-only HTTP server (`LocalSketchServer`, `HttpListener` bound
  to `127.0.0.1` on a dynamic port) serving the embedded p5.js runtime
  (v1.11.3, bundled offline under `resources/p5js`), a JS/C# bridge script,
  and the active sketch source — zero external network access.
- `IP5jsEngineHost` contract plus a JS bridge (`bridge.js`) that streams FPS,
  console/error messages, and mouse position back to C# via
  `CoreWebView2.PostWebMessageAsJson`, with no C#-side polling: telemetry is
  pushed once per `p5.redraw()` frame.
- Per-tab engine lifecycle wired end-to-end: `Run`/`Pause`/`Stop`/`Reset`
  toolbar and menu commands now drive the real WebView2/p5.js engine of the
  active sketch tab; the status bar reflects that tab's live FPS, engine
  status, and canvas-space mouse position.
- One-click PNG viewport capture (`CoreWebView2.CapturePreviewAsync`) wired
  to the Export > Quick Export command, writing to a user-chosen path.
- "Fullscreen Viewport" mode (`FullscreenViewportWindow`): a dedicated,
  chrome-less, maximized window hosting an independent `SketchViewport`
  instance for the active sketch, closable with Escape.
- A default starter sketch (bouncing ball) so every new tab renders
  something live immediately, ahead of the Phase 5 code editor.
- Frame rate cap toolbar control (15/24/30/60/120), applied via `frameRate()`
  through the JS bridge and reapplied automatically on every sketch (re)start.
- Real integration tests for `LocalSketchServer` (`HttpClient` against the
  live loopback server, no mocks).

## [0.3.5] - 2026-08-12

### Added

- "About" window (`ui:FluentWindow`, modal, accessible from Help > About):
  product name, current SemVer version, copyright notice, `mailto:` contact
  link, clickable website link, MIT license mention with full embedded
  license text, and a complete third-party notices tab covering p5.js,
  WPF-UI, AvalonEdit, AvalonDock, AvalonDock.Themes.WPFUI, WebView2 SDK,
  FFmpegCore/FFmpeg, SixLabors.ImageSharp, CommunityToolkit.Mvvm, Serilog,
  and Microsoft.Extensions.*.
- `docs/THIRD-PARTY-NOTICES.md`, embedded into the application binary and
  bundled with `LICENSE` as read-only resources for offline display.

### Fixed

- `Application.MainWindow` is now explicitly assigned to the real main
  window instead of relying on WPF's implicit "first shown window" behavior,
  which was incorrectly capturing the startup splash screen and crashing
  the app when opening a second window (`Cannot set Owner property to
  itself`).
- `Hyperlink.NavigateUri` bindings now use `Uri`-typed static members
  instead of raw strings via `x:Static`, which bypassed `Uri`'s
  `TypeConverter` and crashed the app with an `XamlParseException`.
- Assembly informational version no longer has a `+<git-sha>` suffix
  auto-appended by the .NET SDK, keeping it strict SemVer as required by
  project conventions (`IncludeSourceRevisionInInformationalVersion=false`).

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

[Unreleased]: https://github.com/Patrickjaillet/p5ccs/compare/v0.6.0...HEAD
[0.6.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.3.5...v0.4.0
[0.3.5]: https://github.com/Patrickjaillet/p5ccs/compare/v0.3.0...v0.3.5
[0.3.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Patrickjaillet/p5ccs/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/Patrickjaillet/p5ccs/releases/tag/v0.1.0
