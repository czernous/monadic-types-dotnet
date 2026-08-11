# API Behavior Reference

This is the hand-maintained behavioral contract for the public API. An overload
family is documented once when its branch semantics are identical. Delegate,
caller-state, struct-callable, and generated-callable overloads differ only in
dispatch and state transport unless stated otherwise. Result payload access,
branch consumers, and composition reject `default(Result<T,E>)` with
`InvalidOperationException`; state queries and `ToString` can inspect it.

Compiler names are presented using C# syntax: `op_Implicit` appears as implicit
conversion and `Item` appears as the indexer. Static extension-container types
are not separate operations; every callable extension they expose is indexed.
The build emits XML documentation and treats `CS1591` as an error in every
shipped runtime and generator project, so this guide can be audited against the
compiled public-member inventory.

## Result Construction And State

### Result Ok

`Result<T,E>.Ok` constructs success without creating an error.

```csharp
Result<User, LookupError> result = Result<User, LookupError>.Ok(user);
```

### Result Fail

`Result<T,E>.Fail` constructs failure and rejects a null error.

```csharp
Result<User, LookupError> result = Result<User, LookupError>.Fail(LookupError.NotFound);
```

### Unit Result Ok

`Result.Ok<E>()` constructs `Result<Unit,E>` success for an operation with no
success payload.

```csharp
Result<Unit, SaveError> saved = Result.Ok<SaveError>();
```

### Unit Result Fail

`Result.Fail<E>(error)` constructs a failed `Result<Unit,E>`.

```csharp
Result<Unit, SaveError> saved = Result.Fail(SaveError.WriteFailed);
```

### Result State

`IsInitialized`, `IsSuccess`, and `IsFailure` inspect state without reading a
payload. A default Result is uninitialized, neither success nor failure.

```csharp
if (result.IsFailure)
{
    Record(result.Error);
}
```

### Result Value

`Value` returns success and throws for failure or uninitialized state.

```csharp
User user = result.Value;
```

### Result Error

`Error` returns failure and throws for success or uninitialized state.

```csharp
LookupError error = result.Error;
```

### Result Try Get

`TryGetValue` and `TryGetError` return `true` only for the matching branch. Both
reject an uninitialized Result.

```csharp
if (result.TryGetValue(out User user))
{
    Render(user);
}
```

### Result String

`ToString` returns `Ok(value)`, `Fail(error)`, or `Uninitialized`. It is a
diagnostic representation, not a transport format.

```csharp
logger.LogDebug("Lookup result: {Result}", result.ToString());
```

### Result Conversions

Implicit conversion from `T` creates success; conversion from `E` creates
failure. Use it only when the target type makes the branch unambiguous.

```csharp
Result<User, LookupError> success = user;
Result<User, LookupError> failure = LookupError.NotFound;
```

### Unit

`Unit.Value` represents successful completion without data. `ToString` returns
`()`. The value itself requires no heap allocation.

```csharp
Unit completed = Unit.Value;
Console.WriteLine(completed);
```

## Result Railway Operators

### Result Map

`Map` calls its mapper only for success and wraps its output. Failure propagates
unchanged. Caller-state and callable overloads change dispatch, not semantics.

```csharp
Result<int, LookupError> id = result.Map(static user => user.Id);
```

### Result Bind

`Bind` calls a Result-returning continuation only for success and returns it
without nesting. Outer failure skips the callback.

```csharp
Result<Account, LookupError> account = result.Bind(LoadAccount);
```

### Result Ensure

`Ensure` preserves failure. For success it keeps the value when the predicate is
true; otherwise it calls the error factory.

```csharp
Result<Order, CheckoutError> valid = order.Ensure(
    static value => value.Lines.Count > 0,
    static _ => CheckoutError.EmptyOrder);
```

### Result Recover

`Recover` preserves success and invokes recovery only for failure.

```csharp
Result<Settings, ReadError> settings = primary.Recover(ReadFallback);
```

### Result Flatten

`Flatten` removes one layer from `Result<Result<T,E>,E>`. Outer failure wins;
outer success returns the inner Result unchanged.

