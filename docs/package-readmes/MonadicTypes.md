# MonadicTypes.NET

Allocation-conscious `Result<T,E>`, `Option<T>`, `Unit`, composition, and
struct-callable primitives for C# 14 and .NET 10.

Use this core package when expected failure or absence should be represented in
the type system without reflection, exceptions for ordinary control flow, or a
dependency-injection requirement.

## Requirements

- .NET 10 or later
- Compatible with trimming and NativeAOT
- No runtime package dependencies

## Install

```bash
dotnet add package MonadicTypes.NET --prerelease
```

## Quick Start

```csharp
using MonadicTypes;

static Result<int, string> ParsePositive(string text) =>
    int.TryParse(text, out int value) && value > 0
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Fail("A positive integer is required.");

string message = ParsePositive("21")
    .Map(static value => value * 2)
    .Match(
        static value => $"Value: {value}",
        static error => $"Error: {error}");

Option<string> name = Option<string>.Some("Ada");
int length = name.Map(static value => value.Length).ValueOr(0);
```

## Core Behavior

- `Result<T,E>` is exactly `Ok(T)` or `Fail(E)`; its default value is invalid.
- `Option<T>` is `Some(T)` or `None`; `default(Option<T>)` is `None`.
- `Some(null)` is rejected.
- `Map` transforms success, `Bind` composes dependent results, and `MapError`
  transforms failure.
- `Result<Option<T>,E>` represents a fallible lookup where absence is expected.
- `Combine`, `Zip`, and two-input `Map` compose independent results.

Normal static callbacks and successful pipelines allocate 0 B in the accepted
benchmarks. Caller-state overloads avoid closures, while struct callables are
available for measured hot paths. Do not use Result or Option inside parsing or
vectorized inner loops where a simpler branch or `Try*` contract is cheaper.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.Errors` | Structured errors and validation values |
| `MonadicTypes.NET.Async` | Fluent Task and ValueTask composition |
| `MonadicTypes.NET.Effects` | Explicit exception boundaries |
| `MonadicTypes.NET.AspNetCore` | Typed HTTP and problem results |
| `MonadicTypes.NET.Diagnostics` | Optional Activity and Meter projection |
| `MonadicTypes.NET.Generators` | Compile-time struct-callable adapters |

## Documentation

See the [complete API guide](https://github.com/czernous/monadic-types-dotnet#readme),
[API behavior reference](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/api-reference.md),
and [benchmark policy](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/benchmarks.md).

Apache-2.0. Developed with AI assistance.
