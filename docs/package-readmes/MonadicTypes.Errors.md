# MonadicTypes.NET.Errors

Structured, immutable errors and validation values for MonadicTypes.NET.
This package includes `MonadicTypes.NET` transitively.

## Requirements

- .NET 10 or later
- Compatible with trimming and NativeAOT
- No dependency on FluentValidation or another validation framework

## Install

```bash
dotnet add package MonadicTypes.NET.Errors --prerelease
```

## Quick Start

```csharp
using MonadicTypes;

public sealed record Customer(int Id);

static Result<Customer, Error> FindCustomer(int id) => id switch
{
    <= 0 => Result<Customer, Error>.Fail(
        Error.Validation("CUSTOMER_ID_INVALID", "Customer ID must be positive.")),
    _ => Result<Customer, Error>.Fail(
        Error.NotFound("CUSTOMER_NOT_FOUND", "The customer does not exist."))
};

ValidationErrors validation = new(
    new ValidationIssue("email", "EMAIL_INVALID", "Email is invalid."));
```

## Error Contract

`Error` carries a bounded category, stable machine code, message, public-message
policy, and optional exception cause. Built-in categories include validation,
authentication, authorization, not-found, conflict, rate-limit, availability,
timeout, cancellation, and unexpected failure. `Error.Custom` supports positive
application-defined numeric categories.

`Cause` is retained for diagnostics and stack-preserving rethrow; default HTTP
conversion never serializes it. Compact domain errors can implement
`IErrorConvertible<Error>` and widen only at an application boundary.

Equality compares category, numeric category, ordinal code, ordinal message,
disclosure policy, and retained-cause identity. Hashing uses the same fields.
`ToString` returns `[CODE] message`; `TryFormat` writes that representation to a
caller-owned span without allocating a string. These contracts do not expose
the compact internal message/cause representation.

`ValidationErrors.Create` maps third-party validation objects through caller
supplied functions, so compatibility does not require a runtime dependency on
the originating validation library. `ValidationErrors` is an immutable owner
with reference identity; use `AsSpan` for explicit allocation-free sequence
inspection or comparison.

Successful `Result<T,Error>` paths allocate 0 B. Rich errors allocate only when
an error is constructed; use compact readonly domain errors on measured failure
hot paths when that allocation matters.

## Related Packages

| Package | Add it for |
| --- | --- |
| `MonadicTypes.NET.AspNetCore` | Default HTTP/problem mappings and metadata |
| `MonadicTypes.NET.Diagnostics` | Activity and Meter projection |
| `MonadicTypes.NET.Effects` | Mapping thrown exceptions into errors |

## Documentation

See [structured errors and validation](https://github.com/czernous/monadic-types-dotnet#structured-errors)
and the [compatibility contract](https://github.com/czernous/monadic-types-dotnet/blob/master/docs/compatibility.md).

Apache-2.0. Developed with AI assistance.
