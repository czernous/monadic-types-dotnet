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

Every dependency update requires license and vulnerability review. This file
does not select the license for MonadicTypes itself; repository licensing will
be handled separately before distribution.
