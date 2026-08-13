# Qt6 Rewrite — Foundation Notes

This directory (`qt6/`) is a from-scratch rewrite of Processing 5 - Creative
Coding Station targeting v2.0.0, developed on the `qt6-rewrite` branch
alongside (not replacing) the existing WPF app under `src/`. See Phase 13
in the project roadmap for the full checklist.

## Decisions made

- **Language/stack: C++ with native Qt6** (not PySide6/PyQt6). This means
  none of the existing C# code (`P5CCS.Core`, `P5CCS.Engine`,
  `P5CCS.Editor`, `P5CCS.Export`) is reusable — everything is being
  rewritten from scratch in C++.
- **Build system: CMake** (3.21+), using Qt6's `qt_add_executable` /
  `qt_add_library` CMake integration, `AUTOMOC`/`AUTOUIC`/`AUTORCC` enabled
  globally.
- **Module layout** mirrors the legacy solution's project boundaries as
  separate CMake static libraries, so responsibilities stay separated the
  same way they were in C#:
  - `core/` → `p5ccs_core` (analogous to `P5CCS.Core`)
  - `engine/` → `p5ccs_engine` (analogous to `P5CCS.Engine`)
  - `editor/` → `p5ccs_editor` (analogous to `P5CCS.Editor`)
  - `exportpipeline/` → `p5ccs_exportpipeline` (analogous to
    `P5CCS.Export`; named `exportpipeline` rather than `export` since
    `export` is a reserved word in C++)
  - `app/` → `P5CCS.exe`, the executable linking all of the above
  - `tests/` → Qt Test-based unit tests

## Verified working on this machine

- Qt **6.11.1** is installed at `C:/Qt/6.11.1`, with an `msvc2022_64` kit.
- Visual Studio 2022 Build Tools (MSVC `19.44.35228`) provides the C++
  compiler; there is no `cl.exe`/`vcvarsall.bat` on `PATH` by default, so
  every build must run from within an environment that has sourced
  `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvarsall.bat x64`
  (or the "x64 Native Tools Command Prompt for VS 2022" shortcut).
- **Ninja** is installed (via WinGet) and used as the CMake generator —
  significantly faster incremental builds than the Visual Studio
  generator for this project's size.
- The full CMake configure + build succeeds cleanly: all four static
  libraries link, and `P5CCS.exe` links against all of them.
- The built `P5CCS.exe`, after running `windeployqt6.exe` to place the
  required Qt DLLs alongside it, **genuinely launches**: verified via UI
  Automation reading the real window title
  ("Processing 5 - Creative Coding Station"), the splitter containing the
  editor and viewport widgets, the status bar text ("Ready"), and the
  version label. The code editor widget (`QPlainTextEdit`-based)
  was confirmed to genuinely accept and hold text input via
  `ValuePattern.SetValue`/`.Current.Value`, not just assumed to work from
  the widget existing.
- `tests/test_Version.cpp` (Qt Test) builds and runs; verified via exit
  code (0 = pass, 1 = fail — confirmed both by temporarily breaking the
  assertion and rebuilding) since this environment's process stdout
  redirection for freshly spawned native processes doesn't reliably reach
  the tool used to run these commands (a shell/tooling quirk of this
  particular development environment, not a defect in the test itself —
  the same quirk does not affect the `.NET`/`dotnet test` suite for the
  legacy app, which uses a different test runner mechanism entirely).

## QtWebEngine viewport — done and verified

**Update**: QtWebEngine is now installed (`extensions.qtwebengine.6111.win64_msvc2022_64`,
matching this machine's Qt 6.11.1 exactly, via
`MaintenanceTool.exe install ... --confirm-command`), and
`SketchViewportWidget` now hosts a real `QWebEngineView` loading p5.js
content through a custom `p5ccs://` URL scheme
(`SketchUrlSchemeHandler`), serving `index.html` and the embedded
`p5.min.js` from Qt resources, plus a dynamically-generated `sketch.js`
via a callback (the same shape as the legacy `LocalSketchServer`'s
`Func<string>` sketch source provider). This is a genuinely stronger
security posture than the legacy loopback HTTP server: there is no
listening socket at all, on any interface, so a sketch has nothing to
discover or connect to beyond what request interception already
prevents.

Verified for real (not just "it compiles"), via Chrome DevTools Protocol
over `QTWEBENGINE_REMOTE_DEBUGGING`, from a from-scratch clean rebuild:
`typeof p5 !== 'undefined'` is `true`, a `<canvas>` element exists, and
`frameCount` genuinely advances between two checks a second and a half
apart — the sketch's `draw()` loop is actually running continuously, not
just rendering once.

