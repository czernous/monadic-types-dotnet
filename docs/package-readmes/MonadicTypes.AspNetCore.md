# MonadicTypes.NET.AspNetCore

NativeAOT-compatible typed HTTP results, RFC problem responses, validation
conversion, and endpoint metadata for MonadicTypes.NET. This package includes
`MonadicTypes.NET.Errors` and the core package transitively.

## Requirements

- ASP.NET Core and .NET 10 or later
- Minimal APIs are the primary reflection-free path
- No runtime dependency on FluentValidation or an OpenAPI package

## Install

```bash
dotnet add package MonadicTypes.NET.AspNetCore --prerelease
```

## Quick Start

```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;
using MonadicTypes.AspNetCore;

public sealed record Customer(int Id);

static Results<Ok<Customer>, ProblemHttpResult> GetCustomer(int id)
{
    Result<Customer, Error> result = id > 0
        ? Result<Customer, Error>.Ok(new Customer(id))
        : Result<Customer, Error>.Fail(
            Error.NotFound("CUSTOMER_NOT_FOUND", "Customer was not found."));

    return result.ToHttpResult(static customer => TypedResults.Ok(customer));
}

app.MapGet("/customers/{id:int}", GetCustomer)
    .ProducesErrors(ErrorType.NotFound, ErrorType.Unexpected);
```

## HTTP Contract

- Built-in Error categories map to bounded default status codes.
- Private messages and exception causes are not exposed by default.
- `ValidationErrors` maps to a typed validation problem response.
- Errors implementing `IErrorConvertible<Error>` can use the default policy.
- Two-callback and `IHttpResultMapper` overloads are escape hatches for custom
  domain errors, status policies, or ProblemDetails shapes.
- `.ProducesErrors(...)` adds Minimal API metadata; `[ProducesError]` provides
  controller metadata without requiring a runtime OpenAPI dependency.

Minimal API adapters return strongly typed results and are NativeAOT tested.
Controller applications can use the metadata attribute and own their MVC result
adapter because controller serialization and action-result policy vary by app.

Install `MonadicTypes.NET.Async` and `MonadicTypes.NET.Effects` separately when
the endpoint pipeline needs them; this package does not force unrelated
features into an API.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.Async` | Task and ValueTask endpoint pipelines |
| `MonadicTypes.NET.Effects` | Exception-producing dependencies |
| `MonadicTypes.NET.Diagnostics` | Optional error tracing and metrics |

## Documentation

See [ASP.NET Core usage](https://github.com/czernous/monadic-types-dotnet#aspnet-core)
and the [compatibility contract](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/compatibility.md).

Apache-2.0. Developed with AI assistance.
