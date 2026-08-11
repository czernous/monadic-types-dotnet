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
| `MonadicTypes.Generators` | Compile-time callable wrappers for annotated methods | [Generated callables](#generated-callables) | Analyzer only |

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

Every authored public member is indexed below. Each member links to the section
that explains its branch behavior, appropriate use, performance implications,
and examples. An overload family is one conceptual member; its row enumerates
the available callback or receiver shapes.

### Core Result API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Result<T,E>.Ok`](#result) | `Ok(T)` | Construct success explicitly. |
| [`Result<T,E>.Fail`](#result) | `Fail(E)` | Construct failure explicitly. |
| [`Result.Ok`](#result) | `Ok<E>()` | Construct `Result<Unit,E>` success. |
| [`Result.Fail`](#result) | `Fail<E>(E)` | Construct `Result<Unit,E>` failure. |
| [`Result<T,E>.IsInitialized`](#result) | `bool` | Distinguish a constructed Result from `default`. |
| [`Result<T,E>.IsSuccess`](#result) | `bool` | Test for the success branch without reading it. |
| [`Result<T,E>.IsFailure`](#result) | `bool` | Test for the failure branch without reading it. |
| [`Result<T,E>.Value`](#result) | `T` | Read success or throw for failure/uninitialized state. |
| [`Result<T,E>.Error`](#result) | `E` | Read failure or throw for success/uninitialized state. |
| [`Result<T,E>.TryGetValue`](#result) | `out T` | Read success at an imperative boundary. |
| [`Result<T,E>.TryGetError`](#result) | `out E` | Read failure at an imperative boundary. |
| [`Result<T,E>.Map`](#railway-composition) | same-type delegate; type-changing delegate; caller-state; struct callable; generated token | Transform success without changing failure. |
| [`Result<T,E>.Bind`](#railway-composition) | same/type-changing delegate; caller-state; struct callable; generated token; convertible error | Continue success with another Result. |
| [`Result<T,E>.MapError`](#error-composition) | delegate; caller-state | Transform failure without changing success. |
| [`Result<T,E>.BindError`](#error-composition) | delegate; caller-state; struct callable | Continue failure and optionally change its type. |
| [`Result<T,E>.BiMap`](#error-composition) | success and failure delegates | Transform both branch types in one operation. |
| [`Result<T,E>.Recover`](#railway-composition) | Result-returning failure delegate | Recover while retaining the same error type. |
| [`Result<T,E>.Ensure`](#railway-composition) | predicate/error factory; caller-state | Turn success into failure when a guard is false. |
| [`Result<T,E>.Tap`](#side-effects) | action; caller-state; struct action; generated token | Observe success and return the original Result. |
| [`Result<T,E>.TapAsync`](#side-effects) | `Func<T,ValueTask>` | Await a success observation and return the original Result. |
| [`Result<T,E>.TapError`](#side-effects) | action; caller-state; struct action; generated token | Observe failure and return the original Result. |
| [`Result<T,E>.Finally`](#side-effects) | caller-state action | Run one action for either initialized branch. |
| [`Result<T,E>.FinallyAsync`](#side-effects) | caller-state `ValueTask` callback | Await one action for either initialized branch. |
| [`Result<T,E>.Match`](#consuming-a-result) | delegates; caller-state; two struct callables | Exhaustively reduce both branches to one type. |
| [`Result<T,E>.Switch`](#consuming-a-result) | two actions | Execute exactly one terminal branch action. |
| [`Result<T,E>.ValueOr`](#consuming-a-result) | eager fallback | Discard failure and return a fallback value. |
| [`Result<T,E>.ValueOrElse`](#consuming-a-result) | lazy failure callback | Lazily derive a fallback from the error. |
| [`Result<T,E>.ToString`](#result) | diagnostic representation | Format `Ok(...)`, `Fail(...)`, or `Uninitialized`. |
| [`Result<T,E>` implicit conversions](#result) | from `T`; from `E` | Construct a typed Result where target context is unambiguous. |
| [`Result<T,E>.Flatten`](#railway-composition) | nested Result | Remove one same-error Result layer. |
| [`Result<Option<T>,E>.Transpose`](#option-and-result) | Result to Option | Convert to `Option<Result<T,E>>`. |
| [`Result<Option<T>,E>.RequireSome`](#option-and-result) | lazy error factory | Require a present success value. |
| [`Option<Result<T,E>>.Transpose`](#option-and-result) | Option to Result | Convert to `Result<Option<T>,E>`. |
| [`ResultCombination.Combine`](#combining-independent-results) | two Results; `ReadOnlySpan<Result<Unit,E>>` | Fail-fast combination with `Unit` success. |
| [`ResultCombination.Zip`](#combining-independent-results) | two heterogeneous Results | Combine successes into a named tuple. |
| [`ResultCombination.Map`](#combining-independent-results) | two Results plus projection | Combine and project without an intermediate tuple API. |
| [`Unit.Value`](#result) | singleton value | Represent a successful operation with no payload. |
| [`Unit.ToString`](#result) | `()` | Produce the conventional unit representation. |

### Core Option And Callable API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Option<T>.Some`](#option) | `Some(T)` | Construct guaranteed non-null presence. |
| [`Option<T>.None`](#option) | static value | Construct absence; equivalent to `default`. |
| [`Option<T>.HasValue`](#option) | `bool` | Test presence. |
| [`Option<T>.IsSome`](#option) | `bool` | Functional alias for presence. |
| [`Option<T>.IsNone`](#option) | `bool` | Test absence. |
| [`Option<T>.Value`](#option) | `T` | Read presence or throw for `None`. |
| [`Option<T>.TryGetValue`](#option) | `out T` | Read presence at an imperative boundary. |
| [`Option<T>.Map`](#option) | delegate; caller-state; struct callable; generated token | Transform a present value. |
| [`Option<T>.Bind`](#option) | delegate; caller-state; struct callable; generated token | Continue presence without nesting Options. |
| [`Option<T>.Filter`](#option) | predicate; caller-state | Retain presence only when a predicate is true. |
| [`Option<T>.Match`](#option) | some/none delegates | Exhaustively reduce presence and absence. |
| [`Option<T>.Switch`](#option) | some/none actions | Execute exactly one terminal action. |
| [`Option<T>.ValueOr`](#option) | eager fallback | Reduce absence to a fallback. |
| [`Option<T>.ValueOrElse`](#option) | lazy fallback; caller-state | Lazily create a fallback without a closure. |
| [`Option<T>` implicit conversion](#option) | nullable `T` | Convert null to `None` and non-null to `Some`. |
| [`Option<T>.ToResult`](#option-and-result) | eager error; lazy error | Require presence and attach a typed failure. |
| [`IValueFunction<TIn,TOut>.Invoke`](#hot-paths) | value-returning callable | Define generic, devirtualizable callback dispatch. |
| [`IValueAction<T>.Invoke`](#hot-paths) | side-effect callable | Define generic, devirtualizable action dispatch. |
| [`ValueFunction<TIn,TOut,TFunction>.ValueFunction`](#hot-paths) | callable constructor | Wrap a stateful or default struct callable. |
| [`ValueFunction<TIn,TOut,TFunction>.Invoke`](#hot-paths) | forwarding call | Invoke the wrapped callable. |
| [`ValueAction<T,TAction>.Invoke`](#hot-paths) | forwarding call | Invoke the wrapped action. |

### Async And Effect API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`MapAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Map success with a `ValueTask<T>` callback. |
| [`MapTaskAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Map success with a `Task<T>` callback. |
| [`BindAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind success with a `ValueTask<Result<T,E>>` callback. |
| [`BindTaskAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind success with a `Task<Result<T,E>>` callback. |
| [`BindErrorAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind failure with a `ValueTask<Result<T,E>>` callback. |
| [`BindErrorTaskAsync`](#async-pipelines) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind failure with a `Task<Result<T,E>>` callback. |
| [`Map` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous success mapping. |
| [`Bind` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous success binding. |
| [`BindError` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous failure binding. |
| [`Effect.Try`](#exception-boundaries) | broad exception; selected `TException` | Convert a synchronous thrown exception to failure. |
| [`Effect.TryAsync`](#exception-boundaries) | broad exception; selected `TException` | Convert a ValueTask operation's exception to failure. |
| [`Effect.TryTaskAsync`](#exception-boundaries) | broad/typed exception; with/without caller state | Convert a Task operation's exception to failure. |
| [`ExceptionFilter.IsRecoverable`](#selected-exceptions) | `Exception` | Apply the library's broad exception-capture policy. |
| [`Result<T,E>.TryMap`](#exception-boundaries) | broad exception; selected `TException` | Map success through throwing synchronous code. |
| [`Result<T,E>.TryBind`](#exception-boundaries) | broad exception; selected `TException` | Bind success through throwing synchronous code. |
| [`Result<T,E>.TryTap`](#exception-boundaries) | broad exception; selected `TException` | Observe success and convert a thrown exception. |
| [`Result<T,E>.TryMapAsync`](#exception-boundaries) | broad exception; selected `TException` | Map success through throwing ValueTask code. |
| [`Result<T,E>.TryTapAsync`](#exception-boundaries) | broad exception; selected `TException` | Observe success asynchronously and convert a thrown exception. |

### Error And Validation API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Error.Error`](#structured-errors) | category constructor; code/message constructor | Construct a structured built-in error. |
| [`Error.Type`](#structured-errors) | `ErrorType` | Read the bounded built-in category. |
| [`Error.NumericType`](#structured-errors) | `int` | Read built-in or custom numeric category. |
| [`Error.Code`](#structured-errors) | `string` | Read stable machine identity. |
| [`Error.Message`](#structured-errors) | `string` | Read diagnostic/display text. |
| [`Error.IsMessagePublic`](#structured-errors) | `bool` | Control default transport disclosure. |
| [`Error.Cause`](#structured-errors) | `Exception?` | Read a retained exception with its original stack. |
| [`Error.ThrowCause`](#structured-errors) | method | Rethrow the retained exception without resetting its stack. |
| [`Error.Failure`](#structured-errors) | message; code/message/cause/visibility | Construct a general expected failure. |
| [`Error.Unexpected`](#structured-errors) | message; exception/code | Construct a private unexpected failure. |
| [`Error.Validation`](#structured-errors) | message; code/message/cause | Construct invalid-input failure. |
| [`Error.Conflict`](#structured-errors) | code/message/cause/visibility | Construct state-conflict failure. |
| [`Error.NotFound`](#structured-errors) | code/message/cause/visibility | Construct missing-resource failure. |
| [`Error.Unauthorized`](#structured-errors) | code/message/cause/visibility | Construct authentication failure. |
| [`Error.Forbidden`](#structured-errors) | code/message/cause/visibility | Construct authorization failure. |
| [`Error.Unavailable`](#structured-errors) | code/message/cause/visibility | Construct temporary-service failure. |
| [`Error.Timeout`](#structured-errors) | code/message/cause/visibility | Construct timeout failure. |
| [`Error.RateLimited`](#structured-errors) | code/message/cause/visibility | Construct quota/rate failure. |
| [`Error.Cancelled`](#structured-errors) | code/message/cause | Construct cancellation failure. |
| [`Error.Custom`](#structured-errors) | numeric type/code/message/cause/visibility | Construct an application-defined category. |
| [`Error.IO`](#structured-errors) | message | Construct the general I/O convenience error. |
| [`Error.System`](#structured-errors) | message | Construct the general system convenience error. |
| [`Error.ToString`](#structured-errors) | default; format/provider | Allocate `[CODE] message` text. |
| [`Error.TryFormat`](#structured-errors) | destination span | Format without allocating a string. |
| [`IErrorConvertible<TError>.ToError`](#error-composition) | conversion method | Convert compact domain error only at a boundary/failure. |
| [`Result<T,E>.BindWidened`](#error-composition) | convertible continuation error | Keep compact inner errors until the failing branch is used. |
| [`ErrorType`](#structured-errors) | all 13 enum values | Categorize transport and observability policy. |
| [`ValidationIssue.ValidationIssue`](#validation-compatibility) | path/code/message/severity | Construct one immutable validation issue. |
| [`ValidationIssue.Path`](#validation-compatibility) | `string` | Read the affected member path. |
| [`ValidationIssue.Code`](#validation-compatibility) | `string` | Read stable issue identity. |
| [`ValidationIssue.Message`](#validation-compatibility) | `string` | Read display text. |
| [`ValidationIssue.Severity`](#validation-compatibility) | enum | Read error, warning, or information severity. |
| [`ValidationErrors.ValidationErrors`](#validation-compatibility) | sequence; params array | Copy issues into immutable storage. |
| [`ValidationErrors.Create`](#validation-compatibility) | delegate; caller-state; struct mapper | Map third-party failures without a runtime adapter dependency. |
| [`ValidationErrors.Count`](#validation-compatibility) | `int` | Read issue count. |
| [`ValidationErrors.this[int]`](#validation-compatibility) | indexer | Read one issue. |
| [`ValidationErrors.AsSpan`](#validation-compatibility) | `ReadOnlySpan<ValidationIssue>` | Iterate without interface/enumerator allocation. |
| [`ValidationErrors.GetEnumerator`](#validation-compatibility) | generic enumerator | Support standard collection iteration. |
| [`ValidationSeverity`](#validation-compatibility) | `Error`, `Warning`, `Information` | Classify issue severity. |

### Diagnostics, HTTP, And Generation API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`ErrorTelemetry.Record`](#diagnostics) | activity/error/policy | Project an error into an existing BCL Activity. |
| [`ErrorActivityStatusPolicy`](#diagnostics) | `Automatic`, `Preserve`, `MarkError` | Select Activity status mutation policy. |
| [`ErrorMetrics.ErrorMetrics`](#diagnostics) | meter/code-dimension/counter-name | Create an optional caller-owned counter. |
| [`ErrorMetrics.Disabled`](#diagnostics) | static value | Select the cheapest disabled metrics path. |
| [`ErrorMetrics.IsEnabled`](#diagnostics) | `bool` | Check for a listener before expensive caller work. |
| [`ErrorMetrics.Record`](#diagnostics) | `Error?` | Increment the error counter with bounded tags. |
| [`Result<T,E>.ToHttpResult`](#aspnet-core) | Error; ValidationErrors; convertible error; custom delegates; struct mappers | Convert to strongly typed Minimal API results. |
| [`IHttpResultMapper<TError,TResult>.Map`](#aspnet-core) | error/context | Define an allocation-free caller-owned HTTP failure mapper. |
| [`DefaultErrorHttpResultMapper.Map`](#aspnet-core) | error/context | Apply the default Error problem policy. |
| [`ErrorProblemDetails.Create`](#aspnet-core) | error/context | Create RFC ProblemDetails without executing it. |
| [`ErrorProblemDetails.ToHttpResult`](#aspnet-core) | error/context | Create an executable ProblemHttpResult. |
| [`ErrorProblemDetails.GetStatusCode`](#aspnet-core) | `ErrorType` | Read default category-to-status mapping. |
| [`ValidationErrorProblemDetails.ToHttpResult`](#aspnet-core) | errors/context | Create typed validation problem output. |
| [`ProducesErrors`](#aspnet-core) | `ReadOnlySpan<ErrorType>` | Add Minimal API response metadata without reflection. |
| [`ProducesErrorAttribute.ProducesErrorAttribute`](#aspnet-core) | `ErrorType` | Add one controller response category. |
| [`ProducesErrorAttribute.ErrorType`](#aspnet-core) | enum | Read configured category. |
| [`ProducesErrorAttribute.Type`](#aspnet-core) | ProblemDetails type | Expose OpenAPI response body metadata. |
| [`ProducesErrorAttribute.StatusCode`](#aspnet-core) | `int` | Expose mapped status metadata. |
| [`ProducesErrorAttribute.Description`](#aspnet-core) | `null` | Leave description to the OpenAPI pipeline. |
| [`ProducesErrorAttribute.ContentTypes`](#aspnet-core) | problem media types | Expose supported response content types. |
| [`GenerateValueFunctionAttribute.GenerateValueFunctionAttribute`](#generated-callables) | default; generated-name constructor | Request a wrapper while retaining the original method. |
| [`GenerateValueFunctionAttribute.Name`](#generated-callables) | optional name | Read the requested generated property name. |
| [`Functions.<Method>` generated property](#generated-callables) | `ValueFunction` or `ValueAction` | Pass an inferred zero-state token to sync or async operators. |

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

All six async operator families also accept generated `ValueFunction` tokens.
There is no separate async-generation attribute: `[GenerateValueFunction]`
adapts the method's actual return type. A method returning `ValueTask<T>` feeds
`MapAsync`; `Task<T>` feeds `MapTaskAsync`; `ValueTask<Result<T,E>>` feeds
`BindAsync` or `BindErrorAsync`; and the corresponding Task type feeds the
Task-named operator.

```csharp
using MonadicTypes;
using MonadicTypes.Async;

public static partial class InventoryOperations
{
    [GenerateValueFunction]
    public static ValueTask<Result<Reservation, CheckoutError>> ReserveAsync(
        Order order) => inventory.ReserveAsync(order);

    [GenerateValueFunction]
    public static Task<Receipt> CreateReceiptAsync(
        Reservation reservation) => receipts.CreateAsync(reservation);
}

Result<Receipt, CheckoutError> receipt = await parsedOrder
    .BindAsync(InventoryOperations.Functions.ReserveAsync)
    .MapTaskAsync(InventoryOperations.Functions.CreateReceiptAsync);

// Annotation does not replace or hide the original method.
ValueTask<Result<Reservation, CheckoutError>> pending =
    InventoryOperations.ReserveAsync(order);
```

Generated wrappers are supported on `Result<T,E>`,
`ValueTask<Result<T,E>>`, and `Task<Result<T,E>>`, so a token can be used before
or after another asynchronous stage. Failure operators use the same rule:

```csharp
public static partial class RecoveryOperations
{
    [GenerateValueFunction]
    public static ValueTask<Result<Order, FinalError>> RecoverAsync(
        CheckoutError error) => retry.RecoverAsync(error);
}

Result<Order, FinalError> recovered = await failedOrder
    .BindErrorAsync(RecoveryOperations.Functions.RecoverAsync);
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
| `Map(Operations.Functions.Method)` | A benchmark proves delegate dispatch matters | Generated generic callable can inline without handwritten wrapper code |
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
    // None means the call succeeded but the payment was declined.
    Task<Option<Payment>> ChargeAsync(Reservation reservation);
}

// Keep null at a third-party boundary instead of leaking it into application code.
public sealed class LegacyPaymentClient(ILegacyPaymentSdk sdk) : IPaymentClient
{
    public async Task<Option<Payment>> ChargeAsync(Reservation reservation) =>
        (Option<Payment>)await sdk.ChargeAsync(reservation);
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
        Result<Option<Payment>, CheckoutError> charged = await Effect.TryTaskAsync(
            (Client: payment, Reservation: reservation),
            static state => state.Client.ChargeAsync(state.Reservation),
            static (PaymentUnavailableException exception) => new CheckoutError(
                CheckoutErrorCode.PaymentUnavailable,
                exception));

        return charged.RequireSome(static () => new CheckoutError(
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

Only the two known dependency exceptions are converted. The payment contract
uses `Option<Payment>` rather than `Payment?`: `Result` says whether the remote
operation completed as expected, while `Option` says whether that successful
operation produced a payment. `RequireSome` gives expected absence the
`PaymentDeclined` domain meaning. A legacy nullable SDK is normalized once in
its adapter, so null checks cannot spread through the application pipeline. A
different thrown exception propagates to the global handler shown above, where
it is logged and returned as a private 500 problem. Retained causes on expected
dependency errors remain available to tracing without changing their HTTP
classification. The dependency calls pass client and request data as explicit
tuple state; their static callbacks do not allocate closures.

Absence does not always become an error. An optional loyalty lookup can remain
`Option` inside the dependency `Result`; only dependency failure stops the
railway, while no account simply means no discount:

```csharp
Result<Option<LoyaltyAccount>, CheckoutError> loyalty =
    await loyaltyClient.FindAsync(customerId);

Result<Option<decimal>, CheckoutError> discountRate = loyalty.Map(
    static account => account
        .Filter(static value => value.IsActive)
        .Map(static value => value.DiscountRate));

Result<decimal, CheckoutError> total = discountRate.Map(
    subtotal,
    static (rate, amount) => amount * (1m - rate.ValueOr(0m)));
```

This distinguishes three states without nullable branching: dependency failure,
successful lookup with no account, and successful lookup with an account. The
caller-state `Result.Map` overload passes `subtotal` without a closure, and
`Option.ValueOr` deliberately reduces absence only at the calculation that owns
the fallback policy.

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

### Generated Callables

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

Do not annotate every method that happens to consume or return a Result. Use
ordinary static method groups and delegates first: they are easier to debug,
avoid extra generic NativeAOT instantiations, and already allocate 0 B per call
when cached by the runtime. Add `[GenerateValueFunction]` when a representative
benchmark shows callback dispatch is material, or when a library intentionally
offers a pre-measured hot-path token. The generated member is additive: callers
that do not use `Functions.<Method>` keep calling the original method and do not
instantiate the wrapper path.

The same recommendation applies to async methods. Generation can remove
delegate dispatch around a completed ValueTask or Task, but it cannot remove an
allocation made by the underlying operation or the state machine required by a
genuinely pending operation. Compare the generated token with the static
delegate under the application's actual completion pattern before accepting
the additional native code size.

The current full NativeAOT composition run measured a generated completed
`MapAsync` at 6.3249 ns versus 11.7515 ns for the same-run static delegate, both
at 0 B. That result supports generated wrappers for repeatedly executed,
completed-async hot paths. It does not justify annotating unmeasured I/O-bound
methods, where dependency latency dominates and every distinct wrapper can add
a generic instantiation to the native binary.

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
