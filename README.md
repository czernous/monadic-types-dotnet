# MonadicTypes.NET

Reflection-free, NativeAOT-compatible `Result`, `Option`, structured error, and
application-boundary primitives for .NET 10.

The runtime libraries require no service locator, runtime code generation, or
mandatory dependency-injection integration. Delegate APIs prioritize normal C#
usage; caller-state overloads, callable structs, and generated wrappers provide
allocation-free alternatives for measured hot paths.

> [!IMPORTANT]
> This repository is experimental. APIs can change without compatibility
> shims before `1.0.0`.

## Project Intent

This library is built primarily for the maintainer's applications and projects.
Broader adoption is welcome but is not an objective, and the API will not expand
to maximize popularity or cover every functional-programming abstraction. The
intended endpoint is a small, dependable set of documented workflows that has
been exercised in real applications and meets the repository's correctness,
NativeAOT, allocation, performance, and compatibility gates.

## Packages

Install the smallest feature package that provides what the application needs.
NuGet restores the dependencies shown below transitively, so they should not be
installed again. Preview releases require `--prerelease` when the version is not
specified explicitly.

```bash
dotnet add package MonadicTypes.NET --prerelease
```

| NuGet package | Install it when | Included transitively | Guide |
| --- | --- | --- | --- |
| `MonadicTypes.NET` | You need `Result<T,E>`, `Option<T>`, `Unit`, composition, or callable values. This is the normal starting package. | None | [Result](#result), [Option](#option), [combination](#combining-independent-results), [hot paths](#hot-paths) |
| `MonadicTypes.NET.Errors` | You need the built-in immutable `Error`, validation issues, or domain-error widening. | `MonadicTypes.NET` | [Errors](#structured-errors), [validation](#validation-compatibility) |
| `MonadicTypes.NET.Async` | A pipeline must compose `Task` or `ValueTask` operations fluently. | `MonadicTypes.NET` | [Async pipelines](#async-pipelines) |
| `MonadicTypes.NET.Effects` | Code at a controlled boundary can throw and must become a typed failure. | `MonadicTypes.NET` | [Exception boundaries](#exception-boundaries) |
| `MonadicTypes.NET.Collections` | A count-known collection needs fail-fast traversal into one owned array. | `MonadicTypes.NET` | [Collections and LINQ](#collections-and-linq) |
| `MonadicTypes.NET.Linq` | Result or Option pipelines should use opt-in LINQ method or query syntax. | `MonadicTypes.NET` | [Collections and LINQ](#collections-and-linq) |
| `MonadicTypes.NET.Diagnostics` | Structured errors should be projected to optional `Activity` and `Meter` signals. | `MonadicTypes.NET.Errors`, then core | [Diagnostics](#diagnostics) |
| `MonadicTypes.NET.Testing` | Tests need framework-neutral Result and Option assertions. | `MonadicTypes.NET` | [Testing helpers](#testing-helpers) |
| `MonadicTypes.NET.Analyzers` | Build-time checks should catch precise Option and nested-railway mistakes. | None; analyzer only | [Analyzers](#analyzers) |
| `MonadicTypes.NET.AspNetCore` | An ASP.NET Core API needs typed HTTP results, RFC problem responses, validation conversion, or endpoint metadata. | `MonadicTypes.NET.Errors`, then core | [ASP.NET Core](#aspnet-core) |
| `MonadicTypes.NET.AspNetCore.OpenApi` | Explicit error catalogs should become status-scoped OpenAPI code enums and problem examples. | ASP.NET Core package, Microsoft OpenAPI runtime assemblies | [OpenAPI error catalogs](#openapi-error-catalogs) |
| `MonadicTypes.NET.Generators` | Annotated methods need compile-time struct-callable adapters for measured hot paths. | None; analyzer only | [Generated callables](#generated-callables) |

For example, a web API that needs HTTP conversion, asynchronous pipelines, and
exception boundaries installs these three top-level features:

```bash
dotnet add package MonadicTypes.NET.AspNetCore --prerelease
dotnet add package MonadicTypes.NET.Async --prerelease
dotnet add package MonadicTypes.NET.Effects --prerelease
```

There is intentionally no all-in-one package: optional features remain explicit
and do not add unused runtime or framework dependencies. Add diagnostics only
when using the provided projection helpers. Add the generator only for paths
where measurement justifies generated callables:

```bash
dotnet add package MonadicTypes.NET.Generators --prerelease
```

## Contents

- [Mental model](#mental-model)
- [Public API index](#api-guide)
- [Efficient use](#efficient-use)
- [Designing pipelines](#designing-pipelines)
- [Result](#result)
- [Option](#option)
- [Collections and LINQ](#collections-and-linq)
- [Async pipelines](#async-pipelines)
- [Exception boundaries](#exception-boundaries)
- [Structured errors](#structured-errors)
- [Validation compatibility](#validation-compatibility)
- [ASP.NET Core](#aspnet-core)
- [OpenAPI error catalogs](#openapi-error-catalogs)
- [Diagnostics](#diagnostics)
- [Complete application flow](#complete-application-flow)
- [Hot paths and generated callables](#hot-paths)
- [Performance contract](#performance-contract)
- [Status and licensing](#status-and-licensing)

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

Both values support positional patterns. Result exposes
`(isSuccess, value, error)` and Option exposes `(hasValue, value)`, allowing
property and relational patterns without delegate callbacks. Pattern paths are
allocation-free and beat `Match` in the isolated NativeAOT measurements.

If the individual operators are familiar but structuring an application around
them is not, start with [Designing Pipelines](#designing-pipelines) before using
the member-by-member reference.

## API Guide

Every authored public member is indexed below. Each member links to its exact
contract in the [API behavior reference](docs/api-reference.md), while the
chapters after the index explain composition, performance implications, and
larger examples. An overload family is one conceptual member when every overload
has identical branch semantics.

### Core Result API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Result<T,E>.Ok`](docs/api-reference.md#member-resultt-eok) | `Ok(T)` | Construct success explicitly. |
| [`Result<T,E>.Fail`](docs/api-reference.md#member-resultt-efail) | `Fail(E)` | Construct failure explicitly. |
| [`Result.Ok`](docs/api-reference.md#member-resultok) | `Ok<E>()` | Construct `Result<Unit,E>` success. |
| [`Result.Fail`](docs/api-reference.md#member-resultfail) | `Fail<E>(E)` | Construct `Result<Unit,E>` failure. |
| [`Result<T,E>.IsInitialized`](docs/api-reference.md#member-resultt-eisinitialized) | `bool` | Distinguish a constructed Result from `default`. |
| [`Result<T,E>.IsSuccess`](docs/api-reference.md#member-resultt-eissuccess) | `bool` | Test for the success branch without reading it. |
| [`Result<T,E>.IsFailure`](docs/api-reference.md#member-resultt-eisfailure) | `bool` | Test for the failure branch without reading it. |
| [`Result<T,E>.Value`](docs/api-reference.md#member-resultt-evalue) | `T` | Read success or throw for failure/uninitialized state. |
| [`Result<T,E>.Error`](docs/api-reference.md#member-resultt-eerror) | `E` | Read failure or throw for success/uninitialized state. |
| [`Result<T,E>.TryGetValue`](docs/api-reference.md#member-resultt-etrygetvalue) | `out T` | Read success at an imperative boundary. |
| [`Result<T,E>.TryGetError`](docs/api-reference.md#member-resultt-etrygeterror) | `out E` | Read failure at an imperative boundary. |
| [`Result<T,E>.Deconstruct`](docs/api-reference.md#member-resultt-edeconstruct) | `out bool`, `out T`, `out E` | Expose the active case to positional patterns. |
| [`Result<T,E>.Map`](docs/api-reference.md#member-resultt-emap) | same-type delegate; type-changing delegate; caller-state; struct callable; generated token | Transform success without changing failure. |
| [`Result<T,E>.Bind`](docs/api-reference.md#member-resultt-ebind) | same/type-changing delegate; caller-state; struct callable; generated token; convertible error | Continue success with another Result. |
| [`Result<T,E>.MapError`](docs/api-reference.md#member-resultt-emaperror) | delegate; caller-state | Transform failure without changing success. |
| [`Result<T,E>.BindError`](docs/api-reference.md#member-resultt-ebinderror) | delegate; caller-state; struct callable | Continue failure and optionally change its type. |
| [`Result<T,E>.BiMap`](docs/api-reference.md#member-resultt-ebimap) | success and failure delegates | Transform both branch types in one operation. |
| [`Result<T,E>.Recover`](docs/api-reference.md#member-resultt-erecover) | Result-returning failure delegate | Recover while retaining the same error type. |
| [`Result<T,E>.Ensure`](docs/api-reference.md#member-resultt-eensure) | predicate/error factory; caller-state | Turn success into failure when a guard is false. |
| [`Result<T,E>.Tap`](docs/api-reference.md#member-resultt-etap) | action; caller-state; struct action; generated token | Observe success and return the original Result. |
| [`Result<T,E>.TapAsync`](docs/api-reference.md#member-resultt-etapasync) | `Func<T,ValueTask>` | Await a success observation and return the original Result. |
| [`Result<T,E>.TapError`](docs/api-reference.md#member-resultt-etaperror) | action; caller-state; struct action; generated token | Observe failure and return the original Result. |
| [`Result<T,E>.Finally`](docs/api-reference.md#member-resultt-efinally) | caller-state action | Run one action for either initialized branch. |
| [`Result<T,E>.FinallyAsync`](docs/api-reference.md#member-resultt-efinallyasync) | caller-state `ValueTask` callback | Await one action for either initialized branch. |
| [`Result<T,E>.Match`](docs/api-reference.md#member-resultt-ematch) | delegates; caller-state; two struct callables | Exhaustively reduce both branches to one type. |
| [`Result<T,E>.Switch`](docs/api-reference.md#member-resultt-eswitch) | two actions | Execute exactly one terminal branch action. |
| [`Result<T,E>.ValueOr`](docs/api-reference.md#member-resultt-evalueor) | eager fallback | Discard failure and return a fallback value. |
| [`Result<T,E>.ValueOrElse`](docs/api-reference.md#member-resultt-evalueorelse) | lazy failure callback | Lazily derive a fallback from the error. |
| [`Result<T,E>.ToString`](docs/api-reference.md#member-resultt-etostring) | diagnostic representation | Format `Ok(...)`, `Fail(...)`, or `Uninitialized`. |
| [`Result<T,E>.Equals`](docs/api-reference.md#member-resultt-eequals) | another Result | Compare state and only the active payload. |
| [`Result<T,E>.GetHashCode`](docs/api-reference.md#member-resultt-egethashcode) | none | Hash state and only the active payload. |
| [`Result<T,E>` implicit conversions](docs/api-reference.md#member-resultt-eimplicit-operator) | from `T`; from `E` | Construct a typed Result where target context is unambiguous. |
| [`Result<T,E>.Flatten`](docs/api-reference.md#member-resultcompositionextensionsflatten) | nested Result | Remove one same-error Result layer. |
| [`Result<Option<T>,E>.Transpose`](docs/api-reference.md#member-resultcompositionextensionstranspose) | Result to Option | Convert to `Option<Result<T,E>>`. |
| [`Result<Option<T>,E>.RequireSome`](docs/api-reference.md#member-resultcompositionextensionsrequiresome) | lazy error factory | Require a present success value. |
| [`Option<Result<T,E>>.Transpose`](docs/api-reference.md#member-resultcompositionextensionstranspose) | Option to Result | Convert to `Result<Option<T>,E>`. |
| [`ResultCombination.Combine`](docs/api-reference.md#member-resultcombinationcombine) | two Results; `ReadOnlySpan<Result<Unit,E>>` | Return the first existing failure or `Unit` success. |
| [`ResultCombination.Zip`](docs/api-reference.md#member-resultcombinationzip) | two heterogeneous Results | Combine successes into a named tuple. |
| [`ResultCombination.Map`](docs/api-reference.md#member-resultcombinationmap) | two through six Results; delegate or caller-state projection | Combine and project without an intermediate tuple API or captured state. |
| [`ResultCombination.Bind`](docs/api-reference.md#member-resultcombinationbind) | two through six Results; delegate or caller-state Result projection | Combine and flatten a fallible projection. |
| [`Unit.Value`](docs/api-reference.md#member-unitvalue) | singleton value | Represent a successful operation with no payload. |
| [`Unit.ToString`](docs/api-reference.md#member-unittostring) | `()` | Produce the conventional unit representation. |

### Core Option And Callable API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Option<T>.Some`](docs/api-reference.md#member-optiontsome) | `Some(T)` | Construct guaranteed non-null presence. |
| [`Option<T>.None`](docs/api-reference.md#member-optiontnone) | static value | Construct absence; equivalent to `default`. |
| [`Option<T>.HasValue`](docs/api-reference.md#member-optionthasvalue) | `bool` | Test presence. |
| [`Option<T>.IsSome`](docs/api-reference.md#member-optiontissome) | `bool` | Functional alias for presence. |
| [`Option<T>.IsNone`](docs/api-reference.md#member-optiontisnone) | `bool` | Test absence. |
| [`Option<T>.Value`](docs/api-reference.md#member-optiontvalue) | `T` | Read presence or throw for `None`. |
| [`Option<T>.TryGetValue`](docs/api-reference.md#member-optionttrygetvalue) | `out T` | Read presence at an imperative boundary. |
| [`Option<T>.Deconstruct`](docs/api-reference.md#member-optiontdeconstruct) | `out bool`, `out T` | Expose presence to positional patterns. |
| [`Option<T>.Map`](docs/api-reference.md#member-optiontmap) | delegate; caller-state; struct callable; generated token | Transform a present value. |
| [`Option<T>.MapNullable`](docs/api-reference.md#member-optiontmapnullable) | nullable reference/value projection | Turn a null projection into `None` without changing `Map`'s null-rejection contract. |
| [`Option<T>.Bind`](docs/api-reference.md#member-optiontbind) | delegate; caller-state; struct callable; generated token | Continue presence without nesting Options. |
| [`Option<T>.Tap`](docs/api-reference.md#member-optionttap) | action; caller-state; struct action; generated action | Observe `Some` and return the original Option. |
| [`Option<T>.Zip`](docs/api-reference.md#member-optiontzip) | another Option | Pair two present values or return `None`. |
| [`Option<T>.Filter`](docs/api-reference.md#member-optiontfilter) | predicate; caller-state | Retain presence only when a predicate is true. |
| [`Option<T>.Match`](docs/api-reference.md#member-optiontmatch) | delegates; caller-state; two struct callables | Exhaustively reduce presence and absence. |
| [`Option<T>.Equals`](docs/api-reference.md#member-optiontequals) | another Option | Compare presence and only the active value. |
| [`Option<T>.GetHashCode`](docs/api-reference.md#member-optiontgethashcode) | none | Hash presence and only the active value. |
| [`Option<T>.Switch`](docs/api-reference.md#member-optiontswitch) | some/none actions | Execute exactly one terminal action. |
| [`Option<T>.ValueOr`](docs/api-reference.md#member-optiontvalueor) | eager fallback | Reduce absence to a fallback. |
| [`Option<T>.ValueOrElse`](docs/api-reference.md#member-optiontvalueorelse) | lazy fallback; caller-state | Lazily create a fallback without a closure. |
| [`Option<T>` implicit conversion](docs/api-reference.md#member-optiontimplicit-operator) | nullable `T` | Convert null to `None` and non-null to `Some`. |
| [`Option.FromNullable`](docs/api-reference.md#member-optionfromnullable) | nullable reference or value | Convert an explicit nullable boundary value. |
| [`Option<T>.ToNullable`](docs/api-reference.md#member-optionnullableextensionstonullable) | reference Option | Convert Some/None to a nullable reference. |
| [`Option<T>.ToNullableValue`](docs/api-reference.md#member-optionnullableextensionstonullablevalue) | value Option | Convert Some/None to `Nullable<T>`. |
| [`Option<T>.ToResult`](docs/api-reference.md#member-resultcompositionextensionstoresult) | eager error; lazy error | Require presence and attach a typed failure. |
| [`Option<T>.Traverse`](docs/api-reference.md#member-resultcompositionextensionstraverse) | delegate; caller-state; struct callable | Exchange Option and Result while mapping Some. |
| [`IValueFunction<TIn,TOut>.Invoke`](docs/api-reference.md#member-ivaluefunctiontin-toutinvoke) | value-returning callable | Define generic, devirtualizable callback dispatch. |
| [`IValueAction<T>.Invoke`](docs/api-reference.md#member-ivalueactiontinvoke) | side-effect callable | Define generic, devirtualizable action dispatch. |
| [`ValueFunction<TIn,TOut,TFunction>.ValueFunction`](docs/api-reference.md#member-valuefunctiontin-tout-tfunctionvaluefunction) | callable constructor | Wrap a stateful or default struct callable. |
| [`ValueFunction<TIn,TOut,TFunction>.Invoke`](docs/api-reference.md#member-valuefunctiontin-tout-tfunctioninvoke) | forwarding call | Invoke the wrapped callable. |
| [`ValueAction<T,TAction>.Invoke`](docs/api-reference.md#member-valueactiont-tactioninvoke) | forwarding call | Invoke the wrapped action. |
| [`ValueAction<T,TAction>.ValueAction`](docs/api-reference.md#member-valueactiont-tactionvalueaction) | action value | Wrap a stateful or default struct action. |

### Collection And LINQ API

| Member | Package | Purpose |
| --- | --- | --- |
| [`IReadOnlyList<T>.TraverseToArray`](docs/api-reference.md#member-resultcollectionextensionstraversetoarray) | `MonadicTypes.NET.Collections` | Fail-fast indexed traversal with one explicit output-array allocation. |
| [`ReadOnlySpan<T>.TraverseToArray`](docs/api-reference.md#member-resultcollectionextensionstraversetoarray) | `MonadicTypes.NET.Collections` | Fail-fast contiguous traversal with one explicit output-array allocation. |
| [`ReadOnlySpan<Result<T,E>>.SequenceToArray`](docs/api-reference.md#member-resultcollectionextensionssequencetoarray) | `MonadicTypes.NET.Collections` | Convert ordered Results into one owned array. |
| [`Result<T,E>.Select`](docs/api-reference.md#member-queryextensionsselect) | `MonadicTypes.NET.Linq` | Opt-in conventional name for Result Map. |
| [`Result<T,E>.SelectMany`](docs/api-reference.md#member-queryextensionsselectmany) | `MonadicTypes.NET.Linq` | Bind and project Result values. |
| [`Option<T>.Select`](docs/api-reference.md#member-queryextensionsselect) | `MonadicTypes.NET.Linq` | Opt-in conventional name for Option Map. |
| [`Option<T>.SelectMany`](docs/api-reference.md#member-queryextensionsselectmany) | `MonadicTypes.NET.Linq` | Bind and project Option values. |
| [`Option<T>.Where`](docs/api-reference.md#member-queryextensionswhere) | `MonadicTypes.NET.Linq` | Conventional name for Option Filter. |

### Async And Effect API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`MapAsync`](docs/api-reference.md#member-asyncresultextensionsmapasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Map success with a `ValueTask<T>` callback. |
| [`MapTaskAsync`](docs/api-reference.md#member-asyncresultextensionsmaptaskasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Map success with a `Task<T>` callback. |
| [`BindAsync`](docs/api-reference.md#member-asyncresultextensionsbindasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind success with a `ValueTask<Result<T,E>>` callback. |
| [`BindTaskAsync`](docs/api-reference.md#member-asyncresultextensionsbindtaskasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind success with a `Task<Result<T,E>>` callback. |
| [`BindErrorAsync`](docs/api-reference.md#member-asyncresultextensionsbinderrorasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind failure with a `ValueTask<Result<T,E>>` callback. |
| [`BindErrorTaskAsync`](docs/api-reference.md#member-asyncresultextensionsbinderrortaskasync) | delegate or generated `ValueFunction`; Result/ValueTask/Task receiver | Bind failure with a `Task<Result<T,E>>` callback. |
| [`Map` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous success mapping. |
| [`Bind` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous success binding. |
| [`BindError` on awaitable Result](#async-pipelines) | ValueTask/Task receiver | Continue an async pipeline with synchronous failure binding. |
| [`Effect.Try`](docs/api-reference.md#member-effecttry) | broad exception; selected `TException` | Convert a synchronous thrown exception to failure. |
| [`Effect.TryAsync`](docs/api-reference.md#member-effecttryasync) | broad exception; selected `TException` | Convert a ValueTask operation's exception to failure. |
| [`Effect.TryTaskAsync`](docs/api-reference.md#member-effecttrytaskasync) | broad/typed exception; with/without caller state | Convert a Task operation's exception to failure. |
| [`Result<T,E>.TryMap`](docs/api-reference.md#member-resulteffectextensionstrymap) | broad exception | Map success through throwing synchronous code. |
| [`Result<T,E>.TryBind`](docs/api-reference.md#member-resulteffectextensionstrybind) | broad exception | Bind success through throwing synchronous code. |
| [`Result<T,E>.TryTap`](docs/api-reference.md#member-resulteffectextensionstrytap) | broad exception | Observe success and convert a thrown exception. |
| [`Result<T,E>.TryMapAsync`](docs/api-reference.md#member-resulteffectextensionstrymapasync) | broad exception | Map success through throwing ValueTask code. |
| [`Result<T,E>.TryTapAsync`](docs/api-reference.md#member-resulteffectextensionstrytapasync) | broad exception | Observe success asynchronously and convert a thrown exception. |

### Error And Validation API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`Error.Error`](docs/api-reference.md#member-errorerror) | category constructor; code/message constructor | Construct a structured built-in error. |
| [`Error.Type`](docs/api-reference.md#member-errortype) | `ErrorType` | Read the bounded built-in category. |
| [`Error.NumericType`](docs/api-reference.md#member-errornumerictype) | `int` | Read built-in or custom numeric category. |
| [`Error.Code`](docs/api-reference.md#member-errorcode) | `string` | Read stable machine identity. |
| [`Error.Message`](docs/api-reference.md#member-errormessage) | `string` | Read diagnostic/display text. |
| [`Error.IsMessagePublic`](docs/api-reference.md#member-errorismessagepublic) | `bool` | Control default transport disclosure. |
| [`Error.Cause`](docs/api-reference.md#member-errorcause) | `Exception?` | Read a retained exception with its original stack. |
| [`Error.ThrowCause`](docs/api-reference.md#member-errorthrowcause) | method | Rethrow the retained exception without resetting its stack. |
| [`Error.Failure`](docs/api-reference.md#member-errorfailure) | message; code/message/cause/visibility | Construct a general expected failure. |
| [`Error.Unexpected`](docs/api-reference.md#member-errorunexpected) | message; exception/code | Construct a private unexpected failure. |
| [`Error.Validation`](docs/api-reference.md#member-errorvalidation) | message; code/message/cause | Construct invalid-input failure. |
| [`Error.Conflict`](docs/api-reference.md#member-errorconflict) | code/message/cause/visibility | Construct state-conflict failure. |
| [`Error.NotFound`](docs/api-reference.md#member-errornotfound) | code/message/cause/visibility | Construct missing-resource failure. |
| [`Error.Unauthorized`](docs/api-reference.md#member-errorunauthorized) | code/message/cause/visibility | Construct authentication failure. |
| [`Error.Forbidden`](docs/api-reference.md#member-errorforbidden) | code/message/cause/visibility | Construct authorization failure. |
| [`Error.Unavailable`](docs/api-reference.md#member-errorunavailable) | code/message/cause/visibility | Construct temporary-service failure. |
| [`Error.Timeout`](docs/api-reference.md#member-errortimeout) | code/message/cause/visibility | Construct timeout failure. |
| [`Error.RateLimited`](docs/api-reference.md#member-errorratelimited) | code/message/cause/visibility | Construct quota/rate failure. |
| [`Error.Cancelled`](docs/api-reference.md#member-errorcancelled) | code/message/cause | Construct cancellation failure. |
| [`Error.Custom`](docs/api-reference.md#member-errorcustom) | numeric type/code/message/cause/visibility | Construct an application-defined category. |
| [`Error.IO`](docs/api-reference.md#member-errorio) | message; code/message/cause/visibility | Construct a general I/O convenience error. |
| [`Error.System`](docs/api-reference.md#member-errorsystem) | message; code/message/cause/visibility | Construct a general system convenience error. |
| [`Error.Equals`](docs/api-reference.md#member-errorequals) | another error | Compare semantic fields and retained-cause identity. |
| [`Error.GetHashCode`](docs/api-reference.md#member-errorgethashcode) | none | Hash the same semantic fields used by equality. |
| [`Error.ToString`](docs/api-reference.md#member-errortostring) | default; format/provider | Allocate `[CODE] message` text. |
| [`Error.TryFormat`](docs/api-reference.md#member-errortryformat) | destination span | Format without allocating a string. |
| [`IErrorConvertible<TError>.ToError`](docs/api-reference.md#member-ierrorconvertibleterrortoerror) | conversion method | Convert compact domain error only at a boundary/failure. |
| [`Result<T,E>.BindWidened`](docs/api-reference.md#member-resulterrorextensionsbindwidened) | convertible continuation error | Keep compact inner errors until the failing branch is used. |
| [`ErrorType.Uninitialized`](docs/api-reference.md#member-errortypeuninitialized) | enum value | Represent an invalid default category; public APIs reject it. |
| [`ErrorType.Failure`](docs/api-reference.md#member-errortypefailure) | enum value | Categorize a general expected failure. |
| [`ErrorType.Unexpected`](docs/api-reference.md#member-errortypeunexpected) | enum value | Categorize an unclassified fault. |
| [`ErrorType.Validation`](docs/api-reference.md#member-errortypevalidation) | enum value | Categorize invalid input. |
| [`ErrorType.Conflict`](docs/api-reference.md#member-errortypeconflict) | enum value | Categorize a state conflict. |
| [`ErrorType.NotFound`](docs/api-reference.md#member-errortypenotfound) | enum value | Categorize a missing resource. |
| [`ErrorType.Unauthorized`](docs/api-reference.md#member-errortypeunauthorized) | enum value | Categorize missing or invalid authentication. |
| [`ErrorType.Forbidden`](docs/api-reference.md#member-errortypeforbidden) | enum value | Categorize denied authorization. |
| [`ErrorType.Unavailable`](docs/api-reference.md#member-errortypeunavailable) | enum value | Categorize temporary unavailability. |
| [`ErrorType.Timeout`](docs/api-reference.md#member-errortypetimeout) | enum value | Categorize a time-budget failure. |
| [`ErrorType.RateLimited`](docs/api-reference.md#member-errortyperatelimited) | enum value | Categorize quota or rate exhaustion. |
| [`ErrorType.Cancelled`](docs/api-reference.md#member-errortypecancelled) | enum value | Categorize cancellation. |
| [`ErrorType.Custom`](docs/api-reference.md#member-errortypecustom) | enum value | Categorize an application-defined numeric type. |
| [`ValidationIssue.ValidationIssue`](docs/api-reference.md#member-validationissuevalidationissue) | path/code/message/severity | Construct one immutable validation issue. |
| [`ValidationIssue.Path`](docs/api-reference.md#member-validationissuepath) | `string` | Read the affected member path. |
| [`ValidationIssue.Code`](docs/api-reference.md#member-validationissuecode) | `string` | Read stable issue identity. |
| [`ValidationIssue.Message`](docs/api-reference.md#member-validationissuemessage) | `string` | Read display text. |
| [`ValidationIssue.Severity`](docs/api-reference.md#member-validationissueseverity) | enum | Read error, warning, or information severity. |
| [`ValidationErrors.ValidationErrors`](docs/api-reference.md#member-validationerrorsvalidationerrors) | sequence; params array | Copy issues into immutable storage. |
| [`ValidationErrors.Create`](docs/api-reference.md#member-validationerrorscreate) | delegate; caller-state; struct mapper | Map third-party failures without a runtime adapter dependency. |
| [`ValidationErrors.Count`](docs/api-reference.md#member-validationerrorscount) | `int` | Read issue count. |
| [`ValidationErrors.this[int]`](docs/api-reference.md#member-validationerrorsthis) | indexer | Read one issue. |
| [`ValidationErrors.AsSpan`](docs/api-reference.md#member-validationerrorsasspan) | `ReadOnlySpan<ValidationIssue>` | Iterate without interface/enumerator allocation. |
| [`ValidationErrors.GetEnumerator`](docs/api-reference.md#member-validationerrorsgetenumerator) | generic enumerator | Support standard collection iteration. |
| [`ValidationSeverity.Error`](docs/api-reference.md#member-validationseverityerror) | enum value | Classify a blocking validation issue. |
| [`ValidationSeverity.Warning`](docs/api-reference.md#member-validationseveritywarning) | enum value | Classify a non-blocking warning. |
| [`ValidationSeverity.Information`](docs/api-reference.md#member-validationseverityinformation) | enum value | Classify an informational issue. |

### Diagnostics, HTTP, And Generation API

| Member | Overloads or value | Purpose |
| --- | --- | --- |
| [`ErrorTelemetry.Record`](docs/api-reference.md#member-errortelemetryrecord) | activity/error/policy | Project an error into an existing BCL Activity. |
| [`ErrorActivityStatusPolicy.Automatic`](docs/api-reference.md#member-erroractivitystatuspolicyautomatic) | enum value | Mark only server-failure categories as activity errors. |
| [`ErrorActivityStatusPolicy.Preserve`](docs/api-reference.md#member-erroractivitystatuspolicypreserve) | enum value | Preserve the caller's activity status. |
| [`ErrorActivityStatusPolicy.MarkError`](docs/api-reference.md#member-erroractivitystatuspolicymarkerror) | enum value | Mark every recorded category as an activity error. |
| [`ErrorMetrics.ErrorMetrics`](docs/api-reference.md#member-errormetricserrormetrics) | meter/code-dimension/counter-name | Create an optional caller-owned counter. |
| [`ErrorMetrics.Disabled`](docs/api-reference.md#member-errormetricsdisabled) | static value | Select the cheapest disabled metrics path. |
| [`ErrorMetrics.IsEnabled`](docs/api-reference.md#member-errormetricsisenabled) | `bool` | Check for a listener before expensive caller work. |
| [`ErrorMetrics.Record`](docs/api-reference.md#member-errormetricsrecord) | `Error?` | Increment the error counter with bounded tags. |
| [`Result<T,E>.ToHttpResult`](docs/api-reference.md#member-resulthttpextensionstohttpresult) | Error; ValidationErrors; convertible error; custom delegates; struct mappers | Convert to strongly typed Minimal API results. |
| [`IHttpResultMapper<TError,TResult>.Map`](docs/api-reference.md#member-ihttpresultmapperterror-tresultmap) | error/context | Define an allocation-free caller-owned HTTP failure mapper. |
| [`DefaultErrorHttpResultMapper.Map`](docs/api-reference.md#member-defaulterrorhttpresultmappermap) | error/context | Apply the default Error problem policy. |
| [`ErrorProblemDetails.Create`](docs/api-reference.md#member-errorproblemdetailscreate) | error/context | Create RFC ProblemDetails without executing it. |
| [`ErrorProblemDetails.CreateExample`](docs/api-reference.md#member-errorproblemdetailscreateexample) | error | Create deterministic documentation output without ambient trace data. |
| [`ErrorProblemDetails.ToHttpResult`](docs/api-reference.md#member-errorproblemdetailstohttpresult) | error/context | Create an executable ProblemHttpResult. |
| [`ErrorProblemDetails.GetStatusCode`](docs/api-reference.md#member-errorproblemdetailsgetstatuscode) | `ErrorType` | Read default category-to-status mapping. |
| [`ValidationErrorProblemDetails.ToHttpResult`](docs/api-reference.md#member-validationerrorproblemdetailstohttpresult) | errors/context | Create typed validation problem output. |
| [`ProducesErrors`](docs/api-reference.md#member-errorendpointconventionextensionsproduceserrors) | `ReadOnlySpan<ErrorType>` | Add Minimal API response metadata without reflection. |
| [`ErrorCatalogEntry.ErrorCatalogEntry`](docs/api-reference.md#member-errorcatalogentryerrorcatalogentry) | type/code/description | Define one initialized public documentation entry. |
| [`ErrorCatalogEntry.Type`](docs/api-reference.md#member-errorcatalogentrytype) | `ErrorType` | Read the documented response category. |
| [`ErrorCatalogEntry.Code`](docs/api-reference.md#member-errorcatalogentrycode) | `string` | Read the stable documented code. |
| [`ErrorCatalogEntry.Description`](docs/api-reference.md#member-errorcatalogentrydescription) | `string` | Read the public documentation text. |
| [`ErrorCatalogMetadata.ErrorCatalogMetadata`](docs/api-reference.md#member-errorcatalogmetadataerrorcatalogmetadata) | `ReadOnlySpan<ErrorCatalogEntry>` | Copy and validate endpoint-owned catalog metadata. |
| [`ErrorCatalogMetadata.Count`](docs/api-reference.md#member-errorcatalogmetadatacount) | `int` | Read the owned entry count. |
| [`ErrorCatalogMetadata.AsSpan`](docs/api-reference.md#member-errorcatalogmetadataasspan) | `ReadOnlySpan<ErrorCatalogEntry>` | Read entries without allocation. |
| [`ProducesErrorCatalog`](docs/api-reference.md#member-errorendpointconventionextensionsproduceserrorcatalog) | `ReadOnlySpan<ErrorCatalogEntry>` | Attach a copied catalog and unique status metadata to a Minimal API endpoint. |
| [`ProducesErrorAttribute.ProducesErrorAttribute`](docs/api-reference.md#member-produceserrorattributeproduceserrorattribute) | `ErrorType` | Add one controller response category. |
| [`ProducesErrorAttribute.ErrorType`](docs/api-reference.md#member-produceserrorattributeerrortype) | enum | Read configured category. |
| [`ProducesErrorAttribute.Type`](docs/api-reference.md#member-produceserrorattributetype) | ProblemDetails type | Expose OpenAPI response body metadata. |
| [`ProducesErrorAttribute.StatusCode`](docs/api-reference.md#member-produceserrorattributestatuscode) | `int` | Expose mapped status metadata. |
| [`ProducesErrorAttribute.Description`](docs/api-reference.md#member-produceserrorattributedescription) | `null` | Leave description to the OpenAPI pipeline. |
| [`ProducesErrorAttribute.ContentTypes`](docs/api-reference.md#member-produceserrorattributecontenttypes) | problem media types | Expose supported response content types. |
| [`ProducesErrorCatalogAttribute.ProducesErrorCatalogAttribute`](docs/api-reference.md#member-produceserrorcatalogattributeproduceserrorcatalogattribute) | type/code/description | Attach one catalog entry to a controller or endpoint. |
| [`ProducesErrorCatalogAttribute.Entry`](docs/api-reference.md#member-produceserrorcatalogattributeentry) | `ErrorCatalogEntry` | Read the validated documented entry. |
| [`ProducesErrorCatalogAttribute.Type`](docs/api-reference.md#member-produceserrorcatalogattributetype) | ProblemDetails type | Expose OpenAPI response body metadata. |
| [`ProducesErrorCatalogAttribute.StatusCode`](docs/api-reference.md#member-produceserrorcatalogattributestatuscode) | `int` | Expose the category's mapped status. |
| [`ProducesErrorCatalogAttribute.Description`](docs/api-reference.md#member-produceserrorcatalogattributedescription) | `null` | Leave response description to the document pipeline. |
| [`ProducesErrorCatalogAttribute.ContentTypes`](docs/api-reference.md#member-produceserrorcatalogattributecontenttypes) | problem media types | Expose supported response content types. |
| [`IServiceCollection.AddErrorCatalogOpenApi`](docs/api-reference.md#member-openapiservicecollectionextensionsadderrorcatalogopenapi) | service extension | Register the default document transformer and package-owned `ProblemHttpResult` JSON metadata. |
| [`OpenApiOptions.AddErrorCatalogs`](docs/api-reference.md#member-openapioptionsextensionsadderrorcatalogs) | options extension | Register the singleton reflection-free operation transformer. |
| [`MTAPI001`](#openapi-error-catalogs) | informational analyzer | Identify documented handlers that need explicit metadata or intentional XML projection. |
| [`GenerateValueFunctionAttribute.GenerateValueFunctionAttribute`](#generated-callables) | default; generated-name constructor | Request a wrapper while retaining the original method. |
| [`GenerateValueFunctionAttribute.Name`](#generated-callables) | optional name | Read the requested generated property name. |
| [`Functions.<Method>` generated property](#generated-callables) | `ValueFunction` or `ValueAction` | Pass an inferred zero-state token to sync or async operators. |
| [`ValueFunctionGenerator`](#generated-callables) | Roslyn component | Discover attributed methods and emit callable adapters. |
| [`ValueFunctionGenerator.Initialize`](#generated-callables) | generator context | Register the incremental generation pipeline; consumers do not call it. |

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

## Designing Pipelines

A fluent pipeline starts with function signatures, not with a long expression.
Write each operation to accept the previous stage's success value and return the
smallest type that describes its own outcome. The pipeline operator then follows
from that return type:

| Stage signature | Operator | Meaning |
| --- | --- | --- |
| `T -> U` | `Map` | Infallible transformation of success. |
| `T -> Result<U,E>` | `Bind` | A dependent operation that can fail. |
| `T -> bool` plus `T -> E` | `Ensure` | A success value must satisfy a guard. |
| `E -> F` | `MapError` | Translate failure without retrying work. |
| `E -> Result<T,F>` | `BindError` | Recover, retry, or replace a failure. |
| `T -> Option<U>` | `Map` or `Bind` on Option | Preserve expected absence explicitly. |
| `T -> ValueTask<U>` | `MapAsync` | Asynchronous, infallible transformation. |
| `T -> ValueTask<Result<U,E>>` | `BindAsync` | Asynchronous operation that can fail. |
| `T -> Task<U>` | `MapTaskAsync` | Existing Task-returning transformation. |
| `T -> Task<Result<U,E>>` | `BindTaskAsync` | Existing Task-returning fallible operation. |
| `T -> void` or `T -> ValueTask` | `Tap` or `TapAsync` | Observe success without changing it. |

The important distinction is `Map` versus `Bind`. If a function already returns
`Result`, `Map` would produce `Result<Result<U,E>,E>`; `Bind` keeps one railway.
The same rule applies to Option: use `Bind` when the callback already returns an
Option instead of creating `Option<Option<T>>`.

### Shape Leaf Operations First

Keep parsing, validation, dependency access, and projection as independently
testable functions. Their input and output types should line up:

```csharp
static Result<ParsedOrder, CheckoutError> ParseOrder(ReadOnlySpan<char> input);

static Result<ValidatedOrder, CheckoutError> ValidateOrder(ParsedOrder order);

static Order CreateOrder(ValidatedOrder order);

static ValueTask<Result<Reservation, CheckoutError>> ReserveInventoryAsync(
    Order order);

static ValueTask<Result<Payment, CheckoutError>> ChargeAsync(
    Reservation reservation);

static Receipt CreateReceipt(Payment payment);
```

Once the signatures align, the orchestration method contains no branch plumbing
and does not need a temporary Result for every operation:

```csharp
static ValueTask<Result<Receipt, CheckoutError>> CheckoutAsync(
    ReadOnlySpan<char> input) => ParseOrder(input)
        .Bind(ValidateOrder)
        .Map(CreateOrder)
        .BindAsync(ReserveInventoryAsync)
        .BindAsync(ChargeAsync)
        .Map(CreateReceipt);
```

Each callback runs only when the preceding stage succeeded. The first failure
passes through unchanged, including across completed or pending asynchronous
stages. Returning the pipeline directly also avoids an unnecessary orchestration
`async` state machine when no local work is required after awaiting it.

Contrast that with manually advancing a Result:

```csharp
static async ValueTask<Result<Receipt, CheckoutError>> CheckoutFragmentedAsync(
    ReadOnlyMemory<char> input)
{
    Result<ParsedOrder, CheckoutError> parsed = ParseOrder(input.Span);
    if (parsed.IsFailure)
    {
        return Result<Receipt, CheckoutError>.Fail(parsed.Error);
    }

    Result<ValidatedOrder, CheckoutError> validated = ValidateOrder(parsed.Value);
    if (validated.IsFailure)
    {
        return Result<Receipt, CheckoutError>.Fail(validated.Error);
    }

    Result<Reservation, CheckoutError> reserved =
        await ReserveInventoryAsync(CreateOrder(validated.Value));
    if (reserved.IsFailure)
    {
        return Result<Receipt, CheckoutError>.Fail(reserved.Error);
    }

    Result<Payment, CheckoutError> paid = await ChargeAsync(reserved.Value);
    return paid.Map(CreateReceipt);
}
```

This version is not more explicit about business behavior; it repeats the same
failure propagation four times and makes accidental error conversion or missed
branches easier. Imperative inspection is still appropriate at loops and
framework boundaries, but it should not be the default orchestration style.

### Keep Decisions Inside Stages

Pipelines do not eliminate branching. They move each decision into the function
that owns it, where every branch returns the same typed shape:

```csharp
static Result<Payment, CheckoutError> SelectPayment(
    PaymentAttempt attempt) => attempt.Status switch
    {
        PaymentStatus.Accepted => Result<Payment, CheckoutError>.Ok(attempt.Payment),
        PaymentStatus.Declined => Result<Payment, CheckoutError>.Fail(
            CheckoutError.PaymentDeclined),
        PaymentStatus.RequiresAction => RequestAdditionalAction(attempt),
        _ => Result<Payment, CheckoutError>.Fail(CheckoutError.InvalidPaymentState)
    };

Result<Receipt, CheckoutError> receipt = attemptResult
    .Bind(SelectPayment)
    .Map(CreateReceipt);
```

The orchestrator remains linear even though `SelectPayment` has several domain
branches. Do not force complex branch logic into an inline lambda merely to keep
everything on one line; a named function improves testing and lets the runtime
cache a static method-group delegate.

### Align Error Types

Stages compose directly when they share one domain error type. Prefer a compact
error union or enum-backed record for one use case instead of returning unrelated
framework errors from every leaf:

```csharp
static Result<Order, CheckoutError> LoadOrder(OrderId id) =>
    repository.Find(id).MapError(static error => error.Code switch
    {
        RepositoryErrorCode.Missing => CheckoutError.OrderNotFound,
        _ => CheckoutError.StorageUnavailable
    });
```

Translate a narrow dependency error once with `MapError`, or use `BindWidened`
when conversion should happen only if an inner operation actually fails. Avoid
repeatedly widening successful Results to rich `Error`; keep the compact domain
error through the application pipeline and convert at HTTP, telemetry, or other
integration boundaries.

When a lambda returns one derived case of a closed error hierarchy, specify only
the desired output error type to prevent C# from narrowing inference:

```csharp
Result<Order, CheckoutError> order = gateway.Load(orderId)
    .MapError<CheckoutError>(static error => new CheckoutError.Dependency(error));
```

### Preserve Optional Success

Use `Result<Option<T>,E>` when an operation can fail but successful execution may
legitimately find nothing. Keep the Option in the pipeline until a stage owns the
absence policy:

```csharp
Result<Option<Customer>, LookupError> lookup = FindCustomer(customerId);

Result<Option<string>, LookupError> email = lookup.Map(
    static customer => customer
        .Filter(static value => value.AcceptsEmail)
        .Map(static value => value.Email));

Result<Customer, LookupError> requiredCustomer = lookup.RequireSome(
    static () => LookupError.CustomerRequired);
```

The first branch preserves absence because sending email is optional. The second
turns the same absence into failure because that operation requires a customer.
Neither path uses null as an undocumented third state.

### Introduce Side Effects Deliberately

Keep value-producing stages pure where practical. Add logging, tracing, metrics,
or auditing with `Tap` and `TapError`, then consume the Result once at the outer
boundary:

```csharp
Results<Ok<Receipt>, ProblemHttpResult> response = checkout
    .Tap(audit, static (receipt, sink) => sink.Record(receipt))
    .TapError(observer, static (error, sink) => sink.Record(error))
    .ToHttpResult(static receipt => TypedResults.Ok(receipt), httpContext);
```

If a side effect can fail as part of business behavior, it is not merely an
observation and should return Result for use with `Bind`. If uncontrolled code
can throw, put one `Effect.Try`, `TryMap`, or `TryBind` boundary around that call
instead of catching exceptions throughout the pipeline.

### Know Where To Stop

A pipeline should describe one use case from input to boundary. End it with
`Match`, `Switch`, `ToHttpResult`, or another explicit adapter. Do not unwrap a
Result only to construct the same Result again, and do not turn every local
calculation into a pipeline stage. Tight parsers, SIMD loops, and simple local
branches remain ordinary C#; return one Result when that lower-level operation
crosses back into fallible application flow.

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

| Operator | On `Ok(value)` | On `Fail(error)` |
| --- | --- | --- |
| `Map(map)` | Calls `map(value)` and wraps its output in `Ok` | Returns the same error without calling `map` |
| `Bind(next)` | Calls `next(value)` and returns its Result without nesting | Returns the same error without calling `next` |
| `Ensure(predicate, onFailure)` | Keeps success when true; calls `onFailure(value)` when false | Returns the same failure; neither callback runs |
| `Recover(recover)` | Returns the same success | Calls `recover(error)` and returns its Result |

All Result operators reject an uninitialized Result before producing an output.
Caller-state overloads have identical branch behavior; they pass explicit state
to static callbacks so callers do not need a capturing closure. Struct-callable
and generated-token overloads likewise change dispatch, not semantics.

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

| Operator | On `Ok(value)` | On `Fail(error)` |
| --- | --- | --- |
| `MapError(map)` | Preserves the value under the new error type | Calls `map(error)` and wraps the output in `Fail` |
| `BindError(next)` | Preserves the value under the continuation's error type | Calls `next(error)` and returns its Result |
| `BiMap(mapValue, mapError)` | Calls only `mapValue` | Calls only `mapError` |
| `BindWidened(next)` | Calls `next`; converts its error only if that continuation fails | Preserves the already-wide outer error and does not call `next` |

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

`Tap` and `TapAsync` skip failures. `TapError` skips successes. `Finally` and
`FinallyAsync` run once for either initialized branch but receive only the
caller-provided state, not the active payload. Every callback exception is
allowed to propagate; use the corresponding `Try*` effect boundary only when a
thrown exception should become a typed failure.

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

#### Switch

`Switch(Action<T>, Action<E>)` returns `void` and invokes exactly one action. It
is a terminal consumer, not a railway operator: it cannot transform the value,
return the original Result, or be followed by another stage.

```csharp
checkout.Switch(
    static receipt => Console.WriteLine($"Receipt {receipt.Id}"),
    static error => Console.Error.WriteLine($"Checkout failed: {error.Code}"));
```

Use `Match` when both branches produce a value:

```csharp
string message = checkout.Match(
    static receipt => $"Receipt {receipt.Id}",
    static error => $"Checkout failed: {error.Code}");
```

Use `Tap` or `TapError` instead when observation belongs inside a continuing
pipeline. `Switch` accepts delegates; static lambdas and static method groups
avoid captures and per-call closure allocation. An uninitialized Result throws
before either action runs.

### Combining Independent Results

```csharp
Result<(User User, Account Account), LoadError> loaded =
    ResultCombination.Zip(LoadUser(id), LoadAccount(id));

Result<Invoice, LoadError> invoice = ResultCombination.Map(
    LoadUser(id),
    LoadAccount(id),
    static (user, account) => new Invoice(user, account));

Result<Invoice, LoadError> validated = ResultCombination.Bind(
    LoadUser(id),
    LoadAccount(id),
    static (user, account) => Invoice.Create(user, account));

ReadOnlySpan<Result<Unit, LoadError>> checks = [CheckUser(id), CheckAccount(id)];
Result<Unit, LoadError> valid = ResultCombination.Combine(checks);
```

Combination returns the first failure. It does not accumulate independent
validation errors; use `ValidationErrors` when accumulation is required.
`Error` is one structured error, not a composite collection.

More precisely, these methods inspect already-created Results in argument or
span order and return the first failure. `Zip` returns `Ok((first, second))`;
two-through-six-result `Map` calls its projection only when every input succeeds;
`Bind` accepts the same arities and returns a Result projection without nesting;
`Combine` returns `Ok(Unit.Value)` only when every input succeeds. An
uninitialized input throws when inspected.

C# evaluates method arguments before calling the combinator, so this does not
short-circuit producer execution:

```csharp
ResultCombination.Zip(LoadUser(id), LoadAccount(id)); // both calls run
```

Use `Bind` when the second operation must not run after the first failure:

```csharp
Result<(User User, Account Account), LoadError> loaded = LoadUser(id)
    .Bind(id, static (user, customerId) => LoadAccount(customerId)
        .Map(user, static (account, loadedUser) => (loadedUser, account)));
```

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

| Operator | On `Some(value)` | On `None` |
| --- | --- | --- |
| `Map(map)` | Calls `map(value)` and requires its output to be non-null | Returns `None` without calling `map` |
| `Bind(next)` | Calls `next(value)` and returns its Option without nesting | Returns `None` without calling `next` |
| `Filter(predicate)` | Returns the original Some when true, otherwise `None` | Returns `None` without calling the predicate |
| `ValueOr(fallback)` | Returns `value` | Returns the eagerly supplied fallback |
| `ValueOrElse(factory)` | Returns `value` without calling the factory | Calls the factory and returns its output |

```csharp
Option<Address> address = customer
    .Filter(static value => value.IsActive)
    .Bind(static value => value.PrimaryAddress);

string city = address.Match(
    static value => value.City,
    static () => "No address");
```

Option's `Switch(Action<T>, Action)` is likewise terminal and invokes exactly
one branch. `None` is a valid state, so unlike an uninitialized Result it runs
the `none` action rather than throwing:

```csharp
address.Switch(
    static value => RenderAddress(value),
    static () => RenderMissingAddress());
```

Use `Match` instead when both cases must produce a value that subsequent code
will consume.

### Option And Result

Use `IsNone`, `IsSome`, or comparison with `Option<T>.None` to test absence.
`option == null` is not a consistent absence test: null converts to `None` for
reference-type payloads, but a value-type Option can instead participate in a
lifted nullable comparison. `Assert.Null(option)` also tests a boxed struct,
not its presence state.

With nullable analysis enabled, the reference-type spelling produces CS8625;
the value-type spelling produces CS8073 (always false). This repository treats
both diagnostics as errors. Do not suppress them to perform an absence check.

`Map` preserves the non-null `Some` contract and throws if a projection returns
null, including an empty nullable value type. Use `MapNullable` for nullable
reference projections and `MapNullableValue` for nullable value projections:

```csharp
Option<string> nickname = person.MapNullable(static value => value.Nickname);
Option<Guid> userId = person.MapNullableValue(static value => value.UserId);
```

Both yield `None` for an absent source or null projection. `Bind` with
`Option.FromNullable` remains the general composition form when the projection
already returns an Option.

Nested Result/Option shapes can be transposed without ad-hoc branching:

```csharp
Result<Option<Customer>, LookupError> lookup = FindOptionalCustomer(id);
Option<Result<Customer, LookupError>> presentResult = lookup.Transpose();

Result<Customer, LookupError> required = lookup.RequireSome(
    static () => LookupError.NotFound);
```

`RequireSome` applies only to `Result<Option<T>,E>` and has three exact cases:

| Input | Output | Error factory called? |
| --- | --- | --- |
| `Ok(Some(value))` | `Ok(value)` | No |
| `Ok(None)` | `Fail(whenNone())` | Yes, once |
| `Fail(error)` | `Fail(error)` | No |

`Option<T>.ToResult` performs the same presence requirement without an outer
Result. Its eager overload receives an existing error; its lazy overload calls
the factory only for `None`.

`RequireSome` also accepts an existing error or caller-owned factory state:

```csharp
Result<Option<Customer>, Error> customerLookup = FindCustomer(id);
var required = customerLookup.RequireSome(Error.NotFound("MISSING", "Customer missing."));
var requiredWithId = customerLookup.RequireSome(
    id, static key => Error.NotFound("MISSING", $"Customer {key} missing."));
```

Use these forms with a `Result<Option<Customer>, Error>` lookup. The eager form
evaluates its argument at the call site; the caller-state form constructs an
error only for `Ok(None)` and avoids a captured delegate. Error construction or
message formatting can still allocate. Both preserve an existing failure.

Use explicit nullable bridges at framework, persistence, and serialization
boundaries. Reference and value nullability remain distinct to avoid ambiguous
overloads:

```csharp
Option<Customer> customer = Option.FromNullable(entity.Customer);
Option<int> retryCount = Option.FromNullable(dto.RetryCount);

Customer? nullableCustomer = customer.ToNullable();
int? nullableRetryCount = retryCount.ToNullableValue();
```

`Traverse` is the direct way to combine optional input with a fallible stage.
The selector runs only for Some; None becomes `Ok(None)` without invocation:

```csharp
Result<Option<Customer>, LookupError> loaded = optionalId.Traverse(LoadCustomer);
```

Generated wrappers also infer the success and error types in
`optionalId.Traverse(Projections.Functions.LoadCustomer)` and
`rows.TraverseToArray(Projections.Functions.ToDomain)`, including span receivers.
For explicitly generic bare-callable calls, use a typed value such as
`default(MyProjection)`; an untyped `default` can be ambiguous with the wrapper
overload.

Transpose changes nesting, not meaning:

| Input | Output |
| --- | --- |
| `Result<Option<T>,E>.Ok(Some(value))` | `Some(Ok(value))` |
| `Result<Option<T>,E>.Ok(None)` | `None` |
| `Result<Option<T>,E>.Fail(error)` | `Some(Fail(error))` |
| `Option<Result<T,E>>.Some(Ok(value))` | `Ok(Some(value))` |
| `Option<Result<T,E>>.Some(Fail(error))` | `Fail(error)` |
| `Option<Result<T,E>>.None` | `Ok(None)` |

`Flatten` applies to `Result<Result<T,E>,E>`: outer failure wins, outer success
returns the inner Result unchanged, and no callback or allocation is involved.

## Collections And LINQ

These features are separate packages so the core assembly and consumers that
do not use them remain unchanged.

Install count-known collection traversal only when the application owns an
indexed input and needs an output array:

```bash
dotnet add package MonadicTypes.NET.Collections --prerelease
```

```csharp
using MonadicTypes.Collections;

Result<Customer[], LookupError> customers = ids.TraverseToArray(LoadCustomer);
```

`TraverseToArray` is fail-fast and one-pass. Empty input returns the shared empty
array. Non-empty input allocates exactly one result array, including when a later
item fails; this avoids hidden iterator, builder, and resize allocations while
preserving selector side effects and exception stacks. Use the caller-state or
struct-callable overload when a captured delegate would allocate. Use
`SequenceToArray` when the inputs are already a span of Results.

LINQ names and query expressions are opt-in:

```bash
dotnet add package MonadicTypes.NET.Linq --prerelease
```

```csharp
using MonadicTypes.Linq;

Result<Invoice, LoadError> invoice = LoadUser(id).SelectMany(
    static user => LoadAccount(user.AccountId),
    static (user, account) => new Invoice(user, account));

Option<Customer> active = optionalCustomer
    .Where(static customer => customer.IsActive)
    .Select(static customer => customer.Normalize());
```

The equivalent query expression is supported when it reads better:

```csharp
Result<Invoice, LoadError> invoice =
    from user in LoadUser(id)
    from account in LoadAccount(user.AccountId)
    select new Invoice(user, account);
```

Fluent syntax is the default recommendation. In the initial NativeAOT run it
measured `2.095 ns` versus `2.355 ns` for query `Select`, and `4.509 ns` versus
`4.952 ns` for query `SelectMany`; every row allocated `0 B`. The LINQ members
do not catch callbacks, so exceptions propagate unchanged. Convert exceptions
to typed failures only through an explicit Effects boundary.

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
| `TryMap`, `TryMapAsync` | Map an existing success through code that can throw |
| `TryBind` | Bind an existing success through Result-returning code that can throw |
| `TryTap`, `TryTapAsync` | Run a throwing success side effect and turn its exception into failure |

Use the broad overload when every recoverable exception has the same domain
meaning. Use a typed overload when only one exception is expected; all other
types propagate. Use a caller-state overload when the operation needs local
data and a capturing lambda would allocate.

Broad Effect overloads do not convert cancellation, stack overflow,
out-of-memory, access violation, bad image, and similar runtime failures. Typed
Effect overloads intentionally do not apply the broad filter: selecting an
exception type is an explicit decision to convert that type. A custom boundary
should define its own explicit policy rather than depending on internal Effects
implementation.

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
| `PaymentRequired` | 402 | Billing or entitlement requirement |
| `Forbidden` | 403 | Authenticated caller lacks permission |
| `NotFound` | 404 | Requested resource is absent |
| `NotAcceptable` | 406 | Requested representation cannot be produced |
| `RequestTimeout` | 408 | Caller did not send the request in time |
| `Conflict` | 409 | State or concurrency conflict |
| `Gone` | 410 | Resource was deliberately removed |
| `PreconditionFailed` | 412 | Supplied request precondition was not met |
| `ContentTooLarge` | 413 | Request content exceeds the accepted limit |
| `UnsupportedMediaType` | 415 | Request media type is unsupported |
| `UnprocessableContent` | 422 | Parsed content cannot be processed semantically |
| `Locked` | 423 | Target resource is locked |
| `PreconditionRequired` | 428 | A required request precondition is absent |
| `RateLimited` | 429 | Quota or rate exceeded |
| `Cancelled` | 499 | Request cancelled by the caller; non-IANA compatibility mapping |
| `NotImplemented` | 501 | Declared operation is not implemented |
| `BadGateway` | 502 | Upstream response was unusable |
| `Unavailable` | 503 | Temporary dependency or service outage |
| `Timeout` | 504 | Dependency exceeded its time budget |
| `Failure`, `Unexpected` | 500 | Private failure unless a custom HTTP mapper overrides policy |
| `Custom` | 500 by default; 400–599 when supplied | Application-defined numeric category; the ASP.NET adapter honors valid custom HTTP statuses |

| Construction API | Category |
| --- | --- |
| `Error.Validation`, `Conflict`, `NotFound` | Expected input and resource failures |
| `Error.Unauthorized`, `Forbidden` | Authentication and authorization failures |
| `Error.RateLimited`, `Unavailable`, `Timeout`, `Cancelled` | Operational failures with explicit transport policy |
| `Error.Failure`, `Unexpected` | General private failures; use `Unexpected` for unclassified faults |
| `Error.Custom` | Positive application-defined numeric category |
| `Error.IO`, `System` | Convenience codes for general I/O and system failures |
| `new Error(type, code, message, ...)` | All focused built-in categories, including the twelve precise HTTP-oriented categories above |

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

Two errors are equal when their category, numeric category, ordinal code,
ordinal message, disclosure policy, and retained-cause reference are equal.
Exception causes intentionally use identity rather than exception message or
stack comparison. Hashing uses the same fields. This semantic contract does not
expose or depend on the error's compact internal storage representation.

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

`ValidationErrors` retains reference identity; it does not perform an implicit
sequence comparison or sequence hash. Use `AsSpan` when an application needs an
explicit issue-by-issue comparison. The same reference-identity rule applies to
the immutable `ErrorCatalogMetadata` owner used during endpoint registration.

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

Use an explicit HTTP status when the endpoint needs a response outside the
default category policy. The existing failure callback composes with the adapter:

```csharp
Results<Ok<Customer>, ProblemHttpResult> response = result.ToHttpResult(
    static customer => TypedResults.Ok(customer),
    static error => ErrorProblemDetails.ToHttpResult(error, StatusCodes.Status402PaymentRequired));
```

`Create`, `CreateExample`, and `ToHttpResult` accept an explicit status from 400
through 599. This covers standard HTTP errors and deliberate application/proxy
extensions; other ranges are rejected. Overrides use a status-specific title
and `urn:problem-type:http-{status}` identity while preserving error codes,
message visibility, and request tracing. Unknown reason phrases use `HTTP error`.
`CreateExample` continues to omit ambient trace data.

`ErrorType` remains a focused set of application categories. Core treats
`Error.Custom` as an application-defined numeric category; at the ASP.NET Core
boundary, a custom numeric value from 400 through 599 is also honored as its HTTP
error status. Other custom numeric values retain the generic 500 mapping. Use an
explicit override when transport policy differs from the stored category.
`Cancelled` retains its existing nonstandard 499 default; choose 408 explicitly
only when the request-timeout meaning is appropriate.

Endpoints remain responsible for status-specific protocol behavior and headers,
such as `Allow` for 405, authentication challenges for 401/407, and precondition
evaluation for 412/428. Consult the
[HTTP status registry](https://www.iana.org/assignments/http-status-codes/) and
[HTTP semantics](https://www.rfc-editor.org/rfc/rfc9110.html) when choosing a status.
Successful or conditional 2xx/3xx responses belong on the success channel.
The two-callback overload also supports fully caller-owned `ProblemDetails`.

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

### OpenAPI Error Catalogs

`MonadicTypes.NET.AspNetCore` can attach stable public error metadata without an
OpenAPI runtime dependency. Each `ErrorCatalogEntry` requires an initialized
defined category, a non-empty machine code, and a non-empty public description.
Minimal API registration copies the entries, rejects duplicate codes across the
complete endpoint regardless of HTTP status, and adds one problem response per
distinct HTTP status:

```csharp
app.MapGet("/customers/{id:int}", GetCustomer)
    .ProducesErrorCatalog(
        new ErrorCatalogEntry(
            ErrorType.NotFound,
            "CUSTOMER_NOT_FOUND",
            "The customer does not exist."),
        new ErrorCatalogEntry(
            ErrorType.Unavailable,
            "CUSTOMER_STORE_UNAVAILABLE",
            "The customer store is unavailable."));
```

The input may be stack-backed through the `params ReadOnlySpan<T>` contract.

Catalog entries and attributes accept the same explicit HTTP status policy:

```csharp
app.MapPut("/customers/{id:int}", UpdateCustomer)
    .ProducesErrorCatalog(
        new(ErrorType.Conflict, "STALE_VERSION", "Reload the resource.", 412),
        new(ErrorType.Conflict, "PRECONDITION_REQUIRED", "Supply If-Match.", 428));

// Controller equivalent:
// [ProducesErrorCatalog(ErrorType.Validation, "INVALID_CONTENT", "Invalid content.", 422)]
// [ProducesError(ErrorType.Validation, 422)]
```

Runtime mapping and declared metadata must use the same status. OpenAPI groups
each code under that status and uses matching status-specific example titles
and types. Metadata documents behavior; it does not change runtime responses.

Registration performs one owned array copy so later caller mutation cannot
change endpoint metadata. This is a cold startup cost; request execution does
not construct, transform, or inspect catalogs. Controllers attach repeatable
entries with attributes:

```csharp
[HttpGet("{id:int}")]
[ProducesErrorCatalog(
    ErrorType.NotFound,
    "CUSTOMER_NOT_FOUND",
    "The customer does not exist.")]
public ActionResult<Customer> Get(int id) => Handle(id);
```

Install `MonadicTypes.NET.AspNetCore.OpenApi` only when those entries should be
projected into an ASP.NET Core OpenAPI document:

```csharp
using MonadicTypes.AspNetCore.OpenApi;

builder.Services.AddErrorCatalogOpenApi();
app.MapOpenApi();
```

NativeAOT OpenAPI generation also requires source-generated JSON metadata for
the application's request, response, and bound parameter types. This includes
primitive route parameters used in schemas. `AddErrorCatalogOpenApi()` supplies
package-owned metadata for the `ProblemHttpResult` payload without registering
problem-details services; the application context supplies application-owned
types:

```csharp
using System.Text.Json.Serialization;

builder.Services.ConfigureHttpJsonOptions(static options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0,
        ApiJsonSerializerContext.Default));

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(CustomerResponse))]
internal sealed partial class ApiJsonSerializerContext : JsonSerializerContext;
```

Missing application metadata fails document generation; the adapter does not
fall back to reflection. Register all endpoint types for which ASP.NET Core
must produce a schema.

The transformer reads endpoint metadata by type, groups codes by mapped HTTP
status, and adds a code enum plus deterministic problem examples. It contains
no reflection, assembly scanning, service location, or request-path work.
`ErrorProblemDetails.CreateExample` is available when application-owned
document transformers need the same trace-free problem shape.

The integration package deliberately removes Microsoft's transitive XML-comment
generator. `MTAPI001` reports a documented Minimal API handler that has neither
`.WithSummary(...)`/`.WithDescription(...)` nor an active Microsoft XML
projection. Use explicit metadata for the reflection-free profile:

```csharp
app.MapGet("/customers/{id:int}", GetCustomer)
    .WithSummary("Gets a customer.")
    .WithDescription("Returns the customer when it exists.");
```

For Microsoft's complete XML-comment projection, add a direct reference to the
pinned `Microsoft.AspNetCore.OpenApi` package. The direct reference is the
intentional opt-in signal: our build target retains Microsoft's generator and
its reflection-based document transformer. An alternative is to generate the
document in a separate docs process and serve the static artifact, keeping that
reflection outside the production executable.

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

## Testing Helpers

`MonadicTypes.NET.Testing` is an optional framework-neutral package for test
projects that want typed extraction without taking a dependency on a specific
test runner:

```csharp
using MonadicTypes.Testing;

User user = LoadUser().ShouldBeOk("load user").ValueOrFail();
Option<User> maybeUser = FindUser();
maybeUser.ShouldBeSome().ValueOrFail();
```

The helpers return the unchanged Result or Option for composition and throw
only the package-owned `MonadicAssertionException` when the expected case is
absent. Production packages do not reference this package.

## Analyzers

`MonadicTypes.NET.Analyzers` is an explicit build-time package. `MT0001` flags
`Option<T> == null`/`!= null`; `MT0002` flags a direct `Map` that creates a
nested `Result` or `Option` where `Bind` is usually intended. Both diagnostics
are suppressible for intentional boundary code, and the analyzer package adds
no runtime dependency.

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
- Every shipped runtime and generator project emits XML documentation and treats
  missing public-member documentation (`CS1591`) as an error.
- Benchmark inputs, delegates, Tasks, errors, and setup allocations are created
  outside measured operations.
- Accepted success/composition paths must allocate 0 B.
- New operations must beat an architectural target derived from an analogous
  optimized primitive before their first measurement becomes a baseline.
- Primitive, composition, and Switch NativeAOT runners have isolated output
  directories so build order cannot contaminate code layout or execute the
  wrong harness.

See [benchmark policy](docs/benchmarks.md), [accepted baselines](benchmarks/baseline.md),
[compatibility](docs/compatibility.md), [development policy](docs/development.md),
[dependency policy](docs/dependency-policy.md), and the
[documentation architecture](docs/documentation-architecture.md).

## Status And Licensing

Versioning, Git tags, GitHub Releases, and trusted NuGet publication are defined
in the [release policy](docs/releases.md). Development builds use a
`0.1.0-dev` version; release builds receive the exact immutable workflow
version.

This repository is maintained primarily for use in the maintainer's
applications and projects. Preview releases carry no stability or support
commitment.

Contributions require prior arrangement; unsolicited pull requests may not be
reviewed.

Licensed under the [Apache License 2.0](LICENSE). Commercial and workplace use,
modification, and redistribution are permitted subject to the license terms,
including preservation of the license, modification notices, and NOTICE
attributions. The license includes an explicit contributor patent grant and
does not grant rights to project names or trademarks.

## AI-Assisted Development

This project is AI-assisted. Architecture, acceptance decisions, performance
criteria, and published changes remain subject to human direction and review.

<!-- BEGIN GENERATED API INDEX -->

[Complete API reference](docs/api-reference.md)

393 documented public members

<!-- END GENERATED API INDEX -->
