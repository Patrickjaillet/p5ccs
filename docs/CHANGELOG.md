# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0.0] - 2026-08-13

First stable release. Everything below is new since `0.9.9-rc.1`; see
earlier entries in this file for the full feature history leading up to
this milestone.

### Added

- Nothing feature-level: this release is exclusively the release-engineering
  work needed to go from release-candidate to a shippable 1.0 (below).

### Changed

- Version bumped to `1.0.0.0` throughout: `Directory.Build.props`
  (`VersionPrefix`/`AssemblyVersion`/`FileVersion`), and `README.md`'s
  badge. Verified end-to-end by reading the compiled `P5CCS.App.exe`'s
  actual `FileVersionInfo` (`FileVersion: 1.0.0.0`, `ProductVersion: 1.0.0`)
  rather than assuming the MSBuild property flowed through correctly.
- `installers/windows/setup.iss` no longer hardcodes its version string —
  `MyAppVersion` is now read directly off the published executable's file
  version resource via ISPP's `GetVersionNumbersString`, so it can never
  drift out of sync with `Directory.Build.props` again (a hardcoded
  `"0.9.5"` was caught still present here during this release, silently
  three releases stale).
- The installer now performs a per-user install (`PrivilegesRequired=lowest`,
  installs to `%LocalAppData%\Programs\...`) instead of requiring
  administrator elevation. Found via real end-to-end installer testing: a
  UAC consent prompt has no way to be answered in a non-interactive
  session, which silently hung and then auto-denied the install (Inno
  Setup exit code 2) — the same failure mode a scripted/unattended
  deployment would hit. Switched to the per-user pattern Visual Studio
  Code's "User Installer" uses, which needs no elevation at all and is a
  better fit anyway since P5CCS has no system-wide component to register.

### Known Issues / Verified

- Both installers (`P5CCS-Setup-1.0.0.0-x64.exe`,
  `P5CCS-Setup-1.0.0.0-arm64.exe`) were compiled for real against genuine
  self-contained `dotnet publish` output for each architecture; the ARM64
  publish output's `P5CCS.App.exe` and bundled `ffmpeg.exe` were both
  confirmed to be native ARM64 PE binaries (not x64-under-emulation).
- The x64 installer was silently installed end-to-end with the new
  per-user flow (no elevation prompt), the installed `P5CCS.App.exe`
  launched from its real install location and confirmed running (FPS: 60
  via UI Automation), then silently uninstalled with a confirmed complete
  removal of the install directory.
- `dotnet test P5CCS.sln -c Release -p:Platform=x64` passes all 108 tests.
- ARM64 real-hardware execution and Windows 10 compatibility remain
  unverified in this development environment, as documented since Phase 9
  in `docs/KNOWN-LIMITATIONS.md` — this has not changed for 1.0.0.0, and
  is called out explicitly here rather than silently glossed over for the
  stable release.
- Feature freeze (declared in `0.9.9-rc.1`) held: no feature-level changes
  in this release, only release engineering.

## [0.9.9-rc.1] - 2026-08-13

### Added

- Host-enforced network isolation for sketch code: `SketchViewport` now
  registers `CoreWebView2.AddWebResourceRequestedFilter("*", ...)` and
  blocks (403) every request that isn't loopback traffic to the sketch's
  own `LocalSketchServer` instance, or a `blob:`/`data:` URI (used
  internally by canvas/Web Audio operations, which never leave the
  process). Verified with a real sketch: a same-origin `fetch('/sketch.js')`
  succeeds (200), an external `fetch('https://example.com/')` fails with
  `Failed to fetch`.
- WebView2 hardening: `AreDefaultContextMenusEnabled`,
  `AreDefaultScriptDialogsEnabled`, `IsStatusBarEnabled`, and
  `IsZoomControlEnabled` disabled unconditionally; `AreDevToolsEnabled`
  and `AreBrowserAcceleratorKeysEnabled` additionally disabled in Release
  builds only (kept available in Debug for engine development) so a
  sketch author can't pop DevTools and step around the network-isolation
  filter above.
