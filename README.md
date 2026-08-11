# MonadicTypes.NET

Reflection-free, NativeAOT-compatible `Result`, `Option`, structured error, and
application-boundary primitives for .NET 10.

The runtime libraries require no service locator, runtime code generation, or
mandatory dependency-injection integration. Delegate APIs prioritize normal C#
usage; caller-state overloads, callable structs, and generated wrappers provide
allocation-free alternatives for measured hot paths.

> [!IMPORTANT]
> This repository is experimental and unpublished. APIs can change without
> compatibility shims until the first versioned release.

## Packages

| Project | Purpose | Guide | Runtime dependencies |
| --- | --- | --- | --- |
| `MonadicTypes` | `Result<T,E>`, `Option<T>`, `Unit`, composition, callable values | [Result](#result), [Option](#option), [combination](#combining-independent-results), [hot paths](#hot-paths) | BCL only |
| `MonadicTypes.Errors` | Structured `Error`, validation issues, domain-error widening | [Errors](#structured-errors), [validation](#validation-compatibility) | Core only |
| `MonadicTypes.Async` | Fluent Task and ValueTask result composition | [Async pipelines](#async-pipelines) | Core only |
| `MonadicTypes.Effects` | Explicit exception-to-error boundaries | [Exception boundaries](#exception-boundaries) | Core only |
| `MonadicTypes.Diagnostics` | Optional `Activity` and `Meter` projection | [Diagnostics](#diagnostics) | Errors and BCL diagnostics |
| `MonadicTypes.AspNetCore` | Typed HTTP results, RFC problem responses, endpoint metadata | [ASP.NET Core](#aspnet-core) | ASP.NET Core shared framework |
| `MonadicTypes.Generators` | Compile-time callable wrappers for annotated methods | [Hot paths](#hot-paths) | Analyzer only |

FluentValidation, OpenAPI test packages, and telemetry exporters are not runtime
dependencies. Compatibility is pinned and tested separately.

## Mental Model

`Result<T,E>` represents exactly one of two branches: an `Ok(T)` value or a
`Fail(E)` error. `Option<T>` represents a present, non-null `Some(T)` value or
`None`. Operators run only the callback for the active branch, so a pipeline
can describe control flow without repeatedly inspecting state.

Use `Result` when an operation can explain why it failed. Use `Option` when
absence is expected and needs no error by itself. Use `Result<Option<T>,E>` when
an operation can fail but a successful lookup may still find nothing.

The types are deeply immutable value types. `default(Result<T,E>)` is an
invalid, uninitialized state and throws when observed; always construct it with
`Ok` or `Fail`. `default(Option<T>)` is `None`, and `Some(null)` is rejected.

## API Guide

The core operators follow conventional functional-programming names:

| Result API | Purpose | Details |
| --- | --- | --- |
| `Ok`, `Fail`, `Result.Ok`, `Result.Fail` | Construct valued or `Unit` success/failure | [Construction](#result) |
| `IsInitialized`, `IsSuccess`, `IsFailure`, `Value`, `Error` | Query or read state | [Construction](#result) |
| `TryGetValue`, `TryGetError` | Inspect a branch at an imperative boundary | [Construction](#result) |
| `Map`, `Bind`, `Flatten` | Transform or continue the success railway | [Railway composition](#railway-composition) |
| `MapError`, `BindError`, `BindWidened`, `BiMap` | Transform, continue, or widen the failure railway | [Error composition](#error-composition) |
| `Ensure`, `Recover` | Introduce a guard or recover from failure | [Railway composition](#railway-composition) |
| `Tap`, `TapError`, `Finally` | Run explicit side effects without changing the value | [Side effects](#side-effects) |
| `Match`, `Switch` | Exhaustively terminate both branches | [Consuming a result](#consuming-a-result) |
| `ValueOr`, `ValueOrElse` | Deliberately reduce failure to a fallback value | [Consuming a result](#consuming-a-result) |
| `Transpose`, `RequireSome` | Convert between Result and Option shapes | [Option and Result](#option-and-result) |
| `Combine`, `Zip`, `ResultCombination.Map` | Compose independent Results, fail-fast | [Combination](#combining-independent-results) |
| implicit value/error conversion | Enter a typed Result when context is unambiguous | [Construction](#result) |

| Option API | Purpose | Details |
| --- | --- | --- |
| `Some`, `None`, implicit nullable conversion | Construct presence or absence | [Option](#option) |
| `HasValue`, `IsSome`, `IsNone`, `Value`, `TryGetValue` | Query or read presence | [Option](#option) |
| `Map`, `Bind`, `Filter` | Transform, continue, or filter presence | [Option](#option) |
| `Match`, `Switch`, `ValueOr`, `ValueOrElse` | Consume the two cases | [Option](#option) |
| `ToResult`, `Transpose`, `RequireSome` | Move between Option and Result | [Option and Result](#option-and-result) |

| Additional API family | Purpose | Details |
| --- | --- | --- |
| `AsyncResultExtensions` | Compose Result, Task, and ValueTask callbacks | [Async pipelines](#async-pipelines) |
| `Effect`, `ResultEffectExtensions` | Convert selected thrown exceptions to typed failures | [Exception boundaries](#exception-boundaries) |
| `Error`, `ErrorType`, `IErrorConvertible` | Structured and domain-specific failure representation | [Structured errors](#structured-errors) |
| `ValidationIssue`, `ValidationErrors` | Accumulated validation failures and third-party mapping | [Validation compatibility](#validation-compatibility) |
| `ErrorTelemetry`, `ErrorMetrics` | Optional vendor-neutral tracing and metrics | [Diagnostics](#diagnostics) |
| `ResultHttpExtensions`, problem helpers, metadata | Typed HTTP and OpenAPI boundaries | [ASP.NET Core](#aspnet-core) |
| callable interfaces, wrappers, generator | Generic dispatch for measured hot paths | [Hot paths](#hot-paths) |
| complete layered usage | Domain, dependency, telemetry, and endpoint composition | [Complete application flow](#complete-application-flow) |

Prefer pipelines for ordinary flow. Reserve `TryGetValue`, `TryGetError`, and
`Switch` for framework adapters, loops, or other explicit terminal boundaries.

## Efficient Use

Current NativeAOT benchmarks measure 0 B managed allocation for successful
synchronous pipelines that use pre-created or static delegates. Keep domain
values and errors small, use static callbacks when no state is required, pass
captured state explicitly, and convert to richer framework objects only at an
application boundary.

```csharp
Result<Receipt, CheckoutError> receipt = ParseOrder(input)
    .Bind(ReserveInventory)
    .Map(static reservation => Receipt.From(reservation));

Result<Receipt, CheckoutError> named = receipt.Map(
    prefix,
    static (value, state) => value with { DisplayName = state + value.DisplayName });
```

The first pipeline can reuse static delegate instances. The second passes
`prefix` as caller state instead of allocating a closure. Benchmarked completed
ValueTask paths allocate 0 B, and failure callbacks do not run on success.

These abstractions are not universally zero-cost in the Rust sense. A delegate
operator still performs delegate dispatch, every operator copies its small
value wrapper, an actual rich `Error` allocates on the failure path, and pending
asynchronous work may require a state machine. Do not split trivial arithmetic
into many operators or wrap an operation in Result when ordinary local control
flow is clearer. Start with the normal pipeline API; use caller-state overloads
to remove captures, then use generated or handwritten `IValueFunction` structs
only where a benchmark identifies dispatch as material.

Keep abstraction boundaries outside tight data-processing loops. A CSV parser,
SIMD normalizer, or token scanner should use direct spans and local control flow
per element, then return one `Result<ParsedValue,ParseError>` for the complete
operation. Constructing and composing a Result for every character, token, or
row adds work without improving the inner loop.

The library cannot change the cost of a database query, blocking callback, or
allocating async dependency. Its benchmarks cover the wrapper and composition
overhead, not the work performed inside callbacks.

## Result

Create results explicitly when the branch should be obvious at the call site:

```csharp
using MonadicTypes;

static Result<Customer, CustomerError> FindCustomer(int id) => id switch
{
    > 0 => Result<Customer, CustomerError>.Ok(new Customer(id)),
    _ => Result<Customer, CustomerError>.Fail(CustomerError.InvalidId)
};
```

`default(Result<T,E>)` is intentionally uninitialized. Reading or composing it
throws, which prevents accidental treatment of zeroed memory as success or
failure. Construct values with `Ok` or `Fail`.

`IsSuccess` and `IsFailure` query the active case; `Value` and `Error` read it
and throw for the other case. `TryGetValue` and `TryGetError` are intended for
imperative boundaries. Operations with no meaningful success payload use
`Result.Ok<TError>()` and `Result.Fail(error)`, which return `Result<Unit,TError>`.

```csharp
Result<Unit, SaveError> saved = repository.Save(entity)
    ? Result.Ok<SaveError>()
    : Result.Fail(SaveError.WriteFailed);

if (saved.TryGetError(out SaveError error))
{
    Console.Error.WriteLine(error);
}
```

Implicit conversion from `T` or `E` is available when the target Result type is
already explicit. Prefer `Ok` and `Fail` when both types could be confused at a
call site. Value equality is structural because Result is a record struct.
`ToString` returns `Ok(value)`, `Fail(error)`, or `Uninitialized`; use `Match`
for user-facing formatting rather than depending on that diagnostic form.

### Railway Composition

`Map` transforms success, `Bind` composes another operation, `MapError`
transforms failure, and `BindError` composes failure recovery. Inactive branch
callbacks are never invoked.

```csharp
Result<Receipt, CheckoutError> checkout = ParseOrder(input)
    .Ensure(static order => order.Lines.Count > 0, static _ => CheckoutError.EmptyOrder)
    .Bind(ReserveInventory)
    .Bind(ChargePayment)
    .Map(CreateReceipt)
    .Tap(static receipt => Audit(receipt))
    .TapError(static error => RecordFailure(error));

string response = checkout.Match(
    static receipt => $"Receipt {receipt.Id}",
    static error => $"Checkout failed: {error.Code}");
```

Use `BiMap` when both branch types must change, `Recover` for same-error-type
recovery, and `ValueOr`/`ValueOrElse` when the failure is intentionally reduced
to a fallback value.

### Error Composition

`MapError` changes an error value directly. `BindError` runs a Result-returning
failure continuation and may change the error type. `BiMap` changes both branch
types. None of them invokes the inactive branch.

```csharp
Result<Customer, ApiError> normalized = repositoryResult
    .MapError(static error => ApiError.FromRepository(error));

Result<Customer, FinalError> retried = normalized.BindError(
    static error => error.IsTransient
        ? RetryCustomerLoad(error)
        : Result<Customer, FinalError>.Fail(FinalError.From(error)));

Result<CustomerDto, ProblemCode> projected = retried.BiMap(
    static customer => CustomerDto.From(customer),
    static error => ProblemCode.From(error));
```

`BindWidened` is for a wide outer error plus a continuation whose compact domain
error implements `IErrorConvertible<TWideError>`. Conversion happens only when
the continuation actually fails:

```csharp
Result<Order, Error> loaded = LoadOrder(id);
Result<Receipt, Error> charged = loaded.BindWidened(ChargeWithDomainError);

static Result<Receipt, PaymentError> ChargeWithDomainError(Order order) =>
    paymentGateway.TryCharge(order);
```

### Side Effects

`Tap` observes only success, `TapError` observes only failure, and `Finally`
runs for either initialized branch. All return the original Result. Async side
effects use `TapAsync` and `FinallyAsync`.

```csharp
Result<Receipt, CheckoutError> observed = checkout
    .Tap(audit, static (receipt, sink) => sink.Record(receipt))
    .TapError(metrics, static (error, sink) => sink.Record(error))
    .Finally(timer, static value => value.Stop());
```

Use these for observation, not for hidden control flow. If an action can throw
and that exception belongs in the Result, use `TryTap` or `TryTapAsync` from
`MonadicTypes.Effects`. If the action's failure must affect later business
logic, model it as `Bind` instead.

### Consuming A Result

`Match` returns one common type, `Switch` executes one terminal action, and
`ValueOr`/`ValueOrElse` explicitly discard error information in favor of a
fallback. Prefer `Match` at presentation boundaries and `ValueOrElse` only when
fallback is genuinely the required behavior.

```csharp
string message = result.Match(
    static value => $"Loaded {value.Id}",
    static error => $"Failed with {error.Code}");

result.Switch(RenderValue, RenderError);

Customer customer = cached.ValueOrElse(static _ => Customer.Anonymous);
```

### Combining Independent Results

```csharp
Result<(User User, Account Account), LoadError> loaded =
    ResultCombination.Zip(LoadUser(id), LoadAccount(id));

Result<Invoice, LoadError> invoice = ResultCombination.Map(
    LoadUser(id),
    LoadAccount(id),
    static (user, account) => new Invoice(user, account));

ReadOnlySpan<Result<Unit, LoadError>> checks = [CheckUser(id), CheckAccount(id)];
Result<Unit, LoadError> valid = ResultCombination.Combine(checks);
```

Combination is fail-fast. It does not accumulate independent validation errors;
use `ValidationErrors` when accumulation is the required domain behavior.

## Option

`Option<T>` represents one non-null value or `None`. Conversion from null yields
`None`; `Some(null)` throws so populated options cannot silently contain null.

```csharp
Option<Customer> customer = repository.TryFind(id);

string displayName = customer
    .Filter(static value => value.IsActive)
    .Map(static value => value.DisplayName)
    .ValueOr("Unknown customer");
```

`Bind` prevents nested Options, `Filter` turns a present value into `None` when
its predicate fails, `Match` returns one type for both cases, and `Switch`
executes one terminal action. `ValueOrElse` delays fallback creation and has a
caller-state overload for allocation-free state access.

```csharp
Option<Address> address = customer
    .Filter(static value => value.IsActive)
    .Bind(static value => value.PrimaryAddress);

string city = address.Match(
    static value => value.City,
    static () => "No address");
```

### Option And Result

Nested Result/Option shapes can be transposed without ad-hoc branching:

```csharp
Result<Option<Customer>, LookupError> lookup = FindOptionalCustomer(id);
Option<Result<Customer, LookupError>> presentResult = lookup.Transpose();

Result<Customer, LookupError> required = lookup.RequireSome(
    static () => LookupError.NotFound);
```

## Async Pipelines

Import `MonadicTypes.Async` to mix synchronous and asynchronous operations in a
single fluent pipeline. Operators converge on `ValueTask<Result<T,E>>`; completed
ValueTask paths avoid async state-machine and Task allocation.

| Receiver | Synchronous callback | ValueTask callback | Task callback |
| --- | --- | --- | --- |
| `Result<T,E>` | Core `Map`, `Bind`, `BindError` | `MapAsync`, `BindAsync`, `BindErrorAsync` | `MapTaskAsync`, `BindTaskAsync`, `BindErrorTaskAsync` |
| `ValueTask<Result<T,E>>` | `Map`, `Bind`, `BindError` | `MapAsync`, `BindAsync`, `BindErrorAsync` | `MapTaskAsync`, `BindTaskAsync`, `BindErrorTaskAsync` |
| `Task<Result<T,E>>` | `Map`, `Bind`, `BindError` | `MapAsync`, `BindAsync`, `BindErrorAsync` | `MapTaskAsync`, `BindTaskAsync`, `BindErrorTaskAsync` |

The callback runs only for its active branch. Completed sources use a direct
path; pending sources are awaited exactly once with `ConfigureAwait(false)`.
The returned ValueTask must also be consumed once.

```csharp
using MonadicTypes;
using MonadicTypes.Async;

Result<Receipt, CheckoutError> result = await ParseOrder(input)
    .BindAsync(ReserveInventoryAsync)       // ValueTask callback
    .Map(static reservation => reservation.Order)
    .BindTaskAsync(ChargePaymentAsync)      // Task callback
    .Map(CreateReceipt);
```

Task-returning callback methods are named `MapTaskAsync`, `BindTaskAsync`, and
`BindErrorTaskAsync`. ValueTask callbacks use `MapAsync`, `BindAsync`, and
`BindErrorAsync`. Separate names are intentional: overloads differing only by
Task versus ValueTask make ordinary `async` lambdas ambiguous to the compiler.

Task and ValueTask result receivers also expose synchronous `Map`, `Bind`, and
`BindError`, allowing the rest of a pipeline to remain fluent after its first
asynchronous operation.

### Choosing An Overload

| API shape | Use when | Benefit |
| --- | --- | --- |
| `Map(value => ...)` | Default application code | Simplest call site; static delegates allocate nothing per invocation |
| `Map(state, static (value, state) => ...)` | A lambda would capture caller state | Avoids closure allocation while retaining normal functions |
| `Map(default(MyFunction))` | A benchmark proves delegate dispatch matters | Generic callable can inline and is materially faster in current primitive benchmarks |
| `MapAsync(ValueTaskCallback)` | The operation naturally returns ValueTask | Completed paths avoid Task and async-state-machine allocation |
| `MapTaskAsync(TaskCallback)` | Existing APIs naturally return Task | No adapter lambda or ambiguous Task/ValueTask overload resolution |

The distinct Task method names are an ergonomics and overload-resolution
benefit, not an intrinsic speed claim. Do not wrap a naturally returned Task in
ValueTask merely to call `MapAsync`; use `MapTaskAsync`. Conversely, do not
convert a naturally synchronous operation to an awaitable. Callable structs and
caller-state overloads are performance tools and should be selected from
benchmark evidence, not used mechanically throughout application code. In the
current NativeAOT Effects benchmark, the typed caller-state completed Task path
is 13.156 ns and 0 B versus 16.366 ns and 0 B for the non-state path.

## Exception Boundaries

Normal Result callbacks do not catch exceptions. Use `MonadicTypes.Effects`
where code outside your control can throw and the exception should become a
domain error.

| API | Use |
| --- | --- |
| `Effect.Try` | Run a synchronous operation and map recoverable or one selected exception type |
| `Effect.TryAsync` | Run a ValueTask operation; typed and broad exception overloads are available |
| `Effect.TryTaskAsync` | Run a Task operation directly; typed, broad, and caller-state overloads are available |
| `ExceptionFilter.IsRecoverable` | Apply the same broad-capture policy in a custom boundary |
| `TryMap`, `TryMapAsync` | Map an existing success through code that can throw |
| `TryBind` | Bind an existing success through Result-returning code that can throw |
| `TryTap`, `TryTapAsync` | Run a throwing success side effect and turn its exception into failure |

Use the broad overload when every recoverable exception has the same domain
meaning. Use a typed overload when only one exception is expected; all other
types propagate. Use a caller-state overload when the operation needs local
data and a capturing lambda would allocate.

`ExceptionFilter.IsRecoverable` returns false for cancellation, stack overflow,
out-of-memory, access violation, bad image, and similar runtime failures. It is
public so a custom adapter can use exactly the same broad-capture policy. Typed
Effect overloads intentionally do not apply this filter: selecting an exception
type is an explicit decision to convert that type.

```csharp
using MonadicTypes.Effects;

Result<Document, ImportError> imported = Effect.Try(
    () => thirdPartyParser.Parse(payload),
    static exception => ImportError.From(exception));

Result<Dto, ImportError> mapped = imported.TryMap(
    static document => legacyMapper.Map(document),
    static exception => ImportError.From(exception));

Result<SavedDocument, ImportError> saved = mapped.TryBind(
    static dto => legacyStore.Save(dto),
    static exception => ImportError.Storage(exception));

Result<SavedDocument, ImportError> audited = saved.TryTap(
    static document => legacyAudit.Write(document),
    static exception => ImportError.Audit(exception));
```

Broad capture deliberately excludes cancellation and process-corrupting/runtime
exceptions. Use the typed `Effect.Try<T,TError,TException>` overload only when a
specific exception, including cancellation, must be converted intentionally.
The mapper decides the error type. A known timeout can become a domain timeout,
a dependency exception can become `Unavailable`, and only an unclassified fault
should become `Unexpected`. Every `Error` category can retain the original
exception as `Cause`; `Error.ThrowCause()` uses `ExceptionDispatchInfo` to
preserve its original stack.

```csharp
Result<Quote, Error> quote = Effect.Try<Quote, Error, VendorUnavailableException>(
    () => vendor.GetQuote(request),
    static exception => Error.Unavailable(
        "QUOTE_VENDOR_UNAVAILABLE",
        "Quote service is temporarily unavailable.",
        cause: exception));
```

Exceptions not matched by the typed overload propagate normally and remain the
responsibility of the application-wide exception handler.

### Files And Parsing

Catch exceptions only around the operations that can throw, then continue with
normal Result and Option composition:

```csharp
static Result<AppSettings, Error> LoadSettings(string path) => Effect.Try(
        () => File.ReadAllText(path),
        static exception => Error.Failure(
            "SETTINGS_READ_FAILED",
            "Settings could not be read.",
            cause: exception))
    .TryMap(
        static json => (Option<AppSettings>)JsonSerializer.Deserialize<AppSettings>(json),
        static exception => Error.Failure(
            "SETTINGS_JSON_INVALID",
            "Settings contain invalid JSON.",
            cause: exception))
    .RequireSome(static () =>
        Error.Validation("SETTINGS_EMPTY", "The settings document was empty."));
```

File and JSON exceptions become typed failures. A valid JSON `null` becomes
`Option.None`, then `RequireSome` gives absence its own domain meaning.

### Selected Exceptions

Use the typed overload when only one known exception belongs in the Result and
all other exceptions should propagate:

```csharp
static Result<Customer, LookupError> FindCustomer(
    IReadOnlyDictionary<int, Customer> customers,
    int id) => Effect.Try<Customer, LookupError, KeyNotFoundException>(
        () => customers[id],
        static _ => LookupError.NotFound);
```

### Async Dependencies

Task-returning dependencies can be caught without an adapter Task allocation,
then converted to Option explicitly when null means absence:

```csharp
static async ValueTask<Result<Customer, Error>> FetchCustomerAsync(
    HttpClient client,
    int id)
{
    Result<Customer?, Error> fetched = await Effect.TryTaskAsync(
        () => client.GetFromJsonAsync<Customer>($"customers/{id}"),
        static exception => Error.Unavailable(
            "CUSTOMER_SERVICE_UNAVAILABLE",
            "Customer service is temporarily unavailable.",
            cause: exception));

    return fetched
        .Map(static customer => (Option<Customer>)customer)
        .RequireSome(static () =>
            Error.NotFound("CUSTOMER_NOT_FOUND", "The customer does not exist."));
}
```

Cancellation propagates by default. Do not turn request cancellation into an
error response unless the application has an explicit reason to do so.

## Structured Errors

`Error` supplies stable machine codes, broad transport/telemetry categories,
message-visibility policy, optional exception causes, and custom numeric
categories.

| Error type | Default HTTP status | Typical meaning |
| --- | ---: | --- |
| `Validation` | 400 | Invalid input or a client-correctable rule failure |
| `Unauthorized` | 401 | Missing or invalid authentication |
| `Forbidden` | 403 | Authenticated caller lacks permission |
| `NotFound` | 404 | Requested resource is absent |
| `Conflict` | 409 | State or concurrency conflict |
| `Cancelled` | 499 | Request cancelled by the caller |
| `RateLimited` | 429 | Quota or rate exceeded |
| `Unavailable` | 503 | Temporary dependency or service outage |
| `Timeout` | 504 | Dependency exceeded its time budget |
| `Failure`, `Unexpected`, `Custom` | 500 | Private failure unless a custom HTTP mapper overrides policy |

| Construction API | Category |
| --- | --- |
| `Error.Validation`, `Conflict`, `NotFound` | Expected input and resource failures |
| `Error.Unauthorized`, `Forbidden` | Authentication and authorization failures |
| `Error.RateLimited`, `Unavailable`, `Timeout`, `Cancelled` | Operational failures with explicit transport policy |
| `Error.Failure`, `Unexpected` | General private failures; use `Unexpected` for unclassified faults |
| `Error.Custom` | Positive application-defined numeric category |
| `Error.IO`, `System` | Convenience codes for general I/O and system failures |
| `new Error(type, code, message, ...)` | Full built-in-category control |

`Code` is stable machine-readable identity. `Message` is diagnostic text and is
included in a problem response only when `IsMessagePublic` is true. `Cause` is
never serialized by the default adapter; it exists for tracing, logging, and
stack-preserving rethrow. Use the public `Error` constructor for a built-in
category, `Error.Custom` for an application-defined numeric category, or a
compact domain error implementing `IErrorConvertible<Error>`.

`ToString` formats `[CODE] message`. `TryFormat` writes the same representation
to a caller-provided span without creating a string. `ThrowCause` rethrows a
retained exception through `ExceptionDispatchInfo`; never write `throw
error.Cause`, which resets the stack trace.

```csharp
Span<char> buffer = stackalloc char[128];
if (error.TryFormat(buffer, out int written, default, null))
{
    WriteDiagnostic(buffer[..written]);
}
```

```csharp
Error notFound = Error.NotFound(
    "CUSTOMER_NOT_FOUND",
    "The requested customer does not exist.");

Error internalFailure = Error.Unexpected(exception, "CUSTOMER_QUERY_FAILED");

Error vendorSpecific = Error.Custom(
    numericType: 10_001,
    code: "VENDOR_REJECTED",
    message: "The vendor rejected the request.");
```

For allocation-sensitive domains, carry a compact readonly struct error through
the hot path and convert only at the HTTP or telemetry boundary:

```csharp
public readonly record struct CustomerError(int Code) : IErrorConvertible<Error>
{
    public Error ToError() => Error.Failure($"CUSTOMER_{Code}", "Customer operation failed.");
}
```

The rich `Error` is reference-backed by design. Constructing an actual error
allocates on the failure path, while successful `Result<T,Error>` construction
and propagation remain allocation-free and avoid copying a large error struct.

## Validation Compatibility

Map any standard third-party validation list through the generic API. This keeps
FluentValidation and similar libraries out of the runtime dependency graph.

`ValidationIssue` contains `Path`, stable `Code`, display `Message`, and
`ValidationSeverity`. `ValidationErrors` is an immutable read-only collection.
Construct it from issues directly, from an array, or with `Create`:

| Construction | Use |
| --- | --- |
| `new ValidationErrors(issues)` | Existing issue sequence |
| `new ValidationErrors(issueArray)` | Existing issue array |
| `ValidationErrors.Create(source, mapper)` | Third-party failures with a normal mapper |
| `ValidationErrors.Create(source, state, mapper)` | Mapper needs caller state without a closure |
| `ValidationErrors.Create<TFailure,TMapper>` | Struct mapper for measured hot paths |

```csharp
ValidationErrors errors = new(
    new ValidationIssue("email", "EMAIL_INVALID", "Email is invalid."),
    new ValidationIssue(
        "displayName",
        "DISPLAY_NAME_SHORT",
        "Display name is short.",
        ValidationSeverity.Warning));

Result<User, ValidationErrors> invalid = Result<User, ValidationErrors>.Fail(errors);
```

```csharp
ValidationErrors errors = ValidationErrors.Create(
    validationResult.Errors,
    static failure => new ValidationIssue(
        failure.PropertyName,
        failure.ErrorCode,
        failure.ErrorMessage,
        failure.Severity switch
        {
            Severity.Error => ValidationSeverity.Error,
            Severity.Warning => ValidationSeverity.Warning,
            Severity.Info => ValidationSeverity.Information,
            _ => throw new ArgumentOutOfRangeException(nameof(failure))
        }));
```

The compatibility suite currently pins FluentValidation 12.1.1. Consumers own
their mapping policy; no version-coupled runtime adapter is required.

## ASP.NET Core

Minimal APIs can return strongly typed success and RFC problem results without
reflection or controller infrastructure:

| API | Use |
| --- | --- |
| `result.ToHttpResult(success)` | Default `Error`, convertible domain-error, or `ValidationErrors` mapping |
| `result.ToHttpResult(success, failure)` | Caller-owned typed failure mapping escape hatch |
| `IHttpResultMapper<TError,TResult>` | Allocation-free struct-based failure mapper contract |
| `DefaultErrorHttpResultMapper` | Default `Error` to `ProblemHttpResult` mapper |
| `ErrorProblemDetails.Create`, `ToHttpResult` | Build default RFC problem representations directly |
| `ErrorProblemDetails.GetStatusCode` | Read the default status policy for an `ErrorType` |
| `ValidationErrorProblemDetails.ToHttpResult` | Build a typed validation problem with codes |
| `.ProducesErrors(...)` | Add Minimal API response metadata for multiple categories |
| `[ProducesError(...)]` | Add controller response metadata for one category |

```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes.AspNetCore;

static Results<Ok<Customer>, ProblemHttpResult> GetCustomer(
    int id,
    HttpContext context)
{
    Result<Customer, Error> result = LoadCustomer(id);
    return result.ToHttpResult(static customer => TypedResults.Ok(customer), context);
}

app.MapGet("/customers/{id:int}", GetCustomer)
    .ProducesErrors(ErrorType.NotFound, ErrorType.Unexpected);
```

Use the two-callback overload when an application owns its ProblemDetails
shape or needs a status outside the default category policy:

```csharp
Results<Ok<Customer>, ProblemHttpResult> response = result.ToHttpResult(
    static customer => TypedResults.Ok(customer),
    static error => TypedResults.Problem(
        statusCode: StatusCodes.Status402PaymentRequired,
        title: "Payment required",
        extensions: new Dictionary<string, object?>
        {
            ["code"] = error.Code
        }));
```

For a measured hot boundary, implement `IHttpResultMapper<TError,TResult>` as a
readonly struct and pass it to the mapper overload; success mappers can likewise
implement `IValueFunction<T,TResult>`. This removes delegate dispatch and
captures while retaining a strongly typed result union.

`Result<T,ValidationErrors>` maps to `ValidationProblem`. Domain errors that
implement `IErrorConvertible<Error>` map at the boundary without widening the
success-path Result type. Fully generic `ToHttpResult(success, failure)`
overloads are the escape hatch for custom `ProblemDetails`, framework results,
or application-specific responses.

Result conversion does not replace global exception handling. Expected failures
that code deliberately maps enter `ToHttpResult`; programming defects and
exceptions outside an `Effect` boundary continue to the ASP.NET Core exception
handler. Keep that handler application-owned so logging and disclosure policy
remain explicit:

```csharp
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled request failure");

        Error error = Error.Unexpected(exception);
        ErrorTelemetry.Record(Activity.Current, error);

        await ErrorProblemDetails
            .ToHttpResult(error, context)
            .ExecuteAsync(context);
        return true;
    }
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

WebApplication app = builder.Build();
app.UseExceptionHandler();
```

The resulting response is a generic 500 problem because `Unexpected` messages
are private by default. The retained exception remains available to logging and
tracing. Do not wrap an entire endpoint in `Effect.Try`; map only exceptions
that have a meaningful domain classification and let all others reach this
handler.

Controllers can use `[ProducesError(ErrorType.NotFound)]` for response metadata.
Controller runtime support depends on ASP.NET Core's own NativeAOT limitations;
the Result and Minimal API paths are NativeAOT-compatible.

For controllers, keep MVC conversion in one explicit handler instead of
branching inside every action:

```csharp
public static class ControllerResults
{
    public static ActionResult<T> ToActionResult<T>(
        ControllerBase controller,
        in Result<T, Error> result)
    {
        if (result.TryGetValue(out T value))
        {
            return value;
        }

        Error error = result.Error;
        return new ObjectResult(ErrorProblemDetails.Create(error, controller.HttpContext))
        {
            StatusCode = ErrorProblemDetails.GetStatusCode(error.Type)
        };
    }
}

[ApiController]
[Route("customers")]
public sealed class CustomersController(CustomerService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesError(ErrorType.NotFound)]
    [ProducesError(ErrorType.Unexpected)]
    public async Task<ActionResult<Customer>> Get(int id)
    {
        Result<Option<Customer>, Error> lookup = await service.FindAsync(id);
        Result<Customer, Error> result = lookup.RequireSome(static () =>
            Error.NotFound("CUSTOMER_NOT_FOUND", "The customer does not exist."));

        return ControllerResults.ToActionResult(this, result);
    }
}
```

The controller adapter is application code because MVC result policy varies.
Minimal APIs can use the supplied strongly typed adapter directly.

## Diagnostics

Diagnostics are explicit and optional. Creating or propagating a Result never
logs, traces, or records metrics automatically.

| API | Behavior |
| --- | --- |
| `ErrorTelemetry.Record(activity, error)` | Adds standard error tags, an exception/event, and status according to policy |
| `ErrorActivityStatusPolicy.Automatic` | Marks failure, unexpected, unavailable, timeout, and custom errors |
| `ErrorActivityStatusPolicy.Preserve` | Records data without changing Activity status |
| `ErrorActivityStatusPolicy.MarkError` | Always marks the Activity as error |
| `new ErrorMetrics(meter, includeErrorCode, name)` | Creates a caller-owned error counter |
| `ErrorMetrics.Record(error)` | Increments the enabled counter with bounded category tags |
| `ErrorMetrics.IsEnabled` | Reports whether the underlying counter currently has a listener |
| `ErrorMetrics.Disabled` | Zero-configuration disabled value |

```csharp
public readonly struct ObserveError(ErrorMetrics metrics) : IValueAction<Error>
{
    public void Invoke(Error error)
    {
        ErrorTelemetry.Record(Activity.Current, error);
        metrics.Record(error);
    }
}

Result<Receipt, Error> observed = result.TapError(new ObserveError(errorMetrics));
```

`ErrorTelemetry` uses BCL `Activity` data understood by OpenTelemetry exporters.
`ErrorMetrics` uses a caller-owned `Meter`, so Prometheus and other
`System.Diagnostics.Metrics` consumers can export it. Use
`ErrorMetrics.Disabled` or omit the diagnostics package for the lowest-cost
disabled path. Applications remain free to project public error values into
Serilog, Application Insights, Elastic, or a custom stack.

## Complete Application Flow

Keep compact domain errors inside application logic, convert them at the
boundary, and observe a failure once rather than logging it at every railway
step.

```csharp
public sealed record OrderLine(string Sku, int Quantity);
public sealed record CheckoutCommand(IReadOnlyList<OrderLine> Lines);
public sealed record Order(IReadOnlyList<OrderLine> Lines)
{
    public static Order Create(CheckoutCommand command) => new(command.Lines);
}

public readonly record struct Reservation(Order Order);
public readonly record struct Payment(string Id);
public readonly record struct Receipt(string PaymentId)
{
    public static Receipt From(Payment payment) => new(payment.Id);
}

public interface IInventoryClient
{
    Task<Reservation> ReserveAsync(Order order);
}

public interface IPaymentClient
{
    // A null response means the payment was declined, not that the call failed.
    Task<Payment?> ChargeAsync(Reservation reservation);
}

public sealed class InventoryUnavailableException(string message)
    : Exception(message);

public sealed class PaymentUnavailableException(string message)
    : Exception(message);

public enum CheckoutErrorCode : byte
{
    EmptyBasket,
    InventoryUnavailable,
    PaymentDeclined,
    PaymentUnavailable
}

public readonly record struct CheckoutError(
    CheckoutErrorCode Code,
    Exception? Cause = null)
    : IErrorConvertible<Error>
{
    public Error ToError() => Code switch
    {
        CheckoutErrorCode.EmptyBasket =>
            Error.Validation("EMPTY_BASKET", "The basket contains no items."),
        CheckoutErrorCode.InventoryUnavailable =>
            Error.Unavailable(
                "INVENTORY_UNAVAILABLE",
                "Inventory service is temporarily unavailable.",
                cause: Cause),
        CheckoutErrorCode.PaymentDeclined =>
            Error.Validation("PAYMENT_DECLINED", "Payment was declined."),
        CheckoutErrorCode.PaymentUnavailable =>
            Error.Unavailable(
                "PAYMENT_UNAVAILABLE",
                "Payment is temporarily unavailable.",
                cause: Cause),
        _ => Error.Unexpected("Unknown checkout failure.")
    };
}

static Result<Order, CheckoutError> Validate(CheckoutCommand command) =>
    command.Lines.Count is 0
        ? Result<Order, CheckoutError>.Fail(new(CheckoutErrorCode.EmptyBasket))
        : Result<Order, CheckoutError>.Ok(Order.Create(command));

public sealed class CheckoutService(
    IInventoryClient inventory,
    IPaymentClient payment)
{
    public ValueTask<Result<Reservation, CheckoutError>> ReserveInventoryAsync(
        Order order) => Effect.TryTaskAsync(
            (Client: inventory, Order: order),
            static state => state.Client.ReserveAsync(state.Order),
            static (InventoryUnavailableException exception) => new CheckoutError(
                CheckoutErrorCode.InventoryUnavailable,
                exception));

    public async ValueTask<Result<Payment, CheckoutError>> ChargeAsync(
        Reservation reservation)
    {
        Result<Payment?, CheckoutError> charged = await Effect.TryTaskAsync(
            (Client: payment, Reservation: reservation),
            static state => state.Client.ChargeAsync(state.Reservation),
            static (PaymentUnavailableException exception) => new CheckoutError(
                CheckoutErrorCode.PaymentUnavailable,
                exception));

        return charged
            .Map(static value => (Option<Payment>)value)
            .RequireSome(static () => new CheckoutError(
                CheckoutErrorCode.PaymentDeclined));
    }
}

static ValueTask<Result<Receipt, CheckoutError>> CheckoutAsync(
    CheckoutCommand command,
    CheckoutService service) => Validate(command)
        .BindAsync(service.ReserveInventoryAsync)
        .BindAsync(service.ChargeAsync)
        .Map(static payment => Receipt.From(payment));
```

Only the two known dependency exceptions are converted. A null payment response
becomes `Option.None`, then `RequireSome` gives that expected absence the
`PaymentDeclined` domain meaning. A different thrown exception propagates to the
global handler shown above, where it is logged and returned as a private 500
problem. Retained causes on expected dependency errors remain available to
tracing without changing their HTTP classification. The dependency calls pass
client and request data as explicit tuple state; their static callbacks do not
allocate closures.

At a Minimal API boundary, logging remains application-owned while tracing,
metrics, HTTP conversion, and OpenAPI metadata use optional packages:

```csharp
public sealed class CheckoutLogCategory;

public readonly struct ObserveCheckoutFailure(
    ILogger<CheckoutLogCategory> logger,
    ErrorMetrics metrics) : IValueAction<CheckoutError>
{
    public void Invoke(CheckoutError domainError)
    {
        Error error = domainError.ToError();

        logger.LogWarning(
            "Checkout failed with {ErrorCode} ({ErrorCategory})",
            error.Code,
            error.Type);
        ErrorTelemetry.Record(Activity.Current, error);
        metrics.Record(error);
    }
}

static async Task<Results<Ok<Receipt>, ProblemHttpResult>> CheckoutEndpoint(
    CheckoutCommand command,
    CheckoutService service,
    ILogger<CheckoutLogCategory> logger,
    ErrorMetrics metrics,
    HttpContext context) => (await CheckoutAsync(command, service))
        .TapError(new ObserveCheckoutFailure(logger, metrics))
        .ToHttpResult(static receipt => TypedResults.Ok(receipt), context);

app.MapPost("/checkout", CheckoutEndpoint)
    .ProducesErrors(
        ErrorType.Validation,
        ErrorType.Unavailable,
        ErrorType.Unexpected);
```

The application wires exporters using normal OpenTelemetry packages. The
library does not reference or initialize an exporter:

```csharp
const string meterName = "MyCompany.Checkout";
Meter checkoutMeter = new(meterName);
builder.Services.AddSingleton(new ErrorMetrics(checkoutMeter));

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(meterName)
        .AddPrometheusExporter());

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});
```

`ErrorTelemetry.Record` adds `error.type`, `error.category`, and
`error.message` tags to the current sampled `Activity`, records the retained
exception or an `error` event, and applies the configured activity-status
policy. `ErrorMetrics.Record` increments `monadic.errors` with a bounded
`error.category` dimension; error-code dimensions are opt-in because they can
have high cardinality. OTLP can flow through an OpenTelemetry Collector to
Grafana, Elastic/Kibana, Application Insights, or another backend. Prometheus
scrapes the metrics exporter directly. Structured `ILogger` fields flow through
the application's selected logging provider.

This shape avoids repeated observation, keeps domain code independent of HTTP
and telemetry packages, retains machine-readable error information, and lets
the application replace the explicit boundary handler independently.

## Hot Paths

Normal delegate overloads are the default ergonomic API. Before replacing them,
measure the actual call site. For a proven hot path, use caller-owned state to
avoid closures or an `IValueFunction<TIn,TOut>` readonly struct to enable direct
generic dispatch:

| API | Use |
| --- | --- |
| `IValueFunction<TIn,TOut>` | Struct callable for value-returning operations |
| `IValueAction<T>` | Struct callable for side effects |
| `ValueFunction<TIn,TOut,TFunction>` | Generated callable wrapper passed to overloaded APIs |
| `ValueAction<T,TAction>` | Generated action wrapper passed to `Tap`/`TapError` |
| `[GenerateValueFunction]` | Generate public wrappers for an existing public static method |
| caller-state overloads | Keep normal static functions while passing capture data explicitly |

```csharp
public readonly struct Increment : IValueFunction<int, int>
{
    public int Invoke(int value) => value + 1;
}

Result<int, ParseError> incremented = result.Map(default(Increment));
```

The source generator can create public callable wrappers for public static
methods while leaving the original method callable normally:

```csharp
public static partial class Operations
{
    [GenerateValueFunction]
    public static long Widen(int value) => value;
}

Result<long, ParseError> widened = result.Map(Operations.Functions.Widen);
```

The annotated method remains an ordinary callable method; generation neither
replaces it nor changes its body. For the example above the generator adds:

- `Operations.Functions.Widen`, a public zero-field `ValueFunction` token.
- A readonly adapter struct implementing `IValueFunction<int,long>` whose
  `Invoke` forwards to `Operations.Widen`.
- An aggressively inlined forwarding call, with no runtime reflection or
  registration.

Void methods generate `ValueAction`/`IValueAction` wrappers. An optional
`[GenerateValueFunction("Name")]` argument changes the generated property name.
The method must be implemented, static, non-generic, and have exactly one
by-value parameter. Its containing type must be a non-generic, top-level,
`static partial` class. Generated member accessibility follows the annotated
method and containing type.

Generator diagnostics use the `MTGEN` prefix, short for MonadicTypes Generator:

| Diagnostic | Meaning | Fix |
| --- | --- | --- |
| `MTGEN001` | Annotated method is not an implemented, non-generic static method with one by-value parameter | Make the method static and concrete, remove method type parameters, and accept exactly one value parameter |
| `MTGEN002` | Containing type cannot host generated members | Use a non-generic, top-level `static partial class` |
| `MTGEN003` | Requested generated name is not a valid C# identifier | Remove the custom name or supply a valid identifier |
| `MTGEN004` | Two methods request the same generated property name | Give one attribute a unique name |

These diagnostics are compile-time errors because silently skipping a wrapper
would make a referenced generated symbol disappear. Each distinct callable type
can create another generic instantiation in consumer code, increasing native
binary size. Generate wrappers for measured hot operations, not every callback.
Callers that do not need generic dispatch continue calling `Operations.Widen`
normally and do not depend on the generated token.

## Performance Contract

- Runtime projects build with trim and NativeAOT analyzers as errors.
- Core, Errors, Async, and Effects emit XML documentation and reject missing
  public-member documentation.
- Benchmark inputs, delegates, Tasks, errors, and setup allocations are created
  outside measured operations.
- Accepted success/composition paths must allocate 0 B.
- New operations must beat an architectural target derived from an analogous
  optimized primitive before their first measurement becomes a baseline.
- Primitive and composition NativeAOT runners have isolated output directories
  so build order cannot contaminate code layout or execute the wrong harness.

See [benchmark policy](docs/benchmarks.md), [accepted baselines](benchmarks/baseline.md),
[compatibility](docs/compatibility.md), and [dependency policy](docs/dependency-policy.md).

## Status And Licensing

This repository is maintained primarily for personal experimentation and reuse;
it currently carries no support or stability commitment.

No license has been selected. Until a license is added explicitly, publication
does not grant permission to copy, modify, redistribute, or use the code
commercially.

## AI-Assisted Development

This project is AI-assisted. Architecture, acceptance decisions, performance
criteria, and published changes remain subject to human direction and review.
