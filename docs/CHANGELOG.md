# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.7.0] - 2026-08-12

### Added

- `p5.sound.min.js` (v1.0.1, matching the vendored p5.js v1.11.3) embedded
  as a .NET resource and served locally alongside `p5.min.js` — Oscillator,
  SoundFile, Amplitude, FFT, Envelope, Noise, Delay, Reverb, and Filter all
  confirmed working at runtime.
- Local asset loading: `LocalSketchServer` now serves static files (images,
  fonts, JSON, CSV, audio, GLSL, etc.) from the sketch's own directory via
  a new `AssetDirectory` property, with path-traversal protection
  (`ResolveAssetPath`) and MIME-type mapping. `SketchTabViewModel.FilePath`
  now propagates the sketch's folder to `IP5jsEngineHost.SetAssetDirectory`
  whenever a sketch is opened, saved-as, or its engine is (re)attached.
- New "API Reference" tab (`ApiReferencePanel` / `ApiReferenceViewModel`) in
  every sketch's side panel: a searchable, category-grouped, fully offline
  browser for the p5.js API. `P5ApiCatalog` (`P5CCS.Editor`) expanded from
  73 to ~120 entries and gained a `Category` field, adding full coverage of
  Sound, WEBGL/3D, Vector & Data (Vector/Table/TypedDict), and DOM, on top
  of the existing 2D Core/Math/Typography/Events/Constants entries.
- `docs/validation-sketches/`: a committed manual validation suite, one
  sketch per p5.js module (2D core, Sound, WEBGL, Vector/Table/TypedDict,
  custom GLSL shaders, DOM, local asset loading), each printing
  `VALIDATION:<module>:true` to the console when the module works
  correctly — see its `README.md` for how to run them.
- 6 new xUnit tests for the local asset server (nested paths, MIME
  mapping, missing-directory 404, and path-traversal rejection via a
  directly-testable `LocalSketchServer.ResolveAssetPath`), 3 new tests
  for the expanded API catalog (70 tests solution-wide).

### Fixed

- Confirmed WEBGL/3D rendering (lights, geometry), custom GLSL shaders
  (`createShader`), `p5.Vector`/`p5.Table`/`p5.TypedDict`, and `p5.dom`
  (bundled in core `p5.min.js` for this version, no separate addon needed)
  all work correctly in the embedded WebView2 runtime — validated with
  real running sketches, not just code review.

### Known Issues

- Video/camera texture support (`createCapture` + WEBGL texture) was not
  validated in this phase — camera/microphone access is unavailable in
  the sandboxed test environment used for this session.
- The automated validation suite is currently a manually-run set of
  fixture sketches (`docs/validation-sketches/`), not a headless/CI-
  integrated test harness; building a full WebView2-based automated E2E
  suite remains a future task.
- End-to-end verification of local asset loading through the app's
  native "Save As" file dialog could not be completed via UI automation
  in this session's environment (the dialog reliably failed to appear
  under synthetic input); the underlying server-side logic is covered by
  6 real HTTP-level tests instead.

## [0.6.3] - 2026-08-12

### Fixed

- **Viewport FPS counter and grid overlay were invisible**: both were WPF
  elements (`TextBlock` / `Canvas`) layered as siblings of the `WebView2`
  control inside the same `Grid`. `WebView2` hosts its content in a
  native child surface that always composites on top of ordinary WPF
  visuals regardless of XAML z-order ("airspace"), so neither overlay
  could ever actually be seen. Both are now rendered inside the WebView's
  own HTML page instead (`index.html` / `bridge.js`): a `#hud-fps` div and
  a `#hud-grid` canvas, sized/positioned to track the real sketch canvas
  via `getBoundingClientRect`, both drawn by the browser itself and
  therefore always on top of the sketch's own canvas content. The C#
  `ShowGrid` property now posts a `showGrid` bridge command instead of
  drawing locally, and re-sends its current state on every page `ready`
  so the grid survives a sketch reload.
- **Viewport did not shrink to fit its pane on window resize**: the
  render surface was a fixed 800x450 `Grid` with only a manual
  Ctrl+scroll-wheel zoom; shrinking the containing pane just revealed
  scrollbars instead of scaling the content down. The viewport now
  recomputes a fit-to-container scale on every `SizeChanged` of its
  container and applies it through the existing `ScaleTransform`,
  combined multiplicatively with the manual zoom so Ctrl+scroll still
  works as a zoom-beyond-fit gesture.

## [0.6.2] - 2026-08-12

### Fixed

- **Root-caused the blank/white rendering bug from 0.6.1**: the actual
  cause was never `WindowState` — it was the Explorer `LayoutAnchorablePane`
  still present in `DockingManager` (the same AvalonDock anchorable-content
  hosting defect noted as a "Known Issue" since 0.6.0, previously worked
  around for Sliders/Console by moving them into `SketchTabView`). Its mere
  presence in the layout, even fully collapsed/placeholder, corrupted
  layout/paint for the whole window whenever `WindowState` was `Normal`.
  Confirmed live: at `Normal` size the Explorer content rendered detached
  and overlapping the sketch tab area instead of docked on the left.
  The Explorer pane is now a plain `DockPanel` + `GridSplitter` next to
  `DockingManager` (which now hosts only a `LayoutDocumentPane`, no
  anchorable), matching the pattern already proven for Sliders/Console.
  The forced-maximize-on-launch workaround from 0.6.1 is no longer needed
  and has been removed — the app now renders correctly at any window state,
  including a fresh launch at the default `Normal` size.

## [0.6.1] - 2026-08-12

### Fixed

- **Blank/white rendering at normal window size**: on this environment's
  display/compositor, WPF only ever produced a correct full-tree layout and
  paint pass for the main window while it was in the `Maximized` state —
  `Normal` (restored) window state, at any size, reliably left the
  AvalonEdit `TextView` and the Sliders/Console `TabControl` unpainted
  beyond the first couple of lines, with the underlying data confirmed
  correct via UI Automation. Removing the `Mica` backdrop and forcing a
  programmatic maximize/restore layout pass did not change this: the
  problem tracks `WindowState` itself, not window size, backdrop, or
  invalidation. `MainWindow` now starts `Maximized` by default
  (`MainWindow.xaml`), which reliably avoids the broken state on launch.
- **Phantom code-folding covering large valid code regions while typing**:
  `JsBraceFoldingStrategy` matched `{`/`[` openers against `}`/`]` closers
  on a single stack without checking the bracket type, and did not skip
  string/template-literal or comment content. Transiently unbalanced code
  while typing (e.g. an unclosed `fill(` call) could pop an unrelated
  opening brace from deep inside real code, producing a wildly incorrect
  folding range that AvalonEdit then rendered as a large blank/collapsed
  gap in the middle of the editor — this was the actual cause of a second,
  distinct report of the editor appearing to "mask" its own content even
  with the window maximized. The strategy now tracks bracket type per
  stack entry and skips string/template-literal/line-comment/block-comment
  content while scanning.
- 3 new unit tests for the folding fix (52 tests solution-wide).

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
