# MonadicTypes.NET.Effects

Explicit exception-to-error boundaries and throwing-operation helpers for
MonadicTypes.NET. This package includes `MonadicTypes.NET` transitively.

## Requirements

- .NET 10 or later
- Compatible with trimming and NativeAOT

## Install

```bash
dotnet add package MonadicTypes.NET.Effects --prerelease
```

## Quick Start

```csharp
using MonadicTypes;
using MonadicTypes.Effects;

Result<string, string> text = Effect.Try(
    static () => File.ReadAllText("settings.json"),
    static exception => $"Read failed: {exception.Message}");

Result<int, string> length = text.TryMap(
    static value => value.Length,
    static exception => $"Mapping failed: {exception.Message}");
```

## Boundary Rules

- `Effect.Try`, `TryAsync`, and `TryTaskAsync` start a Result pipeline around
  throwing code.
- `TryMap`, `TryBind`, and `TryTap` protect one stage in an existing pipeline.
- Broad overloads exclude cancellation and process-corrupting/runtime failures.
- Typed overloads convert only the selected exception type; other exceptions
  propagate normally.
- The caller owns exception-to-domain-error mapping.

Catch exceptions at narrow dependency boundaries, then return to ordinary
Result composition. Do not wrap an entire application or hide programmer bugs
inside a generic unexpected error.

Caller-state overloads avoid closure allocation. Completed async paths avoid an
adapter Task allocation, but this package cannot remove allocations made by the
underlying dependency.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.Errors` | Structured errors that retain exception causes |
| `MonadicTypes.NET.Async` | Continuing the resulting async pipeline |

## Documentation

See [exception boundaries](https://github.com/czernous/monadic-types-dotnet#exception-boundaries)
and the [API behavior reference](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/api-reference.md#exception-effects).

Apache-2.0. Developed with AI assistance.