**Debugging note for future reference**: the first attempt failed with
`ERR_FILE_NOT_FOUND` for every request. Root cause was two independent
bugs, found by bisection:
1. `QWebEngineUrlScheme::LocalScheme`/`LocalAccessAllowed` flags tell
   Chromium the scheme resolves to real filesystem paths (like `file://`)
   — with a scheme that has no such backing files, every request fails
   before ever reaching the registered handler. Fixed by using only
   `SecureScheme | CorsEnabled`.
2. Separately, CMake's `qt_add_resources(... FILES resources/index.html
   ...)` without an explicit `BASE` directory aliases the resource by its
   path *relative to the CMakeLists.txt*, i.e. `resources/index.html`,
   not `index.html` — so `QFile(":/p5ccs/engine/index.html")` in the
   handler was silently failing to open (`job->fail(UrlNotFound)`, which
   Chromium also reports as `ERR_FILE_NOT_FOUND`, identical to bug #1's
   symptom). Fixed by adding `BASE "${CMAKE_CURRENT_SOURCE_DIR}/resources"`
   to the `qt_add_resources` call. A temporary file-based trace inside
   `requestStarted` (since this environment's stdout capture for freshly
   spawned native processes is unreliable — see the Qt Test note above)
   was what separated these two causes: it proved the handler *was*
   being invoked with the right path once bug #1 was fixed, narrowing
   the remaining `ERR_FILE_NOT_FOUND` down to the resource lookup itself.

## Known gaps (explicitly not done yet)

- **No network isolation for sketch-issued requests yet.** The custom
  scheme itself has no network access by construction, which covers
  top-level navigation, but the legacy app's
  `WebResourceRequested`-based interception of everything a sketch's own
  JS might `fetch()` has no Qt6 equivalent yet — needed before this can
  be considered equivalent to the legacy app's security posture, not
  just its rendering.
- **No FPS/console/mouse reporting back to the host** — the legacy
  `bridge.js` → `WebMessageReceived` channel isn't ported. There is
  currently no way for `MainWindow` to show a live FPS counter or route
  sketch `console.log` calls to a UI console panel.
- **No export frame capture** — the legacy `CaptureScreenshotPngAsync`/
  `beginExport`/`captureFrame` protocol isn't ported.
- **No p5.sound** — only `p5.min.js` is embedded so far, not
  `p5.sound.min.js`.
- **No p5.js-aware editor features** (syntax highlighting, folding,
  autocomplete, inline error markers) — `CodeEditorWidget` is currently
  bare `QPlainTextEdit`. `QSyntaxHighlighter` vs. `QScintilla` still needs
  to be evaluated.
- **No docking system** — the legacy app's AvalonDock-based
  multi-panel layout (Explorer, Sliders, Console, API Reference, etc.) has
  no Qt equivalent yet; `MainWindow` currently only has the two-pane
  splitter. `QDockWidget` is the native Qt option and hasn't been
  evaluated against alternatives yet.
- **No export execution** — `ExportJob` is data-only; no FFmpeg process
  invocation, no encoder selection/fallback chain, no frame capture.
- **No theming** — no attempt yet to approximate the legacy app's
  Fluent/Mica WPF-UI look.
- **Qt6 licensing has not been formally reviewed** against this project's
  MIT distribution. Qt6's open-source licensing is LGPLv3 (for the
  modules used here) or GPLv3 depending on module and linking choice —
  this needs a dedicated review pass (see Phase 13 checklist) before any
  release, not just before v2.0.0's own release.
- **No installer** for the Qt6 build — `windeployqt` was used ad hoc for
  local testing only; no `CMakePresets`-driven packaging step or Inno
  Setup equivalent has been set up.

## Building locally

```
"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvarsall.bat" x64
cd qt6
cmake --preset msvc2022-x64
cmake --build --preset msvc2022-x64
```

`CMakePresets.json`'s `CMAKE_PREFIX_PATH` points at this machine's actual
Qt install path (`C:/Qt/6.11.1/msvc2022_64`) — override it (e.g. via a
gitignored `CMakeUserPresets.json`) if building on a different machine
with Qt installed elsewhere.

To run the built app or tests directly (outside of `windeployqt`'s
deployed copy), put `C:\Qt\6.11.1\msvc2022_64\bin` on `PATH` first so the
Qt DLLs can be found.
