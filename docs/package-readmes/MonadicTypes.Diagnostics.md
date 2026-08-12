# MonadicTypes.NET.Diagnostics

Optional, vendor-neutral `Activity` and `Meter` projection for structured
MonadicTypes.NET errors. This package includes `MonadicTypes.NET.Errors` and the
core package transitively.

## Requirements

- .NET 10 or later
- Compatible with trimming and NativeAOT
- Uses only `System.Diagnostics`; no exporter dependency

## Install

```bash
dotnet add package MonadicTypes.NET.Diagnostics --prerelease
```

## Quick Start

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;
using MonadicTypes;

using var meter = new Meter("Orders");
var metrics = new ErrorMetrics(meter);

Result<Order, Error> result = LoadOrder();
result.TapError(error =>
{
    ErrorTelemetry.Record(Activity.Current, error);
    metrics.Record(error);
});
```

## Telemetry Contract

The library never logs, traces, or records metrics merely because a Result is
created or propagated. Call projection once at the application boundary where
the error is handled.

`ErrorTelemetry.Record` enriches a caller-owned sampled Activity. `ErrorMetrics`
creates a counter on a caller-owned Meter and uses bounded category tags by
default. Error-code tags are opt-in because they can increase cardinality.
OpenTelemetry, Prometheus, Application Insights, and other BCL-compatible
consumers can export these signals without package-specific adapters.

`ErrorMetrics.Disabled` and an absent or unsampled Activity provide low-cost
disabled paths. For allocation-sensitive callbacks, use caller-state or an
`IValueAction<Error>` struct rather than a capturing lambda.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.Errors` | Structured errors without supplied projection |
| `MonadicTypes.NET.AspNetCore` | HTTP boundary conversion and metadata |

## Documentation

See [diagnostics](https://github.com/czernous/monadic-types-dotnet#diagnostics)
and the [compatibility contract](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/compatibility.md).

Apache-2.0. Developed with AI assistance.