- `raf-hook.js`, loaded before `p5.min.js`, patches
  `window.requestAnimationFrame` at the earliest possible point and
  exposes `window.__p5ccsOnFrame(callback)` so `bridge.js` can hook into
  the render loop without racing p5.js's own load-time capture of
  `requestAnimationFrame`.
- `docs/KNOWN-LIMITATIONS.md` and this changelog now document the CSP
  investigation below for future reference.

### Fixed

- A `Content-Security-Policy` response header on `LocalSketchServer`
  silently broke `requestAnimationFrame`-driven rendering in this
  WebView2 environment (canvas got created by `setup()`, but no frame
  ever advanced or got reported — zero console errors, zero CSP
  violations). Root-bisected via `git stash` down to "the mere presence
  of the header," independent of its directives or of any bridge.js
  implementation detail; the underlying WebView2/Chromium mechanism was
  never identified. Resolved by dropping the CSP header entirely and
  reaching the same "no exfiltration" property through host-side request
  filtering instead (see "Added," above), which doesn't exhibit the
  regression and is arguably a stronger boundary since it can't be
  bypassed by any page-level script.

### Known Issues / Verified

- `dotnet test P5CCS.sln` passes all 108 tests (31 Core, 26 Editor, 15
  Engine, 24 Export, 12 App) after the network-isolation and WebView2
  hardening changes.
- Re-ran the Phase 7 feature validation sketch (WEBGL, custom GLSL
  shaders, p5.sound `Oscillator`/`SoundFile`, p5.dom `createDiv`/
  `createButton`) against a freshly launched app with the new
  `WebResourceRequested` filter and hardened WebView2 settings active;
  all five checks (`webgl`, `shader`, `sound`, `dom`,
  `oscillator-construct`) still report `true`, confirmed via UI
  Automation reading the Console panel's actual rendered text — not
  assumed from the code alone.
- The full offline-network audit could not fully eliminate the WebView2
  Evergreen Runtime dependency: no Fixed Version Runtime is vendored in
  this repository (see 0.9.5's Known Issues and
  `docs/KNOWN-LIMITATIONS.md`), so a machine without any Evergreen
  WebView2 install still needs one for the app to run at all. Everything
  the *application itself* fetches at runtime (p5.js, p5.sound.js,
  fonts, FFmpeg binaries) is embedded and local; this limitation is
  scoped strictly to the browser runtime WebView2 itself depends on.
- Final license-compatibility pass over `docs/THIRD-PARTY-NOTICES.md`:
  all listed licenses (MIT, LGPL-2.1, LGPL-3.0, Ms-PL, BSD-2-Clause,
  Apache-2.0, WebView2 SDK terms, Six Labors Split License) remain
  compatible with MIT distribution of the application itself; no new
  third-party dependency was introduced in this phase.
- End-to-end pipeline re-verified post-hardening: pasted a sketch,
  opened the Export dialog, queued and ran a real export through the
  full capture pipeline (`CaptureScreenshotPngAsync` →
  `ExportJobRunner` → file write). Produced a genuine 801x450 PNG
  (confirmed via file-header inspection, not just an exit code), with
  the version-stamped filename correctly reading `v0.9.9-rc.1`. The
  FFmpeg-based WebM/MP4 encoding path itself is unchanged since 0.9.0
  and wasn't touched by this phase's network-isolation/hardening work
  (which is scoped to `SketchViewport`/`LocalSketchServer`), so this
  PNG-path run is the relevant regression check for what actually
  changed.
- Performance snapshot on this machine: idle `P5CCS.App.exe` working
  set ~187 MB / private bytes ~155 MB right after launch (WebView2
  process(es) included in the OS-reported total for the main process
  tree); live viewport steady-state at 60 FPS per the in-app FPS
  counter, matching pre-Phase-11 numbers — no observable regression
  from the added `WebResourceRequested` interception on the per-frame
  hot path (it only fires per HTTP request, not per rendered frame).
- Feature freeze declared for v1.0.0.0: no further feature work is
  planned before Phase 12's release preparation.

## [0.9.5] - 2026-08-13

### Added

- Multi-resolution application icon (`resources/icons/app.ico`, 16–256px,
  PNG-compressed ICO entries) embedded via `ApplicationIcon` — verified by
  reloading the file with `System.Drawing.Icon` and by extracting the icon
  back out of the compiled `P5CCS.App.exe`, not just generated and assumed
  correct.
