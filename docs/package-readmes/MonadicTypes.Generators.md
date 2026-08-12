# MonadicTypes.NET.Generators

Compile-time struct-callable adapters for measured MonadicTypes.NET hot paths.
This is an analyzer package: it adds generated source to the consuming project
and has no runtime dependency.

## Requirements

- C# 14 / .NET 10 consumer project
- Compatible with trimming and NativeAOT
- The annotated container must be a top-level, non-generic `static partial class`

## Install

```bash
dotnet add package MonadicTypes.NET --prerelease
dotnet add package MonadicTypes.NET.Generators --prerelease
```

## Quick Start

```csharp
using MonadicTypes;

public static partial class Operations
{
    [GenerateValueFunction]
    public static long Widen(int value) => value;
}

Result<int, string> source = Result<int, string>.Ok(42);
Result<long, string> widened = source.Map(Operations.Functions.Widen);

// The original method remains callable normally.
long direct = Operations.Widen(42);
```

## Generation Contract

The generator adds a public callable token and an aggressively inlined adapter
implementing `IValueFunction<TIn,TOut>` or `IValueAction<T>`. It uses no runtime
reflection, registration, or dynamic code. Async method return types work with
the corresponding operators in `MonadicTypes.NET.Async`.

Annotatable methods must be implemented, static, non-generic, and accept exactly
one by-value parameter. An optional attribute name controls the generated token
name. Invalid shapes produce `MTGEN001` through `MTGEN004` compile-time errors.

Do not annotate every Result-related method. Cached static delegates already
allocate 0 B per invocation. Generated generic callables can improve dispatch
and inlining in measured hot paths, but each distinct callable can increase
NativeAOT generic instantiation count and binary size.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET` | Result, Option, and callable contracts used by generated tokens |
| `MonadicTypes.NET.Async` | Async operators that accept generated tokens |

## Documentation

See [generated callables](https://github.com/czernous/monadic-types-dotnet#generated-callables)
and the [API behavior reference](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/api-reference.md#source-generation).

Apache-2.0. Developed with AI assistance.
