# API Reference

Generated from public PE metadata and compiler XML documentation.

## Packages

- [MonadicTypes.NET](#package-monadictypesnet)
- [MonadicTypes.NET.AspNetCore](#package-monadictypesnetaspnetcore)
- [MonadicTypes.NET.AspNetCore.OpenApi](#package-monadictypesnetaspnetcoreopenapi)
- [MonadicTypes.NET.Async](#package-monadictypesnetasync)
- [MonadicTypes.NET.Collections](#package-monadictypesnetcollections)
- [MonadicTypes.NET.Diagnostics](#package-monadictypesnetdiagnostics)
- [MonadicTypes.NET.Effects](#package-monadictypesneteffects)
- [MonadicTypes.NET.Errors](#package-monadictypesneterrors)
- [MonadicTypes.NET.Generators](#package-monadictypesnetgenerators)
- [MonadicTypes.NET.Linq](#package-monadictypesnetlinq)
- [MonadicTypes.NET.Testing](#package-monadictypesnettesting)

## Package MonadicTypes.NET

**Types:** [`IValueAction<T>`](#type-ivalueactiont) · [`IValueFunction<TIn, TOut>`](#type-ivaluefunctiontin-tout) · [`Option`](#type-option) · [`Option<T>`](#type-optiont) · [`OptionNullableExtensions`](#type-optionnullableextensions) · [`Result`](#type-result) · [`Result<T, E>`](#type-resultt-e) · [`ResultCombination`](#type-resultcombination) · [`ResultCompositionExtensions`](#type-resultcompositionextensions) · [`Unit`](#type-unit) · [`ValueAction<T, TAction>`](#type-valueactiont-taction) · [`ValueFunction<TIn, TOut, TFunction>`](#type-valuefunctiontin-tout-tfunction)

### Type: `IValueAction<T>`

Defines a value-type side-effect callback that avoids delegate allocation and dispatch.

| Member | Description |
| --- | --- |
| [`Invoke`](#member-ivalueactiontinvoke) | Performs the action for `value`. |

#### Member: `IValueAction<T>.Invoke`

**Example**

```csharp
public readonly struct Observe : IValueAction<Error>
{
    public void Invoke(Error value) => Console.WriteLine(value.Message);
}
```

| Overload | Description |
| --- | --- |
| [`void Invoke(T value)`](#overload-void-invoket-value-on-ivalueactiont) | Performs the action for `value`. |

##### Overload: `void Invoke(T value)` on `IValueAction<T>`

Performs the action for `value`.

**Parameters**

- `value`: Input value.

### Type: `IValueFunction<TIn, TOut>`

Defines an allocation-free callable value that can be constrained and inlined by the runtime. Implementations should normally be readonly structs.

| Member | Description |
| --- | --- |
| [`Invoke`](#member-ivaluefunctiontin-toutinvoke) | Transforms `value` into an output value. |

#### Member: `IValueFunction<TIn, TOut>.Invoke`

**Example**

```csharp
public readonly struct GetId : IValueFunction<User, int>
{
    public int Invoke(User value) => value.Id;
}
```

| Overload | Description |
| --- | --- |
| [`TOut Invoke(TIn value)`](#overload-tout-invoketin-value-on-ivaluefunctiontin-tout) | Transforms `value` into an output value. |

##### Overload: `TOut Invoke(TIn value)` on `IValueFunction<TIn, TOut>`

Transforms `value` into an output value.

**Parameters**

- `value`: Input value.

**Returns:** The transformed output.

### Type: `Option`

Creates options from nullable application-boundary values.

| Member | Description |
| --- | --- |
| [`FromNullable`](#member-optionfromnullable) | Converts a nullable value type to Some, or null to None. |

#### Member: `Option.FromNullable`

**Example**

```csharp
Option<User> user = Option.FromNullable(nullableUser);
```

| Overload | Description |
| --- | --- |
| [`Option<T> FromNullable<T>(Nullable<T> value)`](#overload-optiont-fromnullabletnullablet-value-on-option) | Converts a nullable value type to Some, or null to None. |
| [`Option<T> FromNullable<T>(T? value)`](#overload-optiont-fromnullablett-value-on-option) | Converts a nullable reference to Some, or null to None. |

##### Overload: `Option<T> FromNullable<T>(Nullable<T> value)` on `Option`

Converts a nullable value type to Some, or null to None.

**Type parameters**

- `T`: Non-null value type.

**Parameters**

- `value`: Nullable value to convert.

**Returns:** Some for a present value; otherwise None.

##### Overload: `Option<T> FromNullable<T>(T? value)` on `Option`

Converts a nullable reference to Some, or null to None.

**Type parameters**

- `T`: Non-null reference value type.

**Parameters**

- `value`: Nullable value to convert.

**Returns:** Some for a non-null value; otherwise None.

### Type: `Option<T>`

Represents either one non-null value or no value without heap allocation.

**Example**

```csharp
Option<User> user = Option<User>.Some(value);
```

| Member | Description |
| --- | --- |
| [`Bind`](#member-optiontbind) | Composes a present value with another optional operation and propagates `None`. |
| [`Deconstruct`](#member-optiontdeconstruct) | Deconstructs presence and value for positional pattern matching. |
| [`Equals`](#member-optiontequals) | Compares presence and, only when present, the contained value. |
| [`Filter`](#member-optiontfilter) | Retains a present value only when `predicate` returns true. |
| [`GetHashCode`](#member-optiontgethashcode) | Hashes presence and, only when present, the contained value. |
| [`HasValue`](#member-optionthasvalue) | Gets whether this option contains a value. |
| [`IsNone`](#member-optiontisnone) | Gets whether this option is the `None` case. |
| [`IsSome`](#member-optiontissome) | Gets whether this option is the `Some` case. |
| [`Map`](#member-optiontmap) | Maps a present value and propagates `None`. |
| [`MapNullable`](#member-optiontmapnullable) | Maps a present value through a nullable reference projection, treating null as `None`. |
| [`MapNullableValue`](#member-optiontmapnullablevalue) | Maps a present value through a nullable value projection, treating no value as `None`. |
| [`Match`](#member-optiontmatch) | Folds the active case into one output value. |
| [`None`](#member-optiontnone) | Gets the empty option. |
| [`Some`](#member-optiontsome) | Creates an option containing a non-null value. |
| [`Switch`](#member-optiontswitch) | Executes exactly one action for the active case. |
| [`Tap`](#member-optionttap) | Observes a present value and returns this option unchanged. |
| [`TryGetValue`](#member-optionttrygetvalue) | Attempts to retrieve the contained value. |
| [`Value`](#member-optiontvalue) | Gets the contained value. |
| [`ValueOr`](#member-optiontvalueor) | Returns the present value or an eagerly supplied fallback. |
| [`ValueOrElse`](#member-optiontvalueorelse) | Returns the present value or lazily creates a fallback. |
| [`Zip`](#member-optiontzip) | Combines two options in argument order. |
| [`implicit operator`](#member-optiontimplicit-operator) | Converts a value to `Some`, or null to `None`. |

#### Member: `Option<T>.Bind`

**Example**

```csharp
Option<Address> address = option.Bind(static user => user.PrimaryAddress);
```

| Overload | Description |
| --- | --- |
| [`Option<TR> Bind<TR>(Func<T, Option<TR>> bind)`](#overload-optiontr-bindtrfunct-optiontr-bind-on-optiont) | Composes a present value with another optional operation and propagates `None`. |
| [`Option<TR> Bind<TR, TFunction>(ValueFunction<T, Option<TR>, TFunction> bind)`](#overload-optiontr-bindtr-tfunctionvaluefunctiont-optiontr-tfunction-bind-on-optiont) | Composes through a generated callable wrapper and propagates `None`. |
| [`Option<TR> Bind<TState, TR>(TState state, Func<T, TState, Option<TR>> bind)`](#overload-optiontr-bindtstate-trtstate-state-funct-tstate-optiontr-bind-on-optiont) | Composes while passing caller-owned state to a non-capturing continuation. |
| [`Option<TR> Bind<TR, TFunction>(TFunction bind)`](#overload-optiontr-bindtr-tfunctiontfunction-bind-on-optiont) | Composes through an allocation-free callable and propagates `None`. |

##### Overload: `Option<TR> Bind<TR>(Func<T, Option<TR>> bind)` on `Option<T>`

Composes a present value with another optional operation and propagates `None`.

##### Overload: `Option<TR> Bind<TR, TFunction>(ValueFunction<T, Option<TR>, TFunction> bind)` on `Option<T>`

Composes through a generated callable wrapper and propagates `None`.

##### Overload: `Option<TR> Bind<TState, TR>(TState state, Func<T, TState, Option<TR>> bind)` on `Option<T>`

Composes while passing caller-owned state to a non-capturing continuation.

##### Overload: `Option<TR> Bind<TR, TFunction>(TFunction bind)` on `Option<T>`

Composes through an allocation-free callable and propagates `None`.

#### Member: `Option<T>.Deconstruct`

**Example**

```csharp
string text = option switch { (true, User user) => user.Name, _ => "Missing" };
```

| Overload | Description |
| --- | --- |
| [`void Deconstruct(out bool hasValue, out T value)`](#overload-void-deconstructout-bool-hasvalue-out-t-value-on-optiont) | Deconstructs presence and value for positional pattern matching. |

##### Overload: `void Deconstruct(out bool hasValue, out T value)` on `Option<T>`

Deconstructs presence and value for positional pattern matching.

**Parameters**

- `hasValue`: Receives true for Some and false for None.
- `value`: Receives the contained value, or default for None.

#### Member: `Option<T>.Equals`

**Example**

```csharp
bool equal = left.Equals(right);
```

| Overload | Description |
| --- | --- |
| [`bool Equals(Option<T> other)`](#overload-bool-equalsoptiont-other-on-optiont) | Compares presence and, only when present, the contained value. |

##### Overload: `bool Equals(Option<T> other)` on `Option<T>`

Compares presence and, only when present, the contained value.

#### Member: `Option<T>.Filter`

**Example**

```csharp
Option<User> active = option.Filter(static user => user.IsActive);
```

| Overload | Description |
| --- | --- |
| [`Option<T> Filter(Func<T, bool> predicate)`](#overload-optiont-filterfunct-bool-predicate-on-optiont) | Retains a present value only when `predicate` returns true. |
| [`Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate)`](#overload-optiont-filtertstatetstate-state-funct-tstate-bool-predicate-on-optiont) | Filters a present value while passing caller-owned state to the predicate. |

##### Overload: `Option<T> Filter(Func<T, bool> predicate)` on `Option<T>`

Retains a present value only when `predicate` returns true.

##### Overload: `Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate)` on `Option<T>`

Filters a present value while passing caller-owned state to the predicate.

#### Member: `Option<T>.GetHashCode`

**Example**

```csharp
int hash = option.GetHashCode();
```

| Overload | Description |
| --- | --- |
| [`int GetHashCode()`](#overload-int-gethashcode-on-optiont) | Hashes presence and, only when present, the contained value. |

##### Overload: `int GetHashCode()` on `Option<T>`

Hashes presence and, only when present, the contained value.

#### Member: `Option<T>.HasValue`

**Example**

```csharp
bool present = option.HasValue;
```

| Overload | Description |
| --- | --- |
| [`bool HasValue`](#overload-bool-hasvalue-on-optiont) | Gets whether this option contains a value. |

##### Overload: `bool HasValue` on `Option<T>`

Gets whether this option contains a value.

#### Member: `Option<T>.IsNone`

**Example**

```csharp
if (option.IsNone) HandleAbsence();
```

| Overload | Description |
| --- | --- |
| [`bool IsNone`](#overload-bool-isnone-on-optiont) | Gets whether this option is the `None` case. |

##### Overload: `bool IsNone` on `Option<T>`

Gets whether this option is the `None` case.

#### Member: `Option<T>.IsSome`

**Example**

```csharp
if (option.IsSome) Consume(option.Value);
```

| Overload | Description |
| --- | --- |
| [`bool IsSome`](#overload-bool-issome-on-optiont) | Gets whether this option is the `Some` case. |

##### Overload: `bool IsSome` on `Option<T>`

Gets whether this option is the `Some` case.

#### Member: `Option<T>.Map`

**Example**

```csharp
Option<int> id = option.Map(static user => user.Id);
```

| Overload | Description |
| --- | --- |
| [`Option<TR> Map<TR>(Func<T, TR> map)`](#overload-optiontr-maptrfunct-tr-map-on-optiont) | Maps a present value and propagates `None`. |
| [`Option<TR> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)`](#overload-optiontr-maptr-tfunctionvaluefunctiont-tr-tfunction-map-on-optiont) | Maps a present value through a generated callable wrapper and propagates `None`. |
| [`Option<TR> Map<TState, TR>(TState state, Func<T, TState, TR> map)`](#overload-optiontr-maptstate-trtstate-state-funct-tstate-tr-map-on-optiont) | Maps a present value while passing caller-owned state to a non-capturing function. |
| [`Option<TR> Map<TR, TFunction>(TFunction map)`](#overload-optiontr-maptr-tfunctiontfunction-map-on-optiont) | Maps a present value through an allocation-free callable and propagates `None`. |

##### Overload: `Option<TR> Map<TR>(Func<T, TR> map)` on `Option<T>`

Maps a present value and propagates `None`.

The projection must return a non-null value. For nullable members, use Bind with Option.FromNullable to turn null into None.

**Throws**

- `ArgumentNullException`: A present value's projection returns null, including an empty nullable value type.

##### Overload: `Option<TR> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)` on `Option<T>`

Maps a present value through a generated callable wrapper and propagates `None`.

**Throws**

- `ArgumentNullException`: A present value's projection returns null.

##### Overload: `Option<TR> Map<TState, TR>(TState state, Func<T, TState, TR> map)` on `Option<T>`

Maps a present value while passing caller-owned state to a non-capturing function.

**Throws**

- `ArgumentNullException`: A present value's projection returns null.

##### Overload: `Option<TR> Map<TR, TFunction>(TFunction map)` on `Option<T>`

Maps a present value through an allocation-free callable and propagates `None`.

**Throws**

- `ArgumentNullException`: A present value's projection returns null.

#### Member: `Option<T>.MapNullable`

**Example**

```csharp
Option<string> nickname = person.MapNullable(static value => value.Nickname);
```

| Overload | Description |
| --- | --- |
| [`Option<TR> MapNullable<TR>(Func<T, TR> map)`](#overload-optiontr-mapnullabletrfunct-tr-map-on-optiont) | Maps a present value through a nullable reference projection, treating null as `None`. |

##### Overload: `Option<TR> MapNullable<TR>(Func<T, TR> map)` on `Option<T>`

Maps a present value through a nullable reference projection, treating null as `None`.

**Type parameters**

- `TR`: Projected reference type.

**Parameters**

- `map`: Projection invoked only for `Some`.

**Returns:** `Some` for a non-null projection, otherwise `None`.

#### Member: `Option<T>.MapNullableValue`

**Example**

```csharp
Option<Guid> id = person.MapNullableValue(static value => value.UserId);
```

| Overload | Description |
| --- | --- |
| [`Option<TR> MapNullableValue<TR>(Func<T, Nullable<TR>> map)`](#overload-optiontr-mapnullablevaluetrfunct-nullabletr-map-on-optiont) | Maps a present value through a nullable value projection, treating no value as `None`. |

##### Overload: `Option<TR> MapNullableValue<TR>(Func<T, Nullable<TR>> map)` on `Option<T>`

Maps a present value through a nullable value projection, treating no value as `None`.

**Type parameters**

- `TR`: Projected value type.

**Parameters**

- `map`: Projection invoked only for `Some`.

**Returns:** `Some` for a present projection, otherwise `None`.

#### Member: `Option<T>.Match`

**Example**

```csharp
string name = option.Match(static user => user.Name, static () => "Unknown");
```

| Overload | Description |
| --- | --- |
| [`TR Match<TR>(Func<T, TR> some, Func<TR> none)`](#overload-tr-matchtrfunct-tr-some-functr-none-on-optiont) | Folds the active case into one output value. |
| [`TR Match<TState, TR>(TState state, Func<T, TState, TR> some, Func<TState, TR> none)`](#overload-tr-matchtstate-trtstate-state-funct-tstate-tr-some-functstate-tr-none-on-optiont) | Folds the active case while passing caller-owned state to non-capturing functions. |
| [`TR Match<TR, TSome, TNone>(TSome some, TNone none)`](#overload-tr-matchtr-tsome-tnonetsome-some-tnone-none-on-optiont) | Folds the active case through allocation-free callable values. |

##### Overload: `TR Match<TR>(Func<T, TR> some, Func<TR> none)` on `Option<T>`

Folds the active case into one output value.

**Type parameters**

- `TR`: Output type.

**Parameters**

- `some`: Function invoked for a populated option.
- `none`: Function invoked for an empty option.

**Returns:** The selected function's output.

##### Overload: `TR Match<TState, TR>(TState state, Func<T, TState, TR> some, Func<TState, TR> none)` on `Option<T>`

Folds the active case while passing caller-owned state to non-capturing functions.

**Type parameters**

- `TState`: Caller state type.
- `TR`: Output type.

**Parameters**

- `state`: State passed unchanged to the selected branch.
- `some`: Function invoked for a populated option.
- `none`: Function invoked for an empty option.

**Returns:** The selected function's output.

##### Overload: `TR Match<TR, TSome, TNone>(TSome some, TNone none)` on `Option<T>`

Folds the active case through allocation-free callable values.

**Type parameters**

- `TR`: Output type.
- `TSome`: Populated-case callable type.
- `TNone`: Empty-case callable type accepting `Unit`.

**Parameters**

- `some`: Callable invoked for a populated option.
- `none`: Callable invoked for an empty option.

**Returns:** The selected callable's output.

#### Member: `Option<T>.None`

**Example**

```csharp
Option<User> option = Option<User>.None;
```

| Overload | Description |
| --- | --- |
| [`Option<T> None`](#overload-optiont-none-on-optiont) | Gets the empty option. |

##### Overload: `Option<T> None` on `Option<T>`

Gets the empty option.

#### Member: `Option<T>.Some`

**Example**

```csharp
Option<User> option = Option<User>.Some(user);
```

| Overload | Description |
| --- | --- |
| [`Option<T> Some(T value)`](#overload-optiont-somet-value-on-optiont) | Creates an option containing a non-null value. |

##### Overload: `Option<T> Some(T value)` on `Option<T>`

Creates an option containing a non-null value.

**Parameters**

- `value`: Value to contain.

**Returns:** A populated option.

**Throws**

- `ArgumentNullException`: `value` is null.

#### Member: `Option<T>.Switch`

**Example**

```csharp
option.Switch(RenderUser, RenderMissingUser);
```

| Overload | Description |
| --- | --- |
| [`void Switch(Action<T> some, Action none)`](#overload-void-switchactiont-some-action-none-on-optiont) | Executes exactly one action for the active case. |

##### Overload: `void Switch(Action<T> some, Action none)` on `Option<T>`

Executes exactly one action for the active case.

**Parameters**

- `some`: Action invoked for a populated option.
- `none`: Action invoked for an empty option.

#### Member: `Option<T>.Tap`

**Example**

```csharp
Option<User> observed = option.Tap(static user => audit.Record(user));
```

| Overload | Description |
| --- | --- |
| [`Option<T> Tap(Action<T> action)`](#overload-optiont-tapactiont-action-on-optiont) | Observes a present value and returns this option unchanged. |
| [`Option<T> Tap<TAction>(ValueAction<T, TAction> action)`](#overload-optiont-taptactionvalueactiont-taction-action-on-optiont) | Observes a present value through a generated callable wrapper. |
| [`Option<T> Tap<TAction>(TAction action)`](#overload-optiont-taptactiontaction-action-on-optiont) | Observes a present value through an allocation-free callable and returns this option unchanged. |
| [`Option<T> Tap<TState>(TState state, Action<T, TState> action)`](#overload-optiont-taptstatetstate-state-actiont-tstate-action-on-optiont) | Observes a present value while passing caller-owned state. |

##### Overload: `Option<T> Tap(Action<T> action)` on `Option<T>`

Observes a present value and returns this option unchanged.

**Parameters**

- `action`: Action invoked only for `Some`.

**Returns:** This option unchanged.

##### Overload: `Option<T> Tap<TAction>(ValueAction<T, TAction> action)` on `Option<T>`

Observes a present value through a generated callable wrapper.

**Type parameters**

- `TAction`: Callable action type.

**Parameters**

- `action`: Action invoked only for `Some`.

**Returns:** This option unchanged.

##### Overload: `Option<T> Tap<TAction>(TAction action)` on `Option<T>`

Observes a present value through an allocation-free callable and returns this option unchanged.

**Type parameters**

- `TAction`: Callable action type.

**Parameters**

- `action`: Action invoked only for `Some`.

**Returns:** This option unchanged.

##### Overload: `Option<T> Tap<TState>(TState state, Action<T, TState> action)` on `Option<T>`

Observes a present value while passing caller-owned state.

**Type parameters**

- `TState`: Caller state type.

**Parameters**

- `state`: State passed unchanged to the action.
- `action`: Action invoked only for `Some`.

**Returns:** This option unchanged.

#### Member: `Option<T>.TryGetValue`

**Example**

```csharp
if (option.TryGetValue(out User user)) Consume(user);
```

| Overload | Description |
| --- | --- |
| [`bool TryGetValue(out T value)`](#overload-bool-trygetvalueout-t-value-on-optiont) | Attempts to retrieve the contained value. |

##### Overload: `bool TryGetValue(out T value)` on `Option<T>`

Attempts to retrieve the contained value.

**Parameters**

- `value`: Receives the value when present.

**Returns:** `true` when populated; otherwise `false`.

#### Member: `Option<T>.Value`

**Example**

```csharp
User user = option.Value;
```

| Overload | Description |
| --- | --- |
| [`T Value`](#overload-t-value-on-optiont) | Gets the contained value. |

##### Overload: `T Value` on `Option<T>`

Gets the contained value.

**Throws**

- `InvalidOperationException`: The option is `None`.

#### Member: `Option<T>.ValueOr`

**Example**

```csharp
User user = option.ValueOr(User.Anonymous);
```

| Overload | Description |
| --- | --- |
| [`T ValueOr(T fallback)`](#overload-t-valueort-fallback-on-optiont) | Returns the present value or an eagerly supplied fallback. |

##### Overload: `T ValueOr(T fallback)` on `Option<T>`

Returns the present value or an eagerly supplied fallback.

#### Member: `Option<T>.ValueOrElse`

**Example**

```csharp
User user = option.ValueOrElse(CreateAnonymousUser);
```

| Overload | Description |
| --- | --- |
| [`T ValueOrElse(Func<T> fallback)`](#overload-t-valueorelsefunct-fallback-on-optiont) | Returns the present value or lazily creates a fallback. |
| [`T ValueOrElse<TState>(TState state, Func<TState, T> fallback)`](#overload-t-valueorelsetstatetstate-state-functstate-t-fallback-on-optiont) | Returns the present value or lazily creates a fallback with caller-owned state. |

##### Overload: `T ValueOrElse(Func<T> fallback)` on `Option<T>`

Returns the present value or lazily creates a fallback.

##### Overload: `T ValueOrElse<TState>(TState state, Func<TState, T> fallback)` on `Option<T>`

Returns the present value or lazily creates a fallback with caller-owned state.

#### Member: `Option<T>.Zip`

**Example**

```csharp
Option<(User User, Account Account)> loaded = user.Zip(account);
```

| Overload | Description |
| --- | --- |
| [`Option<ValueTuple<T, TOther>> Zip<TOther>(Option<TOther> other)`](#overload-optionvaluetuplet-tother-ziptotheroptiontother-other-on-optiont) | Combines two options in argument order. |

##### Overload: `Option<ValueTuple<T, TOther>> Zip<TOther>(Option<TOther> other)` on `Option<T>`

Combines two options in argument order.

**Type parameters**

- `TOther`: Second option value type.

**Parameters**

- `other`: Second option.

**Returns:** `Some` containing both values when both options are present; otherwise `None`.

#### Member: `Option<T>.implicit operator`

**Example**

```csharp
Option<User> option = nullableUser;
```

| Overload | Description |
| --- | --- |
| [`implicit operator Option<T>(T value)`](#overload-implicit-operator-optiontt-value-on-optiont) | Converts a value to `Some`, or null to `None`. |

##### Overload: `implicit operator Option<T>(T value)` on `Option<T>`

Converts a value to `Some`, or null to `None`.

### Type: `OptionNullableExtensions`

Converts options to nullable application-boundary values.

| Member | Description |
| --- | --- |
| [`ToNullable`](#member-optionnullableextensionstonullable) | Returns the contained reference, or null for None. |
| [`ToNullableValue`](#member-optionnullableextensionstonullablevalue) | Returns the contained nullable value, or null for None. |

#### Member: `OptionNullableExtensions.ToNullable`

**Example**

```csharp
Option<User> user = Option.FromNullable(nullableUser);
User? value = user.ToNullable();
```

| Overload | Description |
| --- | --- |
| [`T? ToNullable<T>(in Option<T> option)`](#overload-t-tonullabletin-optiont-option-on-optionnullableextensions) | Returns the contained reference, or null for None. |

##### Overload: `T? ToNullable<T>(in Option<T> option)` on `OptionNullableExtensions`

Returns the contained reference, or null for None.

#### Member: `OptionNullableExtensions.ToNullableValue`

**Example**

```csharp
Option<int> age = Option<int>.Some(42);
int? value = age.ToNullableValue();
```

| Overload | Description |
| --- | --- |
| [`Nullable<T> ToNullableValue<T>(in Option<T> option)`](#overload-nullablet-tonullablevaluetin-optiont-option-on-optionnullableextensions) | Returns the contained nullable value, or null for None. |

##### Overload: `Nullable<T> ToNullableValue<T>(in Option<T> option)` on `OptionNullableExtensions`

Returns the contained nullable value, or null for None.

### Type: `Result`

Factories for results whose success case carries no data.

| Member | Description |
| --- | --- |
| [`Fail`](#member-resultfail) | Creates a failed unit result containing `error`. |
| [`Ok`](#member-resultok) | Creates a successful unit result. |

#### Member: `Result.Fail`

**Example**

```csharp
Result<Unit, SaveError> saved = Result.Fail(SaveError.WriteFailed);
```

| Overload | Description |
| --- | --- |
| [`Result<Unit, E> Fail<E>(E error)`](#overload-resultunit-e-failee-error-on-result) | Creates a failed unit result containing `error`. |

##### Overload: `Result<Unit, E> Fail<E>(E error)` on `Result`

Creates a failed unit result containing `error`.

#### Member: `Result.Ok`

**Example**

```csharp
Result<Unit, SaveError> saved = Result.Ok<SaveError>();
```

| Overload | Description |
| --- | --- |
| [`Result<Unit, E> Ok<E>()`](#overload-resultunit-e-oke-on-result) | Creates a successful unit result. |

##### Overload: `Result<Unit, E> Ok<E>()` on `Result`

Creates a successful unit result.

### Type: `Result<T, E>`

Represents either a successful value or a non-null error as a readonly value type.

**Example**

```csharp
Result<User, LookupError> user = Result<User, LookupError>.Ok(value);
```

| Member | Description |
| --- | --- |
| [`BiMap`](#member-resultt-ebimap) | Transforms both cases without invoking the inactive branch. |
| [`Bind`](#member-resultt-ebind) | Composes a success with another same-shaped result and propagates failures. |
| [`BindError`](#member-resultt-ebinderror) | Composes the failure case while preserving a successful value. |
| [`Deconstruct`](#member-resultt-edeconstruct) | Deconstructs the active case for positional pattern matching. |
| [`Ensure`](#member-resultt-eensure) | Converts a success to failure when `predicate` is false. |
| [`Equals`](#member-resultt-eequals) | Compares state and only the payload belonging to the active case. |
| [`Error`](#member-resultt-eerror) | Gets the failure error. |
| [`Fail`](#member-resultt-efail) | Creates a failed result containing a non-null `error`. |
| [`Finally`](#member-resultt-efinally) | Invokes a synchronous finalizer for either initialized case and returns this result. |
| [`FinallyAsync`](#member-resultt-efinallyasync) | Invokes an asynchronous finalizer for either initialized case and returns this result. |
| [`GetHashCode`](#member-resultt-egethashcode) | Hashes state and only the payload belonging to the active case. |
| [`IsFailure`](#member-resultt-eisfailure) | Gets whether this result contains an error. |
| [`IsInitialized`](#member-resultt-eisinitialized) | Gets whether this value was constructed through `Ok` or `Fail`. |
| [`IsSuccess`](#member-resultt-eissuccess) | Gets whether this result contains a successful value. |
| [`Map`](#member-resultt-emap) | Maps a successful value without changing its type and propagates failures unchanged. |
| [`MapError`](#member-resultt-emaperror) | Maps the active error to another non-null type and preserves successes. |
| [`Match`](#member-resultt-ematch) | Folds the active case through exactly one branch function. |
| [`Ok`](#member-resultt-eok) | Creates a successful result containing `value`. |
| [`Recover`](#member-resultt-erecover) | Recovers a failure through `recover` and preserves successes. |
| [`Switch`](#member-resultt-eswitch) | Executes exactly one action for the active case. |
| [`Tap`](#member-resultt-etap) | Invokes `action` only for success and returns this result. |
| [`TapAsync`](#member-resultt-etapasync) | Asynchronously invokes `action` only for success. |
| [`TapError`](#member-resultt-etaperror) | Invokes `action` only for failure and returns this result. |
| [`ToString`](#member-resultt-etostring) | Returns `Ok(value)`, `Fail(error)`, or `Uninitialized`. |
| [`TryGetError`](#member-resultt-etrygeterror) | Attempts to retrieve the failure error and rejects an uninitialized result. |
| [`TryGetValue`](#member-resultt-etrygetvalue) | Attempts to retrieve the successful value and rejects an uninitialized result. |
| [`Value`](#member-resultt-evalue) | Gets the successful value. |
| [`ValueOr`](#member-resultt-evalueor) | Returns the success value or an eagerly supplied fallback. |
| [`ValueOrElse`](#member-resultt-evalueorelse) | Returns the success value or lazily maps the active error to a fallback. |
| [`implicit operator`](#member-resultt-eimplicit-operator) | Converts a success value into a successful result. |

#### Member: `Result<T, E>.BiMap`

**Example**

```csharp
Result<UserDto, ProblemCode> mapped = result.BiMap(UserDto.From, ProblemCode.From);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TNextError> BiMap<TResult, TNextError>(Func<T, TResult> mapValue, Func<E, TNextError> mapError)`](#overload-resulttresult-tnexterror-bimaptresult-tnexterrorfunct-tresult-mapvalue-funce-tnexterror-maperror-on-resultt-e) | Transforms both cases without invoking the inactive branch. |

##### Overload: `Result<TResult, TNextError> BiMap<TResult, TNextError>(Func<T, TResult> mapValue, Func<E, TNextError> mapError)` on `Result<T, E>`

Transforms both cases without invoking the inactive branch.

**Type parameters**

- `TResult`: Mapped success type.
- `TNextError`: Mapped error type.

**Parameters**

- `mapValue`: Success mapping function.
- `mapError`: Failure mapping function.

**Returns:** A result containing the mapped active case.

#### Member: `Result<T, E>.Bind`

**Example**

```csharp
Result<User, LookupError> active = result.Bind(RequireActiveUser);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Bind(Func<T, Result<T, E>> next)`](#overload-resultt-e-bindfunct-resultt-e-next-on-resultt-e) | Composes a success with another same-shaped result and propagates failures. |
| [`Result<TR, E> Bind<TR>(Func<T, Result<TR, E>> next)`](#overload-resulttr-e-bindtrfunct-resulttr-e-next-on-resultt-e) | Composes a success with another result and propagates failures. |
| [`Result<T, E> Bind<TFunction>(TFunction next)`](#overload-resultt-e-bindtfunctiontfunction-next-on-resultt-e) | Composes a success through an allocation-free same-shaped continuation. |
| [`Result<TR, E> Bind<TR, TFunction>(ValueFunction<T, Result<TR, E>, TFunction> next)`](#overload-resulttr-e-bindtr-tfunctionvaluefunctiont-resulttr-e-tfunction-next-on-resultt-e) | Composes a success through a generated callable wrapper and propagates failures. |
| [`Result<TR, E> Bind<TR, TNextError>(Func<T, Result<TR, TNextError>> next, Func<TNextError, E> mapNextError)`](#overload-resulttr-e-bindtr-tnexterrorfunct-resulttr-tnexterror-next-functnexterror-e-mapnexterror-on-resultt-e) | Composes a success and maps the continuation's error into this result's error type. |
| [`Result<TR, E> Bind<TState, TR>(TState state, Func<T, TState, Result<TR, E>> next)`](#overload-resulttr-e-bindtstate-trtstate-state-funct-tstate-resulttr-e-next-on-resultt-e) | Composes a success with caller-owned state and propagates failures. |
| [`Result<TR, E> Bind<TR, TFunction>(TFunction next)`](#overload-resulttr-e-bindtr-tfunctiontfunction-next-on-resultt-e) | Composes a success through an allocation-free callable and propagates failures. |
| [`Result<TR, E> Bind<TState, TR, TNextError>(TState state, Func<T, TState, Result<TR, TNextError>> next, Func<TNextError, E> mapNextError)`](#overload-resulttr-e-bindtstate-tr-tnexterrortstate-state-funct-tstate-resulttr-tnexterror-next-functnexterror-e-mapnexterror-on-resultt-e) | Composes with caller-owned state and maps the continuation's error type. |

##### Overload: `Result<T, E> Bind(Func<T, Result<T, E>> next)` on `Result<T, E>`

Composes a success with another same-shaped result and propagates failures.

##### Overload: `Result<TR, E> Bind<TR>(Func<T, Result<TR, E>> next)` on `Result<T, E>`

Composes a success with another result and propagates failures.

##### Overload: `Result<T, E> Bind<TFunction>(TFunction next)` on `Result<T, E>`

Composes a success through an allocation-free same-shaped continuation.

##### Overload: `Result<TR, E> Bind<TR, TFunction>(ValueFunction<T, Result<TR, E>, TFunction> next)` on `Result<T, E>`

Composes a success through a generated callable wrapper and propagates failures.

##### Overload: `Result<TR, E> Bind<TR, TNextError>(Func<T, Result<TR, TNextError>> next, Func<TNextError, E> mapNextError)` on `Result<T, E>`

Composes a success and maps the continuation's error into this result's error type.

##### Overload: `Result<TR, E> Bind<TState, TR>(TState state, Func<T, TState, Result<TR, E>> next)` on `Result<T, E>`

Composes a success with caller-owned state and propagates failures.

##### Overload: `Result<TR, E> Bind<TR, TFunction>(TFunction next)` on `Result<T, E>`

Composes a success through an allocation-free callable and propagates failures.

##### Overload: `Result<TR, E> Bind<TState, TR, TNextError>(TState state, Func<T, TState, Result<TR, TNextError>> next, Func<TNextError, E> mapNextError)` on `Result<T, E>`

Composes with caller-owned state and maps the continuation's error type.

#### Member: `Result<T, E>.BindError`

**Example**

```csharp
Result<User, FinalError> recovered = result.BindError(RetryLookup);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TNextError> BindError<TNextError>(Func<E, Result<T, TNextError>> next)`](#overload-resultt-tnexterror-binderrortnexterrorfunce-resultt-tnexterror-next-on-resultt-e) | Composes the failure case while preserving a successful value. |
| [`Result<T, TNextError> BindError<TState, TNextError>(TState state, Func<E, TState, Result<T, TNextError>> next)`](#overload-resultt-tnexterror-binderrortstate-tnexterrortstate-state-funce-tstate-resultt-tnexterror-next-on-resultt-e) | Composes the failure case with caller-owned state. |
| [`Result<T, TNextError> BindError<TNextError, TFunction>(TFunction next)`](#overload-resultt-tnexterror-binderrortnexterror-tfunctiontfunction-next-on-resultt-e) | Composes the failure case through an allocation-free callable value. |

##### Overload: `Result<T, TNextError> BindError<TNextError>(Func<E, Result<T, TNextError>> next)` on `Result<T, E>`

Composes the failure case while preserving a successful value.

**Type parameters**

- `TNextError`: Error type returned by the failure continuation.

**Parameters**

- `next`: Continuation invoked only when this result is a failure.

**Returns:** The unchanged success or the result returned by `next`.

##### Overload: `Result<T, TNextError> BindError<TState, TNextError>(TState state, Func<E, TState, Result<T, TNextError>> next)` on `Result<T, E>`

Composes the failure case with caller-owned state.

**Type parameters**

- `TState`: Caller state passed to the continuation.
- `TNextError`: Error type returned by the failure continuation.

**Parameters**

- `state`: State passed unchanged to `next`.
- `next`: Continuation invoked only when this result is a failure.

**Returns:** The unchanged success or the result returned by `next`.

##### Overload: `Result<T, TNextError> BindError<TNextError, TFunction>(TFunction next)` on `Result<T, E>`

Composes the failure case through an allocation-free callable value.

**Type parameters**

- `TNextError`: Error type returned by the failure continuation.
- `TFunction`: Callable value type.

**Parameters**

- `next`: Continuation invoked only when this result is a failure.

**Returns:** The unchanged success or the result returned by `next`.

#### Member: `Result<T, E>.Deconstruct`

**Example**

```csharp
string text = result switch { (true, User user, _) => user.Name, (false, _, LookupError error) => error.Code };
```

| Overload | Description |
| --- | --- |
| [`void Deconstruct(out bool isSuccess, out T value, out E error)`](#overload-void-deconstructout-bool-issuccess-out-t-value-out-e-error-on-resultt-e) | Deconstructs the active case for positional pattern matching. |

##### Overload: `void Deconstruct(out bool isSuccess, out T value, out E error)` on `Result<T, E>`

Deconstructs the active case for positional pattern matching.

**Parameters**

- `isSuccess`: Receives true for success and false for failure.
- `value`: Receives the success value, or default for failure.
- `error`: Receives the failure value, or default for success.

#### Member: `Result<T, E>.Ensure`

**Example**

```csharp
Result<Order, CheckoutError> valid = order.Ensure(static value => value.Lines.Count > 0, static _ => CheckoutError.EmptyOrder);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Ensure(Func<T, bool> predicate, Func<T, E> onFailure)`](#overload-resultt-e-ensurefunct-bool-predicate-funct-e-onfailure-on-resultt-e) | Converts a success to failure when `predicate` is false. |
| [`Result<T, E> Ensure<TState>(TState state, Func<T, TState, bool> predicate, Func<T, TState, E> onFailure)`](#overload-resultt-e-ensuretstatetstate-state-funct-tstate-bool-predicate-funct-tstate-e-onfailure-on-resultt-e) | Validates a success while passing caller-owned state to both callbacks. |

##### Overload: `Result<T, E> Ensure(Func<T, bool> predicate, Func<T, E> onFailure)` on `Result<T, E>`

Converts a success to failure when `predicate` is false.

##### Overload: `Result<T, E> Ensure<TState>(TState state, Func<T, TState, bool> predicate, Func<T, TState, E> onFailure)` on `Result<T, E>`

Validates a success while passing caller-owned state to both callbacks.

#### Member: `Result<T, E>.Equals`

**Example**

```csharp
bool equal = left.Equals(right);
```

| Overload | Description |
| --- | --- |
| [`bool Equals(Result<T, E> other)`](#overload-bool-equalsresultt-e-other-on-resultt-e) | Compares state and only the payload belonging to the active case. |

##### Overload: `bool Equals(Result<T, E> other)` on `Result<T, E>`

Compares state and only the payload belonging to the active case.

#### Member: `Result<T, E>.Error`

**Example**

```csharp
LookupError error = result.Error;
```

| Overload | Description |
| --- | --- |
| [`E Error`](#overload-e-error-on-resultt-e) | Gets the failure error. |

##### Overload: `E Error` on `Result<T, E>`

Gets the failure error.

**Throws**

- `InvalidOperationException`: The result is successful or uninitialized.

#### Member: `Result<T, E>.Fail`

**Example**

```csharp
Result<User, LookupError> result = Result<User, LookupError>.Fail(error);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Fail(E error)`](#overload-resultt-e-faile-error-on-resultt-e) | Creates a failed result containing a non-null `error`. |

##### Overload: `Result<T, E> Fail(E error)` on `Result<T, E>`

Creates a failed result containing a non-null `error`.

**Throws**

- `ArgumentNullException`: `error` is null.

#### Member: `Result<T, E>.Finally`

**Example**

```csharp
Result<Order, CheckoutError> completed = order.Finally(timer, static value => value.Stop());
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Finally<TState>(TState state, Action<TState> action)`](#overload-resultt-e-finallytstatetstate-state-actiontstate-action-on-resultt-e) | Invokes a synchronous finalizer for either initialized case and returns this result. |

##### Overload: `Result<T, E> Finally<TState>(TState state, Action<TState> action)` on `Result<T, E>`

Invokes a synchronous finalizer for either initialized case and returns this result.

#### Member: `Result<T, E>.FinallyAsync`

**Example**

```csharp
Result<Order, CheckoutError> completed = await order.FinallyAsync(scope, static value => value.DisposeAsync());
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, E>> FinallyAsync<TState>(TState state, Func<TState, ValueTask> action)`](#overload-valuetaskresultt-e-finallyasynctstatetstate-state-functstate-valuetask-action-on-resultt-e) | Invokes an asynchronous finalizer for either initialized case and returns this result. |

##### Overload: `ValueTask<Result<T, E>> FinallyAsync<TState>(TState state, Func<TState, ValueTask> action)` on `Result<T, E>`

Invokes an asynchronous finalizer for either initialized case and returns this result.

#### Member: `Result<T, E>.GetHashCode`

**Example**

```csharp
int hash = result.GetHashCode();
```

| Overload | Description |
| --- | --- |
| [`int GetHashCode()`](#overload-int-gethashcode-on-resultt-e) | Hashes state and only the payload belonging to the active case. |

##### Overload: `int GetHashCode()` on `Result<T, E>`

Hashes state and only the payload belonging to the active case.

#### Member: `Result<T, E>.IsFailure`

**Example**

```csharp
if (result.IsFailure) Record(result.Error);
```

| Overload | Description |
| --- | --- |
| [`bool IsFailure`](#overload-bool-isfailure-on-resultt-e) | Gets whether this result contains an error. |

##### Overload: `bool IsFailure` on `Result<T, E>`

Gets whether this result contains an error.

#### Member: `Result<T, E>.IsInitialized`

**Example**

```csharp
if (!result.IsInitialized) HandleInvalidState();
```

| Overload | Description |
| --- | --- |
| [`bool IsInitialized`](#overload-bool-isinitialized-on-resultt-e) | Gets whether this value was constructed through `Ok` or `Fail`. |

##### Overload: `bool IsInitialized` on `Result<T, E>`

Gets whether this value was constructed through `Ok` or `Fail`.

#### Member: `Result<T, E>.IsSuccess`

**Example**

```csharp
if (result.IsSuccess) Consume(result.Value);
```

| Overload | Description |
| --- | --- |
| [`bool IsSuccess`](#overload-bool-issuccess-on-resultt-e) | Gets whether this result contains a successful value. |

##### Overload: `bool IsSuccess` on `Result<T, E>`

Gets whether this result contains a successful value.

#### Member: `Result<T, E>.Map`

**Example**

```csharp
Result<User, LookupError> normalized = result.Map(static user => user.Normalize());
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Map(Func<T, T> map)`](#overload-resultt-e-mapfunct-t-map-on-resultt-e) | Maps a successful value without changing its type and propagates failures unchanged. |
| [`Result<TR, E> Map<TR>(Func<T, TR> map)`](#overload-resulttr-e-maptrfunct-tr-map-on-resultt-e) | Maps a successful value to another type and propagates failures. |
| [`Result<T, E> Map<TFunction>(TFunction map)`](#overload-resultt-e-maptfunctiontfunction-map-on-resultt-e) | Maps a successful value through an allocation-free callable and propagates failures. |
| [`Result<TR, E> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)`](#overload-resulttr-e-maptr-tfunctionvaluefunctiont-tr-tfunction-map-on-resultt-e) | Maps a successful value through a generated callable wrapper and propagates failures. |
| [`Result<TR, E> Map<TState, TR>(TState state, Func<T, TState, TR> map)`](#overload-resulttr-e-maptstate-trtstate-state-funct-tstate-tr-map-on-resultt-e) | Maps a successful value with caller-owned state and propagates failures. |
| [`Result<TR, E> Map<TR, TFunction>(TFunction map)`](#overload-resulttr-e-maptr-tfunctiontfunction-map-on-resultt-e) | Maps a successful value to another type through an allocation-free callable. |

##### Overload: `Result<T, E> Map(Func<T, T> map)` on `Result<T, E>`

Maps a successful value without changing its type and propagates failures unchanged.

##### Overload: `Result<TR, E> Map<TR>(Func<T, TR> map)` on `Result<T, E>`

Maps a successful value to another type and propagates failures.

##### Overload: `Result<T, E> Map<TFunction>(TFunction map)` on `Result<T, E>`

Maps a successful value through an allocation-free callable and propagates failures.

##### Overload: `Result<TR, E> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)` on `Result<T, E>`

Maps a successful value through a generated callable wrapper and propagates failures.

##### Overload: `Result<TR, E> Map<TState, TR>(TState state, Func<T, TState, TR> map)` on `Result<T, E>`

Maps a successful value with caller-owned state and propagates failures.

##### Overload: `Result<TR, E> Map<TR, TFunction>(TFunction map)` on `Result<T, E>`

Maps a successful value to another type through an allocation-free callable.

#### Member: `Result<T, E>.MapError`

**Example**

```csharp
Result<User, ApiError> mapped = result.MapError(ApiError.FromLookup);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TE> MapError<TE>(Func<E, TE> map)`](#overload-resultt-te-maperrortefunce-te-map-on-resultt-e) | Maps the active error to another non-null type and preserves successes. |
| [`Result<T, TE> MapError<TState, TE>(TState state, Func<E, TState, TE> map)`](#overload-resultt-te-maperrortstate-tetstate-state-funce-tstate-te-map-on-resultt-e) | Maps the active error with caller-owned state and preserves successes. |

##### Overload: `Result<T, TE> MapError<TE>(Func<E, TE> map)` on `Result<T, E>`

Maps the active error to another non-null type and preserves successes.

##### Overload: `Result<T, TE> MapError<TState, TE>(TState state, Func<E, TState, TE> map)` on `Result<T, E>`

Maps the active error with caller-owned state and preserves successes.

#### Member: `Result<T, E>.Match`

**Example**

```csharp
string text = result.Match(static user => user.Name, static error => error.Code);
```

| Overload | Description |
| --- | --- |
| [`TR Match<TR>(Func<T, TR> ok, Func<E, TR> error)`](#overload-tr-matchtrfunct-tr-ok-funce-tr-error-on-resultt-e) | Folds the active case through exactly one branch function. |
| [`TR Match<TState, TR>(TState state, Func<T, TState, TR> ok, Func<E, TState, TR> error)`](#overload-tr-matchtstate-trtstate-state-funct-tstate-tr-ok-funce-tstate-tr-error-on-resultt-e) | Folds the active case while passing caller-owned state to non-capturing branch functions. |
| [`TR Match<TR, TOk, TError>(TOk ok, TError error)`](#overload-tr-matchtr-tok-terrortok-ok-terror-error-on-resultt-e) | Folds the active case through allocation-free callable values. |

##### Overload: `TR Match<TR>(Func<T, TR> ok, Func<E, TR> error)` on `Result<T, E>`

Folds the active case through exactly one branch function.

##### Overload: `TR Match<TState, TR>(TState state, Func<T, TState, TR> ok, Func<E, TState, TR> error)` on `Result<T, E>`

Folds the active case while passing caller-owned state to non-capturing branch functions.

**Type parameters**

- `TState`: Caller state type.
- `TR`: Folded result type.

**Parameters**

- `state`: State passed unchanged to the selected branch.
- `ok`: Success branch.
- `error`: Failure branch.

**Returns:** The value returned by the selected branch.

##### Overload: `TR Match<TR, TOk, TError>(TOk ok, TError error)` on `Result<T, E>`

Folds the active case through allocation-free callable values.

#### Member: `Result<T, E>.Ok`

**Example**

```csharp
Result<User, LookupError> result = Result<User, LookupError>.Ok(user);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Ok(T value)`](#overload-resultt-e-okt-value-on-resultt-e) | Creates a successful result containing `value`. |

##### Overload: `Result<T, E> Ok(T value)` on `Result<T, E>`

Creates a successful result containing `value`.

#### Member: `Result<T, E>.Recover`

**Example**

```csharp
Result<Settings, ReadError> settings = primary.Recover(ReadFallback);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Recover(Func<E, Result<T, E>> recover)`](#overload-resultt-e-recoverfunce-resultt-e-recover-on-resultt-e) | Recovers a failure through `recover` and preserves successes. |
| [`Result<T, E> Recover<TState>(TState state, Func<E, TState, Result<T, E>> recover)`](#overload-resultt-e-recovertstatetstate-state-funce-tstate-resultt-e-recover-on-resultt-e) | Recovers a failure while passing caller-owned state to a non-capturing function. |

##### Overload: `Result<T, E> Recover(Func<E, Result<T, E>> recover)` on `Result<T, E>`

Recovers a failure through `recover` and preserves successes.

##### Overload: `Result<T, E> Recover<TState>(TState state, Func<E, TState, Result<T, E>> recover)` on `Result<T, E>`

Recovers a failure while passing caller-owned state to a non-capturing function.

**Type parameters**

- `TState`: Caller state type.

**Parameters**

- `state`: State passed unchanged to the recovery function.
- `recover`: Function invoked only for a failure.

**Returns:** This success or the recovery result.

#### Member: `Result<T, E>.Switch`

**Example**

```csharp
result.Switch(RenderUser, RenderLookupError);
```

| Overload | Description |
| --- | --- |
| [`void Switch(Action<T> ok, Action<E> error)`](#overload-void-switchactiont-ok-actione-error-on-resultt-e) | Executes exactly one action for the active case. |

##### Overload: `void Switch(Action<T> ok, Action<E> error)` on `Result<T, E>`

Executes exactly one action for the active case.

#### Member: `Result<T, E>.Tap`

**Example**

```csharp
Result<Order, CheckoutError> observed = order.Tap(AuditOrder);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> Tap(Action<T> action)`](#overload-resultt-e-tapactiont-action-on-resultt-e) | Invokes `action` only for success and returns this result. |
| [`Result<T, E> Tap<TAction>(ValueAction<T, TAction> action)`](#overload-resultt-e-taptactionvalueactiont-taction-action-on-resultt-e) | Invokes a generated callable action only for success and returns this result. |
| [`Result<T, E> Tap<TAction>(TAction action)`](#overload-resultt-e-taptactiontaction-action-on-resultt-e) | Invokes an allocation-free action only for success and returns this result. |
| [`Result<T, E> Tap<TState>(TState state, Action<T, TState> action)`](#overload-resultt-e-taptstatetstate-state-actiont-tstate-action-on-resultt-e) | Invokes a success action with caller-owned state and returns this result. |

##### Overload: `Result<T, E> Tap(Action<T> action)` on `Result<T, E>`

Invokes `action` only for success and returns this result.

##### Overload: `Result<T, E> Tap<TAction>(ValueAction<T, TAction> action)` on `Result<T, E>`

Invokes a generated callable action only for success and returns this result.

##### Overload: `Result<T, E> Tap<TAction>(TAction action)` on `Result<T, E>`

Invokes an allocation-free action only for success and returns this result.

##### Overload: `Result<T, E> Tap<TState>(TState state, Action<T, TState> action)` on `Result<T, E>`

Invokes a success action with caller-owned state and returns this result.

#### Member: `Result<T, E>.TapAsync`

**Example**

```csharp
Result<Order, CheckoutError> observed = await order.TapAsync(AuditOrderAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, E>> TapAsync(Func<T, ValueTask> action)`](#overload-valuetaskresultt-e-tapasyncfunct-valuetask-action-on-resultt-e) | Asynchronously invokes `action` only for success. |

##### Overload: `ValueTask<Result<T, E>> TapAsync(Func<T, ValueTask> action)` on `Result<T, E>`

Asynchronously invokes `action` only for success.

#### Member: `Result<T, E>.TapError`

**Example**

```csharp
Result<Order, CheckoutError> observed = order.TapError(RecordFailure);
```

| Overload | Description |
| --- | --- |
| [`Result<T, E> TapError(Action<E> action)`](#overload-resultt-e-taperroractione-action-on-resultt-e) | Invokes `action` only for failure and returns this result. |
| [`Result<T, E> TapError<TAction>(ValueAction<E, TAction> action)`](#overload-resultt-e-taperrortactionvalueactione-taction-action-on-resultt-e) | Invokes a generated callable action only for failure and returns this result. |
| [`Result<T, E> TapError<TAction>(TAction action)`](#overload-resultt-e-taperrortactiontaction-action-on-resultt-e) | Invokes an allocation-free action only for failure and returns this result. |
| [`Result<T, E> TapError<TState>(TState state, Action<E, TState> action)`](#overload-resultt-e-taperrortstatetstate-state-actione-tstate-action-on-resultt-e) | Invokes a failure action with caller-owned state and returns this result. |

##### Overload: `Result<T, E> TapError(Action<E> action)` on `Result<T, E>`

Invokes `action` only for failure and returns this result.

##### Overload: `Result<T, E> TapError<TAction>(ValueAction<E, TAction> action)` on `Result<T, E>`

Invokes a generated callable action only for failure and returns this result.

##### Overload: `Result<T, E> TapError<TAction>(TAction action)` on `Result<T, E>`

Invokes an allocation-free action only for failure and returns this result.

##### Overload: `Result<T, E> TapError<TState>(TState state, Action<E, TState> action)` on `Result<T, E>`

Invokes a failure action with caller-owned state and returns this result.

#### Member: `Result<T, E>.ToString`

**Example**

```csharp
logger.LogDebug("Lookup result: {Result}", result.ToString());
```

| Overload | Description |
| --- | --- |
| [`string ToString()`](#overload-string-tostring-on-resultt-e) | Returns `Ok(value)`, `Fail(error)`, or `Uninitialized`. |

##### Overload: `string ToString()` on `Result<T, E>`

Returns `Ok(value)`, `Fail(error)`, or `Uninitialized`.

#### Member: `Result<T, E>.TryGetError`

**Example**

```csharp
if (result.TryGetError(out LookupError error)) Record(error);
```

| Overload | Description |
| --- | --- |
| [`bool TryGetError(out E error)`](#overload-bool-trygeterrorout-e-error-on-resultt-e) | Attempts to retrieve the failure error and rejects an uninitialized result. |

##### Overload: `bool TryGetError(out E error)` on `Result<T, E>`

Attempts to retrieve the failure error and rejects an uninitialized result.

#### Member: `Result<T, E>.TryGetValue`

**Example**

```csharp
if (result.TryGetValue(out User user)) Consume(user);
```

| Overload | Description |
| --- | --- |
| [`bool TryGetValue(out T value)`](#overload-bool-trygetvalueout-t-value-on-resultt-e) | Attempts to retrieve the successful value and rejects an uninitialized result. |

##### Overload: `bool TryGetValue(out T value)` on `Result<T, E>`

Attempts to retrieve the successful value and rejects an uninitialized result.

#### Member: `Result<T, E>.Value`

**Example**

```csharp
User user = result.Value;
```

| Overload | Description |
| --- | --- |
| [`T Value`](#overload-t-value-on-resultt-e) | Gets the successful value. |

##### Overload: `T Value` on `Result<T, E>`

Gets the successful value.

**Throws**

- `InvalidOperationException`: The result is failed or uninitialized.

#### Member: `Result<T, E>.ValueOr`

**Example**

```csharp
User user = result.ValueOr(User.Anonymous);
```

| Overload | Description |
| --- | --- |
| [`T ValueOr(T fallback)`](#overload-t-valueort-fallback-on-resultt-e) | Returns the success value or an eagerly supplied fallback. |

##### Overload: `T ValueOr(T fallback)` on `Result<T, E>`

Returns the success value or an eagerly supplied fallback.

**Parameters**

- `fallback`: Value returned for a failure.

**Returns:** The success value or `fallback`.

#### Member: `Result<T, E>.ValueOrElse`

**Example**

```csharp
User user = result.ValueOrElse(static error => User.Missing(error.Code));
```

| Overload | Description |
| --- | --- |
| [`T ValueOrElse(Func<E, T> fallback)`](#overload-t-valueorelsefunce-t-fallback-on-resultt-e) | Returns the success value or lazily maps the active error to a fallback. |
| [`T ValueOrElse<TState>(TState state, Func<E, TState, T> fallback)`](#overload-t-valueorelsetstatetstate-state-funce-tstate-t-fallback-on-resultt-e) | Returns the success value or maps the active error with caller-owned state. |

##### Overload: `T ValueOrElse(Func<E, T> fallback)` on `Result<T, E>`

Returns the success value or lazily maps the active error to a fallback.

**Parameters**

- `fallback`: Function invoked only for a failure.

**Returns:** The success value or the fallback value.

##### Overload: `T ValueOrElse<TState>(TState state, Func<E, TState, T> fallback)` on `Result<T, E>`

Returns the success value or maps the active error with caller-owned state.

**Type parameters**

- `TState`: Caller state type.

**Parameters**

- `state`: State passed unchanged to the fallback function.
- `fallback`: Function invoked only for a failure.

**Returns:** The success value or the fallback value.

#### Member: `Result<T, E>.implicit operator`

**Example**

```csharp
Result<User, LookupError> result = user;
```

| Overload | Description |
| --- | --- |
| [`implicit operator Result<T, E>(T value)`](#overload-implicit-operator-resultt-et-value-on-resultt-e) | Converts a success value into a successful result. |
| [`implicit operator Result<T, E>(E error)`](#overload-implicit-operator-resultt-ee-error-on-resultt-e) | Converts an error into a failed result. |

##### Overload: `implicit operator Result<T, E>(T value)` on `Result<T, E>`

Converts a success value into a successful result.

##### Overload: `implicit operator Result<T, E>(E error)` on `Result<T, E>`

Converts an error into a failed result.

### Type: `ResultCombination`

Combines independent results using fail-fast semantics.

| Member | Description |
| --- | --- |
| [`Bind`](#member-resultcombinationbind) | Binds two independent success values and returns the first failure. |
| [`Combine`](#member-resultcombinationcombine) | Combines two unit results and returns the first failure in argument order. |
| [`Map`](#member-resultcombinationmap) | Projects two success values directly and returns the first failure. |
| [`Zip`](#member-resultcombinationzip) | Combines two success values into a value tuple and returns the first failure. |

#### Member: `ResultCombination.Bind`

**Example**

```csharp
Result<Invoice, LoadError> invoice = ResultCombination.Bind(userResult, accountResult, Invoice.Create);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TError> Bind<TFirst, TSecond, TResult, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second, Func<TFirst, TSecond, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindtfirst-tsecond-tresult-terrorin-resulttfirst-terror-first-in-resulttsecond-terror-second-functfirst-tsecond-resulttresult-terror-bind-on-resultcombination) | Binds two independent success values and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, Func<T1, T2, T3, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-funct1-t2-t3-resulttresult-terror-bind-on-resultcombination) | Binds three independent success values and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, TState state, Func<T1, T2, TState, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-tstate-state-funct1-t2-tstate-resulttresult-terror-bind-on-resultcombination) | Binds 2 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, Func<T1, T2, T3, T4, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-funct1-t2-t3-t4-resulttresult-terror-bind-on-resultcombination) | Binds four independent success values and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, TState state, Func<T1, T2, T3, TState, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-tstate-state-funct1-t2-t3-tstate-resulttresult-terror-bind-on-resultcombination) | Binds 3 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, Func<T1, T2, T3, T4, T5, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-t5-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-funct1-t2-t3-t4-t5-resulttresult-terror-bind-on-resultcombination) | Binds five independent success values and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, TState state, Func<T1, T2, T3, T4, TState, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-tstate-state-funct1-t2-t3-t4-tstate-resulttresult-terror-bind-on-resultcombination) | Binds 4 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, Func<T1, T2, T3, T4, T5, T6, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-t5-t6-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-in-resultt6-terror-sixth-funct1-t2-t3-t4-t5-t6-resulttresult-terror-bind-on-resultcombination) | Binds six independent success values and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, TState state, Func<T1, T2, T3, T4, T5, TState, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-t5-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-tstate-state-funct1-t2-t3-t4-t5-tstate-resulttresult-terror-bind-on-resultcombination) | Binds 5 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, TState state, Func<T1, T2, T3, T4, T5, T6, TState, Result<TResult, TError>> bind)`](#overload-resulttresult-terror-bindt1-t2-t3-t4-t5-t6-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-in-resultt6-terror-sixth-tstate-state-funct1-t2-t3-t4-t5-t6-tstate-resulttresult-terror-bind-on-resultcombination) | Binds 6 independent success values with caller-owned state and returns the first failure. |

##### Overload: `Result<TResult, TError> Bind<TFirst, TSecond, TResult, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second, Func<TFirst, TSecond, Result<TResult, TError>> bind)` on `ResultCombination`

Binds two independent success values and returns the first failure.

**Type parameters**

- `TFirst`: First success type.
- `TSecond`: Second success type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: First result.
- `second`: Second result.
- `bind`: Binding function invoked only when both inputs succeed.

**Returns:** The bound result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, Func<T1, T2, T3, Result<TResult, TError>> bind)` on `ResultCombination`

Binds three independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Bind<T1, T2, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, TState state, Func<T1, T2, TState, Result<TResult, TError>> bind)` on `ResultCombination`

Binds 2 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `TState`: Caller state type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `state`: State passed unchanged to the bind function.
- `bind`: Function invoked only when every input succeeds.

**Returns:** The bound result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, Func<T1, T2, T3, T4, Result<TResult, TError>> bind)` on `ResultCombination`

Binds four independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, TState state, Func<T1, T2, T3, TState, Result<TResult, TError>> bind)` on `ResultCombination`

Binds 3 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `TState`: Caller state type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `state`: State passed unchanged to the bind function.
- `bind`: Function invoked only when every input succeeds.

**Returns:** The bound result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, Func<T1, T2, T3, T4, T5, Result<TResult, TError>> bind)` on `ResultCombination`

Binds five independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, TState state, Func<T1, T2, T3, T4, TState, Result<TResult, TError>> bind)` on `ResultCombination`

Binds 4 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `TState`: Caller state type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `state`: State passed unchanged to the bind function.
- `bind`: Function invoked only when every input succeeds.

**Returns:** The bound result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, Func<T1, T2, T3, T4, T5, T6, Result<TResult, TError>> bind)` on `ResultCombination`

Binds six independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, TState state, Func<T1, T2, T3, T4, T5, TState, Result<TResult, TError>> bind)` on `ResultCombination`

Binds 5 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `T5`: Input 5 success type.
- `TState`: Caller state type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `fifth`: Input 5 result.
- `state`: State passed unchanged to the bind function.
- `bind`: Function invoked only when every input succeeds.

**Returns:** The bound result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, TState state, Func<T1, T2, T3, T4, T5, T6, TState, Result<TResult, TError>> bind)` on `ResultCombination`

Binds 6 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `T5`: Input 5 success type.
- `T6`: Input 6 success type.
- `TState`: Caller state type.
- `TResult`: Bound success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `fifth`: Input 5 result.
- `sixth`: Input 6 result.
- `state`: State passed unchanged to the bind function.
- `bind`: Function invoked only when every input succeeds.

**Returns:** The bound result or the first input failure in argument order.

#### Member: `ResultCombination.Combine`

**Example**

```csharp
Result<Unit, ValidationError> valid = ResultCombination.Combine(nameCheck, emailCheck);
```

| Overload | Description |
| --- | --- |
| [`Result<Unit, TError> Combine<TError>(in Result<Unit, TError> first, in Result<Unit, TError> second)`](#overload-resultunit-terror-combineterrorin-resultunit-terror-first-in-resultunit-terror-second-on-resultcombination) | Combines two unit results and returns the first failure in argument order. |
| [`Result<Unit, TError> Combine<TError>(ReadOnlySpan<Result<Unit, TError>> results)`](#overload-resultunit-terror-combineterrorreadonlyspanresultunit-terror-results-on-resultcombination) | Combines a span of unit results and returns the first failure in span order. |

##### Overload: `Result<Unit, TError> Combine<TError>(in Result<Unit, TError> first, in Result<Unit, TError> second)` on `ResultCombination`

Combines two unit results and returns the first failure in argument order.

**Type parameters**

- `TError`: Failure type.

**Parameters**

- `first`: First result.
- `second`: Second result.

**Returns:** Success when both inputs succeed; otherwise the first failure.

##### Overload: `Result<Unit, TError> Combine<TError>(ReadOnlySpan<Result<Unit, TError>> results)` on `ResultCombination`

Combines a span of unit results and returns the first failure in span order.

**Type parameters**

- `TError`: Failure type.

**Parameters**

- `results`: Results to inspect exactly once.

**Returns:** Success when every input succeeds; otherwise the first failure.

#### Member: `ResultCombination.Map`

**Example**

```csharp
Result<Invoice, LoadError> invoice = ResultCombination.Map(userResult, accountResult, static (user, account) => new Invoice(user, account));
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TError> Map<TFirst, TSecond, TResult, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second, Func<TFirst, TSecond, TResult> map)`](#overload-resulttresult-terror-maptfirst-tsecond-tresult-terrorin-resulttfirst-terror-first-in-resulttsecond-terror-second-functfirst-tsecond-tresult-map-on-resultcombination) | Projects two success values directly and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, Func<T1, T2, T3, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-funct1-t2-t3-tresult-map-on-resultcombination) | Projects three independent success values and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, TState state, Func<T1, T2, TState, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-tstate-state-funct1-t2-tstate-tresult-map-on-resultcombination) | Projects 2 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, Func<T1, T2, T3, T4, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-funct1-t2-t3-t4-tresult-map-on-resultcombination) | Projects four independent success values and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, TState state, Func<T1, T2, T3, TState, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-tstate-state-funct1-t2-t3-tstate-tresult-map-on-resultcombination) | Projects 3 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, T5, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, Func<T1, T2, T3, T4, T5, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-t5-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-funct1-t2-t3-t4-t5-tresult-map-on-resultcombination) | Projects five independent success values and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, TState state, Func<T1, T2, T3, T4, TState, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-tstate-state-funct1-t2-t3-t4-tstate-tresult-map-on-resultcombination) | Projects 4 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, Func<T1, T2, T3, T4, T5, T6, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-t5-t6-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-in-resultt6-terror-sixth-funct1-t2-t3-t4-t5-t6-tresult-map-on-resultcombination) | Projects six independent success values and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, T5, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, TState state, Func<T1, T2, T3, T4, T5, TState, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-t5-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-tstate-state-funct1-t2-t3-t4-t5-tstate-tresult-map-on-resultcombination) | Projects 5 independent success values with caller-owned state and returns the first failure. |
| [`Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, TState state, Func<T1, T2, T3, T4, T5, T6, TState, TResult> map)`](#overload-resulttresult-terror-mapt1-t2-t3-t4-t5-t6-tstate-tresult-terrorin-resultt1-terror-first-in-resultt2-terror-second-in-resultt3-terror-third-in-resultt4-terror-fourth-in-resultt5-terror-fifth-in-resultt6-terror-sixth-tstate-state-funct1-t2-t3-t4-t5-t6-tstate-tresult-map-on-resultcombination) | Projects 6 independent success values with caller-owned state and returns the first failure. |

##### Overload: `Result<TResult, TError> Map<TFirst, TSecond, TResult, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second, Func<TFirst, TSecond, TResult> map)` on `ResultCombination`

Projects two success values directly and returns the first failure.

**Type parameters**

- `TFirst`: First success type.
- `TSecond`: Second success type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: First result.
- `second`: Second result.
- `map`: Projection invoked only when both inputs succeed.

**Returns:** The projected success or the first failure in argument order.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, Func<T1, T2, T3, TResult> map)` on `ResultCombination`

Projects three independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Map<T1, T2, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, TState state, Func<T1, T2, TState, TResult> map)` on `ResultCombination`

Projects 2 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `TState`: Caller state type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `state`: State passed unchanged to the map function.
- `map`: Function invoked only when every input succeeds.

**Returns:** The projected result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, Func<T1, T2, T3, T4, TResult> map)` on `ResultCombination`

Projects four independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, TState state, Func<T1, T2, T3, TState, TResult> map)` on `ResultCombination`

Projects 3 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `TState`: Caller state type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `state`: State passed unchanged to the map function.
- `map`: Function invoked only when every input succeeds.

**Returns:** The projected result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, T5, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, Func<T1, T2, T3, T4, T5, TResult> map)` on `ResultCombination`

Projects five independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, TState state, Func<T1, T2, T3, T4, TState, TResult> map)` on `ResultCombination`

Projects 4 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `TState`: Caller state type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `state`: State passed unchanged to the map function.
- `map`: Function invoked only when every input succeeds.

**Returns:** The projected result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, Func<T1, T2, T3, T4, T5, T6, TResult> map)` on `ResultCombination`

Projects six independent success values and returns the first failure.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, T5, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, TState state, Func<T1, T2, T3, T4, T5, TState, TResult> map)` on `ResultCombination`

Projects 5 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `T5`: Input 5 success type.
- `TState`: Caller state type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `fifth`: Input 5 result.
- `state`: State passed unchanged to the map function.
- `map`: Function invoked only when every input succeeds.

**Returns:** The projected result or the first input failure in argument order.

##### Overload: `Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(in Result<T1, TError> first, in Result<T2, TError> second, in Result<T3, TError> third, in Result<T4, TError> fourth, in Result<T5, TError> fifth, in Result<T6, TError> sixth, TState state, Func<T1, T2, T3, T4, T5, T6, TState, TResult> map)` on `ResultCombination`

Projects 6 independent success values with caller-owned state and returns the first failure.

**Type parameters**

- `T1`: Input 1 success type.
- `T2`: Input 2 success type.
- `T3`: Input 3 success type.
- `T4`: Input 4 success type.
- `T5`: Input 5 success type.
- `T6`: Input 6 success type.
- `TState`: Caller state type.
- `TResult`: Projected success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: Input 1 result.
- `second`: Input 2 result.
- `third`: Input 3 result.
- `fourth`: Input 4 result.
- `fifth`: Input 5 result.
- `sixth`: Input 6 result.
- `state`: State passed unchanged to the map function.
- `map`: Function invoked only when every input succeeds.

**Returns:** The projected result or the first input failure in argument order.

#### Member: `ResultCombination.Zip`

**Example**

```csharp
Result<(User First, Account Second), LoadError> loaded = ResultCombination.Zip(userResult, accountResult);
```

| Overload | Description |
| --- | --- |
| [`Result<ValueTuple<TFirst, TSecond>, TError> Zip<TFirst, TSecond, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second)`](#overload-resultvaluetupletfirst-tsecond-terror-ziptfirst-tsecond-terrorin-resulttfirst-terror-first-in-resulttsecond-terror-second-on-resultcombination) | Combines two success values into a value tuple and returns the first failure. |

##### Overload: `Result<ValueTuple<TFirst, TSecond>, TError> Zip<TFirst, TSecond, TError>(in Result<TFirst, TError> first, in Result<TSecond, TError> second)` on `ResultCombination`

Combines two success values into a value tuple and returns the first failure.

**Type parameters**

- `TFirst`: First success type.
- `TSecond`: Second success type.
- `TError`: Shared failure type.

**Parameters**

- `first`: First result.
- `second`: Second result.

**Returns:** A tuple of both values or the first failure in argument order.

### Type: `ResultCompositionExtensions`

Provides composition between nested `Result` and `Option` values.

| Member | Description |
| --- | --- |
| [`Flatten`](#member-resultcompositionextensionsflatten) | Removes one result layer while preserving the first failure encountered. |
| [`RequireSome`](#member-resultcompositionextensionsrequiresome) | Requires a successful option to contain a value. |
| [`ToResult`](#member-resultcompositionextensionstoresult) | Converts an option to a result using a lazy absence error factory. |
| [`Transpose`](#member-resultcompositionextensionstranspose) | Exchanges the option and result layers while treating absence as a successful absence. |
| [`Traverse`](#member-resultcompositionextensionstraverse) | Traverses a present value through a fallible selector and preserves absence. |

#### Member: `ResultCompositionExtensions.Flatten`

**Example**

```csharp
Result<User, LookupError> flat = nestedResult.Flatten();
```

| Overload | Description |
| --- | --- |
| [`Result<T, TError> Flatten<T, TError>(in Result<Result<T, TError>, TError> result)`](#overload-resultt-terror-flattent-terrorin-resultresultt-terror-terror-result-on-resultcompositionextensions) | Removes one result layer while preserving the first failure encountered. |

##### Overload: `Result<T, TError> Flatten<T, TError>(in Result<Result<T, TError>, TError> result)` on `ResultCompositionExtensions`

Removes one result layer while preserving the first failure encountered.

**Returns:** The nested success result or the outer failure.

#### Member: `ResultCompositionExtensions.RequireSome`

**Example**

```csharp
Result<User, LookupError> required = result.RequireSome(static () => LookupError.NotFound);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TError> RequireSome<T, TError>(in Result<Option<T>, TError> result, Func<TError> whenNone)`](#overload-resultt-terror-requiresomet-terrorin-resultoptiont-terror-result-functerror-whennone-on-resultcompositionextensions) | Requires a successful option to contain a value. |
| [`Result<T, TError> RequireSome<T, TError>(in Result<Option<T>, TError> result, TError whenNone)`](#overload-resultt-terror-requiresomet-terrorin-resultoptiont-terror-result-terror-whennone-on-resultcompositionextensions) | Requires a successful option to contain a value using an eagerly supplied error. |
| [`Result<T, TError> RequireSome<T, TError, TState>(in Result<Option<T>, TError> result, TState state, Func<TState, TError> whenNone)`](#overload-resultt-terror-requiresomet-terror-tstatein-resultoptiont-terror-result-tstate-state-functstate-terror-whennone-on-resultcompositionextensions) | Requires a successful option to contain a value using a caller-state error factory. |

##### Overload: `Result<T, TError> RequireSome<T, TError>(in Result<Option<T>, TError> result, Func<TError> whenNone)` on `ResultCompositionExtensions`

Requires a successful option to contain a value.

**Parameters**

- `whenNone`: Error factory invoked only for a successful absent option.

**Returns:** The contained value, the original failure, or the generated absence failure.

##### Overload: `Result<T, TError> RequireSome<T, TError>(in Result<Option<T>, TError> result, TError whenNone)` on `ResultCompositionExtensions`

Requires a successful option to contain a value using an eagerly supplied error.

**Parameters**

- `whenNone`: Failure used only for a successful absent option.

**Returns:** The contained value, the original failure, or the supplied absence failure.

##### Overload: `Result<T, TError> RequireSome<T, TError, TState>(in Result<Option<T>, TError> result, TState state, Func<TState, TError> whenNone)` on `ResultCompositionExtensions`

Requires a successful option to contain a value using a caller-state error factory.

**Type parameters**

- `TState`: Caller state type.

**Parameters**

- `state`: State passed unchanged to the error factory.
- `whenNone`: Factory invoked only for a successful absent option.

**Returns:** The contained value, the original failure, or the generated absence failure.

#### Member: `ResultCompositionExtensions.ToResult`

**Example**

```csharp
Result<User, LookupError> required = option.ToResult(LookupError.NotFound);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TError> ToResult<T, TError>(in Option<T> option, Func<TError> whenNone)`](#overload-resultt-terror-toresultt-terrorin-optiont-option-functerror-whennone-on-resultcompositionextensions) | Converts an option to a result using a lazy absence error factory. |
| [`Result<T, TError> ToResult<T, TError>(in Option<T> option, TError whenNone)`](#overload-resultt-terror-toresultt-terrorin-optiont-option-terror-whennone-on-resultcompositionextensions) | Converts an option to a result using an eagerly supplied absence error. |

##### Overload: `Result<T, TError> ToResult<T, TError>(in Option<T> option, Func<TError> whenNone)` on `ResultCompositionExtensions`

Converts an option to a result using a lazy absence error factory.

**Type parameters**

- `TError`: Failure type.

**Parameters**

- `whenNone`: Factory invoked only for None.

**Returns:** Success containing the present value or the generated failure.

##### Overload: `Result<T, TError> ToResult<T, TError>(in Option<T> option, TError whenNone)` on `ResultCompositionExtensions`

Converts an option to a result using an eagerly supplied absence error.

**Type parameters**

- `TError`: Failure type.

**Parameters**

- `whenNone`: Failure returned for None.

**Returns:** Success containing the present value or the supplied failure.

#### Member: `ResultCompositionExtensions.Transpose`

**Example**

```csharp
Result<Option<User>, LookupError> transposed = option.Transpose();
```

| Overload | Description |
| --- | --- |
| [`Result<Option<T>, TError> Transpose<T, TError>(in Option<Result<T, TError>> option)`](#overload-resultoptiont-terror-transposet-terrorin-optionresultt-terror-option-on-resultcompositionextensions) | Exchanges the option and result layers while treating absence as a successful absence. |
| [`Option<Result<T, TError>> Transpose<T, TError>(in Result<Option<T>, TError> result)`](#overload-optionresultt-terror-transposet-terrorin-resultoptiont-terror-result-on-resultcompositionextensions) | Exchanges the result and option layers without losing a failure. |

##### Overload: `Result<Option<T>, TError> Transpose<T, TError>(in Option<Result<T, TError>> option)` on `ResultCompositionExtensions`

Exchanges the option and result layers while treating absence as a successful absence.

**Returns:** The contained result with its success wrapped in an option, or a successful None.

##### Overload: `Option<Result<T, TError>> Transpose<T, TError>(in Result<Option<T>, TError> result)` on `ResultCompositionExtensions`

Exchanges the result and option layers without losing a failure.

**Returns:** None for a successful absent value, Some containing success for a present value, or Some containing the original failure.

#### Member: `ResultCompositionExtensions.Traverse`

**Example**

```csharp
Result<Option<Address>, LookupError> address = option.Traverse(LoadAddress);
```

| Overload | Description |
| --- | --- |
| [`Result<Option<TResult>, TError> Traverse<TSource, TResult, TError>(in Option<TSource> option, Func<TSource, Result<TResult, TError>> selector)`](#overload-resultoptiontresult-terror-traversetsource-tresult-terrorin-optiontsource-option-functsource-resulttresult-terror-selector-on-resultcompositionextensions) | Traverses a present value through a fallible selector and preserves absence. |
| [`Result<Option<TResult>, TError> Traverse<TSource, TResult, TError, TFunction>(in Option<TSource> option, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)`](#overload-resultoptiontresult-terror-traversetsource-tresult-terror-tfunctionin-optiontsource-option-valuefunctiontsource-resulttresult-terror-tfunction-selector-on-resultcompositionextensions) | Traverses Some through a generated callable wrapper and preserves None. |
| [`Result<Option<TResult>, TError> Traverse<TSource, TState, TResult, TError>(in Option<TSource> option, TState state, Func<TSource, TState, Result<TResult, TError>> selector)`](#overload-resultoptiontresult-terror-traversetsource-tstate-tresult-terrorin-optiontsource-option-tstate-state-functsource-tstate-resulttresult-terror-selector-on-resultcompositionextensions) | Traverses Some with caller-owned state and preserves None. |
| [`Result<Option<TResult>, TError> Traverse<TSource, TResult, TError, TFunction>(in Option<TSource> option, TFunction selector)`](#overload-resultoptiontresult-terror-traversetsource-tresult-terror-tfunctionin-optiontsource-option-tfunction-selector-on-resultcompositionextensions) | Traverses Some through an allocation-free callable and preserves None. |

##### Overload: `Result<Option<TResult>, TError> Traverse<TSource, TResult, TError>(in Option<TSource> option, Func<TSource, Result<TResult, TError>> selector)` on `ResultCompositionExtensions`

Traverses a present value through a fallible selector and preserves absence.

**Type parameters**

- `TResult`: Selected success type.
- `TError`: Failure type.

**Parameters**

- `selector`: Selector invoked only for Some.

**Returns:** A failed selector result, Some containing its success, or successful None.

##### Overload: `Result<Option<TResult>, TError> Traverse<TSource, TResult, TError, TFunction>(in Option<TSource> option, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)` on `ResultCompositionExtensions`

Traverses Some through a generated callable wrapper and preserves None.

**Type parameters**

- `TResult`: Selected success type.
- `TError`: Failure type.
- `TFunction`: Wrapped value-function type.

**Parameters**

- `selector`: Selector invoked only for Some.

**Returns:** A failed selector result, Some containing its success, or successful None.

##### Overload: `Result<Option<TResult>, TError> Traverse<TSource, TState, TResult, TError>(in Option<TSource> option, TState state, Func<TSource, TState, Result<TResult, TError>> selector)` on `ResultCompositionExtensions`

Traverses Some with caller-owned state and preserves None.

**Type parameters**

- `TState`: Caller state type.
- `TResult`: Selected success type.
- `TError`: Failure type.

**Parameters**

- `state`: State passed unchanged to the selector.
- `selector`: Selector invoked only for Some.

**Returns:** A failed selector result, Some containing its success, or successful None.

##### Overload: `Result<Option<TResult>, TError> Traverse<TSource, TResult, TError, TFunction>(in Option<TSource> option, TFunction selector)` on `ResultCompositionExtensions`

Traverses Some through an allocation-free callable and preserves None.

**Type parameters**

- `TResult`: Selected success type.
- `TError`: Failure type.
- `TFunction`: Value-function type.

**Parameters**

- `selector`: Selector invoked only for Some.

**Returns:** A failed selector result, Some containing its success, or successful None.

### Type: `Unit`

Represents the single possible value of a successful operation that does not return data. It is the C# equivalent of Rust's unit value, `()`.

| Member | Description |
| --- | --- |
| [`ToString`](#member-unittostring) | Returns the canonical unit representation. |
| [`Value`](#member-unitvalue) | Gets the sole unit value. |

#### Member: `Unit.ToString`

**Example**

```csharp
string text = Unit.Value.ToString();
```

| Overload | Description |
| --- | --- |
| [`string ToString()`](#overload-string-tostring-on-unit) | Returns the canonical unit representation. |

##### Overload: `string ToString()` on `Unit`

Returns the canonical unit representation.

#### Member: `Unit.Value`

**Example**

```csharp
Unit completed = Unit.Value;
```

| Overload | Description |
| --- | --- |
| [`Unit Value`](#overload-unit-value-on-unit) | Gets the sole unit value. |

##### Overload: `Unit Value` on `Unit`

Gets the sole unit value.

### Type: `ValueAction<T, TAction>`

Carries an action's input type so generated call sites remain inferable.

**Example**

```csharp
ValueAction<Error, Observe> action = new(default);
```

| Member | Description |
| --- | --- |
| [`ValueAction`](#member-valueactiont-tactionvalueaction) | Carries an action's input type so generated call sites remain inferable. |
| [`Invoke`](#member-valueactiont-tactioninvoke) | Forwards `value` to the wrapped value action. |

#### Member: `ValueAction<T, TAction>.ValueAction`

**Example**

```csharp
ValueAction<Error, Observe> action = new(default);
```

| Overload | Description |
| --- | --- |
| [`ValueAction<T, TAction>(TAction action)`](#overload-valueactiont-tactiontaction-action-on-valueactiont-taction) | Carries an action's input type so generated call sites remain inferable. |

##### Overload: `ValueAction<T, TAction>(TAction action)` on `ValueAction<T, TAction>`

Carries an action's input type so generated call sites remain inferable.

**Parameters**

- `action`: Callable action value to wrap.

#### Member: `ValueAction<T, TAction>.Invoke`

**Example**

```csharp
Operations.Functions.Observe.Invoke(error);
```

| Overload | Description |
| --- | --- |
| [`void Invoke(T value)`](#overload-void-invoket-value-on-valueactiont-taction) | Forwards `value` to the wrapped value action. |

##### Overload: `void Invoke(T value)` on `ValueAction<T, TAction>`

Forwards `value` to the wrapped value action.

**Parameters**

- `value`: Input value.

### Type: `ValueFunction<TIn, TOut, TFunction>`

Carries the complete input, output, and implementation types of an allocation-free callable so generic consumers can infer every type.

**Example**

```csharp
ValueFunction<User, int, GetId> function = new(default);
```

| Member | Description |
| --- | --- |
| [`ValueFunction`](#member-valuefunctiontin-tout-tfunctionvaluefunction) | Carries the complete input, output, and implementation types of an allocation-free callable so generic consumers can infer every type. |
| [`Invoke`](#member-valuefunctiontin-tout-tfunctioninvoke) | Forwards `value` to the wrapped value function. |

#### Member: `ValueFunction<TIn, TOut, TFunction>.ValueFunction`

**Example**

```csharp
ValueFunction<User, int, GetId> function = new(default);
```

| Overload | Description |
| --- | --- |
| [`ValueFunction<TIn, TOut, TFunction>(TFunction function)`](#overload-valuefunctiontin-tout-tfunctiontfunction-function-on-valuefunctiontin-tout-tfunction) | Carries the complete input, output, and implementation types of an allocation-free callable so generic consumers can infer every type. |

##### Overload: `ValueFunction<TIn, TOut, TFunction>(TFunction function)` on `ValueFunction<TIn, TOut, TFunction>`

Carries the complete input, output, and implementation types of an allocation-free callable so generic consumers can infer every type.

**Parameters**

- `function`: Callable value to wrap.

#### Member: `ValueFunction<TIn, TOut, TFunction>.Invoke`

**Example**

```csharp
int id = Operations.Functions.GetId.Invoke(user);
```

| Overload | Description |
| --- | --- |
| [`TOut Invoke(TIn value)`](#overload-tout-invoketin-value-on-valuefunctiontin-tout-tfunction) | Forwards `value` to the wrapped value function. |

##### Overload: `TOut Invoke(TIn value)` on `ValueFunction<TIn, TOut, TFunction>`

Forwards `value` to the wrapped value function.

**Parameters**

- `value`: Input value.

**Returns:** The transformed output.


## Package MonadicTypes.NET.AspNetCore

**Types:** [`DefaultErrorHttpResultMapper`](#type-defaulterrorhttpresultmapper) · [`ErrorCatalogEntry`](#type-errorcatalogentry) · [`ErrorCatalogMetadata`](#type-errorcatalogmetadata) · [`ErrorEndpointConventionExtensions`](#type-errorendpointconventionextensions) · [`ErrorProblemDetails`](#type-errorproblemdetails) · [`IHttpResultMapper<TError, TResult>`](#type-ihttpresultmapperterror-tresult) · [`ProducesErrorAttribute`](#type-produceserrorattribute) · [`ProducesErrorCatalogAttribute`](#type-produceserrorcatalogattribute) · [`ResultHttpExtensions`](#type-resulthttpextensions) · [`ValidationErrorProblemDetails`](#type-validationerrorproblemdetails)

### Type: `DefaultErrorHttpResultMapper`

Maps structured errors to the library's default RFC 9457 HTTP result.

**Example**

```csharp
ProblemHttpResult response = default(DefaultErrorHttpResultMapper).Map(error, httpContext);
```

| Member | Description |
| --- | --- |
| [`Map`](#member-defaulterrorhttpresultmappermap) | Maps `failure` to a strongly typed problem result. |

#### Member: `DefaultErrorHttpResultMapper.Map`

**Example**

```csharp
ProblemHttpResult response = mapper.Map(error, httpContext);
```

| Overload | Description |
| --- | --- |
| [`ProblemHttpResult Map(in Error failure, HttpContext? httpContext)`](#overload-problemhttpresult-mapin-error-failure-httpcontext-httpcontext-on-defaulterrorhttpresultmapper) | Maps `failure` to a strongly typed problem result. |

##### Overload: `ProblemHttpResult Map(in Error failure, HttpContext? httpContext)` on `DefaultErrorHttpResultMapper`

Maps `failure` to a strongly typed problem result.

**Parameters**

- `failure`: Error to map.
- `httpContext`: Optional request context included in the problem payload.

**Returns:** The mapped problem result.

### Type: `ErrorCatalogEntry`

Describes one stable, publicly documented error returned by an endpoint. This value is metadata only and is never created while handling a request.

**Example**

```csharp
ErrorCatalogEntry entry = new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.");
```

| Member | Description |
| --- | --- |
| [`ErrorCatalogEntry`](#member-errorcatalogentryerrorcatalogentry) | Creates one documented error entry. |
| [`Code`](#member-errorcatalogentrycode) | Gets the stable machine-readable error code. |
| [`Description`](#member-errorcatalogentrydescription) | Gets the public description emitted into API documentation. |
| [`StatusCode`](#member-errorcatalogentrystatuscode) | Gets the explicit HTTP status override, or null to use the category's default mapping. |
| [`Type`](#member-errorcatalogentrytype) | Gets the application category used for the default HTTP mapping when no status override is supplied. |

#### Member: `ErrorCatalogEntry.ErrorCatalogEntry`

**Example**

```csharp
ErrorCatalogEntry entry = new(ErrorType.Conflict, "VERSION_CONFLICT", "Resource changed.");
```

| Overload | Description |
| --- | --- |
| [`ErrorCatalogEntry(ErrorType type, string code, string description)`](#overload-errorcatalogentryerrortype-type-string-code-string-description-on-errorcatalogentry) | Creates one documented error entry. |
| [`ErrorCatalogEntry(ErrorType type, string code, string description, int statusCode)`](#overload-errorcatalogentryerrortype-type-string-code-string-description-int-statuscode-on-errorcatalogentry) | Creates a documented error with an explicit HTTP error status. |

##### Overload: `ErrorCatalogEntry(ErrorType type, string code, string description)` on `ErrorCatalogEntry`

Creates one documented error entry.

**Parameters**

- `type`: The initialized category that determines the HTTP status.
- `code`: The stable machine-readable error code.
- `description`: The public description exposed in API documentation.

##### Overload: `ErrorCatalogEntry(ErrorType type, string code, string description, int statusCode)` on `ErrorCatalogEntry`

Creates a documented error with an explicit HTTP error status.

**Parameters**

- `type`: The initialized application error category.
- `code`: The stable machine-readable error code.
- `description`: The public description exposed in API documentation.
- `statusCode`: HTTP error status from 400 through 599.

#### Member: `ErrorCatalogEntry.Code`

**Example**

```csharp
string code = entry.Code;
```

| Overload | Description |
| --- | --- |
| [`string Code`](#overload-string-code-on-errorcatalogentry) | Gets the stable machine-readable error code. |

##### Overload: `string Code` on `ErrorCatalogEntry`

Gets the stable machine-readable error code.

#### Member: `ErrorCatalogEntry.Description`

**Example**

```csharp
string description = entry.Description;
```

| Overload | Description |
| --- | --- |
| [`string Description`](#overload-string-description-on-errorcatalogentry) | Gets the public description emitted into API documentation. |

##### Overload: `string Description` on `ErrorCatalogEntry`

Gets the public description emitted into API documentation.

#### Member: `ErrorCatalogEntry.StatusCode`

**Example**

```csharp
int status = entry.StatusCode ?? ErrorProblemDetails.GetStatusCode(entry.Type);
```

| Overload | Description |
| --- | --- |
| [`Nullable<int> StatusCode`](#overload-nullableint-statuscode-on-errorcatalogentry) | Gets the explicit HTTP status override, or null to use the category's default mapping. |

##### Overload: `Nullable<int> StatusCode` on `ErrorCatalogEntry`

Gets the explicit HTTP status override, or null to use the category's default mapping.

#### Member: `ErrorCatalogEntry.Type`

**Example**

```csharp
ErrorType type = entry.Type;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Type`](#overload-errortype-type-on-errorcatalogentry) | Gets the application category used for the default HTTP mapping when no status override is supplied. |

##### Overload: `ErrorType Type` on `ErrorCatalogEntry`

Gets the application category used for the default HTTP mapping when no status override is supplied.

### Type: `ErrorCatalogMetadata`

Owns the immutable error catalog attached to one endpoint.

**Example**

```csharp
ErrorCatalogMetadata metadata = new([new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.")]);
```

| Member | Description |
| --- | --- |
| [`ErrorCatalogMetadata`](#member-errorcatalogmetadataerrorcatalogmetadata) | Copies and validates a non-empty endpoint error catalog. |
| [`AsSpan`](#member-errorcatalogmetadataasspan) | Returns a zero-allocation view over the owned entries. |
| [`Count`](#member-errorcatalogmetadatacount) | Gets the number of catalog entries. |

#### Member: `ErrorCatalogMetadata.ErrorCatalogMetadata`

**Example**

```csharp
ErrorCatalogMetadata metadata = new(entries);
```

| Overload | Description |
| --- | --- |
| [`ErrorCatalogMetadata(ReadOnlySpan<ErrorCatalogEntry> entries)`](#overload-errorcatalogmetadatareadonlyspanerrorcatalogentry-entries-on-errorcatalogmetadata) | Copies and validates a non-empty endpoint error catalog. |

##### Overload: `ErrorCatalogMetadata(ReadOnlySpan<ErrorCatalogEntry> entries)` on `ErrorCatalogMetadata`

Copies and validates a non-empty endpoint error catalog.

**Parameters**

- `entries`: The public errors the endpoint can return.

#### Member: `ErrorCatalogMetadata.AsSpan`

**Example**

```csharp
ReadOnlySpan<ErrorCatalogEntry> entries = metadata.AsSpan();
```

| Overload | Description |
| --- | --- |
| [`ReadOnlySpan<ErrorCatalogEntry> AsSpan()`](#overload-readonlyspanerrorcatalogentry-asspan-on-errorcatalogmetadata) | Returns a zero-allocation view over the owned entries. |

##### Overload: `ReadOnlySpan<ErrorCatalogEntry> AsSpan()` on `ErrorCatalogMetadata`

Returns a zero-allocation view over the owned entries.

#### Member: `ErrorCatalogMetadata.Count`

**Example**

```csharp
int count = metadata.Count;
```

| Overload | Description |
| --- | --- |
| [`int Count`](#overload-int-count-on-errorcatalogmetadata) | Gets the number of catalog entries. |

##### Overload: `int Count` on `ErrorCatalogMetadata`

Gets the number of catalog entries.

### Type: `ErrorEndpointConventionExtensions`

Provides reflection-free error response metadata for Minimal API endpoints.

| Member | Description |
| --- | --- |
| [`ProducesErrorCatalog`](#member-errorendpointconventionextensionsproduceserrorcatalog) | Adds stable error-code metadata and corresponding problem responses to an endpoint. |
| [`ProducesErrors`](#member-errorendpointconventionextensionsproduceserrors) | Adds one problem response metadata entry for each error category. |

#### Member: `ErrorEndpointConventionExtensions.ProducesErrorCatalog`

**Example**

```csharp
app.MapGet("/users/{id}", GetUser).ProducesErrorCatalog(new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found."));
```

| Overload | Description |
| --- | --- |
| [`TBuilder ProducesErrorCatalog<TBuilder>(TBuilder builder, ReadOnlySpan<ErrorCatalogEntry> entries)`](#overload-tbuilder-produceserrorcatalogtbuildertbuilder-builder-readonlyspanerrorcatalogentry-entries-on-errorendpointconventionextensions) | Adds stable error-code metadata and corresponding problem responses to an endpoint. |

##### Overload: `TBuilder ProducesErrorCatalog<TBuilder>(TBuilder builder, ReadOnlySpan<ErrorCatalogEntry> entries)` on `ErrorEndpointConventionExtensions`

Adds stable error-code metadata and corresponding problem responses to an endpoint.

**Parameters**

- `entries`: The public errors the endpoint can return.

**Returns:** The same endpoint builder for continued convention composition.

#### Member: `ErrorEndpointConventionExtensions.ProducesErrors`

**Example**

```csharp
app.MapGet("/users/{id}", GetUser).ProducesErrors(ErrorType.NotFound, ErrorType.Unavailable);
```

| Overload | Description |
| --- | --- |
| [`TBuilder ProducesErrors<TBuilder>(TBuilder builder, ReadOnlySpan<ErrorType> errorTypes)`](#overload-tbuilder-produceserrorstbuildertbuilder-builder-readonlyspanerrortype-errortypes-on-errorendpointconventionextensions) | Adds one problem response metadata entry for each error category. |

##### Overload: `TBuilder ProducesErrors<TBuilder>(TBuilder builder, ReadOnlySpan<ErrorType> errorTypes)` on `ErrorEndpointConventionExtensions`

Adds one problem response metadata entry for each error category.

**Parameters**

- `errorTypes`: The categories the endpoint can return.

**Returns:** The same endpoint builder for continued convention composition.

### Type: `ErrorProblemDetails`

Converts structured errors to default RFC 9457 problem details.

| Member | Description |
| --- | --- |
| [`Create`](#member-errorproblemdetailscreate) | Creates problem details using the built-in category, visibility, and trace policy. |
| [`CreateExample`](#member-errorproblemdetailscreateexample) | Creates deterministic problem details for documentation without request or activity data. |
| [`GetStatusCode`](#member-errorproblemdetailsgetstatuscode) | Gets the HTTP status for an initialized error, honoring valid HTTP numeric custom categories. |
| [`ToHttpResult`](#member-errorproblemdetailstohttpresult) | Creates a strongly typed problem HTTP result for an error. |

#### Member: `ErrorProblemDetails.Create`

**Example**

```csharp
ProblemDetails problem = ErrorProblemDetails.Create(error, httpContext);
```

| Overload | Description |
| --- | --- |
| [`ProblemDetails Create(in Error error, HttpContext? httpContext)`](#overload-problemdetails-createin-error-error-httpcontext-httpcontext-on-errorproblemdetails) | Creates problem details using the built-in category, visibility, and trace policy. |
| [`ProblemDetails Create(in Error error, int statusCode, HttpContext? httpContext)`](#overload-problemdetails-createin-error-error-int-statuscode-httpcontext-httpcontext-on-errorproblemdetails) | Creates problem details with an explicit HTTP error status and the existing visibility and trace policy. |

##### Overload: `ProblemDetails Create(in Error error, HttpContext? httpContext)` on `ErrorProblemDetails`

Creates problem details using the built-in category, visibility, and trace policy.

**Parameters**

- `error`: The initialized error to convert.
- `httpContext`: An optional context supplying a fallback trace identifier.

**Returns:** A populated problem-details value.

##### Overload: `ProblemDetails Create(in Error error, int statusCode, HttpContext? httpContext)` on `ErrorProblemDetails`

Creates problem details with an explicit HTTP error status and the existing visibility and trace policy.

Protocol-specific headers remain the endpoint's responsibility.

**Parameters**

- `error`: The initialized error to convert.
- `statusCode`: HTTP error status from 400 through 599, independent of the error's numeric category.
- `httpContext`: Optional request context supplying a fallback trace identifier.

**Returns:** Problem details with a status-specific title and type URI.

**Throws**

- `ArgumentOutOfRangeException`: The status is outside the HTTP error range.

#### Member: `ErrorProblemDetails.CreateExample`

**Example**

```csharp
ProblemDetails example = ErrorProblemDetails.CreateExample(error);
```

| Overload | Description |
| --- | --- |
| [`ProblemDetails CreateExample(in Error error)`](#overload-problemdetails-createexamplein-error-error-on-errorproblemdetails) | Creates deterministic problem details for documentation without request or activity data. |
| [`ProblemDetails CreateExample(in Error error, int statusCode)`](#overload-problemdetails-createexamplein-error-error-int-statuscode-on-errorproblemdetails) | Creates a deterministic example with an explicit HTTP error status and no ambient trace data. |

##### Overload: `ProblemDetails CreateExample(in Error error)` on `ErrorProblemDetails`

Creates deterministic problem details for documentation without request or activity data.

**Parameters**

- `error`: The initialized error to convert.

**Returns:** Problem details without a trace identifier.

##### Overload: `ProblemDetails CreateExample(in Error error, int statusCode)` on `ErrorProblemDetails`

Creates a deterministic example with an explicit HTTP error status and no ambient trace data.

**Parameters**

- `error`: The initialized error to convert.
- `statusCode`: HTTP error status from 400 through 599.

**Returns:** Problem details without a trace identifier.

#### Member: `ErrorProblemDetails.GetStatusCode`

**Example**

```csharp
int status = ErrorProblemDetails.GetStatusCode(error);
```

| Overload | Description |
| --- | --- |
| [`int GetStatusCode(in Error error)`](#overload-int-getstatuscodein-error-error-on-errorproblemdetails) | Gets the HTTP status for an initialized error, honoring valid HTTP numeric custom categories. |
| [`int GetStatusCode(ErrorType type)`](#overload-int-getstatuscodeerrortype-type-on-errorproblemdetails) | Gets the default HTTP status code for an error category. |

##### Overload: `int GetStatusCode(in Error error)` on `ErrorProblemDetails`

Gets the HTTP status for an initialized error, honoring valid HTTP numeric custom categories.

**Parameters**

- `error`: The initialized error to map.

**Returns:** The mapped status, or 500 for a non-HTTP custom category.

##### Overload: `int GetStatusCode(ErrorType type)` on `ErrorProblemDetails`

Gets the default HTTP status code for an error category.

**Parameters**

- `type`: The initialized error category.

**Returns:** The corresponding HTTP status code.

#### Member: `ErrorProblemDetails.ToHttpResult`

**Example**

```csharp
ProblemHttpResult result = ErrorProblemDetails.ToHttpResult(error, httpContext);
```

| Overload | Description |
| --- | --- |
| [`ProblemHttpResult ToHttpResult(in Error error, HttpContext? httpContext)`](#overload-problemhttpresult-tohttpresultin-error-error-httpcontext-httpcontext-on-errorproblemdetails) | Creates a strongly typed problem HTTP result for an error. |
| [`ProblemHttpResult ToHttpResult(in Error error, int statusCode, HttpContext? httpContext)`](#overload-problemhttpresult-tohttpresultin-error-error-int-statuscode-httpcontext-httpcontext-on-errorproblemdetails) | Creates a typed problem result with an explicit HTTP error status. |

##### Overload: `ProblemHttpResult ToHttpResult(in Error error, HttpContext? httpContext)` on `ErrorProblemDetails`

Creates a strongly typed problem HTTP result for an error.

**Parameters**

- `error`: The initialized error to convert.
- `httpContext`: An optional context supplying a fallback trace identifier.

**Returns:** A strongly typed problem result.

##### Overload: `ProblemHttpResult ToHttpResult(in Error error, int statusCode, HttpContext? httpContext)` on `ErrorProblemDetails`

Creates a typed problem result with an explicit HTTP error status.

**Parameters**

- `error`: The initialized error to convert.
- `statusCode`: HTTP error status from 400 through 599.
- `httpContext`: Optional request context included in the problem payload.

**Returns:** A typed problem result preserving the error's code and visibility policy.

### Type: `IHttpResultMapper<TError, TResult>`

Maps any Result error at the HTTP boundary without DI or reflection.

| Member | Description |
| --- | --- |
| [`Map`](#member-ihttpresultmapperterror-tresultmap) | Maps an error to a strongly typed HTTP result. |

#### Member: `IHttpResultMapper<TError, TResult>.Map`

**Example**

```csharp
ProblemHttpResult result = mapper.Map(error, httpContext);
```

| Overload | Description |
| --- | --- |
| [`TResult Map(in TError failure, HttpContext? httpContext)`](#overload-tresult-mapin-terror-failure-httpcontext-httpcontext-on-ihttpresultmapperterror-tresult) | Maps an error to a strongly typed HTTP result. |

##### Overload: `TResult Map(in TError failure, HttpContext? httpContext)` on `IHttpResultMapper<TError, TResult>`

Maps an error to a strongly typed HTTP result.

**Parameters**

- `failure`: The failure value to map.
- `httpContext`: Optional request context for transport-specific metadata.

**Returns:** The mapped HTTP result.

### Type: `ProducesErrorAttribute`

Adds one structured problem response to controller or endpoint metadata.

**Example**

```csharp
[ProducesError(ErrorType.NotFound)]
```

| Member | Description |
| --- | --- |
| [`ProducesErrorAttribute`](#member-produceserrorattributeproduceserrorattribute) | Adds one structured problem response to controller or endpoint metadata. |
| [`ContentTypes`](#member-produceserrorattributecontenttypes) | Gets the supported RFC 9457 response content type. |
| [`Description`](#member-produceserrorattributedescription) | Gets the optional response description; this attribute leaves it unspecified. |
| [`ErrorType`](#member-produceserrorattributeerrortype) | Gets the configured error category. |
| [`StatusCode`](#member-produceserrorattributestatuscode) | Gets the explicit HTTP status, or the default mapping from `ErrorType`. |
| [`Type`](#member-produceserrorattributetype) | Gets the documented RFC 9457 response body type. |

#### Member: `ProducesErrorAttribute.ProducesErrorAttribute`

**Example**

```csharp
[ProducesError(ErrorType.NotFound)]
```

| Overload | Description |
| --- | --- |
| [`ProducesErrorAttribute(ErrorType errorType)`](#overload-produceserrorattributeerrortype-errortype-on-produceserrorattribute) | Adds one structured problem response to controller or endpoint metadata. |
| [`ProducesErrorAttribute(ErrorType errorType, int statusCode)`](#overload-produceserrorattributeerrortype-errortype-int-statuscode-on-produceserrorattribute) | Documents an explicit HTTP error status for an application category. |

##### Overload: `ProducesErrorAttribute(ErrorType errorType)` on `ProducesErrorAttribute`

Adds one structured problem response to controller or endpoint metadata.

**Parameters**

- `errorType`: The initialized error category exposed by the operation.

##### Overload: `ProducesErrorAttribute(ErrorType errorType, int statusCode)` on `ProducesErrorAttribute`

Documents an explicit HTTP error status for an application category.

**Parameters**

- `errorType`: The initialized application category.
- `statusCode`: HTTP error status from 400 through 599.

#### Member: `ProducesErrorAttribute.ContentTypes`

**Example**

```csharp
IEnumerable<string> contentTypes = metadata.ContentTypes;
```

| Overload | Description |
| --- | --- |
| [`IEnumerable<string> ContentTypes`](#overload-ienumerablestring-contenttypes-on-produceserrorattribute) | Gets the supported RFC 9457 response content type. |

##### Overload: `IEnumerable<string> ContentTypes` on `ProducesErrorAttribute`

Gets the supported RFC 9457 response content type.

#### Member: `ProducesErrorAttribute.Description`

**Example**

```csharp
string? description = metadata.Description;
```

| Overload | Description |
| --- | --- |
| [`string? Description`](#overload-string-description-on-produceserrorattribute) | Gets the optional response description; this attribute leaves it unspecified. |

##### Overload: `string? Description` on `ProducesErrorAttribute`

Gets the optional response description; this attribute leaves it unspecified.

#### Member: `ProducesErrorAttribute.ErrorType`

**Example**

```csharp
ErrorType type = metadata.ErrorType;
```

| Overload | Description |
| --- | --- |
| [`ErrorType ErrorType`](#overload-errortype-errortype-on-produceserrorattribute) | Gets the configured error category. |

##### Overload: `ErrorType ErrorType` on `ProducesErrorAttribute`

Gets the configured error category.

#### Member: `ProducesErrorAttribute.StatusCode`

**Example**

```csharp
int status = metadata.StatusCode;
```

| Overload | Description |
| --- | --- |
| [`int StatusCode`](#overload-int-statuscode-on-produceserrorattribute) | Gets the explicit HTTP status, or the default mapping from `ErrorType`. |

##### Overload: `int StatusCode` on `ProducesErrorAttribute`

Gets the explicit HTTP status, or the default mapping from `ErrorType`.

#### Member: `ProducesErrorAttribute.Type`

**Example**

```csharp
Type? bodyType = metadata.Type;
```

| Overload | Description |
| --- | --- |
| [`Type? Type`](#overload-type-type-on-produceserrorattribute) | Gets the documented RFC 9457 response body type. |

##### Overload: `Type? Type` on `ProducesErrorAttribute`

Gets the documented RFC 9457 response body type.

### Type: `ProducesErrorCatalogAttribute`

Adds one stable domain error and its response category to controller or endpoint metadata.

**Example**

```csharp
[ProducesErrorCatalog(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.")]
```

| Member | Description |
| --- | --- |
| [`ProducesErrorCatalogAttribute`](#member-produceserrorcatalogattributeproduceserrorcatalogattribute) | Adds one stable domain error and its response category to controller or endpoint metadata. |
| [`ContentTypes`](#member-produceserrorcatalogattributecontenttypes) | Gets the supported RFC 9457 response content type. |
| [`Description`](#member-produceserrorcatalogattributedescription) | Gets the optional response description; this attribute leaves it unspecified. |
| [`Entry`](#member-produceserrorcatalogattributeentry) | Gets the documented error entry. |
| [`StatusCode`](#member-produceserrorcatalogattributestatuscode) | Gets the HTTP status mapped from the catalog entry. |
| [`Type`](#member-produceserrorcatalogattributetype) | Gets the documented RFC 9457 response body type. |

#### Member: `ProducesErrorCatalogAttribute.ProducesErrorCatalogAttribute`

**Example**

```csharp
[ProducesErrorCatalog(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.")]
```

| Overload | Description |
| --- | --- |
| [`ProducesErrorCatalogAttribute(ErrorType type, string code, string description)`](#overload-produceserrorcatalogattributeerrortype-type-string-code-string-description-on-produceserrorcatalogattribute) | Adds one stable domain error and its response category to controller or endpoint metadata. |
| [`ProducesErrorCatalogAttribute(ErrorType type, string code, string description, int statusCode)`](#overload-produceserrorcatalogattributeerrortype-type-string-code-string-description-int-statuscode-on-produceserrorcatalogattribute) | Documents a stable error code with an explicit HTTP error status. |

##### Overload: `ProducesErrorCatalogAttribute(ErrorType type, string code, string description)` on `ProducesErrorCatalogAttribute`

Adds one stable domain error and its response category to controller or endpoint metadata.

**Parameters**

- `type`: The initialized category that determines the HTTP status.
- `code`: The stable machine-readable error code.
- `description`: The public description exposed in API documentation.

##### Overload: `ProducesErrorCatalogAttribute(ErrorType type, string code, string description, int statusCode)` on `ProducesErrorCatalogAttribute`

Documents a stable error code with an explicit HTTP error status.

**Parameters**

- `type`: The initialized application category.
- `code`: The stable machine-readable error code.
- `description`: The public description exposed in API documentation.
- `statusCode`: HTTP error status from 400 through 599.

#### Member: `ProducesErrorCatalogAttribute.ContentTypes`

**Example**

```csharp
IEnumerable<string> contentTypes = metadata.ContentTypes;
```

| Overload | Description |
| --- | --- |
| [`IEnumerable<string> ContentTypes`](#overload-ienumerablestring-contenttypes-on-produceserrorcatalogattribute) | Gets the supported RFC 9457 response content type. |

##### Overload: `IEnumerable<string> ContentTypes` on `ProducesErrorCatalogAttribute`

Gets the supported RFC 9457 response content type.

#### Member: `ProducesErrorCatalogAttribute.Description`

**Example**

```csharp
string? description = metadata.Description;
```

| Overload | Description |
| --- | --- |
| [`string? Description`](#overload-string-description-on-produceserrorcatalogattribute) | Gets the optional response description; this attribute leaves it unspecified. |

##### Overload: `string? Description` on `ProducesErrorCatalogAttribute`

Gets the optional response description; this attribute leaves it unspecified.

#### Member: `ProducesErrorCatalogAttribute.Entry`

**Example**

```csharp
ErrorCatalogEntry entry = metadata.Entry;
```

| Overload | Description |
| --- | --- |
| [`ErrorCatalogEntry Entry`](#overload-errorcatalogentry-entry-on-produceserrorcatalogattribute) | Gets the documented error entry. |

##### Overload: `ErrorCatalogEntry Entry` on `ProducesErrorCatalogAttribute`

Gets the documented error entry.

#### Member: `ProducesErrorCatalogAttribute.StatusCode`

**Example**

```csharp
int status = metadata.StatusCode;
```

| Overload | Description |
| --- | --- |
| [`int StatusCode`](#overload-int-statuscode-on-produceserrorcatalogattribute) | Gets the HTTP status mapped from the catalog entry. |

##### Overload: `int StatusCode` on `ProducesErrorCatalogAttribute`

Gets the HTTP status mapped from the catalog entry.

#### Member: `ProducesErrorCatalogAttribute.Type`

**Example**

```csharp
Type? bodyType = metadata.Type;
```

| Overload | Description |
| --- | --- |
| [`Type? Type`](#overload-type-type-on-produceserrorcatalogattribute) | Gets the documented RFC 9457 response body type. |

##### Overload: `Type? Type` on `ProducesErrorCatalogAttribute`

Gets the documented RFC 9457 response body type.

### Type: `ResultHttpExtensions`

Maps result branches to strongly typed ASP.NET Core HTTP results.

| Member | Description |
| --- | --- |
| [`ToHttpResult`](#member-resulthttpextensionstohttpresult) | Maps success with a delegate and structured failure with the default problem policy. |

#### Member: `ResultHttpExtensions.ToHttpResult`

**Example**

```csharp
Results<Ok<User>, ProblemHttpResult> response = result.ToHttpResult(TypedResults.Ok, httpContext);
```

| Overload | Description |
| --- | --- |
| [`Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TSuccess>(in Result<T, Error> result, Func<T, TSuccess> success, HttpContext? httpContext)`](#overload-resultstsuccess-problemhttpresult-tohttpresultt-tsuccessin-resultt-error-result-funct-tsuccess-success-httpcontext-httpcontext-on-resulthttpextensions) | Maps success with a delegate and structured failure with the default problem policy. |
| [`Results<TSuccess, ValidationProblem> ToHttpResult<T, TSuccess>(in Result<T, ValidationErrors> result, Func<T, TSuccess> success, HttpContext? httpContext)`](#overload-resultstsuccess-validationproblem-tohttpresultt-tsuccessin-resultt-validationerrors-result-funct-tsuccess-success-httpcontext-httpcontext-on-resulthttpextensions) | Maps success with a delegate and failures to a validation problem result. |
| [`Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TSuccess, TSuccessMapper>(in Result<T, Error> result, TSuccessMapper success, HttpContext? httpContext)`](#overload-resultstsuccess-problemhttpresult-tohttpresultt-tsuccess-tsuccessmapperin-resultt-error-result-tsuccessmapper-success-httpcontext-httpcontext-on-resulthttpextensions) | Maps success with a value-function struct and structured failure with the default problem policy. |
| [`Results<TSuccess, ValidationProblem> ToHttpResult<T, TSuccess, TSuccessMapper>(in Result<T, ValidationErrors> result, TSuccessMapper success, HttpContext? httpContext)`](#overload-resultstsuccess-validationproblem-tohttpresultt-tsuccess-tsuccessmapperin-resultt-validationerrors-result-tsuccessmapper-success-httpcontext-httpcontext-on-resulthttpextensions) | Maps success with a value-function struct and failures to a validation problem result. |
| [`Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TError, TSuccess>(in Result<T, TError> result, Func<T, TSuccess> success, HttpContext? httpContext)`](#overload-resultstsuccess-problemhttpresult-tohttpresultt-terror-tsuccessin-resultt-terror-result-funct-tsuccess-success-httpcontext-httpcontext-on-resulthttpextensions) | Maps success with a delegate and converts a domain error to the default problem result. |
| [`Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure>(in Result<T, TError> result, Func<T, TSuccess> success, Func<TError, TFailure> failure)`](#overload-resultstsuccess-tfailure-tohttpresultt-terror-tsuccess-tfailurein-resultt-terror-result-funct-tsuccess-success-functerror-tfailure-failure-on-resulthttpextensions) | Fully caller-owned mapping path for any error type. Use this to return custom ProblemDetails, framework results, or application-specific results. |
| [`Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure, TMapper>(in Result<T, TError> result, Func<T, TSuccess> success, TMapper failure, HttpContext? httpContext)`](#overload-resultstsuccess-tfailure-tohttpresultt-terror-tsuccess-tfailure-tmapperin-resultt-terror-result-funct-tsuccess-success-tmapper-failure-httpcontext-httpcontext-on-resulthttpextensions) | Maps failure with a value-type mapper while retaining a delegate success mapper. |
| [`Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure, TSuccessMapper, TFailureMapper>(in Result<T, TError> result, TSuccessMapper success, TFailureMapper failure, HttpContext? httpContext)`](#overload-resultstsuccess-tfailure-tohttpresultt-terror-tsuccess-tfailure-tsuccessmapper-tfailuremapperin-resultt-terror-result-tsuccessmapper-success-tfailuremapper-failure-httpcontext-httpcontext-on-resulthttpextensions) | Maps both branches through value-type mappers for allocation-free dispatch. |

##### Overload: `Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TSuccess>(in Result<T, Error> result, Func<T, TSuccess> success, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps success with a delegate and structured failure with the default problem policy.

##### Overload: `Results<TSuccess, ValidationProblem> ToHttpResult<T, TSuccess>(in Result<T, ValidationErrors> result, Func<T, TSuccess> success, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps success with a delegate and failures to a validation problem result.

##### Overload: `Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TSuccess, TSuccessMapper>(in Result<T, Error> result, TSuccessMapper success, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps success with a value-function struct and structured failure with the default problem policy.

##### Overload: `Results<TSuccess, ValidationProblem> ToHttpResult<T, TSuccess, TSuccessMapper>(in Result<T, ValidationErrors> result, TSuccessMapper success, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps success with a value-function struct and failures to a validation problem result.

##### Overload: `Results<TSuccess, ProblemHttpResult> ToHttpResult<T, TError, TSuccess>(in Result<T, TError> result, Func<T, TSuccess> success, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps success with a delegate and converts a domain error to the default problem result.

##### Overload: `Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure>(in Result<T, TError> result, Func<T, TSuccess> success, Func<TError, TFailure> failure)` on `ResultHttpExtensions`

Fully caller-owned mapping path for any error type. Use this to return custom ProblemDetails, framework results, or application-specific results.

##### Overload: `Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure, TMapper>(in Result<T, TError> result, Func<T, TSuccess> success, TMapper failure, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps failure with a value-type mapper while retaining a delegate success mapper.

##### Overload: `Results<TSuccess, TFailure> ToHttpResult<T, TError, TSuccess, TFailure, TSuccessMapper, TFailureMapper>(in Result<T, TError> result, TSuccessMapper success, TFailureMapper failure, HttpContext? httpContext)` on `ResultHttpExtensions`

Maps both branches through value-type mappers for allocation-free dispatch.

### Type: `ValidationErrorProblemDetails`

Converts validation issues to strongly typed validation problem results.

| Member | Description |
| --- | --- |
| [`ToHttpResult`](#member-validationerrorproblemdetailstohttpresult) | Groups validation issues by path and preserves their machine-readable codes. |

#### Member: `ValidationErrorProblemDetails.ToHttpResult`

**Example**

```csharp
ValidationProblem result = ValidationErrorProblemDetails.ToHttpResult(errors, httpContext);
```

| Overload | Description |
| --- | --- |
| [`ValidationProblem ToHttpResult(ValidationErrors validationErrors, HttpContext? httpContext)`](#overload-validationproblem-tohttpresultvalidationerrors-validationerrors-httpcontext-httpcontext-on-validationerrorproblemdetails) | Groups validation issues by path and preserves their machine-readable codes. |

##### Overload: `ValidationProblem ToHttpResult(ValidationErrors validationErrors, HttpContext? httpContext)` on `ValidationErrorProblemDetails`

Groups validation issues by path and preserves their machine-readable codes.

**Parameters**

- `validationErrors`: The validation issues to convert.
- `httpContext`: An optional context supplying a fallback trace identifier.

**Returns:** A strongly typed validation problem result.


## Package MonadicTypes.NET.AspNetCore.OpenApi

**Types:** [`OpenApiOptionsExtensions`](#type-openapioptionsextensions) · [`OpenApiServiceCollectionExtensions`](#type-openapiservicecollectionextensions)

### Type: `OpenApiOptionsExtensions`

Registers MonadicTypes error-catalog document generation.

| Member | Description |
| --- | --- |
| [`AddErrorCatalogs`](#member-openapioptionsextensionsadderrorcatalogs) | Adds the explicit endpoint error-catalog transformer without reflection or DI activation. |

#### Member: `OpenApiOptionsExtensions.AddErrorCatalogs`

**Example**

```csharp
builder.Services.AddOpenApi(options => options.AddErrorCatalogs());
```

| Overload | Description |
| --- | --- |
| [`OpenApiOptions AddErrorCatalogs(OpenApiOptions options)`](#overload-openapioptions-adderrorcatalogsopenapioptions-options-on-openapioptionsextensions) | Adds the explicit endpoint error-catalog transformer without reflection or DI activation. |

##### Overload: `OpenApiOptions AddErrorCatalogs(OpenApiOptions options)` on `OpenApiOptionsExtensions`

Adds the explicit endpoint error-catalog transformer without reflection or DI activation.

**Returns:** The same options instance for continued configuration.

### Type: `OpenApiServiceCollectionExtensions`

Registers reflection-free error-catalog OpenAPI services.

| Member | Description |
| --- | --- |
| [`AddErrorCatalogOpenApi`](#member-openapiservicecollectionextensionsadderrorcatalogopenapi) | Adds OpenAPI error-catalog transformation and source-generated JSON metadata for the problem payload returned by `ProblemHttpResult`. |

#### Member: `OpenApiServiceCollectionExtensions.AddErrorCatalogOpenApi`

**Example**

```csharp
builder.Services.AddErrorCatalogOpenApi();
```

| Overload | Description |
| --- | --- |
| [`IServiceCollection AddErrorCatalogOpenApi(IServiceCollection services)`](#overload-iservicecollection-adderrorcatalogopenapiiservicecollection-services-on-openapiservicecollectionextensions) | Adds OpenAPI error-catalog transformation and source-generated JSON metadata for the problem payload returned by `ProblemHttpResult`. |

##### Overload: `IServiceCollection AddErrorCatalogOpenApi(IServiceCollection services)` on `OpenApiServiceCollectionExtensions`

Adds OpenAPI error-catalog transformation and source-generated JSON metadata for the problem payload returned by `ProblemHttpResult`.

**Returns:** The same service collection for continued configuration.


## Package MonadicTypes.NET.Async

**Types:** [`AsyncResultExtensions`](#type-asyncresultextensions)

### Type: `AsyncResultExtensions`

Lifts synchronous and asynchronous result combinators over `Task` and `ValueTask` receivers. Every operator consumes its source once and converges on `ValueTask` for continued composition.

| Member | Description |
| --- | --- |
| [`Bind`](#member-asyncresultextensionsbind) | Binds a Task-backed result through a synchronous continuation. |
| [`BindAsync`](#member-asyncresultextensionsbindasync) | Binds a successful value through an asynchronous continuation. |
| [`BindError`](#member-asyncresultextensionsbinderror) | Binds a Task-backed failure through a synchronous continuation. |
| [`BindErrorAsync`](#member-asyncresultextensionsbinderrorasync) | Binds a failure through an asynchronous continuation. |
| [`BindErrorTaskAsync`](#member-asyncresultextensionsbinderrortaskasync) | Binds a failure through a Task-returning continuation. |
| [`BindTaskAsync`](#member-asyncresultextensionsbindtaskasync) | Binds a successful value through a Task-returning continuation. |
| [`Map`](#member-asyncresultextensionsmap) | Maps a Task-backed result through a synchronous callback. |
| [`MapAsync`](#member-asyncresultextensionsmapasync) | Maps a successful value through an asynchronous callback. |
| [`MapTaskAsync`](#member-asyncresultextensionsmaptaskasync) | Maps a successful value through a Task-returning callback. |

#### Member: `AsyncResultExtensions.Bind`

**Example**

```csharp
Result<Account, LoadError> account = await pending.Bind(LoadAccount);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> Bind<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)`](#overload-valuetaskresulttresult-terror-bindt-terror-tresulttaskresultt-terror-source-funct-resulttresult-terror-bind-on-asyncresultextensions) | Binds a Task-backed result through a synchronous continuation. |
| [`ValueTask<Result<TResult, TError>> Bind<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)`](#overload-valuetaskresulttresult-terror-bindt-terror-tresultin-valuetaskresultt-terror-source-funct-resulttresult-terror-bind-on-asyncresultextensions) | Binds a completed or pending result through a synchronous continuation. |

##### Overload: `ValueTask<Result<TResult, TError>> Bind<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)` on `AsyncResultExtensions`

Binds a Task-backed result through a synchronous continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> Bind<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Result<TResult, TError>> bind)` on `AsyncResultExtensions`

Binds a completed or pending result through a synchronous continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A single-consumption awaitable containing the continuation result.

#### Member: `AsyncResultExtensions.BindAsync`

**Example**

```csharp
Result<Account, LoadError> account = await result.BindAsync(LoadAccountAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresultin-resultt-terror-result-funct-valuetaskresulttresult-terror-bind-on-asyncresultextensions) | Binds a successful value through an asynchronous continuation. |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, ValueTask<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresulttaskresultt-terror-source-funct-valuetaskresulttresult-terror-bind-on-asyncresultextensions) | Binds a Task-backed result through an asynchronous continuation. |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, ValueTask<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresultin-valuetaskresultt-terror-source-funct-valuetaskresulttresult-terror-bind-on-asyncresultextensions) | Binds a completed or pending result through an asynchronous continuation. |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresult-tfunctionin-resultt-terror-result-valuefunctiont-valuetaskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a successful value through a generated ValueTask-returning callable. |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresult-tfunctiontaskresultt-terror-source-valuefunctiont-valuetaskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a Task-backed result through a generated ValueTask-returning callable. |
| [`ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindasynct-terror-tresult-tfunctionin-valuetaskresultt-terror-source-valuefunctiont-valuetaskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a completed or pending result through a generated ValueTask-returning callable. |

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a successful value through an asynchronous continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** The asynchronous continuation result or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, ValueTask<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a Task-backed result through an asynchronous continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, ValueTask<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a completed or pending result through an asynchronous continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A single-consumption awaitable containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a successful value through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** The asynchronous continuation result or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a Task-backed result through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a completed or pending result through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** A single-consumption awaitable containing the continuation result.

#### Member: `AsyncResultExtensions.BindError`

**Example**

```csharp
Result<User, FinalError> recovered = await pending.BindError(Retry);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TNextError>> BindError<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, Result<T, TNextError>> bind)`](#overload-valuetaskresultt-tnexterror-binderrort-terror-tnexterrortaskresultt-terror-source-functerror-resultt-tnexterror-bind-on-asyncresultextensions) | Binds a Task-backed failure through a synchronous continuation. |
| [`ValueTask<Result<T, TNextError>> BindError<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, Result<T, TNextError>> bind)`](#overload-valuetaskresultt-tnexterror-binderrort-terror-tnexterrorin-valuetaskresultt-terror-source-functerror-resultt-tnexterror-bind-on-asyncresultextensions) | Binds a completed or pending failure through a synchronous continuation. |

##### Overload: `ValueTask<Result<T, TNextError>> BindError<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, Result<T, TNextError>> bind)` on `AsyncResultExtensions`

Binds a Task-backed failure through a synchronous continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A ValueTask-backed pipeline containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindError<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, Result<T, TNextError>> bind)` on `AsyncResultExtensions`

Binds a completed or pending failure through a synchronous continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A single-consumption awaitable containing the resulting value.

#### Member: `AsyncResultExtensions.BindErrorAsync`

**Example**

```csharp
Result<User, FinalError> recovered = await result.BindErrorAsync(RetryAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(in Result<T, TError> result, Func<TError, ValueTask<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterrorin-resultt-terror-result-functerror-valuetaskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a failure through an asynchronous continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, ValueTask<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterrortaskresultt-terror-source-functerror-valuetaskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a Task-backed failure through an asynchronous continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, ValueTask<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterrorin-valuetaskresultt-terror-source-functerror-valuetaskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a completed or pending failure through an asynchronous continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(in Result<T, TError> result, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterror-tfunctionin-resultt-terror-result-valuefunctionterror-valuetaskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a failure through a generated ValueTask-returning callable. |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(Task<Result<T, TError>> source, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterror-tfunctiontaskresultt-terror-source-valuefunctionterror-valuetaskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a Task-backed failure through a generated ValueTask-returning callable. |
| [`ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrorasynct-terror-tnexterror-tfunctionin-valuetaskresultt-terror-source-valuefunctionterror-valuetaskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a completed or pending failure through a generated ValueTask-returning callable. |

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(in Result<T, TError> result, Func<TError, ValueTask<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a failure through an asynchronous continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** The unchanged success or asynchronous failure continuation result.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, ValueTask<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a Task-backed failure through an asynchronous continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A ValueTask-backed pipeline containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, ValueTask<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a completed or pending failure through an asynchronous continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A single-consumption awaitable containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(in Result<T, TError> result, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a failure through a generated ValueTask-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** The unchanged success or asynchronous failure continuation result.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(Task<Result<T, TError>> source, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a Task-backed failure through a generated ValueTask-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** A ValueTask-backed pipeline containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorAsync<T, TError, TNextError, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a completed or pending failure through a generated ValueTask-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** A single-consumption awaitable containing the resulting value.

#### Member: `AsyncResultExtensions.BindErrorTaskAsync`

**Example**

```csharp
Result<User, FinalError> recovered = await result.BindErrorTaskAsync(RetryTaskAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(in Result<T, TError> result, Func<TError, Task<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterrorin-resultt-terror-result-functerror-taskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a failure through a Task-returning continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, Task<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterrortaskresultt-terror-source-functerror-taskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a Task-backed failure through a Task-returning continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, Task<Result<T, TNextError>>> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterrorin-valuetaskresultt-terror-source-functerror-taskresultt-tnexterror-bind-on-asyncresultextensions) | Binds a completed or pending failure through a Task-returning continuation. |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(in Result<T, TError> result, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterror-tfunctionin-resultt-terror-result-valuefunctionterror-taskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a failure through a generated Task-returning callable. |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(Task<Result<T, TError>> source, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterror-tfunctiontaskresultt-terror-source-valuefunctionterror-taskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a Task-backed failure through a generated Task-returning callable. |
| [`ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)`](#overload-valuetaskresultt-tnexterror-binderrortaskasynct-terror-tnexterror-tfunctionin-valuetaskresultt-terror-source-valuefunctionterror-taskresultt-tnexterror-tfunction-bind-on-asyncresultextensions) | Binds a completed or pending failure through a generated Task-returning callable. |

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(in Result<T, TError> result, Func<TError, Task<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a failure through a Task-returning continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** The unchanged success or asynchronous failure continuation result.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(Task<Result<T, TError>> source, Func<TError, Task<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a Task-backed failure through a Task-returning continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A ValueTask-backed pipeline containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError>(in ValueTask<Result<T, TError>> source, Func<TError, Task<Result<T, TNextError>>> bind)` on `AsyncResultExtensions`

Binds a completed or pending failure through a Task-returning continuation.

**Type parameters**

- `TNextError`: Continuation error type.

**Parameters**

- `bind`: Continuation invoked only for failure.

**Returns:** A single-consumption awaitable containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(in Result<T, TError> result, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a failure through a generated Task-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** The unchanged success or asynchronous failure continuation result.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(Task<Result<T, TError>> source, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a Task-backed failure through a generated Task-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** A ValueTask-backed pipeline containing the resulting value.

##### Overload: `ValueTask<Result<T, TNextError>> BindErrorTaskAsync<T, TError, TNextError, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a completed or pending failure through a generated Task-returning callable.

**Type parameters**

- `TNextError`: Continuation error type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for failure.

**Returns:** A single-consumption awaitable containing the resulting value.

#### Member: `AsyncResultExtensions.BindTaskAsync`

**Example**

```csharp
Result<Account, LoadError> account = await result.BindTaskAsync(LoadAccountTaskAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, Task<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresultin-resultt-terror-result-funct-taskresulttresult-terror-bind-on-asyncresultextensions) | Binds a successful value through a Task-returning continuation. |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresulttaskresultt-terror-source-funct-taskresulttresult-terror-bind-on-asyncresultextensions) | Binds a Task-backed result through a Task-returning continuation. |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresultin-valuetaskresultt-terror-source-funct-taskresulttresult-terror-bind-on-asyncresultextensions) | Binds a completed or pending result through a Task-returning continuation. |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresult-tfunctionin-resultt-terror-result-valuefunctiont-taskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a successful value through a generated Task-returning callable. |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresult-tfunctiontaskresultt-terror-source-valuefunctiont-taskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a Task-backed result through a generated Task-returning callable. |
| [`ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)`](#overload-valuetaskresulttresult-terror-bindtaskasynct-terror-tresult-tfunctionin-valuetaskresultt-terror-source-valuefunctiont-taskresulttresult-terror-tfunction-bind-on-asyncresultextensions) | Binds a completed or pending result through a generated Task-returning callable. |

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, Task<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a successful value through a Task-returning continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** The asynchronous continuation result or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a Task-backed result through a Task-returning continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Task<Result<TResult, TError>>> bind)` on `AsyncResultExtensions`

Binds a completed or pending result through a Task-returning continuation.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Continuation invoked only for success.

**Returns:** A single-consumption awaitable containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a successful value through a generated Task-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** The asynchronous continuation result or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a Task-backed result through a generated Task-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the continuation result.

##### Overload: `ValueTask<Result<TResult, TError>> BindTaskAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)` on `AsyncResultExtensions`

Binds a completed or pending result through a generated Task-returning callable.

**Type parameters**

- `TResult`: Continuation success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `bind`: Allocation-free callable token invoked only for success.

**Returns:** A single-consumption awaitable containing the continuation result.

#### Member: `AsyncResultExtensions.Map`

**Example**

```csharp
Result<int, LoadError> id = await pending.Map(static user => user.Id);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> Map<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, TResult> map)`](#overload-valuetaskresulttresult-terror-mapt-terror-tresulttaskresultt-terror-source-funct-tresult-map-on-asyncresultextensions) | Maps a Task-backed result through a synchronous callback. |
| [`ValueTask<Result<TResult, TError>> Map<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, TResult> map)`](#overload-valuetaskresulttresult-terror-mapt-terror-tresultin-valuetaskresultt-terror-source-funct-tresult-map-on-asyncresultextensions) | Maps a completed or pending result through a synchronous callback. |

##### Overload: `ValueTask<Result<TResult, TError>> Map<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, TResult> map)` on `AsyncResultExtensions`

Maps a Task-backed result through a synchronous callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> Map<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, TResult> map)` on `AsyncResultExtensions`

Maps a completed or pending result through a synchronous callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A single-consumption awaitable containing the mapped result.

#### Member: `AsyncResultExtensions.MapAsync`

**Example**

```csharp
Result<UserDto, LoadError> mapped = await result.MapAsync(LoadDtoAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<TResult>> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresultin-resultt-terror-result-funct-valuetasktresult-map-on-asyncresultextensions) | Maps a successful value through an asynchronous callback. |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, ValueTask<TResult>> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresulttaskresultt-terror-source-funct-valuetasktresult-map-on-asyncresultextensions) | Maps a Task-backed result through an asynchronous callback. |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, ValueTask<TResult>> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresultin-valuetaskresultt-terror-source-funct-valuetasktresult-map-on-asyncresultextensions) | Maps a completed or pending result through an asynchronous callback. |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, ValueTask<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresult-tfunctionin-resultt-terror-result-valuefunctiont-valuetasktresult-tfunction-map-on-asyncresultextensions) | Maps a successful value through a generated ValueTask-returning callable. |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, ValueTask<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresult-tfunctiontaskresultt-terror-source-valuefunctiont-valuetasktresult-tfunction-map-on-asyncresultextensions) | Maps a Task-backed result through a generated ValueTask-returning callable. |
| [`ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, ValueTask<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-mapasynct-terror-tresult-tfunctionin-valuetaskresultt-terror-source-valuefunctiont-valuetasktresult-tfunction-map-on-asyncresultextensions) | Maps a completed or pending result through a generated ValueTask-returning callable. |

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<TResult>> map)` on `AsyncResultExtensions`

Maps a successful value through an asynchronous callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** An awaitable result containing the mapped value or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, ValueTask<TResult>> map)` on `AsyncResultExtensions`

Maps a Task-backed result through an asynchronous callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, ValueTask<TResult>> map)` on `AsyncResultExtensions`

Maps a completed or pending result through an asynchronous callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A single-consumption awaitable containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, ValueTask<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a successful value through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** An awaitable result containing the mapped value or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, ValueTask<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a Task-backed result through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, ValueTask<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a completed or pending result through a generated ValueTask-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** A single-consumption awaitable containing the mapped result.

#### Member: `AsyncResultExtensions.MapTaskAsync`

**Example**

```csharp
Result<UserDto, LoadError> mapped = await result.MapTaskAsync(LoadDtoTaskAsync);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, Task<TResult>> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresultin-resultt-terror-result-funct-tasktresult-map-on-asyncresultextensions) | Maps a successful value through a Task-returning callback. |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Task<TResult>> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresulttaskresultt-terror-source-funct-tasktresult-map-on-asyncresultextensions) | Maps a Task-backed result through a Task-returning callback. |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresultin-valuetaskresultt-terror-source-funct-tasktresult-map-on-asyncresultextensions) | Maps a completed or pending result through a Task-returning callback. |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, Task<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresult-tfunctionin-resultt-terror-result-valuefunctiont-tasktresult-tfunction-map-on-asyncresultextensions) | Maps a successful value through a generated Task-returning callable. |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, Task<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresult-tfunctiontaskresultt-terror-source-valuefunctiont-tasktresult-tfunction-map-on-asyncresultextensions) | Maps a Task-backed result through a generated Task-returning callable. |
| [`ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, Task<TResult>, TFunction> map)`](#overload-valuetaskresulttresult-terror-maptaskasynct-terror-tresult-tfunctionin-valuetaskresultt-terror-source-valuefunctiont-tasktresult-tfunction-map-on-asyncresultextensions) | Maps a completed or pending result through a generated Task-returning callable. |

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, Task<TResult>> map)` on `AsyncResultExtensions`

Maps a successful value through a Task-returning callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** An awaitable result containing the mapped value or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(Task<Result<T, TError>> source, Func<T, Task<TResult>> map)` on `AsyncResultExtensions`

Maps a Task-backed result through a Task-returning callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult>(in ValueTask<Result<T, TError>> source, Func<T, Task<TResult>> map)` on `AsyncResultExtensions`

Maps a completed or pending result through a Task-returning callback.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Callback invoked only for success.

**Returns:** A single-consumption awaitable containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(in Result<T, TError> result, ValueFunction<T, Task<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a successful value through a generated Task-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** An awaitable result containing the mapped value or original failure.

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(Task<Result<T, TError>> source, ValueFunction<T, Task<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a Task-backed result through a generated Task-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** A ValueTask-backed pipeline containing the mapped result.

##### Overload: `ValueTask<Result<TResult, TError>> MapTaskAsync<T, TError, TResult, TFunction>(in ValueTask<Result<T, TError>> source, ValueFunction<T, Task<TResult>, TFunction> map)` on `AsyncResultExtensions`

Maps a completed or pending result through a generated Task-returning callable.

**Type parameters**

- `TResult`: Mapped success type.
- `TFunction`: Generated callable adapter type.

**Parameters**

- `map`: Allocation-free callable token invoked only for success.

**Returns:** A single-consumption awaitable containing the mapped result.


## Package MonadicTypes.NET.Collections

**Types:** [`ResultCollectionExtensions`](#type-resultcollectionextensions)

### Type: `ResultCollectionExtensions`

Provides fail-fast traversal for count-known collections.

| Member | Description |
| --- | --- |
| [`SequenceToArray`](#member-resultcollectionextensionssequencetoarray) | Converts a span of results to one newly allocated array using fail-fast semantics. |
| [`TraverseToArray`](#member-resultcollectionextensionstraversetoarray) | Traverses each item once and returns a newly allocated array of successful values. |

#### Member: `ResultCollectionExtensions.SequenceToArray`

**Example**

```csharp
Result<User[], LoadError> users = results.AsSpan().SequenceToArray();
```

| Overload | Description |
| --- | --- |
| [`Result<T[], TError> SequenceToArray<T, TError>(ReadOnlySpan<Result<T, TError>> source)`](#overload-resultt-terror-sequencetoarrayt-terrorreadonlyspanresultt-terror-source-on-resultcollectionextensions) | Converts a span of results to one newly allocated array using fail-fast semantics. |

##### Overload: `Result<T[], TError> SequenceToArray<T, TError>(ReadOnlySpan<Result<T, TError>> source)` on `ResultCollectionExtensions`

Converts a span of results to one newly allocated array using fail-fast semantics.

Empty input reuses `Empty`. Non-empty input allocates one array.

#### Member: `ResultCollectionExtensions.TraverseToArray`

**Example**

```csharp
Result<User[], LoadError> users = ids.TraverseToArray(LoadUser);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError>(IReadOnlyList<TSource> source, Func<TSource, Result<TResult, TError>> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terrorireadonlylisttsource-source-functsource-resulttresult-terror-selector-on-resultcollectionextensions) | Traverses each item once and returns a newly allocated array of successful values. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError>(ReadOnlySpan<TSource> source, Func<TSource, Result<TResult, TError>> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terrorreadonlyspantsource-source-functsource-resulttresult-terror-selector-on-resultcollectionextensions) | Traverses each span item once and returns a newly allocated array of successful values. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(IReadOnlyList<TSource> source, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terror-tfunctionireadonlylisttsource-source-valuefunctiontsource-resulttresult-terror-tfunction-selector-on-resultcollectionextensions) | Traverses each item through a generated callable wrapper with inferred result types. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TState, TResult, TError>(IReadOnlyList<TSource> source, TState state, Func<TSource, TState, Result<TResult, TError>> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tstate-tresult-terrorireadonlylisttsource-source-tstate-state-functsource-tstate-resulttresult-terror-selector-on-resultcollectionextensions) | Traverses each item once using caller-owned state and returns a new array. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(IReadOnlyList<TSource> source, TFunction selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terror-tfunctionireadonlylisttsource-source-tfunction-selector-on-resultcollectionextensions) | Traverses each item once using an allocation-free callable and returns a new array. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(ReadOnlySpan<TSource> source, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terror-tfunctionreadonlyspantsource-source-valuefunctiontsource-resulttresult-terror-tfunction-selector-on-resultcollectionextensions) | Traverses each span item through a generated callable wrapper with inferred result types. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TState, TResult, TError>(ReadOnlySpan<TSource> source, TState state, Func<TSource, TState, Result<TResult, TError>> selector)`](#overload-resulttresult-terror-traversetoarraytsource-tstate-tresult-terrorreadonlyspantsource-source-tstate-state-functsource-tstate-resulttresult-terror-selector-on-resultcollectionextensions) | Traverses each span item once using caller-owned state and returns a new array. |
| [`Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(ReadOnlySpan<TSource> source, TFunction selector)`](#overload-resulttresult-terror-traversetoarraytsource-tresult-terror-tfunctionreadonlyspantsource-source-tfunction-selector-on-resultcollectionextensions) | Traverses each span item once using an allocation-free callable and returns a new array. |

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError>(IReadOnlyList<TSource> source, Func<TSource, Result<TResult, TError>> selector)` on `ResultCollectionExtensions`

Traverses each item once and returns a newly allocated array of successful values.

Empty input reuses `Empty`. Non-empty input allocates exactly one output array, including when a later item fails.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError>(ReadOnlySpan<TSource> source, Func<TSource, Result<TResult, TError>> selector)` on `ResultCollectionExtensions`

Traverses each span item once and returns a newly allocated array of successful values.

Empty input reuses `Empty`. Non-empty input allocates exactly one output array, including when a later item fails.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(IReadOnlyList<TSource> source, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)` on `ResultCollectionExtensions`

Traverses each item through a generated callable wrapper with inferred result types.

Preserves fail-fast order. Empty input reuses an empty array; non-empty input allocates one output array.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TState, TResult, TError>(IReadOnlyList<TSource> source, TState state, Func<TSource, TState, Result<TResult, TError>> selector)` on `ResultCollectionExtensions`

Traverses each item once using caller-owned state and returns a new array.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(IReadOnlyList<TSource> source, TFunction selector)` on `ResultCollectionExtensions`

Traverses each item once using an allocation-free callable and returns a new array.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(ReadOnlySpan<TSource> source, ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)` on `ResultCollectionExtensions`

Traverses each span item through a generated callable wrapper with inferred result types.

Preserves fail-fast order. Empty input reuses an empty array; non-empty input allocates one output array.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TState, TResult, TError>(ReadOnlySpan<TSource> source, TState state, Func<TSource, TState, Result<TResult, TError>> selector)` on `ResultCollectionExtensions`

Traverses each span item once using caller-owned state and returns a new array.

##### Overload: `Result<TResult[], TError> TraverseToArray<TSource, TResult, TError, TFunction>(ReadOnlySpan<TSource> source, TFunction selector)` on `ResultCollectionExtensions`

Traverses each span item once using an allocation-free callable and returns a new array.


## Package MonadicTypes.NET.Diagnostics

**Types:** [`ErrorActivityStatusPolicy`](#type-erroractivitystatuspolicy) · [`ErrorMetrics`](#type-errormetrics) · [`ErrorTelemetry`](#type-errortelemetry)

### Type: `ErrorActivityStatusPolicy`

Controls whether recording an error changes the current activity status.

**Example**

```csharp
ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Automatic;
```

| Member | Description |
| --- | --- |
| [`Automatic`](#member-erroractivitystatuspolicyautomatic) | Marks categories that normally represent server failures as errors. |
| [`MarkError`](#member-erroractivitystatuspolicymarkerror) | Marks every recorded error category as an activity error. |
| [`Preserve`](#member-erroractivitystatuspolicypreserve) | Records error tags and events without changing the activity status. |

#### Member: `ErrorActivityStatusPolicy.Automatic`

**Example**

```csharp
ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Automatic;
```

| Overload | Description |
| --- | --- |
| [`ErrorActivityStatusPolicy Automatic`](#overload-erroractivitystatuspolicy-automatic-on-erroractivitystatuspolicy) | Marks categories that normally represent server failures as errors. |

##### Overload: `ErrorActivityStatusPolicy Automatic` on `ErrorActivityStatusPolicy`

Marks categories that normally represent server failures as errors.

#### Member: `ErrorActivityStatusPolicy.MarkError`

**Example**

```csharp
ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.MarkError;
```

| Overload | Description |
| --- | --- |
| [`ErrorActivityStatusPolicy MarkError`](#overload-erroractivitystatuspolicy-markerror-on-erroractivitystatuspolicy) | Marks every recorded error category as an activity error. |

##### Overload: `ErrorActivityStatusPolicy MarkError` on `ErrorActivityStatusPolicy`

Marks every recorded error category as an activity error.

#### Member: `ErrorActivityStatusPolicy.Preserve`

**Example**

```csharp
ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Preserve;
```

| Overload | Description |
| --- | --- |
| [`ErrorActivityStatusPolicy Preserve`](#overload-erroractivitystatuspolicy-preserve-on-erroractivitystatuspolicy) | Records error tags and events without changing the activity status. |

##### Overload: `ErrorActivityStatusPolicy Preserve` on `ErrorActivityStatusPolicy`

Records error tags and events without changing the activity status.

### Type: `ErrorMetrics`

Vendor-neutral error counter backed by a caller-owned Meter. Export through OpenTelemetry, Prometheus, or any System.Diagnostics.Metrics listener.

| Member | Description |
| --- | --- |
| [`ErrorMetrics`](#member-errormetricserrormetrics) | Creates an error counter on a caller-owned meter. |
| [`Disabled`](#member-errormetricsdisabled) | Gets a recorder that performs no work and creates no instrument. |
| [`IsEnabled`](#member-errormetricsisenabled) | Gets whether the counter currently has an enabled listener. |
| [`Record`](#member-errormetricsrecord) | Records one observed error when the counter has an enabled listener. |

#### Member: `ErrorMetrics.ErrorMetrics`

**Example**

```csharp
ErrorMetrics metrics = new(meter, includeErrorCode: false);
```

| Overload | Description |
| --- | --- |
| [`ErrorMetrics(Meter meter, bool includeErrorCode, string instrumentName)`](#overload-errormetricsmeter-meter-bool-includeerrorcode-string-instrumentname-on-errormetrics) | Creates an error counter on a caller-owned meter. |

##### Overload: `ErrorMetrics(Meter meter, bool includeErrorCode, string instrumentName)` on `ErrorMetrics`

Creates an error counter on a caller-owned meter.

**Parameters**

- `meter`: The meter through which consumers export measurements.
- `includeErrorCode`: Whether to add the potentially high-cardinality error code tag.
- `instrumentName`: The counter instrument name.

#### Member: `ErrorMetrics.Disabled`

**Example**

```csharp
ErrorMetrics metrics = ErrorMetrics.Disabled;
```

| Overload | Description |
| --- | --- |
| [`ErrorMetrics Disabled`](#overload-errormetrics-disabled-on-errormetrics) | Gets a recorder that performs no work and creates no instrument. |

##### Overload: `ErrorMetrics Disabled` on `ErrorMetrics`

Gets a recorder that performs no work and creates no instrument.

#### Member: `ErrorMetrics.IsEnabled`

**Example**

```csharp
if (metrics.IsEnabled) metrics.Record(error);
```

| Overload | Description |
| --- | --- |
| [`bool IsEnabled`](#overload-bool-isenabled-on-errormetrics) | Gets whether the counter currently has an enabled listener. |

##### Overload: `bool IsEnabled` on `ErrorMetrics`

Gets whether the counter currently has an enabled listener.

#### Member: `ErrorMetrics.Record`

**Example**

```csharp
metrics.Record(error);
```

| Overload | Description |
| --- | --- |
| [`void Record(Error? error)`](#overload-void-recorderror-error-on-errormetrics) | Records one observed error when the counter has an enabled listener. |

##### Overload: `void Record(Error? error)` on `ErrorMetrics`

Records one observed error when the counter has an enabled listener.

**Parameters**

- `error`: The initialized error to categorize and count.

**Throws**

- `ArgumentNullException`: The counter is enabled and `error` is null.

### Type: `ErrorTelemetry`

Explicitly records an observed error. Call this once at an application boundary; constructing or propagating an Error has no telemetry side effects.

| Member | Description |
| --- | --- |
| [`Record`](#member-errortelemetryrecord) | Records an error on a sampled activity without creating an activity. |

#### Member: `ErrorTelemetry.Record`

**Example**

```csharp
ErrorTelemetry.Record(Activity.Current, error);
```

| Overload | Description |
| --- | --- |
| [`void Record(Activity? activity, Error? error, ErrorActivityStatusPolicy statusPolicy)`](#overload-void-recordactivity-activity-error-error-erroractivitystatuspolicy-statuspolicy-on-errortelemetry) | Records an error on a sampled activity without creating an activity. |

##### Overload: `void Record(Activity? activity, Error? error, ErrorActivityStatusPolicy statusPolicy)` on `ErrorTelemetry`

Records an error on a sampled activity without creating an activity.

The diagnostic message is written to the `error.message` activity tag, while `IsMessagePublic` controls HTTP disclosure only. Keep diagnostic messages safe for the telemetry backends used by the application; project a redacted error yourself when that is not possible.

**Parameters**

- `activity`: The caller-owned activity, or null to perform no work.
- `error`: The initialized error to record.
- `statusPolicy`: The policy controlling activity status mutation.

**Throws**

- `ArgumentNullException`: The activity is sampled and `error` is null.
- `ArgumentOutOfRangeException`: `statusPolicy` or the error category is invalid.


## Package MonadicTypes.NET.Effects

**Types:** [`Effect`](#type-effect) · [`ResultEffectExtensions`](#type-resulteffectextensions)

### Type: `Effect`

Executes explicitly fallible effects and converts selected exceptions into result failures.

| Member | Description |
| --- | --- |
| [`Try`](#member-effecttry) | Executes a synchronous effect and converts recoverable exceptions to failures. |
| [`TryAsync`](#member-effecttryasync) | Executes a ValueTask-producing effect and converts recoverable exceptions to failures. |
| [`TryTaskAsync`](#member-effecttrytaskasync) | Executes a Task-producing effect and converts recoverable exceptions to failures. |

#### Member: `Effect.Try`

**Example**

```csharp
Result<Config, ReadError> config = Effect.Try(ReadConfig, ReadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TError> Try<T, TError>(Func<T> operation, Func<Exception, TError> mapException)`](#overload-resultt-terror-tryt-terrorfunct-operation-funcexception-terror-mapexception-on-effect) | Executes a synchronous effect and converts recoverable exceptions to failures. |
| [`Result<T, TError> Try<T, TError, TException>(Func<T> operation, Func<TException, TError> mapException)`](#overload-resultt-terror-tryt-terror-texceptionfunct-operation-functexception-terror-mapexception-on-effect) | Executes a synchronous effect and converts only the selected exception type. |

##### Overload: `Result<T, TError> Try<T, TError>(Func<T> operation, Func<Exception, TError> mapException)` on `Effect`

Executes a synchronous effect and converts recoverable exceptions to failures.

Cancellation and fatal runtime exceptions propagate. Use the typed overload when cancellation or another normally excluded exception must be represented explicitly.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** The effect value or mapped failure.

##### Overload: `Result<T, TError> Try<T, TError, TException>(Func<T> operation, Func<TException, TError> mapException)` on `Effect`

Executes a synchronous effect and converts only the selected exception type.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.
- `TException`: Exception type to convert.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** The effect value or mapped failure.

#### Member: `Effect.TryAsync`

**Example**

```csharp
Result<User, LoadError> user = await Effect.TryAsync(LoadUserAsync, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TError>> TryAsync<T, TError>(Func<ValueTask<T>> operation, Func<Exception, TError> mapException)`](#overload-valuetaskresultt-terror-tryasynct-terrorfuncvaluetaskt-operation-funcexception-terror-mapexception-on-effect) | Executes a ValueTask-producing effect and converts recoverable exceptions to failures. |
| [`ValueTask<Result<T, TError>> TryAsync<T, TError, TException>(Func<ValueTask<T>> operation, Func<TException, TError> mapException)`](#overload-valuetaskresultt-terror-tryasynct-terror-texceptionfuncvaluetaskt-operation-functexception-terror-mapexception-on-effect) | Executes a ValueTask-producing effect and converts only the selected exception type. |

##### Overload: `ValueTask<Result<T, TError>> TryAsync<T, TError>(Func<ValueTask<T>> operation, Func<Exception, TError> mapException)` on `Effect`

Executes a ValueTask-producing effect and converts recoverable exceptions to failures.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

##### Overload: `ValueTask<Result<T, TError>> TryAsync<T, TError, TException>(Func<ValueTask<T>> operation, Func<TException, TError> mapException)` on `Effect`

Executes a ValueTask-producing effect and converts only the selected exception type.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.
- `TException`: Exception type to convert.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

#### Member: `Effect.TryTaskAsync`

**Example**

```csharp
Result<User, LoadError> user = await Effect.TryTaskAsync(LoadUserTaskAsync, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TError>> TryTaskAsync<T, TError>(Func<Task<T>> operation, Func<Exception, TError> mapException)`](#overload-valuetaskresultt-terror-trytaskasynct-terrorfunctaskt-operation-funcexception-terror-mapexception-on-effect) | Executes a Task-producing effect and converts recoverable exceptions to failures. |
| [`ValueTask<Result<T, TError>> TryTaskAsync<T, TError, TException>(Func<Task<T>> operation, Func<TException, TError> mapException)`](#overload-valuetaskresultt-terror-trytaskasynct-terror-texceptionfunctaskt-operation-functexception-terror-mapexception-on-effect) | Executes a Task-producing effect and converts only the selected exception type. |
| [`ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError>(TState state, Func<TState, Task<T>> operation, Func<Exception, TError> mapException)`](#overload-valuetaskresultt-terror-trytaskasynctstate-t-terrortstate-state-functstate-taskt-operation-funcexception-terror-mapexception-on-effect) | Executes a Task effect with caller-owned state and converts recoverable exceptions. |
| [`ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError, TException>(TState state, Func<TState, Task<T>> operation, Func<TException, TError> mapException)`](#overload-valuetaskresultt-terror-trytaskasynctstate-t-terror-texceptiontstate-state-functstate-taskt-operation-functexception-terror-mapexception-on-effect) | Executes a Task effect with caller-owned state and converts one exception type. |

##### Overload: `ValueTask<Result<T, TError>> TryTaskAsync<T, TError>(Func<Task<T>> operation, Func<Exception, TError> mapException)` on `Effect`

Executes a Task-producing effect and converts recoverable exceptions to failures.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

##### Overload: `ValueTask<Result<T, TError>> TryTaskAsync<T, TError, TException>(Func<Task<T>> operation, Func<TException, TError> mapException)` on `Effect`

Executes a Task-producing effect and converts only the selected exception type.

**Type parameters**

- `T`: Effect value type.
- `TError`: Failure type.
- `TException`: Exception type to convert.

**Parameters**

- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

##### Overload: `ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError>(TState state, Func<TState, Task<T>> operation, Func<Exception, TError> mapException)` on `Effect`

Executes a Task effect with caller-owned state and converts recoverable exceptions.

**Type parameters**

- `TState`: Caller state passed to the operation.
- `T`: Effect value type.
- `TError`: Failure type.

**Parameters**

- `state`: State passed unchanged to `operation`.
- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

##### Overload: `ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError, TException>(TState state, Func<TState, Task<T>> operation, Func<TException, TError> mapException)` on `Effect`

Executes a Task effect with caller-owned state and converts one exception type.

**Type parameters**

- `TState`: Caller state passed to the operation.
- `T`: Effect value type.
- `TError`: Failure type.
- `TException`: Exception type to convert.

**Parameters**

- `state`: State passed unchanged to `operation`.
- `operation`: Effect to execute exactly once.
- `mapException`: Maps a caught exception to a failure.

**Returns:** An awaitable containing the effect value or mapped failure.

### Type: `ResultEffectExtensions`

Provides explicit exception-catching composition for result pipelines.

| Member | Description |
| --- | --- |
| [`TryBind`](#member-resulteffectextensionstrybind) | Binds success while converting recoverable callback exceptions to failures. |
| [`TryMap`](#member-resulteffectextensionstrymap) | Maps success while converting recoverable callback exceptions to failures. |
| [`TryMapAsync`](#member-resulteffectextensionstrymapasync) | Maps success asynchronously while converting recoverable exceptions to failures. |
| [`TryTap`](#member-resulteffectextensionstrytap) | Runs a success side effect while converting recoverable callback exceptions to failures. |
| [`TryTapAsync`](#member-resulteffectextensionstrytapasync) | Runs an asynchronous success side effect and converts recoverable exceptions to failures. |

#### Member: `ResultEffectExtensions.TryBind`

**Example**

```csharp
Result<User, LoadError> loaded = id.TryBind(LoadUser, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TError> TryBind<T, TError, TResult>(in Result<T, TError> result, Func<T, Result<TResult, TError>> bind, Func<Exception, TError> mapException)`](#overload-resulttresult-terror-trybindt-terror-tresultin-resultt-terror-result-funct-resulttresult-terror-bind-funcexception-terror-mapexception-on-resulteffectextensions) | Binds success while converting recoverable callback exceptions to failures. |

##### Overload: `Result<TResult, TError> TryBind<T, TError, TResult>(in Result<T, TError> result, Func<T, Result<TResult, TError>> bind, Func<Exception, TError> mapException)` on `ResultEffectExtensions`

Binds success while converting recoverable callback exceptions to failures.

**Type parameters**

- `TResult`: Continuation success type.

**Parameters**

- `bind`: Potentially throwing continuation invoked only for success.
- `mapException`: Maps a caught exception to the result error type.

**Returns:** The continuation result, original failure, or mapped exception failure.

#### Member: `ResultEffectExtensions.TryMap`

**Example**

```csharp
Result<Config, ReadError> parsed = text.TryMap(ParseConfig, ReadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TError> TryMap<T, TError, TResult>(in Result<T, TError> result, Func<T, TResult> map, Func<Exception, TError> mapException)`](#overload-resulttresult-terror-trymapt-terror-tresultin-resultt-terror-result-funct-tresult-map-funcexception-terror-mapexception-on-resulteffectextensions) | Maps success while converting recoverable callback exceptions to failures. |

##### Overload: `Result<TResult, TError> TryMap<T, TError, TResult>(in Result<T, TError> result, Func<T, TResult> map, Func<Exception, TError> mapException)` on `ResultEffectExtensions`

Maps success while converting recoverable callback exceptions to failures.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Potentially throwing callback invoked only for success.
- `mapException`: Maps a caught exception to the result error type.

**Returns:** The mapped success, original failure, or mapped exception failure.

#### Member: `ResultEffectExtensions.TryMapAsync`

**Example**

```csharp
Result<UserDto, LoadError> mapped = await result.TryMapAsync(LoadDtoAsync, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<TResult, TError>> TryMapAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<TResult>> map, Func<Exception, TError> mapException)`](#overload-valuetaskresulttresult-terror-trymapasynct-terror-tresultin-resultt-terror-result-funct-valuetasktresult-map-funcexception-terror-mapexception-on-resulteffectextensions) | Maps success asynchronously while converting recoverable exceptions to failures. |

##### Overload: `ValueTask<Result<TResult, TError>> TryMapAsync<T, TError, TResult>(in Result<T, TError> result, Func<T, ValueTask<TResult>> map, Func<Exception, TError> mapException)` on `ResultEffectExtensions`

Maps success asynchronously while converting recoverable exceptions to failures.

**Type parameters**

- `TResult`: Mapped success type.

**Parameters**

- `map`: Potentially throwing asynchronous callback invoked only for success.
- `mapException`: Maps a caught exception to the result error type.

**Returns:** An awaitable containing the mapped success, original failure, or exception failure.

#### Member: `ResultEffectExtensions.TryTap`

**Example**

```csharp
Result<User, LoadError> observed = result.TryTap(Audit, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`Result<T, TError> TryTap<T, TError>(in Result<T, TError> result, Action<T> action, Func<Exception, TError> mapException)`](#overload-resultt-terror-trytapt-terrorin-resultt-terror-result-actiont-action-funcexception-terror-mapexception-on-resulteffectextensions) | Runs a success side effect while converting recoverable callback exceptions to failures. |

##### Overload: `Result<T, TError> TryTap<T, TError>(in Result<T, TError> result, Action<T> action, Func<Exception, TError> mapException)` on `ResultEffectExtensions`

Runs a success side effect while converting recoverable callback exceptions to failures.

**Parameters**

- `action`: Potentially throwing action invoked only for success.
- `mapException`: Maps a caught exception to the result error type.

**Returns:** The original result or a mapped exception failure.

#### Member: `ResultEffectExtensions.TryTapAsync`

**Example**

```csharp
Result<User, LoadError> observed = await result.TryTapAsync(AuditAsync, LoadError.FromException);
```

| Overload | Description |
| --- | --- |
| [`ValueTask<Result<T, TError>> TryTapAsync<T, TError>(in Result<T, TError> result, Func<T, ValueTask> action, Func<Exception, TError> mapException)`](#overload-valuetaskresultt-terror-trytapasynct-terrorin-resultt-terror-result-funct-valuetask-action-funcexception-terror-mapexception-on-resulteffectextensions) | Runs an asynchronous success side effect and converts recoverable exceptions to failures. |

##### Overload: `ValueTask<Result<T, TError>> TryTapAsync<T, TError>(in Result<T, TError> result, Func<T, ValueTask> action, Func<Exception, TError> mapException)` on `ResultEffectExtensions`

Runs an asynchronous success side effect and converts recoverable exceptions to failures.

**Parameters**

- `action`: Potentially throwing action invoked only for success.
- `mapException`: Maps a caught exception to the result error type.

**Returns:** An awaitable containing the original result or mapped exception failure.


## Package MonadicTypes.NET.Errors

**Types:** [`Error`](#type-error) · [`ErrorType`](#type-errortype) · [`IErrorConvertible<TError>`](#type-ierrorconvertibleterror) · [`ResultErrorExtensions`](#type-resulterrorextensions) · [`ValidationErrors`](#type-validationerrors) · [`ValidationIssue`](#type-validationissue) · [`ValidationSeverity`](#type-validationseverity)

### Type: `Error`

A structured error occurrence. `Code` identifies the failure for machines and telemetry; `Message` is diagnostic text and is exposed to clients only when `IsMessagePublic` is true.

**Example**

```csharp
Error error = Error.NotFound("USER_NOT_FOUND", "The user does not exist.");
```

| Member | Description |
| --- | --- |
| [`Error`](#member-errorerror) | Creates an error in a built-in category. |
| [`Cancelled`](#member-errorcancelled) | Creates a cancellation error. |
| [`Cause`](#member-errorcause) | Retained exception for telemetry. Use `ThrowCause` rather than throwing this property directly when exception propagation is required. |
| [`Code`](#member-errorcode) | Gets the stable machine-readable error code. |
| [`Conflict`](#member-errorconflict) | Creates a conflict error. |
| [`Custom`](#member-errorcustom) | Creates a consumer-defined error category with a positive numeric identifier. |
| [`Equals`](#member-errorequals) | Compares semantic fields and retained-cause identity. |
| [`Failure`](#member-errorfailure) | Creates a general failure with the default code. |
| [`Forbidden`](#member-errorforbidden) | Creates an authorization-denied error. |
| [`GetHashCode`](#member-errorgethashcode) | Hashes the same fields used by `Equals`. |
| [`IO`](#member-errorio) | Creates a general input/output failure with the standard code. |
| [`IsMessagePublic`](#member-errorismessagepublic) | Gets whether adapters may safely expose `Message` to clients. |
| [`Message`](#member-errormessage) | Gets the diagnostic message. |
| [`NotFound`](#member-errornotfound) | Creates a resource-not-found error. |
| [`NumericType`](#member-errornumerictype) | Gets the stable numeric category, including custom categories. |
| [`RateLimited`](#member-errorratelimited) | Creates a rate-limit error. |
| [`System`](#member-errorsystem) | Creates an unexpected system failure with the standard code. |
| [`ThrowCause`](#member-errorthrowcause) | Rethrows the retained cause while preserving its original stack trace. |
| [`Timeout`](#member-errortimeout) | Creates a timeout error. |
| [`ToString`](#member-errortostring) | Formats the error as `[Code] Message`. |
| [`TryFormat`](#member-errortryformat) | Attempts to write `[Code] Message` into caller-owned storage. |
| [`Type`](#member-errortype) | Gets the broad built-in category. |
| [`Unauthorized`](#member-errorunauthorized) | Creates an authentication-required error. |
| [`Unavailable`](#member-errorunavailable) | Creates a service-unavailable error. |
| [`Unexpected`](#member-errorunexpected) | Creates an unexpected failure that retains `cause` for telemetry and rethrow. |
| [`Validation`](#member-errorvalidation) | Creates a public validation failure with the default code. |

#### Member: `Error.Error`

**Example**

```csharp
Error error = new(ErrorType.NotFound, "USER_NOT_FOUND", "The user does not exist.", true);
```

| Overload | Description |
| --- | --- |
| [`Error(ErrorType type, string code, string message, bool isMessagePublic, Exception? cause)`](#overload-errorerrortype-type-string-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates an error in a built-in category. |
| [`Error(string code, string message)`](#overload-errorstring-code-string-message-on-error) | Creates a general failure with a private diagnostic message. |

##### Overload: `Error(ErrorType type, string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates an error in a built-in category.

##### Overload: `Error(string code, string message)` on `Error`

Creates a general failure with a private diagnostic message.

#### Member: `Error.Cancelled`

**Example**

```csharp
Error error = Error.Cancelled("OPERATION_CANCELLED", "Operation cancelled.");
```

| Overload | Description |
| --- | --- |
| [`Error Cancelled(string code, string message, Exception? cause)`](#overload-error-cancelledstring-code-string-message-exception-cause-on-error) | Creates a cancellation error. |

##### Overload: `Error Cancelled(string code, string message, Exception? cause)` on `Error`

Creates a cancellation error.

#### Member: `Error.Cause`

**Example**

```csharp
Exception? cause = error.Cause;
```

| Overload | Description |
| --- | --- |
| [`Exception? Cause`](#overload-exception-cause-on-error) | Retained exception for telemetry. Use `ThrowCause` rather than throwing this property directly when exception propagation is required. |

##### Overload: `Exception? Cause` on `Error`

Retained exception for telemetry. Use `ThrowCause` rather than throwing this property directly when exception propagation is required.

#### Member: `Error.Code`

**Example**

```csharp
logger.LogWarning("Failure {Code}", error.Code);
```

| Overload | Description |
| --- | --- |
| [`string Code`](#overload-string-code-on-error) | Gets the stable machine-readable error code. |

##### Overload: `string Code` on `Error`

Gets the stable machine-readable error code.

#### Member: `Error.Conflict`

**Example**

```csharp
Error error = Error.Conflict("VERSION_CONFLICT", "The resource changed.");
```

| Overload | Description |
| --- | --- |
| [`Error Conflict(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-conflictstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a conflict error. |

##### Overload: `Error Conflict(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a conflict error.

#### Member: `Error.Custom`

**Example**

```csharp
Error error = Error.Custom(10_001, "VENDOR_REJECTED", "The vendor rejected the request.");
```

| Overload | Description |
| --- | --- |
| [`Error Custom(int numericType, string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-customint-numerictype-string-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a consumer-defined error category with a positive numeric identifier. |

##### Overload: `Error Custom(int numericType, string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a consumer-defined error category with a positive numeric identifier.

#### Member: `Error.Equals`

**Example**

```csharp
bool equal = left.Equals(right);
```

| Overload | Description |
| --- | --- |
| [`bool Equals(Error? other)`](#overload-bool-equalserror-other-on-error) | Compares semantic fields and retained-cause identity. |

##### Overload: `bool Equals(Error? other)` on `Error`

Compares semantic fields and retained-cause identity.

#### Member: `Error.Failure`

**Example**

```csharp
Error error = Error.Failure("Operation failed.");
```

| Overload | Description |
| --- | --- |
| [`Error Failure(string message)`](#overload-error-failurestring-message-on-error) | Creates a general failure with the default code. |
| [`Error Failure(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-failurestring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a general failure with a caller-defined code and visibility. |

##### Overload: `Error Failure(string message)` on `Error`

Creates a general failure with the default code.

##### Overload: `Error Failure(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a general failure with a caller-defined code and visibility.

#### Member: `Error.Forbidden`

**Example**

```csharp
Error error = Error.Forbidden("ACCESS_DENIED", "Access is denied.");
```

| Overload | Description |
| --- | --- |
| [`Error Forbidden(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-forbiddenstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates an authorization-denied error. |

##### Overload: `Error Forbidden(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates an authorization-denied error.

#### Member: `Error.GetHashCode`

**Example**

```csharp
int hash = error.GetHashCode();
```

| Overload | Description |
| --- | --- |
| [`int GetHashCode()`](#overload-int-gethashcode-on-error) | Hashes the same fields used by `Equals`. |

##### Overload: `int GetHashCode()` on `Error`

Hashes the same fields used by `Equals`.

#### Member: `Error.IO`

**Example**

```csharp
Error error = Error.IO("Unable to read the file.");
```

| Overload | Description |
| --- | --- |
| [`Error IO(string message)`](#overload-error-iostring-message-on-error) | Creates a general input/output failure with the standard code. |
| [`Error IO(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-iostring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates an input/output failure with a caller-defined code, visibility, and optional retained cause. |

##### Overload: `Error IO(string message)` on `Error`

Creates a general input/output failure with the standard code.

##### Overload: `Error IO(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates an input/output failure with a caller-defined code, visibility, and optional retained cause.

#### Member: `Error.IsMessagePublic`

**Example**

```csharp
string detail = error.IsMessagePublic ? error.Message : "Request failed.";
```

| Overload | Description |
| --- | --- |
| [`bool IsMessagePublic`](#overload-bool-ismessagepublic-on-error) | Gets whether adapters may safely expose `Message` to clients. |

##### Overload: `bool IsMessagePublic` on `Error`

Gets whether adapters may safely expose `Message` to clients.

#### Member: `Error.Message`

**Example**

```csharp
logger.LogWarning("{Message}", error.Message);
```

| Overload | Description |
| --- | --- |
| [`string Message`](#overload-string-message-on-error) | Gets the diagnostic message. |

##### Overload: `string Message` on `Error`

Gets the diagnostic message.

#### Member: `Error.NotFound`

**Example**

```csharp
Error error = Error.NotFound("USER_NOT_FOUND", "The user does not exist.");
```

| Overload | Description |
| --- | --- |
| [`Error NotFound(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-notfoundstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a resource-not-found error. |

##### Overload: `Error NotFound(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a resource-not-found error.

#### Member: `Error.NumericType`

**Example**

```csharp
int category = error.NumericType;
```

| Overload | Description |
| --- | --- |
| [`int NumericType`](#overload-int-numerictype-on-error) | Gets the stable numeric category, including custom categories. |

##### Overload: `int NumericType` on `Error`

Gets the stable numeric category, including custom categories.

#### Member: `Error.RateLimited`

**Example**

```csharp
Error error = Error.RateLimited("RATE_LIMITED", "Try again later.");
```

| Overload | Description |
| --- | --- |
| [`Error RateLimited(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-ratelimitedstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a rate-limit error. |

##### Overload: `Error RateLimited(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a rate-limit error.

#### Member: `Error.System`

**Example**

```csharp
Error error = Error.System("System operation failed.");
```

| Overload | Description |
| --- | --- |
| [`Error System(string message)`](#overload-error-systemstring-message-on-error) | Creates an unexpected system failure with the standard code. |
| [`Error System(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-systemstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a system failure with a caller-defined code, visibility, and optional retained cause. |

##### Overload: `Error System(string message)` on `Error`

Creates an unexpected system failure with the standard code.

##### Overload: `Error System(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a system failure with a caller-defined code, visibility, and optional retained cause.

#### Member: `Error.ThrowCause`

**Example**

```csharp
if (error.Cause is not null) error.ThrowCause();
```

| Overload | Description |
| --- | --- |
| [`void ThrowCause()`](#overload-void-throwcause-on-error) | Rethrows the retained cause while preserving its original stack trace. |

##### Overload: `void ThrowCause()` on `Error`

Rethrows the retained cause while preserving its original stack trace.

**Throws**

- `InvalidOperationException`: No cause is retained.

#### Member: `Error.Timeout`

**Example**

```csharp
Error error = Error.Timeout("REQUEST_TIMEOUT", "The operation timed out.");
```

| Overload | Description |
| --- | --- |
| [`Error Timeout(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-timeoutstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a timeout error. |

##### Overload: `Error Timeout(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a timeout error.

#### Member: `Error.ToString`

**Example**

```csharp
string diagnostic = error.ToString();
```

| Overload | Description |
| --- | --- |
| [`string ToString()`](#overload-string-tostring-on-error) | Formats the error as `[Code] Message`. |
| [`string ToString(string? format, IFormatProvider? formatProvider)`](#overload-string-tostringstring-format-iformatprovider-formatprovider-on-error) | Formats the error using the general format. |

##### Overload: `string ToString()` on `Error`

Formats the error as `[Code] Message`.

**Returns:** The diagnostic representation.

##### Overload: `string ToString(string? format, IFormatProvider? formatProvider)` on `Error`

Formats the error using the general format.

**Parameters**

- `format`: Empty, null, or `G`.
- `formatProvider`: Ignored because error formatting is culture independent.

**Returns:** The diagnostic representation.

**Throws**

- `FormatException`: `format` is not empty and is not `G`.

#### Member: `Error.TryFormat`

**Example**

```csharp
Span<char> buffer = stackalloc char[128]; bool written = error.TryFormat(buffer, out int count, default, null);
```

| Overload | Description |
| --- | --- |
| [`bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)`](#overload-bool-tryformatspanchar-destination-out-int-charswritten-readonlyspanchar-format-iformatprovider-provider-on-error) | Attempts to write `[Code] Message` into caller-owned storage. |

##### Overload: `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)` on `Error`

Attempts to write `[Code] Message` into caller-owned storage.

**Parameters**

- `destination`: Destination buffer.
- `charsWritten`: Number of characters written, or zero when the buffer is too small.
- `format`: Empty or `G`.
- `provider`: Ignored because error formatting is culture independent.

**Returns:** True when the complete representation was written.

**Throws**

- `FormatException`: `format` is not empty and is not `G`.

#### Member: `Error.Type`

**Example**

```csharp
ErrorType category = error.Type;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Type`](#overload-errortype-type-on-error) | Gets the broad built-in category. |

##### Overload: `ErrorType Type` on `Error`

Gets the broad built-in category.

#### Member: `Error.Unauthorized`

**Example**

```csharp
Error error = Error.Unauthorized("AUTH_REQUIRED", "Authentication is required.");
```

| Overload | Description |
| --- | --- |
| [`Error Unauthorized(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-unauthorizedstring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates an authentication-required error. |

##### Overload: `Error Unauthorized(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates an authentication-required error.

#### Member: `Error.Unavailable`

**Example**

```csharp
Error error = Error.Unavailable("STORE_UNAVAILABLE", "Store unavailable.");
```

| Overload | Description |
| --- | --- |
| [`Error Unavailable(string code, string message, bool isMessagePublic, Exception? cause)`](#overload-error-unavailablestring-code-string-message-bool-ismessagepublic-exception-cause-on-error) | Creates a service-unavailable error. |

##### Overload: `Error Unavailable(string code, string message, bool isMessagePublic, Exception? cause)` on `Error`

Creates a service-unavailable error.

#### Member: `Error.Unexpected`

**Example**

```csharp
Error error = Error.Unexpected("Unexpected failure.");
```

| Overload | Description |
| --- | --- |
| [`Error Unexpected(Exception cause, string code)`](#overload-error-unexpectedexception-cause-string-code-on-error) | Creates an unexpected failure that retains `cause` for telemetry and rethrow. |
| [`Error Unexpected(string message)`](#overload-error-unexpectedstring-message-on-error) | Creates an unexpected failure without a retained exception. |

##### Overload: `Error Unexpected(Exception cause, string code)` on `Error`

Creates an unexpected failure that retains `cause` for telemetry and rethrow.

##### Overload: `Error Unexpected(string message)` on `Error`

Creates an unexpected failure without a retained exception.

#### Member: `Error.Validation`

**Example**

```csharp
Error error = Error.Validation("Email is invalid.");
```

| Overload | Description |
| --- | --- |
| [`Error Validation(string message)`](#overload-error-validationstring-message-on-error) | Creates a public validation failure with the default code. |
| [`Error Validation(string code, string message, Exception? cause)`](#overload-error-validationstring-code-string-message-exception-cause-on-error) | Creates a public validation failure with a caller-defined code. |

##### Overload: `Error Validation(string message)` on `Error`

Creates a public validation failure with the default code.

##### Overload: `Error Validation(string code, string message, Exception? cause)` on `Error`

Creates a public validation failure with a caller-defined code.

### Type: `ErrorType`

Broad operational category used by adapters and telemetry policy.

**Example**

```csharp
ErrorType category = ErrorType.Validation;
```

| Member | Description |
| --- | --- |
| [`BadGateway`](#member-errortypebadgateway) | An upstream gateway returned an unusable response. |
| [`Cancelled`](#member-errortypecancelled) | An operation was cancelled. |
| [`Conflict`](#member-errortypeconflict) | A state or concurrency conflict. |
| [`ContentTooLarge`](#member-errortypecontenttoolarge) | The request content exceeded the accepted limit. |
| [`Custom`](#member-errortypecustom) | A consumer-defined category identified by `NumericType`. |
| [`Failure`](#member-errortypefailure) | A general expected operational failure. |
| [`Forbidden`](#member-errortypeforbidden) | The authenticated caller lacks permission. |
| [`Gone`](#member-errortypegone) | The requested resource was deliberately removed. |
| [`Locked`](#member-errortypelocked) | The target resource is locked. |
| [`NotAcceptable`](#member-errortypenotacceptable) | The requested representation is not acceptable. |
| [`NotFound`](#member-errortypenotfound) | A requested resource does not exist. |
| [`NotImplemented`](#member-errortypenotimplemented) | The requested operation is not implemented. |
| [`PaymentRequired`](#member-errortypepaymentrequired) | A payment or entitlement requirement prevented the operation. |
| [`PreconditionFailed`](#member-errortypepreconditionfailed) | A supplied request precondition was not met. |
| [`PreconditionRequired`](#member-errortypepreconditionrequired) | The request requires a precondition. |
| [`RateLimited`](#member-errortyperatelimited) | A caller exceeded a rate or quota limit. |
| [`RequestTimeout`](#member-errortyperequesttimeout) | The client took too long to send the request. |
| [`Timeout`](#member-errortypetimeout) | An operation exceeded its time budget. |
| [`Unauthorized`](#member-errortypeunauthorized) | Authentication is absent or invalid. |
| [`Unavailable`](#member-errortypeunavailable) | A dependency or service is temporarily unavailable. |
| [`Unexpected`](#member-errortypeunexpected) | An unexpected or internal failure. |
| [`Uninitialized`](#member-errortypeuninitialized) | An invalid default value that no constructed error may use. |
| [`UnprocessableContent`](#member-errortypeunprocessablecontent) | The request was valid but could not be processed semantically. |
| [`UnsupportedMediaType`](#member-errortypeunsupportedmediatype) | The request media type is not supported. |
| [`Validation`](#member-errortypevalidation) | Invalid input or business-rule validation. |

#### Member: `ErrorType.BadGateway`

| Overload | Description |
| --- | --- |
| [`ErrorType BadGateway`](#overload-errortype-badgateway-on-errortype) | An upstream gateway returned an unusable response. |

##### Overload: `ErrorType BadGateway` on `ErrorType`

An upstream gateway returned an unusable response.

#### Member: `ErrorType.Cancelled`

**Example**

```csharp
ErrorType category = ErrorType.Cancelled;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Cancelled`](#overload-errortype-cancelled-on-errortype) | An operation was cancelled. |

##### Overload: `ErrorType Cancelled` on `ErrorType`

An operation was cancelled.

#### Member: `ErrorType.Conflict`

**Example**

```csharp
ErrorType category = ErrorType.Conflict;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Conflict`](#overload-errortype-conflict-on-errortype) | A state or concurrency conflict. |

##### Overload: `ErrorType Conflict` on `ErrorType`

A state or concurrency conflict.

#### Member: `ErrorType.ContentTooLarge`

| Overload | Description |
| --- | --- |
| [`ErrorType ContentTooLarge`](#overload-errortype-contenttoolarge-on-errortype) | The request content exceeded the accepted limit. |

##### Overload: `ErrorType ContentTooLarge` on `ErrorType`

The request content exceeded the accepted limit.

#### Member: `ErrorType.Custom`

**Example**

```csharp
ErrorType category = ErrorType.Custom;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Custom`](#overload-errortype-custom-on-errortype) | A consumer-defined category identified by `NumericType`. |

##### Overload: `ErrorType Custom` on `ErrorType`

A consumer-defined category identified by `NumericType`.

#### Member: `ErrorType.Failure`

**Example**

```csharp
ErrorType category = ErrorType.Failure;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Failure`](#overload-errortype-failure-on-errortype) | A general expected operational failure. |

##### Overload: `ErrorType Failure` on `ErrorType`

A general expected operational failure.

#### Member: `ErrorType.Forbidden`

**Example**

```csharp
ErrorType category = ErrorType.Forbidden;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Forbidden`](#overload-errortype-forbidden-on-errortype) | The authenticated caller lacks permission. |

##### Overload: `ErrorType Forbidden` on `ErrorType`

The authenticated caller lacks permission.

#### Member: `ErrorType.Gone`

| Overload | Description |
| --- | --- |
| [`ErrorType Gone`](#overload-errortype-gone-on-errortype) | The requested resource was deliberately removed. |

##### Overload: `ErrorType Gone` on `ErrorType`

The requested resource was deliberately removed.

#### Member: `ErrorType.Locked`

| Overload | Description |
| --- | --- |
| [`ErrorType Locked`](#overload-errortype-locked-on-errortype) | The target resource is locked. |

##### Overload: `ErrorType Locked` on `ErrorType`

The target resource is locked.

#### Member: `ErrorType.NotAcceptable`

| Overload | Description |
| --- | --- |
| [`ErrorType NotAcceptable`](#overload-errortype-notacceptable-on-errortype) | The requested representation is not acceptable. |

##### Overload: `ErrorType NotAcceptable` on `ErrorType`

The requested representation is not acceptable.

#### Member: `ErrorType.NotFound`

**Example**

```csharp
ErrorType category = ErrorType.NotFound;
```

| Overload | Description |
| --- | --- |
| [`ErrorType NotFound`](#overload-errortype-notfound-on-errortype) | A requested resource does not exist. |

##### Overload: `ErrorType NotFound` on `ErrorType`

A requested resource does not exist.

#### Member: `ErrorType.NotImplemented`

| Overload | Description |
| --- | --- |
| [`ErrorType NotImplemented`](#overload-errortype-notimplemented-on-errortype) | The requested operation is not implemented. |

##### Overload: `ErrorType NotImplemented` on `ErrorType`

The requested operation is not implemented.

#### Member: `ErrorType.PaymentRequired`

| Overload | Description |
| --- | --- |
| [`ErrorType PaymentRequired`](#overload-errortype-paymentrequired-on-errortype) | A payment or entitlement requirement prevented the operation. |

##### Overload: `ErrorType PaymentRequired` on `ErrorType`

A payment or entitlement requirement prevented the operation.

#### Member: `ErrorType.PreconditionFailed`

| Overload | Description |
| --- | --- |
| [`ErrorType PreconditionFailed`](#overload-errortype-preconditionfailed-on-errortype) | A supplied request precondition was not met. |

##### Overload: `ErrorType PreconditionFailed` on `ErrorType`

A supplied request precondition was not met.

#### Member: `ErrorType.PreconditionRequired`

| Overload | Description |
| --- | --- |
| [`ErrorType PreconditionRequired`](#overload-errortype-preconditionrequired-on-errortype) | The request requires a precondition. |

##### Overload: `ErrorType PreconditionRequired` on `ErrorType`

The request requires a precondition.

#### Member: `ErrorType.RateLimited`

**Example**

```csharp
ErrorType category = ErrorType.RateLimited;
```

| Overload | Description |
| --- | --- |
| [`ErrorType RateLimited`](#overload-errortype-ratelimited-on-errortype) | A caller exceeded a rate or quota limit. |

##### Overload: `ErrorType RateLimited` on `ErrorType`

A caller exceeded a rate or quota limit.

#### Member: `ErrorType.RequestTimeout`

| Overload | Description |
| --- | --- |
| [`ErrorType RequestTimeout`](#overload-errortype-requesttimeout-on-errortype) | The client took too long to send the request. |

##### Overload: `ErrorType RequestTimeout` on `ErrorType`

The client took too long to send the request.

#### Member: `ErrorType.Timeout`

**Example**

```csharp
ErrorType category = ErrorType.Timeout;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Timeout`](#overload-errortype-timeout-on-errortype) | An operation exceeded its time budget. |

##### Overload: `ErrorType Timeout` on `ErrorType`

An operation exceeded its time budget.

#### Member: `ErrorType.Unauthorized`

**Example**

```csharp
ErrorType category = ErrorType.Unauthorized;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Unauthorized`](#overload-errortype-unauthorized-on-errortype) | Authentication is absent or invalid. |

##### Overload: `ErrorType Unauthorized` on `ErrorType`

Authentication is absent or invalid.

#### Member: `ErrorType.Unavailable`

**Example**

```csharp
ErrorType category = ErrorType.Unavailable;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Unavailable`](#overload-errortype-unavailable-on-errortype) | A dependency or service is temporarily unavailable. |

##### Overload: `ErrorType Unavailable` on `ErrorType`

A dependency or service is temporarily unavailable.

#### Member: `ErrorType.Unexpected`

**Example**

```csharp
ErrorType category = ErrorType.Unexpected;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Unexpected`](#overload-errortype-unexpected-on-errortype) | An unexpected or internal failure. |

##### Overload: `ErrorType Unexpected` on `ErrorType`

An unexpected or internal failure.

#### Member: `ErrorType.Uninitialized`

**Example**

```csharp
ErrorType category = ErrorType.Uninitialized;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Uninitialized`](#overload-errortype-uninitialized-on-errortype) | An invalid default value that no constructed error may use. |

##### Overload: `ErrorType Uninitialized` on `ErrorType`

An invalid default value that no constructed error may use.

#### Member: `ErrorType.UnprocessableContent`

| Overload | Description |
| --- | --- |
| [`ErrorType UnprocessableContent`](#overload-errortype-unprocessablecontent-on-errortype) | The request was valid but could not be processed semantically. |

##### Overload: `ErrorType UnprocessableContent` on `ErrorType`

The request was valid but could not be processed semantically.

#### Member: `ErrorType.UnsupportedMediaType`

| Overload | Description |
| --- | --- |
| [`ErrorType UnsupportedMediaType`](#overload-errortype-unsupportedmediatype-on-errortype) | The request media type is not supported. |

##### Overload: `ErrorType UnsupportedMediaType` on `ErrorType`

The request media type is not supported.

#### Member: `ErrorType.Validation`

**Example**

```csharp
ErrorType category = ErrorType.Validation;
```

| Overload | Description |
| --- | --- |
| [`ErrorType Validation`](#overload-errortype-validation-on-errortype) | Invalid input or business-rule validation. |

##### Overload: `ErrorType Validation` on `ErrorType`

Invalid input or business-rule validation.

### Type: `IErrorConvertible<TError>`

Converts a domain-specific error into a wider error representation.

| Member | Description |
| --- | --- |
| [`ToError`](#member-ierrorconvertibleterrortoerror) | Creates the wider error representation. |

#### Member: `IErrorConvertible<TError>.ToError`

**Example**

```csharp
ApplicationError error = domainError.ToError();
```

| Overload | Description |
| --- | --- |
| [`TError ToError()`](#overload-terror-toerror-on-ierrorconvertibleterror) | Creates the wider error representation. |

##### Overload: `TError ToError()` on `IErrorConvertible<TError>`

Creates the wider error representation.

**Returns:** The converted error.

### Type: `ResultErrorExtensions`

Provides composition helpers for errors with compile-time widening conversions.

| Member | Description |
| --- | --- |
| [`BindWidened`](#member-resulterrorextensionsbindwidened) | Composes a successful result and widens the continuation's domain error without boxing or requiring a conversion on the already-wide failure branch. |

#### Member: `ResultErrorExtensions.BindWidened`

**Example**

```csharp
Result<Receipt, ApplicationError> receipt = order.BindWidened(CreateReceipt);
```

| Overload | Description |
| --- | --- |
| [`Result<TResult, TError> BindWidened<T, TError, TResult, TDomainError>(in Result<T, TError> result, Func<T, Result<TResult, TDomainError>> next)`](#overload-resulttresult-terror-bindwidenedt-terror-tresult-tdomainerrorin-resultt-terror-result-funct-resulttresult-tdomainerror-next-on-resulterrorextensions) | Composes a successful result and widens the continuation's domain error without boxing or requiring a conversion on the already-wide failure branch. |

##### Overload: `Result<TResult, TError> BindWidened<T, TError, TResult, TDomainError>(in Result<T, TError> result, Func<T, Result<TResult, TDomainError>> next)` on `ResultErrorExtensions`

Composes a successful result and widens the continuation's domain error without boxing or requiring a conversion on the already-wide failure branch.

**Type parameters**

- `TResult`: Continuation success type.
- `TDomainError`: Convertible domain error type.

**Parameters**

- `next`: Continuation invoked only for success.

**Returns:** The continuation result widened to `TError`.

### Type: `ValidationErrors`

Owns one or more validation issues. Allocation is confined to the failure path; successful `Result` values do not construct it.

| Member | Description |
| --- | --- |
| [`ValidationErrors`](#member-validationerrorsvalidationerrors) | Copies a non-empty array of validation issues. |
| [`AsSpan`](#member-validationerrorsasspan) | Returns a zero-allocation readonly view over the owned issues. |
| [`Count`](#member-validationerrorscount) | Gets the number of validation issues. |
| [`Create`](#member-validationerrorscreate) | Maps a third-party validation list without coupling to its assembly. |
| [`GetEnumerator`](#member-validationerrorsgetenumerator) | Returns an enumerator over the owned issues. |
| [`this[]`](#member-validationerrorsthis) | Gets the issue at `index`. |

#### Member: `ValidationErrors.ValidationErrors`

**Example**

```csharp
ValidationErrors errors = new(issues);
```

| Overload | Description |
| --- | --- |
| [`ValidationErrors(ValidationIssue[] issues)`](#overload-validationerrorsvalidationissue-issues-on-validationerrors) | Copies a non-empty array of validation issues. |
| [`ValidationErrors(IEnumerable<ValidationIssue> issues)`](#overload-validationerrorsienumerablevalidationissue-issues-on-validationerrors) | Copies a non-empty sequence of validation issues. |

##### Overload: `ValidationErrors(ValidationIssue[] issues)` on `ValidationErrors`

Copies a non-empty array of validation issues.

##### Overload: `ValidationErrors(IEnumerable<ValidationIssue> issues)` on `ValidationErrors`

Copies a non-empty sequence of validation issues.

#### Member: `ValidationErrors.AsSpan`

**Example**

```csharp
foreach (ref readonly ValidationIssue issue in errors.AsSpan()) Consume(issue);
```

| Overload | Description |
| --- | --- |
| [`ReadOnlySpan<ValidationIssue> AsSpan()`](#overload-readonlyspanvalidationissue-asspan-on-validationerrors) | Returns a zero-allocation readonly view over the owned issues. |

##### Overload: `ReadOnlySpan<ValidationIssue> AsSpan()` on `ValidationErrors`

Returns a zero-allocation readonly view over the owned issues.

#### Member: `ValidationErrors.Count`

**Example**

```csharp
int count = errors.Count;
```

| Overload | Description |
| --- | --- |
| [`int Count`](#overload-int-count-on-validationerrors) | Gets the number of validation issues. |

##### Overload: `int Count` on `ValidationErrors`

Gets the number of validation issues.

#### Member: `ValidationErrors.Create`

**Example**

```csharp
ValidationErrors errors = ValidationErrors.Create(failures, MapFailure);
```

| Overload | Description |
| --- | --- |
| [`ValidationErrors Create<TFailure>(IReadOnlyList<TFailure> failures, Func<TFailure, ValidationIssue> map)`](#overload-validationerrors-createtfailureireadonlylisttfailure-failures-functfailure-validationissue-map-on-validationerrors) | Maps a third-party validation list without coupling to its assembly. |
| [`ValidationErrors Create<TFailure, TMapper>(IReadOnlyList<TFailure> failures, TMapper map)`](#overload-validationerrors-createtfailure-tmapperireadonlylisttfailure-failures-tmapper-map-on-validationerrors) | Maps a third-party validation list through an inlineable value function. |
| [`ValidationErrors Create<TFailure, TState>(IReadOnlyList<TFailure> failures, TState state, Func<TFailure, TState, ValidationIssue> map)`](#overload-validationerrors-createtfailure-tstateireadonlylisttfailure-failures-tstate-state-functfailure-tstate-validationissue-map-on-validationerrors) | Maps a third-party validation list with caller-owned state. |

##### Overload: `ValidationErrors Create<TFailure>(IReadOnlyList<TFailure> failures, Func<TFailure, ValidationIssue> map)` on `ValidationErrors`

Maps a third-party validation list without coupling to its assembly.

##### Overload: `ValidationErrors Create<TFailure, TMapper>(IReadOnlyList<TFailure> failures, TMapper map)` on `ValidationErrors`

Maps a third-party validation list through an inlineable value function.

##### Overload: `ValidationErrors Create<TFailure, TState>(IReadOnlyList<TFailure> failures, TState state, Func<TFailure, TState, ValidationIssue> map)` on `ValidationErrors`

Maps a third-party validation list with caller-owned state.

#### Member: `ValidationErrors.GetEnumerator`

**Example**

```csharp
foreach (ValidationIssue issue in errors) Consume(issue);
```

| Overload | Description |
| --- | --- |
| [`IEnumerator<ValidationIssue> GetEnumerator()`](#overload-ienumeratorvalidationissue-getenumerator-on-validationerrors) | Returns an enumerator over the owned issues. |

##### Overload: `IEnumerator<ValidationIssue> GetEnumerator()` on `ValidationErrors`

Returns an enumerator over the owned issues.

#### Member: `ValidationErrors.this[]`

**Example**

```csharp
ValidationIssue first = errors[0];
```

| Overload | Description |
| --- | --- |
| [`ValidationIssue this[int]`](#overload-validationissue-thisint-on-validationerrors) | Gets the issue at `index`. |

##### Overload: `ValidationIssue this[int]` on `ValidationErrors`

Gets the issue at `index`.

### Type: `ValidationIssue`

A public-safe validation failure associated with an input path.

| Member | Description |
| --- | --- |
| [`ValidationIssue`](#member-validationissuevalidationissue) | Creates a validation issue with stable machine and human-readable fields. |
| [`Code`](#member-validationissuecode) | Gets the stable machine-readable issue code. |
| [`Message`](#member-validationissuemessage) | Gets the human-readable validation message. |
| [`Path`](#member-validationissuepath) | Gets the input path or member associated with the issue. |
| [`Severity`](#member-validationissueseverity) | Gets the issue severity. |

#### Member: `ValidationIssue.ValidationIssue`

**Example**

```csharp
ValidationIssue issue = new("email", "EMAIL_INVALID", "Email is invalid.");
```

| Overload | Description |
| --- | --- |
| [`ValidationIssue(string path, string code, string message, ValidationSeverity severity)`](#overload-validationissuestring-path-string-code-string-message-validationseverity-severity-on-validationissue) | Creates a validation issue with stable machine and human-readable fields. |

##### Overload: `ValidationIssue(string path, string code, string message, ValidationSeverity severity)` on `ValidationIssue`

Creates a validation issue with stable machine and human-readable fields.

#### Member: `ValidationIssue.Code`

**Example**

```csharp
string code = issue.Code;
```

| Overload | Description |
| --- | --- |
| [`string Code`](#overload-string-code-on-validationissue) | Gets the stable machine-readable issue code. |

##### Overload: `string Code` on `ValidationIssue`

Gets the stable machine-readable issue code.

#### Member: `ValidationIssue.Message`

**Example**

```csharp
string message = issue.Message;
```

| Overload | Description |
| --- | --- |
| [`string Message`](#overload-string-message-on-validationissue) | Gets the human-readable validation message. |

##### Overload: `string Message` on `ValidationIssue`

Gets the human-readable validation message.

#### Member: `ValidationIssue.Path`

**Example**

```csharp
string path = issue.Path;
```

| Overload | Description |
| --- | --- |
| [`string Path`](#overload-string-path-on-validationissue) | Gets the input path or member associated with the issue. |

##### Overload: `string Path` on `ValidationIssue`

Gets the input path or member associated with the issue.

#### Member: `ValidationIssue.Severity`

**Example**

```csharp
ValidationSeverity severity = issue.Severity;
```

| Overload | Description |
| --- | --- |
| [`ValidationSeverity Severity`](#overload-validationseverity-severity-on-validationissue) | Gets the issue severity. |

##### Overload: `ValidationSeverity Severity` on `ValidationIssue`

Gets the issue severity.

### Type: `ValidationSeverity`

Describes the diagnostic severity of a validation issue.

| Member | Description |
| --- | --- |
| [`Error`](#member-validationseverityerror) | The input is invalid and processing cannot continue. |
| [`Information`](#member-validationseverityinformation) | Informational validation feedback. |
| [`Warning`](#member-validationseveritywarning) | The input is accepted but potentially problematic. |

#### Member: `ValidationSeverity.Error`

**Example**

```csharp
ValidationSeverity severity = ValidationSeverity.Error;
```

| Overload | Description |
| --- | --- |
| [`ValidationSeverity Error`](#overload-validationseverity-error-on-validationseverity) | The input is invalid and processing cannot continue. |

##### Overload: `ValidationSeverity Error` on `ValidationSeverity`

The input is invalid and processing cannot continue.

#### Member: `ValidationSeverity.Information`

**Example**

```csharp
ValidationSeverity severity = ValidationSeverity.Information;
```

| Overload | Description |
| --- | --- |
| [`ValidationSeverity Information`](#overload-validationseverity-information-on-validationseverity) | Informational validation feedback. |

##### Overload: `ValidationSeverity Information` on `ValidationSeverity`

Informational validation feedback.

#### Member: `ValidationSeverity.Warning`

**Example**

```csharp
ValidationSeverity severity = ValidationSeverity.Warning;
```

| Overload | Description |
| --- | --- |
| [`ValidationSeverity Warning`](#overload-validationseverity-warning-on-validationseverity) | The input is accepted but potentially problematic. |

##### Overload: `ValidationSeverity Warning` on `ValidationSeverity`

The input is accepted but potentially problematic.


## Package MonadicTypes.NET.Generators

**Types:** [`ValueFunctionGenerator`](#type-valuefunctiongenerator)

### Type: `ValueFunctionGenerator`

Generates allocation-free value-function adapters for attributed static methods.

**Example**

```csharp
[GenerateValueFunction]
public static int GetId(User value) => value.Id;
```

| Member | Description |
| --- | --- |
| [`ValueFunctionGenerator`](#member-valuefunctiongeneratorvaluefunctiongenerator) | Creates the generator instance used by the Roslyn compiler host. |
| [`Initialize`](#member-valuefunctiongeneratorinitialize) | Registers attribute emission, method discovery, validation, and adapter generation. |

#### Member: `ValueFunctionGenerator.ValueFunctionGenerator`

| Overload | Description |
| --- | --- |
| [`ValueFunctionGenerator()`](#overload-valuefunctiongenerator-on-valuefunctiongenerator) | Creates the generator instance used by the Roslyn compiler host. |

##### Overload: `ValueFunctionGenerator()` on `ValueFunctionGenerator`

Creates the generator instance used by the Roslyn compiler host.

Application code does not construct the generator; the compiler host discovers it through `GeneratorAttribute`.

#### Member: `ValueFunctionGenerator.Initialize`

| Overload | Description |
| --- | --- |
| [`void Initialize(IncrementalGeneratorInitializationContext context)`](#overload-void-initializeincrementalgeneratorinitializationcontext-context-on-valuefunctiongenerator) | Registers attribute emission, method discovery, validation, and adapter generation. |

##### Overload: `void Initialize(IncrementalGeneratorInitializationContext context)` on `ValueFunctionGenerator`

Registers attribute emission, method discovery, validation, and adapter generation.

The Roslyn compiler host invokes this method. Application code uses the emitted attribute and callable members instead.

**Parameters**

- `context`: The incremental generator initialization context.


## Package MonadicTypes.NET.Linq

**Types:** [`QueryExtensions`](#type-queryextensions)

### Type: `QueryExtensions`

Provides opt-in C# query-expression operators for results and options.

| Member | Description |
| --- | --- |
| [`Select`](#member-queryextensionsselect) | Projects a present option; this is query syntax's map operation. |
| [`SelectMany`](#member-queryextensionsselectmany) | Binds and projects present options for multi-from query expressions. |
| [`Where`](#member-queryextensionswhere) | Keeps a present option only when its predicate succeeds. |

#### Member: `QueryExtensions.Select`

**Example**

```csharp
Result<int, LoadError> id = from user in result select user.Id;
```

| Overload | Description |
| --- | --- |
| [`Option<TResult> Select<T, TResult>(in Option<T> source, Func<T, TResult> selector)`](#overload-optiontresult-selectt-tresultin-optiont-source-funct-tresult-selector-on-queryextensions) | Projects a present option; this is query syntax's map operation. |
| [`Result<TResult, TError> Select<T, TError, TResult>(in Result<T, TError> source, Func<T, TResult> selector)`](#overload-resulttresult-terror-selectt-terror-tresultin-resultt-terror-source-funct-tresult-selector-on-queryextensions) | Projects a successful result; this is query syntax's map operation. |

##### Overload: `Option<TResult> Select<T, TResult>(in Option<T> source, Func<T, TResult> selector)` on `QueryExtensions`

Projects a present option; this is query syntax's map operation.

##### Overload: `Result<TResult, TError> Select<T, TError, TResult>(in Result<T, TError> source, Func<T, TResult> selector)` on `QueryExtensions`

Projects a successful result; this is query syntax's map operation.

#### Member: `QueryExtensions.SelectMany`

**Example**

```csharp
Result<Invoice, LoadError> invoice = from user in userResult from account in LoadAccount(user) select new Invoice(user, account);
```

| Overload | Description |
| --- | --- |
| [`Option<TResult> SelectMany<T, TIntermediate, TResult>(in Option<T> source, Func<T, Option<TIntermediate>> bind, Func<T, TIntermediate, TResult> project)`](#overload-optiontresult-selectmanyt-tintermediate-tresultin-optiont-source-funct-optiontintermediate-bind-funct-tintermediate-tresult-project-on-queryextensions) | Binds and projects present options for multi-from query expressions. |
| [`Result<TResult, TError> SelectMany<T, TError, TIntermediate, TResult>(in Result<T, TError> source, Func<T, Result<TIntermediate, TError>> bind, Func<T, TIntermediate, TResult> project)`](#overload-resulttresult-terror-selectmanyt-terror-tintermediate-tresultin-resultt-terror-source-funct-resulttintermediate-terror-bind-funct-tintermediate-tresult-project-on-queryextensions) | Binds and projects successful results for multi-from query expressions. |

##### Overload: `Option<TResult> SelectMany<T, TIntermediate, TResult>(in Option<T> source, Func<T, Option<TIntermediate>> bind, Func<T, TIntermediate, TResult> project)` on `QueryExtensions`

Binds and projects present options for multi-from query expressions.

##### Overload: `Result<TResult, TError> SelectMany<T, TError, TIntermediate, TResult>(in Result<T, TError> source, Func<T, Result<TIntermediate, TError>> bind, Func<T, TIntermediate, TResult> project)` on `QueryExtensions`

Binds and projects successful results for multi-from query expressions.

#### Member: `QueryExtensions.Where`

**Example**

```csharp
Option<User> active = from user in option where user.IsActive select user;
```

| Overload | Description |
| --- | --- |
| [`Option<T> Where<T>(in Option<T> source, Func<T, bool> predicate)`](#overload-optiont-wheretin-optiont-source-funct-bool-predicate-on-queryextensions) | Keeps a present option only when its predicate succeeds. |

##### Overload: `Option<T> Where<T>(in Option<T> source, Func<T, bool> predicate)` on `QueryExtensions`

Keeps a present option only when its predicate succeeds.


## Package MonadicTypes.NET.Testing

**Types:** [`MonadicAssertionException`](#type-monadicassertionexception) · [`MonadicAssertions`](#type-monadicassertions)

### Type: `MonadicAssertionException`

Exception thrown by framework-neutral MonadicTypes.NET test assertions.

| Member | Description |
| --- | --- |
| [`MonadicAssertionException`](#member-monadicassertionexceptionmonadicassertionexception) | Exception thrown by framework-neutral MonadicTypes.NET test assertions. |

#### Member: `MonadicAssertionException.MonadicAssertionException`

| Overload | Description |
| --- | --- |
| [`MonadicAssertionException(string message)`](#overload-monadicassertionexceptionstring-message-on-monadicassertionexception) | Exception thrown by framework-neutral MonadicTypes.NET test assertions. |

##### Overload: `MonadicAssertionException(string message)` on `MonadicAssertionException`

Exception thrown by framework-neutral MonadicTypes.NET test assertions.

### Type: `MonadicAssertions`

Provides framework-neutral assertions for tests of monadic values.

| Member | Description |
| --- | --- |
| [`ErrorOrFail`](#member-monadicassertionserrororfail) | Returns the failure error or throws a test assertion exception. |
| [`ShouldBeError`](#member-monadicassertionsshouldbeerror) | Asserts that a result is failed and returns it for further checks. |
| [`ShouldBeNone`](#member-monadicassertionsshouldbenone) | Asserts that an option is empty and returns it for further checks. |
| [`ShouldBeOk`](#member-monadicassertionsshouldbeok) | Asserts that a result is successful and returns it for further checks. |
| [`ShouldBeSome`](#member-monadicassertionsshouldbesome) | Asserts that an option is present and returns it for further checks. |
| [`ValueOrFail`](#member-monadicassertionsvalueorfail) | Returns the present value or throws a test assertion exception. |

#### Member: `MonadicAssertions.ErrorOrFail`

| Overload | Description |
| --- | --- |
| [`TError ErrorOrFail<T, TError>(Result<T, TError> result, string? message)`](#overload-terror-errororfailt-terrorresultt-terror-result-string-message-on-monadicassertions) | Returns the failure error or throws a test assertion exception. |

##### Overload: `TError ErrorOrFail<T, TError>(Result<T, TError> result, string? message)` on `MonadicAssertions`

Returns the failure error or throws a test assertion exception.

**Type parameters**

- `T`: Success value type.
- `TError`: Error type.

**Parameters**

- `result`: Result to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The failure error.

#### Member: `MonadicAssertions.ShouldBeError`

| Overload | Description |
| --- | --- |
| [`Result<T, TError> ShouldBeError<T, TError>(Result<T, TError> result, string? message)`](#overload-resultt-terror-shouldbeerrort-terrorresultt-terror-result-string-message-on-monadicassertions) | Asserts that a result is failed and returns it for further checks. |

##### Overload: `Result<T, TError> ShouldBeError<T, TError>(Result<T, TError> result, string? message)` on `MonadicAssertions`

Asserts that a result is failed and returns it for further checks.

**Type parameters**

- `T`: Success value type.
- `TError`: Error type.

**Parameters**

- `result`: Result to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The unchanged result.

#### Member: `MonadicAssertions.ShouldBeNone`

| Overload | Description |
| --- | --- |
| [`Option<T> ShouldBeNone<T>(Option<T> option, string? message)`](#overload-optiont-shouldbenonetoptiont-option-string-message-on-monadicassertions) | Asserts that an option is empty and returns it for further checks. |

##### Overload: `Option<T> ShouldBeNone<T>(Option<T> option, string? message)` on `MonadicAssertions`

Asserts that an option is empty and returns it for further checks.

**Type parameters**

- `T`: Contained value type.

**Parameters**

- `option`: Option to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The unchanged option.

#### Member: `MonadicAssertions.ShouldBeOk`

| Overload | Description |
| --- | --- |
| [`Result<T, TError> ShouldBeOk<T, TError>(Result<T, TError> result, string? message)`](#overload-resultt-terror-shouldbeokt-terrorresultt-terror-result-string-message-on-monadicassertions) | Asserts that a result is successful and returns it for further checks. |

##### Overload: `Result<T, TError> ShouldBeOk<T, TError>(Result<T, TError> result, string? message)` on `MonadicAssertions`

Asserts that a result is successful and returns it for further checks.

**Type parameters**

- `T`: Success value type.
- `TError`: Error type.

**Parameters**

- `result`: Result to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The unchanged result.

#### Member: `MonadicAssertions.ShouldBeSome`

| Overload | Description |
| --- | --- |
| [`Option<T> ShouldBeSome<T>(Option<T> option, string? message)`](#overload-optiont-shouldbesometoptiont-option-string-message-on-monadicassertions) | Asserts that an option is present and returns it for further checks. |

##### Overload: `Option<T> ShouldBeSome<T>(Option<T> option, string? message)` on `MonadicAssertions`

Asserts that an option is present and returns it for further checks.

**Type parameters**

- `T`: Contained value type.

**Parameters**

- `option`: Option to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The unchanged option.

#### Member: `MonadicAssertions.ValueOrFail`

| Overload | Description |
| --- | --- |
| [`T ValueOrFail<T>(Option<T> option, string? message)`](#overload-t-valueorfailtoptiont-option-string-message-on-monadicassertions) | Returns the present value or throws a test assertion exception. |
| [`T ValueOrFail<T, TError>(Result<T, TError> result, string? message)`](#overload-t-valueorfailt-terrorresultt-terror-result-string-message-on-monadicassertions) | Returns the successful value or throws a test assertion exception. |

##### Overload: `T ValueOrFail<T>(Option<T> option, string? message)` on `MonadicAssertions`

Returns the present value or throws a test assertion exception.

**Type parameters**

- `T`: Contained value type.

**Parameters**

- `option`: Option to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The present value.

##### Overload: `T ValueOrFail<T, TError>(Result<T, TError> result, string? message)` on `MonadicAssertions`

Returns the successful value or throws a test assertion exception.

**Type parameters**

- `T`: Success value type.
- `TError`: Error type.

**Parameters**

- `result`: Result to inspect.
- `message`: Optional context included in the failure message.

**Returns:** The successful value.
