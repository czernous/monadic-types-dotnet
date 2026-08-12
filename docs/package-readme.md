# MonadicTypes.NET

Allocation-conscious functional primitives and integrations for C# 14 and
.NET 10, designed for trimming and NativeAOT.

The package family provides `Result<T, E>`, `Option<T>`, structured errors,
async composition, explicit exception boundaries, optional diagnostics,
ASP.NET Core typed results, and source-generated struct callables.

## Which Package To Install

Install only the highest-level features needed. NuGet brings their library
dependencies transitively.

| Package | Use it for |
| --- | --- |
| `MonadicTypes.NET` | Core `Result`, `Option`, `Unit`, and composition. Start here for general use. |
| `MonadicTypes.NET.Errors` | Structured errors and validation values; includes core. |
| `MonadicTypes.NET.Async` | Fluent `Task` and `ValueTask` pipelines; includes core. |
| `MonadicTypes.NET.Effects` | Explicit exception-to-error boundaries; includes core. |
| `MonadicTypes.NET.Diagnostics` | Optional `Activity` and `Meter` projection; includes errors and core. |
| `MonadicTypes.NET.AspNetCore` | Typed HTTP results, problem responses, validation, and endpoint metadata; includes errors and core. |
| `MonadicTypes.NET.Generators` | Optional compile-time callable adapters; analyzer only. |

Install the preview explicitly:

```bash
dotnet add package MonadicTypes.NET --prerelease
```

An API that also uses asynchronous pipelines and exception boundaries installs
the three corresponding top-level packages:

```bash
dotnet add package MonadicTypes.NET.AspNetCore --prerelease
dotnet add package MonadicTypes.NET.Async --prerelease
dotnet add package MonadicTypes.NET.Effects --prerelease
```

There is no all-in-one package. This keeps optional dependencies out of
applications that do not use them.

See the [repository README](https://github.com/czernous/monadic-types-dotnet#readme)
for the package map, complete API guide, examples, compatibility contract, and
performance guidance.

This project is licensed under Apache-2.0 and was developed with AI assistance.