- `installers/windows/setup.iss`: a working Inno Setup 7 script producing
  a Windows installer for either architecture (`/DAppArch=x64` or
  `/DAppArch=arm64`), with its own icon/wizard-image/small-image assets.
  Conditionally bundles a WebView2 Fixed Version Runtime via `#ifexist` if
  one is present under `resources/webview2runtime/win-<arch>/` at compile
  time (none is vendored yet — see Known Issues).
- Real screenshots (`docs/screenshot1.png`, `docs/screenshot2.png`)
  captured from the actual running application, with the app state
  verified via UI Automation at the moment of capture rather than assumed.

### Fixed

- `README.md` corrected to match reality: the version badge was still
  0.1.0, and the feature list claimed a bundled WebView2 "Fixed Version
  Runtime, fully offline" that isn't actually vendored yet (the app
  currently falls back to the system's Evergreen runtime — see
  `WebView2RuntimeLocator`, added in 0.9.0).
- `docs/COMPILATION.md`'s publish command was missing `-p:Platform=x64`,
  which the architecture-conditional `ffmpeg.exe` selection added in 0.9.0
  depends on to pick the right binary.

### Known Issues / Verified

- Both the x64 and ARM64 installer variants were genuinely compiled with
  Inno Setup 7 (already installed on this machine) against real
  self-contained `dotnet publish` output for each architecture — not just
  written and assumed to work. The x64 installer was additionally
  silent-installed to a temporary directory, the installed app launched
  and confirmed working (editor, 7 sliders, live sketch execution via UI
  Automation), and then silently uninstalled with a confirmed clean
  removal of the install directory.
- Repository compliance verified for this phase: no mentions of Claude AI
  anywhere in the tracked repository, git history, or git identity; both
  `ROADMAP.md` and `CLAUDE.md` confirmed excluded via `.gitignore` and
  absent from `git ls-files`.
- No WebView2 Fixed Version Runtime is bundled yet (only the code-level
  detection support added in 0.9.0); see `docs/KNOWN-LIMITATIONS.md`.

## [0.9.0] - 2026-08-13

### Added

- GPU-accelerated video encoding with automatic CPU fallback:
  `VideoExporter` now tries hardware encoders first (`h264_nvenc`,
  `h264_amf`, `h264_qsv` for MP4; `vp9_qsv` for WebM) and falls through
  to the existing CPU software encoder on failure, returning which
  encoder actually succeeded. Verified end-to-end on real hardware: this
  machine's NVIDIA GPU is detected by `ffmpeg` but its NVENC driver
  stack is unavailable, genuinely exercising the fallback chain rather
  than only the disabled-by-flag code path.
- Native ARM64 `ffmpeg.exe` (BtbN `winarm64-lgpl` build) vendored
  alongside the existing x64 one; `P5CCS.Export.csproj` now selects the
  correct architecture's binary at build time based on `Platform`/
  `RuntimeIdentifier`, verified to produce a clean ARM64 build with the
  genuinely-native binary copied to output (not x64 under emulation).
- `WebView2RuntimeLocator` (`P5CCS.Engine`): code-level support for a
  bundled WebView2 Fixed Version Runtime — if a `WebView2Runtime` folder
  with `msedgewebview2.exe` exists next to the app, it's preferred over
  the system's Evergreen runtime. No runtime is bundled yet (that's
  Phase 10's installer responsibility per the roadmap); this only adds
  and tests the detection/fallback code.
- `docs/KNOWN-LIMITATIONS.md`: an honest per-configuration limitations
  log distinguishing what was verified on real hardware in this
  environment (Windows 11 x64, a real second physical monitor, GPU
  encoder fallback, transparency-disabled Mica degradation, clean ARM64
  cross-compilation) from what could not be (real ARM64 execution,
  Windows 10, the full 100–300% DPI range, high-contrast mode).
- 7 new tests (4 `VideoExporter` GPU/fallback tests, 3
  `WebView2RuntimeLocator` tests) — 108 tests solution-wide.

### Fixed