```csharp
Result<User, LookupError> flat = nestedResult.Flatten();
```

## Result Error Operators

### Result Map Error

`MapError` preserves success under the new error type and maps only failure.

```csharp
Result<User, ApiError> apiResult = result.MapError(ApiError.FromLookup);
```

### Result Bind Error

`BindError` preserves success and invokes a Result-returning continuation only
for failure.

```csharp
Result<User, FinalError> recovered = result.BindError(RetryLookup);
```

### Result BiMap

`BiMap` changes both branch types while invoking exactly one mapper.

```csharp
Result<UserDto, ProblemCode> projected = result.BiMap(UserDto.From, ProblemCode.From);
```

### Result Bind Widened

`BindWidened` converts a compact continuation error only when that continuation
fails. Existing wide failure propagates without invoking the continuation.

```csharp
Result<Receipt, Error> charged = loadedOrder.BindWidened(ChargeWithPaymentError);
```

## Result Observation And Consumption

### Result Tap

`Tap` runs only for success and returns the original Result. Callback exceptions
propagate normally.

```csharp
Result<Order, CheckoutError> observed = order.Tap(AuditOrder);
```

### Result Tap Async

`TapAsync` awaits a ValueTask action only for success and returns the original
Result.

```csharp
Result<Order, CheckoutError> observed = await order.TapAsync(AuditOrderAsync);
```

### Result Tap Error

`TapError` runs only for failure and returns the original Result.

```csharp
Result<Order, CheckoutError> observed = order.TapError(RecordFailure);
```

### Result Finally

`Finally` runs one caller-state action for either initialized branch, then
returns the original Result. It does not receive the active payload.

```csharp
Result<Order, CheckoutError> completed = order.Finally(timer, static value => value.Stop());
```

### Result Finally Async

`FinallyAsync` awaits one caller-state action for either initialized branch and
returns the original Result.

```csharp
Result<Order, CheckoutError> completed = await order.FinallyAsync(
    scope,
    static value => value.DisposeAsync());
```

### Result Match

`Match` invokes exactly one branch callback and returns its common output type.

```csharp
IResult response = result.Match<IResult>(
    static user => TypedResults.Ok(user),
    static error => TypedResults.NotFound(error.Code));
```

### Result Switch

`Switch` invokes exactly one branch action and returns `void`. It is terminal,
not composable. Uninitialized Result throws before either action.

```csharp
result.Switch(RenderUser, RenderLookupError);
```

### Result Value Or

`ValueOr` returns success or an eagerly supplied fallback.

```csharp
User user = result.ValueOr(User.Anonymous);
```

### Result Value Or Else

`ValueOrElse` returns success without invoking its callback; failure lazily maps
the error to a fallback.

```csharp
User user = result.ValueOrElse(static error => User.Missing(error.Code));
```

## Result Combination

### Combine

`Combine` inspects two or a span of `Result<Unit,E>` values in order and returns
the first failure or unit success.

```csharp
ReadOnlySpan<Result<Unit, ValidationError>> checks = [CheckName(), CheckEmail()];
Result<Unit, ValidationError> valid = ResultCombination.Combine(checks);
```

### Zip

`Zip` returns the first failure in argument order or a named tuple of both
successes. Producer calls supplied as arguments have already executed.

```csharp
Result<(User User, Account Account), LoadError> loaded =
    ResultCombination.Zip(userResult, accountResult);
```

### Combination Map

Two-result `Map` returns the first failure or invokes its projection once with
both success values.

```csharp
Result<Invoice, LoadError> invoice = ResultCombination.Map(
    userResult,
    accountResult,
    static (user, account) => new Invoice(user, account));
```

## Option

### Option Some

`Some` constructs presence and rejects null.

```csharp
Option<User> option = Option<User>.Some(user);
```

### Option None

`None` represents absence and is equivalent to `default(Option<T>)`.

```csharp
Option<User> option = Option<User>.None;
```

### Option State

