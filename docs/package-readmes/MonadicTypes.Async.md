# MonadicTypes.NET.Async

Fluent `Task` and `ValueTask` operators for MonadicTypes.NET results. This
package includes `MonadicTypes.NET` transitively.

## Requirements

- .NET 10 or later
- Compatible with trimming and NativeAOT

## Install

```bash
dotnet add package MonadicTypes.NET.Async --prerelease
```

## Quick Start

```csharp
using MonadicTypes;
using MonadicTypes.Async;

static ValueTask<Result<int, string>> ValidateAsync(int value) =>
    ValueTask.FromResult(value > 0
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Fail("Value must be positive."));

Result<string, string> result = await Result<int, string>.Ok(21)
    .BindAsync(ValidateAsync)
    .Map(static value => (value * 2).ToString());
```

## Operator Selection

| Underlying callback | Use |
| --- | --- |
| Synchronous | `Map`, `Bind`, or `BindError` |
| `ValueTask<T>` | `MapAsync`, `BindAsync`, or `BindErrorAsync` |
| `Task<T>` | `MapTaskAsync`, `BindTaskAsync`, or `BindErrorTaskAsync` |

The explicit Task names avoid ambiguous `async` lambda overloads. Do not wrap a
natural Task in ValueTask merely to use a different method name.

Completed ValueTask paths avoid Task and async-state-machine allocation. A
genuinely pending operation still pays the cost of its underlying asynchronous
work. Await each returned ValueTask exactly once.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.Effects` | Async operations that can throw |
| `MonadicTypes.NET.Generators` | Generated async callables on measured hot paths |
| `MonadicTypes.NET.Errors` | Standard structured errors |

## Documentation

See [async pipelines](https://github.com/czernous/monadic-types-dotnet#async-pipelines)
and the [API behavior reference](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/api-reference.md#async-result-operators).

Apache-2.0. Developed with AI assistance.