- Nothing broken by this phase's changes: full regression pass (build +
  108 tests + live launch, including a real launch with Windows
  transparency effects disabled) confirmed after every change.

## [0.8.0] - 2026-08-13

### Added

- Full export system: a new "Export..." dialog (`ExportWindow`/`ExportViewModel`,
  replacing "Quick Export" as the Ctrl+E default, which remains available as a
  separate one-click PNG shortcut) supporting PNG, JPEG, GIF, WebM, and MP4,
  with per-format quality controls, a destination-folder picker, an
  auto-generated file name (`<sketch>_v<version>_<timestamp>.<ext>`), a
  batch queue (add multiple jobs, run them sequentially), a progress bar
  with ETA, and cancellation.
- Deterministic offscreen frame capture (`P5CCS.Export.FrameCaptureService`):
  drives the sketch via a virtualized clock (`bridge.js` replaces
  `performance.now` during export) so exported motion is correct regardless
  of how long each frame actually takes to capture — verified by unit tests
  asserting the exact virtual-time sequence sent per frame.
- `p5.sound.min.js` vendored locally (already added in 0.7.0) is now joined
  by a vendored `ffmpeg.exe` (`resources/ffmpeg/`, BtbN win64-lgpl static
  build, LGPL-3.0) and `FFMpegCore`/`SixLabors.ImageSharp`-backed exporters:
  `GifExporter` (Octree quantization, infinite loop), `VideoExporter`
  (WebM via `libvpx-vp9`; MP4 via `libopenh264` — the BSD-licensed Cisco
  H.264 encoder, since the GPL-only `libx264` is absent from LGPL ffmpeg
  builds), and `StillImageExporter` (PNG passthrough / JPEG re-encode).
- New `P5CCS.App.Tests` project (previously missing) covering the export
  file-naming convention and the full `ExportJobRunner` orchestration
  (resize-before-capture, resize-back-after, cancellation mid-export).
- `IP5jsEngineHost` gained `BeginExportAsync`/`CaptureExportFrameAsync`/
  `EndExportAsync`/`ResizeCanvasForExportAsync`; `LocalSketchServer`-adjacent
  export plumbing lives behind a new engine-agnostic `IExportFrameSource`
  interface in `P5CCS.Export`, kept decoupled from WPF/WebView2 so the
  capture/encode pipeline is independently unit-testable.
- 34 new tests (20 `P5CCS.Export.Tests`, some already present pre-video +
  new video/still/frame-capture tests, 12 new `P5CCS.App.Tests`) — 101
  tests solution-wide. WebM and MP4 export are validated against the real
  vendored `ffmpeg.exe`, producing genuinely decodable video files
  (verified via `ffprobe`: correct codec, resolution, framerate, duration).

### Fixed

- Export resolution is now genuinely independent of the live viewport's
  size, as the roadmap requires. Two real bugs were caught by end-to-end
  UI testing (not just unit tests) and fixed: (1) `CapturePreviewAsync`
  captures the WebView2 control's on-screen rendered pixels, which were
  affected by the auto-fit `ZoomTransform` added in 0.6.3 — a requested
  800×450 export came out at whatever the live-view scale happened to be
  (e.g. 497×279). The zoom is now forced to 1:1 and the WebView2 host
  control is itself resized to the export dimensions during capture, not
  just the JS-side canvas. (2) That resize used logical WPF units while
  `CapturePreviewAsync` captures physical device pixels — on a
  DPI-scaled display (e.g. 150%) this produced a capture 1.5x larger than
  requested. The resize now divides by `VisualTreeHelper.GetDpi`, and a
  custom resolution (321×246) was confirmed to export pixel-exact through
  the full running app.

### Known Issues

- The batch queue's `NumberBox` inputs (resolution) were observed, during
  manual UI-automation testing, to sometimes not commit their displayed
  text to the bound value on focus-loss alone (Tab/click-away) — pressing
  Enter reliably commits. This may be a synthetic-input artifact rather
  than a real user-facing bug (WPF-UI's `NumberBox` likely commits
  correctly on genuine OS-level focus changes); flagged for a real-device
  check rather than confirmed as broken for actual users.

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