`HasValue`, `IsSome`, and `IsNone` query presence without reading the payload.

```csharp
if (option.IsNone)
{
    RenderMissingUser();
}
```

### Option Value

`Value` returns the present value and throws for None.

```csharp
User user = option.Value;
```

### Option Try Get

`TryGetValue` assigns and returns `true` for Some; it returns `false` for None.

```csharp
if (option.TryGetValue(out User user))
{
    Render(user);
}
```

### Option Map

`Map` invokes its mapper only for Some and requires a non-null output. None
propagates without invoking the mapper.

```csharp
Option<int> id = option.Map(static user => user.Id);
```

### Option Bind

`Bind` invokes an Option-returning continuation only for Some and avoids a
nested Option. None propagates.

```csharp
Option<Address> address = option.Bind(static user => user.PrimaryAddress);
```

### Option Filter

`Filter` keeps Some when its predicate returns true; otherwise it returns None.
The predicate is not called for None.

```csharp
Option<User> active = option.Filter(static user => user.IsActive);
```

### Option Match

`Match` invokes exactly one Some/None callback and returns a common type.

```csharp
string name = option.Match(static user => user.Name, static () => "Unknown");
```

### Option Switch

`Switch` invokes exactly one Some/None action and returns `void`.

```csharp
option.Switch(RenderUser, RenderMissingUser);
```

### Option Value Or

`ValueOr` returns Some or an eagerly supplied fallback.

```csharp
User user = option.ValueOr(User.Anonymous);
```

### Option Value Or Else

`ValueOrElse` skips its factory for Some and invokes it only for None. The
caller-state overload avoids a closure.

```csharp
User user = option.ValueOrElse(CreateAnonymousUser);
```

### Option Conversion

Implicit conversion maps null to None and a non-null value to Some.

```csharp
Option<User> option = nullableUser;
```

### Option To Result

`ToResult` converts Some to success. None becomes failure from an eager error or
a lazy factory that runs only for None.

```csharp
Result<User, LookupError> required = option.ToResult(LookupError.NotFound);
```

## Result And Option Transposition

### Result Option Transpose

`Result<Option<T>,E>.Transpose` maps `Ok(Some(x))` to `Some(Ok(x))`, `Ok(None)`
to None, and `Fail(e)` to `Some(Fail(e))`.

```csharp
Option<Result<User, LookupError>> transposed = optionalResult.Transpose();
```

### Option Result Transpose

`Option<Result<T,E>>.Transpose` maps `Some(Ok(x))` to `Ok(Some(x))`,
`Some(Fail(e))` to failure, and None to `Ok(None)`.

```csharp
Result<Option<User>, LookupError> transposed = optionalOperation.Transpose();
```

### Require Some

`RequireSome` maps `Ok(Some(x))` to `Ok(x)`. For `Ok(None)` it invokes the error
factory once. Outer failure propagates and skips the factory.

```csharp
Result<User, LookupError> required = lookup.RequireSome(
    static () => LookupError.NotFound);
```

## Callable Abstractions

### IValueFunction

`IValueFunction<TIn,TOut>.Invoke` defines generic struct callback dispatch that
NativeAOT can devirtualize.

```csharp
public readonly struct GetId : IValueFunction<User, int>
{
    public int Invoke(User user) => user.Id;
}
```

### IValueAction

`IValueAction<T>.Invoke` defines a generic struct side-effect callback.

```csharp
public readonly struct ObserveError : IValueAction<Error>
{
    public void Invoke(Error error) => ErrorTelemetry.Record(Activity.Current, error);
}
```

### ValueFunction

`ValueFunction<TIn,TOut,TFunction>` stores a callable struct when state is
required and forwards `Invoke`.

```csharp
var callable = new ValueFunction<User, int, StatefulGetId>(new StatefulGetId(offset));
Result<int, LookupError> id = result.Map(callable);
```

### ValueAction

`ValueAction<T,TAction>` forwards to a default stateless action struct. Generated
action properties expose this token without handwritten wrapper code.

```csharp
Result<User, Error> observed = result.Tap(Operations.Functions.ObserveUser);
```

