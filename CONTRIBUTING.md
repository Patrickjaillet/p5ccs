# Contributing to Processing 5 - Creative Coding Station

Thank you for your interest in contributing.

## Development Conventions

- Language: English only for code, comments-free source, UI, and public docs.
- Naming: .NET conventions strictly enforced — `PascalCase` for public types/members,
  `camelCase` for locals/parameters, `_` prefix for private fields.
- **No comments** in source code.
- Nullable reference types are enabled project-wide (`<Nullable>enable</Nullable>`).
- Target frameworks: `net8.0-windows`, platforms `x64` and `ARM64`.
- Architecture: strict MVVM via `CommunityToolkit.Mvvm`, dependency injection via
  `Microsoft.Extensions.DependencyInjection`.
- Offline-first: no runtime network dependency is permitted anywhere in the codebase.

## Prerequisites

See [docs/COMPILATION.md](docs/COMPILATION.md) for SDK requirements and build instructions.

## Workflow

1. Fork the repository and create a feature branch from `main`.
2. Make your changes, following the conventions above.
3. Add or update unit tests (`xUnit` + `Moq`) for any behavioral change.
4. Ensure `dotnet build` and `dotnet test` succeed locally with zero warnings.
5. Update `docs/CHANGELOG.md` under the `[Unreleased]` section.
6. Open a pull request using the provided template.

## Commit Messages

Use clear, imperative commit messages (e.g. "Add slider bounds detection",
not "Added" or "Adding").

## Versioning

This project follows strict [SemVer](https://semver.org/). Version numbers are
centralized in `Directory.Build.props` and must not be edited ad-hoc in
individual project files.

## Reporting Issues

Use the issue templates under `.github/ISSUE_TEMPLATE/` for bug reports and
feature requests.

## Code of Conduct

Participation in this project is governed by our [Code of Conduct](CODE_OF_CONDUCT.md).
