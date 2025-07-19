This repository contains a multi-project .NET solution targeting net8.0. The code uses C# with Avalonia for UI and MSTest for unit tests.

# Coding guidelines
- Use **four spaces** for indentation; no tabs.
- Namespace and type names follow **PascalCase**.
- Private fields use **camelCase** with leading underscore when intended (`_field`).
- XML documentation comments (`///`) are used for public APIs when appropriate.
- Place `using` directives at the top of the file, outside namespaces.
- Curly braces open on the same line for namespaces, classes, and methods.
- Prefer async APIs returning `Task`/`Task<T>` when performing I/O.
- When adding enums, consider creating extension methods to convert to user-friendly strings similar to `MoonPhaseExtensions`.

# Project structure
- `ID.WeatherDashboard.API` contains core data models, configuration classes and service interfaces.
- `ID.WeatherDashboard.WUnderground`, `ID.WeatherDashboard.WeatherAPI`, `ID.WeatherDashboard.MoonPhase` and `ID.WeatherDashboard.SunriseSunset` provide service implementations.
- `ID.WeatherDashboard.Browser` and `ID.WeatherDashboard.Desktop` contain platform entry points.
- `ID.WeatherDashboard.APITests` hosts MSTest unit tests.

# Testing
Run unit tests with:
```bash
dotnet test --no-build
```
All tests should pass (currently 87 passing).

# Commit messages
Use concise commit messages written in the imperative mood (e.g. "Add forecast data unit tests").