## Async Result Operators

Every async family supports `Result<T,E>`, `ValueTask<Result<T,E>>`, and
`Task<Result<T,E>>` receivers. Pending receivers are awaited once with
`ConfigureAwait(false)`. Synchronous continuations on awaitable receivers use
`Map`, `Bind`, and `BindError` under the same names.

### Map Async

`MapAsync` awaits a `ValueTask<T>` mapper only for success and wraps its output.
Failure propagates. Result, ValueTask, and Task receiver overloads are available.

```csharp
Result<UserDto, LookupError> dto = await result.MapAsync(MapUserAsync);
```

### Map Task Async

`MapTaskAsync` provides the same behavior for a naturally Task-returning mapper.

```csharp
Result<UserDto, LookupError> dto = await result.MapTaskAsync(MapUserTaskAsync);
```

### Bind Async

`BindAsync` awaits a ValueTask Result continuation only for success. Failure
propagates without invoking it.

```csharp
Result<Account, LookupError> account = await result.BindAsync(LoadAccountAsync);
```

### Bind Task Async

`BindTaskAsync` provides the same bind behavior for a Task Result continuation.

```csharp
Result<Account, LookupError> account = await result.BindTaskAsync(LoadAccountTaskAsync);
```

### Bind Error Async

`BindErrorAsync` preserves success under the next error type and awaits a
ValueTask recovery continuation only for failure.

```csharp
Result<User, FinalError> recovered = await result.BindErrorAsync(RetryLookupAsync);
```

### Bind Error Task Async

`BindErrorTaskAsync` provides the same failure bind for a Task continuation.

```csharp
Result<User, FinalError> recovered = await result.BindErrorTaskAsync(RetryLookupTaskAsync);
```

### Awaitable Sync

`Map`, `Bind`, and `BindError` on Task or ValueTask receivers await the receiver,
then apply the matching synchronous core operator.

```csharp
Result<int, LookupError> id = await pendingResult.Map(static user => user.Id);
```

## Exception Effects

Broad overloads catch recoverable `Exception` values while allowing process-
critical exceptions and cancellation to propagate. Typed overloads catch only
the selected exception type. Exception mappers run only after a matching throw.

### Effect Try

`Effect.Try` executes synchronous code and returns success. Broad overloads map
recoverable exceptions; typed overloads map only the selected exception type.

```csharp
Result<Document, ImportError> imported = Effect.Try(
    parser.Parse,
    static exception => ImportError.From(exception));
```

### Effect Try Async

`Effect.TryAsync` applies the same boundary to a ValueTask operation.

```csharp
Result<Document, ImportError> imported = await Effect.TryAsync(
    parser.ParseAsync,
    static exception => ImportError.From(exception));
```

### Effect Try Task Async

`Effect.TryTaskAsync` applies the boundary directly to Task APIs. Caller-state
forms avoid a capturing lambda around an existing dependency or Task.

```csharp
Result<Response, ApiError> response = await Effect.TryTaskAsync(
    client,
    static value => value.SendAsync(request),
    static exception => ApiError.From(exception));
```

### Try Map

`TryMap` maps only success through throwing synchronous code. Existing failure
bypasses the call; a recoverable thrown exception is mapped to failure.

```csharp
Result<Dto, ImportError> mapped = imported.TryMap(MapDocument, ImportError.From);
```

### Try Bind

`TryBind` binds only success through a throwing Result-returning operation.

```csharp
Result<Record, ImportError> stored = imported.TryBind(StoreDocument, ImportError.From);
```

### Try Tap

`TryTap` observes success and returns the original Result unless the action
throws, in which case the exception becomes failure.

```csharp
Result<Document, ImportError> audited = imported.TryTap(AuditDocument, ImportError.From);
```

### Try Map Async

`TryMapAsync` awaits a throwing ValueTask mapper only for success.

```csharp
Result<Dto, ImportError> mapped = await imported.TryMapAsync(MapDocumentAsync, ImportError.From);
```

### Try Tap Async

