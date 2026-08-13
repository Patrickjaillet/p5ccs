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
  editor and viewport placeholder widgets, the status bar text ("Ready"),
  and the version label. The code editor widget (`QPlainTextEdit`-based)
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

## Known gaps (explicitly not done yet — foundation stage only)

- **QtWebEngine is not installed** in this machine's Qt distribution
  (only `Core`, `Gui`, `Widgets`, `Multimedia`, `Network`, `Svg`, `Qml`,
  and a handful of others are present under
  `C:/Qt/6.11.1/msvc2022_64/lib/cmake/`). The sketch viewport
  (`p5ccs::engine::SketchViewportWidget`) is currently a placeholder
  `QLabel`, not a real p5.js host. Getting `QWebEngineView` working is a
  prerequisite for any real viewport work and needs the QtWebEngine
  module installed via the Qt Maintenance Tool first.
- **No local HTTP server** equivalent to `LocalSketchServer` exists yet —
  needed once `QWebEngineView` is available, to serve the embedded p5.js
  runtime and sketch code the same offline, loopback-only way the WPF app
  does.
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
