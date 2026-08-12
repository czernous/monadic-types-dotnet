# Dependency Policy

Shipping projects should use BCL/framework APIs and project references only.
Third-party packages belong in tests or benchmarks unless a runtime dependency
has a measured, documented benefit that cannot be implemented soundly in the
library.

Current development dependencies use licenses permitting commercial use:

- FluentValidation: Apache-2.0, compatibility tests only.
- Microsoft ASP.NET Core, TestHost, and OpenAPI: MIT, framework/test boundary.
- OpenTelemetry-compatible diagnostics use BCL `Activity` and `Meter`; no
  OpenTelemetry runtime package is required.
- BenchmarkDotNet: MIT, benchmarks only.
- xUnit: Apache-2.0, tests only.
- Meziantou.Analyzer: MIT, private build-time analysis only.
- Microsoft.CodeAnalysis.BannedApiAnalyzers: MIT, private build-time architecture
  enforcement for shipping source projects only.

The .NET SDK's built-in analyzers run at the pinned .NET 10 recommended level.
Analyzer references use `PrivateAssets="all"` and do not flow into runtime or
consumer packages. SonarAnalyzer.CSharp is intentionally not referenced because
its current source-available license is not suitable for this repository's
commercial and AI-assisted usage requirements.

Every dependency update requires license and vulnerability review. MonadicTypes
itself is distributed under the repository's Apache License 2.0; dependency
licenses remain independent and must continue to be preserved.