`TryTapAsync` awaits a throwing ValueTask observation only for success.

```csharp
Result<Document, ImportError> audited = await imported.TryTapAsync(AuditAsync, ImportError.From);
```

## Structured Errors

### Error Construction

The Error constructors validate category, code, and message and create a deeply
immutable, reference-backed error.

```csharp
Error error = new(
    ErrorType.NotFound,
    "USER_NOT_FOUND",
    "The user does not exist.");
```

### Error Properties

`Type`, `NumericType`, `Code`, `Message`, `IsMessagePublic`, and `Cause` expose
immutable category, identity, disclosure, and retained exception data.

```csharp
logger.LogWarning(error.Cause, "{Code}: {Message}", error.Code, error.Message);
```

### Error Throw Cause

`ThrowCause` rethrows the retained exception through `ExceptionDispatchInfo`,
preserving its stack. It throws when no cause exists.

```csharp
if (error.Cause is not null)
{
    error.ThrowCause();
}
```

### Error Factories

Built-in factories construct Failure, Unexpected, Validation, Conflict,
NotFound, Unauthorized, Forbidden, Unavailable, Timeout, RateLimited,
Cancelled, IO, and System categories with consistent defaults.

```csharp
Error error = Error.NotFound("USER_NOT_FOUND", "The user does not exist.");
```

### Error Custom

`Error.Custom` constructs a positive application-defined numeric category.

```csharp
Error error = Error.Custom(10_001, "VENDOR_REJECTED", "The vendor rejected the request.");
```

### Error Format

`ToString` allocates `[CODE] message`. `TryFormat` writes the same representation
to caller-owned memory.

```csharp
Span<char> buffer = stackalloc char[128];
bool formatted = error.TryFormat(buffer, out int written, default, null);
```

### Error Convertible

`IErrorConvertible<TError>.ToError` defines explicit boundary conversion from a
compact domain error.

```csharp
public Error ToError() => Error.Failure("PAYMENT_FAILED", "Payment failed.");
```

### Error Type

`ErrorType` provides bounded transport and observability categories.
`Uninitialized` is invalid for a constructed Error.

```csharp
ErrorType category = ErrorType.Validation;
```

## Validation

### Validation Issue

`ValidationIssue` validates and stores immutable path, code, message, and
severity.

```csharp
var issue = new ValidationIssue("email", "EMAIL_INVALID", "Email is invalid.");
```

### Validation Errors Construction

ValidationErrors constructors copy an enumerable or params array into private
immutable storage.

```csharp
var errors = new ValidationErrors(issue);
```

### Validation Errors Create

`Create` maps an `IReadOnlyList<TFailure>` through a delegate, caller-state
callback, or struct mapper without a validator runtime dependency.

```csharp
ValidationErrors errors = ValidationErrors.Create(failures, MapFailure);
```

### Validation Errors Read

`Count`, the indexer, `AsSpan`, and `GetEnumerator` read immutable issues.
`AsSpan` is the allocation-free iteration path.

```csharp
foreach (ref readonly ValidationIssue issue in errors.AsSpan())
{
    Render(issue);
}
```

### Validation Severity

ValidationSeverity classifies Error, Warning, and Information independently of
third-party validation enums.

```csharp
ValidationSeverity severity = ValidationSeverity.Warning;
```

## Diagnostics

### Telemetry Record

`ErrorTelemetry.Record` does nothing for null or unsampled activities. Otherwise
it adds bounded tags, exception/event data, and policy-selected status.

```csharp
ErrorTelemetry.Record(Activity.Current, error);
```

### Activity Policy

`Automatic` marks server failures, `Preserve` does not change status, and
`MarkError` marks every recorded error.

```csharp
ErrorTelemetry.Record(Activity.Current, error, ErrorActivityStatusPolicy.Preserve);
```

### Metrics Construction

The ErrorMetrics constructor creates one counter on a caller-owned Meter. Error
code tags are opt-in because they can have high cardinality.

```csharp
var metrics = new ErrorMetrics(meter, includeErrorCode: false);
```

