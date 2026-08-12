# Processing 5 - Creative Coding Station

[![CI](https://github.com/Patrickjaillet/p5ccs/actions/workflows/ci.yml/badge.svg)](https://github.com/Patrickjaillet/p5ccs/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-0.1.0-blue.svg)](docs/CHANGELOG.md)

A native Windows creative-coding IDE for [p5.js](https://p5js.org/), built with
C# / WPF / WPF-UI. Offline-first: the p5.js runtime, the code editor, and the
export pipeline all run locally with zero network dependency.

## Features

> Under active development — see [ROADMAP.md](ROADMAP.md) for the full plan (internal document, not published).

- Fluent-themed WPF-UI interface (Mica/Acrylic, light/dark/system)
- Embedded WebView2 viewport (Fixed Version Runtime, fully offline)
- Full-featured AvalonEdit code editor with p5.js-aware syntax highlighting
- Intelligent slider system: live numeric-literal-to-UI binding
- Full p5.js API coverage (2D, WEBGL, Sound, `p5.Vector`/`p5.Table`, shaders)
- Video/GIF/image export pipeline (WebM, MP4, GIF, PNG/JPEG)

## Screenshots

![Viewport and editor](docs/screenshot1.png)
![Sliders and export](docs/screenshot2.png)

## Getting Started

See [docs/COMPILATION.md](docs/COMPILATION.md) for build prerequisites and
instructions.

```powershell
dotnet build P5CCS.sln --configuration Release -p:Platform=x64
```

## Installation

Windows installers (x64 and ARM64) will be published on the
[Releases](https://github.com/Patrickjaillet/p5ccs/releases) page.

## Contributing

Contributions are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md) and our
[Code of Conduct](CODE_OF_CONDUCT.md).

## License

**Processing 5 - Creative Coding Station** is licensed under the [MIT License](LICENSE).
Copyright © 2026 Patrick JAILLET.

This project embeds [p5.js](https://p5js.org/) (Copyright © 2026 Processing
Foundation, LGPL-2.1).

## Links

- Website: https://patrickjaillet.github.io/p5ccs
- Repository: https://github.com/Patrickjaillet/p5ccs
- Contact: sandefjord.development@proton.me
