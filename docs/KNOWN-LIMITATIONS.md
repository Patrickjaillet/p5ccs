# Known Limitations by Configuration

This document tracks configuration-specific limitations discovered during
Phase 9 (multi-architecture Windows validation), and is intentionally
explicit about what was verified on real hardware/OS versus what could not
be tested in the development environment used for this project.

## What was verified on real hardware

- **Windows 11 24H2, x64** (build 26100): full build, test suite (108
  tests), and live app execution verified throughout development —
  editor, viewport, sliders, export (GIF/WebM/MP4/PNG/JPEG), and the API
  reference panel all confirmed working.
- **Real multi-monitor setup** (two physical displays, 3840x2160 and
  2880x1800, both at 150% DPI scaling): the app was moved to the
  secondary display and confirmed to render correctly (editor content,
  sliders, live sketch execution) — not a synthetic/virtual multi-monitor
  test.
- **Transparency effects disabled** (Windows Settings > Personalization >
  Colors > Transparency effects, `EnableTransparency` registry value):
  confirmed the app launches and renders correctly with the Mica backdrop
  gracefully degrading to a solid background, with no crash and no loss
  of functionality (sliders, editor, live FPS all continued working).
- **GPU-accelerated video encoding with automatic CPU fallback**
  (`VideoExporter`, Phase 9): this machine has an NVIDIA GPU, and `ffmpeg`
  is compiled with `h264_nvenc`/`h264_amf`/`h264_qsv` support, but the
  actual NVENC driver stack (`nvEncodeAPI64.dll`) is absent — a real,
  reproducible "GPU listed but not actually usable" condition, likely due
  to the virtualized/remote nature of this environment. This directly
  exercises the fallback chain `VideoExporter` implements: each GPU
  encoder candidate is attempted and, on failure, execution falls through
  to the next, ending in the CPU software encoder — verified end-to-end
  with real `ffmpeg` invocations, not mocked.
- **Native ARM64 build correctness**: `dotnet build -p:Platform=ARM64`
  (and the underlying `P5CCS.Export` architecture-conditional `ffmpeg.exe`
  selection) verified to produce a clean, warning-free ARM64 build with
  the correct native `win-arm64` `ffmpeg.exe` (BtbN `winarm64-lgpl`
  build) copied to its output — not x64-under-emulation.

## What could not be validated in this environment

- **Windows 11 ARM64, native execution**: no ARM64 hardware was available
  to actually *run* the built app on. The build itself is verified clean
  (see above); real-device execution (WPF rendering, WebView2 on ARM64,
  native `ffmpeg.exe` actually encoding) needs to be checked on real
  ARM64 Windows hardware (e.g. a Snapdragon-based Surface/Copilot+ PC)
  before shipping.
- **Windows 10 (1809+)**: this development environment runs Windows 11
  24H2 exclusively; there is no Windows 10 install available to test
  against. `net8.0-windows` targets Windows 10 1809+ per its minimum
  supported OS version, and `WindowBackdropType="Mica"` is expected to
  no-op gracefully on Windows 10 (Mica is a Windows 11 22H2+ compositor
  feature) since it degrades the same way transparency-disabled does on
  Windows 11 — but this has not been confirmed on an actual Windows 10
  machine.
- **DPI scaling outside 150%**: both physical displays in this
  environment happen to be configured at exactly 150% scaling. The
  DPI-aware export-resolution fix added in Phase 8
  (`VisualTreeHelper.GetDpi`) was validated at 150% only; 100%, 200%, and
  300% (the roadmap's full target range) were not empirically exercised,
  though the fix is scale-factor-generic by construction (divides by
  whatever `DpiScaleX`/`DpiScaleY` the OS reports) rather than hardcoded
  to 150%.
- **High-contrast mode**: `SystemParameters.HighContrast` was checked
  (confirmed `false` in this environment) but not toggled — full
  high-contrast mode changes system-wide colors in a way that is
  disruptive and awkward to reliably revert in a shared/remote session,
  unlike the much narrower "disable transparency" test above. The Mica
  backdrop is expected to degrade the same way it does with transparency
  disabled (DWM disallows the composition effect and the window falls
  back to a solid background), but this is inferred from the DWM API
  contract, not empirically confirmed under real high-contrast mode.
- **WebView2 Fixed Version Runtime, actually bundled and running**: the
  app now has code-level support for it (`WebView2RuntimeLocator` — if a
  `WebView2Runtime` folder with `msedgewebview2.exe` exists next to the
  app, it's used in preference to the system's Evergreen runtime), but no
  Fixed Version Runtime has actually been vendored into the repo or
  installer yet. Per the roadmap, bundling the actual runtime is Phase
  10's responsibility (the Inno Setup installer script); this phase only
  verified the Evergreen fallback path continues to work correctly.
  Consequently, Phase 11's offline-network audit (v0.9.9-rc.1) cannot
  claim zero runtime network dependency in the strictest sense: a target
  machine with no WebView2 Evergreen Runtime already installed still
  needs one before the app will run at all. Everything the *application*
  itself serves at runtime (p5.js, p5.sound.js, fonts, FFmpeg binaries)
  is embedded and local; this gap is specifically about the browser
  engine WebView2 depends on, not about the app's own content.

## Content-Security-Policy vs. WebView2 rendering (Phase 11)

While hardening `LocalSketchServer` for Phase 11, adding a
`Content-Security-Policy` response header — tried with the full intended
policy, then narrowed down to just `default-src 'self'`, then just
`default-src 'self' 'unsafe-eval'`, all with identical symptoms —
reproducibly and silently broke `requestAnimationFrame`-driven rendering
in this specific WebView2 environment: a sketch's `setup()` still ran
(the canvas got created), but no subsequent frame was ever observed
(reported FPS stayed 0, mouse position never updated), with zero console
errors, zero `securitypolicyviolation` events, and no exceptions
anywhere. This was isolated via `git stash`-based bisection down to "the
mere presence of the header, regardless of directives" as the sole
cause, independent of any `bridge.js` implementation detail. The
underlying WebView2/Chromium mechanism responsible was never identified
— it may be specific to the virtualized/remote environment this project
was developed in. The security goal (preventing sketch code from
exfiltrating data over the network) was instead achieved via
`CoreWebView2.AddWebResourceRequestedFilter` / `WebResourceRequested`
host-side request interception in `SketchViewport`, which does not
exhibit this regression and cannot be bypassed by page-level script the
way a CSP header theoretically could be circumvented by a browser bug.
Anyone revisiting a page-level CSP header for this project in the future
should be aware this failure mode exists and reproduces reliably in this
environment.