### Metrics Disabled

`ErrorMetrics.Disabled` creates no instrument and returns the zero-state
recorder.

```csharp
ErrorMetrics metrics = ErrorMetrics.Disabled;
```

### Metrics Enabled

`IsEnabled` reports whether the counter currently has a listener.

```csharp
if (metrics.IsEnabled)
{
    metrics.Record(error);
}
```

### Metrics Record

`Record` returns immediately when disabled; otherwise it increments the counter
with category and optional code tags.

```csharp
metrics.Record(error);
```

## ASP.NET Core

### To HTTP Result

`ToHttpResult` maps both branches to strongly typed `Results<,>`. Built-in
overloads cover Error, ValidationErrors, and convertible domain errors; generic
delegate and struct-mapper overloads are the escape hatch.

```csharp
Results<Ok<User>, ProblemHttpResult> response = result.ToHttpResult(TypedResults.Ok);
```

### HTTP Result Mapper

`IHttpResultMapper<TError,TResult>.Map` defines a value-type-capable custom
failure mapping contract with optional HttpContext.

```csharp
public readonly struct UserErrorMapper : IHttpResultMapper<UserError, ProblemHttpResult>
{
    public ProblemHttpResult Map(in UserError error, HttpContext? context) =>
        TypedResults.Problem(statusCode: error.StatusCode, title: error.Code);
}
```

### Default HTTP Mapper

`DefaultErrorHttpResultMapper.Map` applies the built-in Error-to-problem policy.

```csharp
ProblemHttpResult problem = default(DefaultErrorHttpResultMapper).Map(error, context);
```

### Problem Create

`ErrorProblemDetails.Create` builds ProblemDetails with status, type URI, stable
code, optional public detail, and trace ID. Cause is never serialized.

```csharp
ProblemDetails details = ErrorProblemDetails.Create(error, context);
```

### Problem Result

`ErrorProblemDetails.ToHttpResult` wraps the created details in a strongly typed
ProblemHttpResult.

```csharp
ProblemHttpResult result = ErrorProblemDetails.ToHttpResult(error, context);
```

### Problem Status

`GetStatusCode` maps an initialized ErrorType to the default HTTP status.

```csharp
int status = ErrorProblemDetails.GetStatusCode(ErrorType.NotFound);
```

### Validation Problem

`ValidationErrorProblemDetails.ToHttpResult` groups issues by path and emits
messages, machine codes, and an optional trace ID.

```csharp
ValidationProblem problem = ValidationErrorProblemDetails.ToHttpResult(errors, context);
```

### Produces Errors

`ProducesErrors` adds one response metadata item per category to a Minimal API
endpoint builder without reflection.

```csharp
app.MapGet("/users/{id:int}", GetUser)
    .ProducesErrors(ErrorType.NotFound, ErrorType.Unexpected);
```

### Produces Error Attribute

`ProducesErrorAttribute` supplies controller or endpoint response type, status,
content type, and category metadata through its public properties.

```csharp
[ProducesError(ErrorType.NotFound)]
public ActionResult<User> GetUser(int id) => Handle(id);
```

## Source Generation

### Generate Value Function

`GenerateValueFunctionAttribute` marks an implemented, non-generic static
one-parameter method in a top-level static partial class. The optional name
controls the generated property while the original method remains callable.

```csharp
public static partial class Operations
{
    [GenerateValueFunction("ParseFast")]
    public static int Parse(string text) => int.Parse(text);
}
```

### Generated Functions

`Functions.<Method>` exposes an inferred ValueFunction or ValueAction token for
sync and async operators.

```csharp
Result<int, Error> parsed = input.Map(Operations.Functions.ParseFast);
```

### Value Function Generator

`ValueFunctionGenerator.Initialize` is the Roslyn entry point that registers
attribute emission, MTGEN001-004 diagnostics, and adapter generation.
Application code does not call it directly.

```csharp
// Referencing MonadicTypes.Generators as an analyzer invokes the generator.
// No runtime registration or reflection is required.
```
