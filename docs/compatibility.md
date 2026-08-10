# Compatibility Contract

Runtime projects are reflection-free and are built with the trimming and
NativeAOT analyzers enabled. A composed NativeAOT smoke executable exercises
core results, structured errors, diagnostics, and typed ASP.NET Core results.

## Pinned Compatibility

| Surface | Tested version | Runtime dependency |
| --- | --- | --- |
| .NET and ASP.NET Core | 10.0 | Framework only in `MonadicTypes.AspNetCore` |
| FluentValidation | 12.1.1 | None; compatibility-test only |
| OpenAPI endpoint metadata | ASP.NET Core 10.0 | Framework only in `MonadicTypes.AspNetCore` |

Compatibility tests currently pin `Microsoft.AspNetCore.OpenApi` and
`Microsoft.AspNetCore.TestHost` 10.0.10 plus patched `Microsoft.OpenApi` 2.7.5.
These packages are test-only and do not flow into runtime consumers.

Version changes are deliberate maintenance work. They require compatibility
tests, trim analysis, NativeAOT publication, and benchmark review before the
tested matrix changes.

## Custom Integrations

`Error`, `ValidationErrors`, and `Result<T, E>` are ordinary public values.
Applications may define extension methods that project them directly into a
logging, telemetry, validation, HTTP, or documentation stack. No adapter
interface is required.

`MonadicTypes.Diagnostics` is an optional convenience layer over the BCL
`Activity` and `Meter` APIs. It is not activated automatically and core result
creation or propagation emits no diagnostics. Applications that need different
events or tags should omit that package and implement a local extension.

`MonadicTypes.AspNetCore` provides common RFC problem responses and typed-result
metadata. Generic mapper overloads allow callers to return custom typed results
without converting domain errors or adopting the built-in problem shape.
