using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Represents either one non-null value or no value without heap allocation.</summary>
/// <example><code>Option&lt;User&gt; user = Option&lt;User&gt;.Some(value);</code></example>
/// <typeparam name="T">Contained value type.</typeparam>
public readonly record struct Option<T>
{
    private readonly T? _value;

    /// <summary>Gets whether this option contains a value.</summary>
    /// <example><code>bool present = option.HasValue;</code></example>
    public bool HasValue { get; }
    /// <summary>Gets whether this option is the <c>Some</c> case.</summary>
    /// <example>
    /// <code>
    /// if (option.IsSome) Consume(option.Value);
    /// </code>
    /// </example>
    public bool IsSome => HasValue;
    /// <summary>Gets whether this option is the <c>None</c> case.</summary>
    /// <example><code>if (option.IsNone) HandleAbsence();</code></example>
    public bool IsNone => !HasValue;

    /// <summary>Gets the contained value.</summary>
    /// <example><code>User user = option.Value;</code></example>
    /// <exception cref="InvalidOperationException">The option is <c>None</c>.</exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a None Option.");

    /// <summary>Compares presence and, only when present, the contained value.</summary>
    /// <example><code>bool equal = left.Equals(right);</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Option<T> other) =>
        HasValue == other.HasValue
        && (!HasValue || EqualityComparer<T>.Default.Equals(_value!, other._value!));

    /// <summary>Hashes presence and, only when present, the contained value.</summary>
    /// <example><code>int hash = option.GetHashCode();</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HasValue
        ? unchecked((397 * 1) ^ EqualityComparer<T>.Default.GetHashCode(_value!))
        : 0;

    private Option(T value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>Creates an option containing a non-null value.</summary>
    /// <example><code>Option&lt;User&gt; option = Option&lt;User&gt;.Some(user);</code></example>
    /// <param name="value">Value to contain.</param>
    /// <returns>A populated option.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new(value);
    }

    /// <summary>Gets the empty option.</summary>
    /// <example><code>Option&lt;User&gt; option = Option&lt;User&gt;.None;</code></example>
    public static Option<T> None => default;

    /// <summary>Attempts to retrieve the contained value.</summary>
    /// <example><code>if (option.TryGetValue(out User user)) Consume(user);</code></example>
    /// <param name="value">Receives the value when present.</param>
    /// <returns><see langword="true"/> when populated; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    /// <summary>Folds the active case into one output value.</summary>
    /// <example><code>string name = option.Match(static user =&gt; user.Name, static () =&gt; "Unknown");</code></example>
    /// <typeparam name="TR">Output type.</typeparam>
    /// <param name="some">Function invoked for a populated option.</param>
    /// <param name="none">Function invoked for an empty option.</param>
    /// <returns>The selected function's output.</returns>
    public TR Match<TR>(Func<T, TR> some, Func<TR> none) =>
        HasValue ? some(_value!) : none();

    /// <summary>Folds the active case while passing caller-owned state to non-capturing functions.</summary>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TR">Output type.</typeparam>
    /// <param name="state">State passed unchanged to the selected branch.</param>
    /// <param name="some">Function invoked for a populated option.</param>
    /// <param name="none">Function invoked for an empty option.</param>
    /// <returns>The selected function's output.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TState, TR>(
        TState state,
        Func<T, TState, TR> some,
        Func<TState, TR> none) => HasValue ? some(_value!, state) : none(state);

    /// <summary>Folds the active case through allocation-free callable values.</summary>
    /// <typeparam name="TR">Output type.</typeparam>
    /// <typeparam name="TSome">Populated-case callable type.</typeparam>
    /// <typeparam name="TNone">Empty-case callable type accepting <see cref="Unit"/>.</typeparam>
    /// <param name="some">Callable invoked for a populated option.</param>
    /// <param name="none">Callable invoked for an empty option.</param>
    /// <returns>The selected callable's output.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TR, TSome, TNone>(TSome some, TNone none)
        where TSome : struct, IValueFunction<T, TR>
        where TNone : struct, IValueFunction<Unit, TR> =>
        HasValue ? some.Invoke(_value!) : none.Invoke(Unit.Value);

    /// <summary>Executes exactly one action for the active case.</summary>
    /// <example><code>option.Switch(RenderUser, RenderMissingUser);</code></example>
    /// <param name="some">Action invoked for a populated option.</param>
    /// <param name="none">Action invoked for an empty option.</param>
    public void Switch(Action<T> some, Action none)
    {
        if (HasValue)
        {
            some(_value!);
        }
        else
        {
            none();
        }
    }

    /// <summary>Maps a present value and propagates <c>None</c>.</summary>
    /// <remarks>The projection must return a non-null value. For nullable members, use Bind with Option.FromNullable to turn null into None.</remarks>
    /// <exception cref="ArgumentNullException">A present value's projection returns null, including an empty nullable value type.</exception>
    /// <example><code>Option&lt;int&gt; id = option.Map(static user =&gt; user.Id);</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR>(Func<T, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value through an allocation-free callable and propagates <c>None</c>.</summary>
    /// <exception cref="ArgumentNullException">A present value's projection returns null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value through a generated callable wrapper and propagates <c>None</c>.</summary>
    /// <exception cref="ArgumentNullException">A present value's projection returns null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value while passing caller-owned state to a non-capturing function.</summary>
    /// <exception cref="ArgumentNullException">A present value's projection returns null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TState, TR>(TState state, Func<T, TState, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!, state)) : Option<TR>.None;

    /// <summary>Maps a present value through a nullable reference projection, treating null as <c>None</c>.</summary>
    /// <example><code>Option&lt;string&gt; nickname = person.MapNullable(static value =&gt; value.Nickname);</code></example>
    /// <typeparam name="TR">Projected reference type.</typeparam>
    /// <param name="map">Projection invoked only for <c>Some</c>.</param>
    /// <returns><c>Some</c> for a non-null projection, otherwise <c>None</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> MapNullable<TR>(Func<T, TR?> map)
        where TR : class
    {
        if (!HasValue)
        {
            return Option<TR>.None;
        }

        TR? value = map(_value!);
        return value is null ? Option<TR>.None : Option<TR>.Some(value);
    }

    /// <summary>Maps a present value through a nullable value projection, treating no value as <c>None</c>.</summary>
    /// <example><code>Option&lt;Guid&gt; id = person.MapNullableValue(static value =&gt; value.UserId);</code></example>
    /// <typeparam name="TR">Projected value type.</typeparam>
    /// <param name="map">Projection invoked only for <c>Some</c>.</param>
    /// <returns><c>Some</c> for a present projection, otherwise <c>None</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> MapNullableValue<TR>(Func<T, TR?> map)
        where TR : struct
    {
        if (!HasValue)
        {
            return Option<TR>.None;
        }

        TR? value = map(_value!);
        return value.HasValue ? Option<TR>.Some(value.Value) : Option<TR>.None;
    }

    /// <summary>Composes a present value with another optional operation and propagates <c>None</c>.</summary>
    /// <example><code>Option&lt;Address&gt; address = option.Bind(static user =&gt; user.PrimaryAddress);</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR>(Func<T, Option<TR>> bind) =>
        HasValue ? bind(_value!) : Option<TR>.None;

    /// <summary>Composes through an allocation-free callable and propagates <c>None</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR, TFunction>(TFunction bind)
        where TFunction : struct, IValueFunction<T, Option<TR>> =>
        HasValue ? bind.Invoke(_value!) : Option<TR>.None;

    /// <summary>Composes through a generated callable wrapper and propagates <c>None</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR, TFunction>(ValueFunction<T, Option<TR>, TFunction> bind)
        where TFunction : struct, IValueFunction<T, Option<TR>> =>
        HasValue ? bind.Invoke(_value!) : Option<TR>.None;

    /// <summary>Composes while passing caller-owned state to a non-capturing continuation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TState, TR>(TState state, Func<T, TState, Option<TR>> bind) =>
        HasValue ? bind(_value!, state) : Option<TR>.None;

    /// <summary>Observes a present value and returns this option unchanged.</summary>
    /// <example><code>Option&lt;User&gt; observed = option.Tap(static user =&gt; audit.Record(user));</code></example>
    /// <param name="action">Action invoked only for <c>Some</c>.</param>
    /// <returns>This option unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (HasValue)
        {
            action(_value!);
        }

        return this;
    }

    /// <summary>Observes a present value through an allocation-free callable and returns this option unchanged.</summary>
    /// <typeparam name="TAction">Callable action type.</typeparam>
    /// <param name="action">Action invoked only for <c>Some</c>.</param>
    /// <returns>This option unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap<TAction>(TAction action)
        where TAction : struct, IValueAction<T>
    {
        if (HasValue)
        {
            action.Invoke(_value!);
        }

        return this;
    }

    /// <summary>Observes a present value through a generated callable wrapper.</summary>
    /// <typeparam name="TAction">Callable action type.</typeparam>
    /// <param name="action">Action invoked only for <c>Some</c>.</param>
    /// <returns>This option unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap<TAction>(ValueAction<T, TAction> action)
        where TAction : struct, IValueAction<T>
    {
        if (HasValue)
        {
            action.Invoke(_value!);
        }

        return this;
    }

    /// <summary>Observes a present value while passing caller-owned state.</summary>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <param name="state">State passed unchanged to the action.</param>
    /// <param name="action">Action invoked only for <c>Some</c>.</param>
    /// <returns>This option unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap<TState>(TState state, Action<T, TState> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (HasValue)
        {
            action(_value!, state);
        }

        return this;
    }

    /// <summary>Combines two options in argument order.</summary>
    /// <example><code>Option&lt;(User User, Account Account)&gt; loaded = user.Zip(account);</code></example>
    /// <typeparam name="TOther">Second option value type.</typeparam>
    /// <param name="other">Second option.</param>
    /// <returns><c>Some</c> containing both values when both options are present; otherwise <c>None</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(T First, TOther Second)> Zip<TOther>(Option<TOther> other) =>
        HasValue && other.HasValue
            ? Option<(T First, TOther Second)>.Some((_value!, other.Value))
            : Option<(T First, TOther Second)>.None;

    /// <summary>Retains a present value only when <paramref name="predicate"/> returns true.</summary>
    /// <example><code>Option&lt;User&gt; active = option.Filter(static user =&gt; user.IsActive);</code></example>
    public Option<T> Filter(Func<T, bool> predicate) =>
        HasValue && predicate(_value!) ? this : None;

    /// <summary>Filters a present value while passing caller-owned state to the predicate.</summary>
    public Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate) =>
        HasValue && predicate(_value!, state) ? this : None;

    /// <summary>Returns the present value or an eagerly supplied fallback.</summary>
    /// <example><code>User user = option.ValueOr(User.Anonymous);</code></example>
    public T ValueOr(T fallback) => HasValue ? _value! : fallback;

    /// <summary>Returns the present value or lazily creates a fallback.</summary>
    /// <example><code>User user = option.ValueOrElse(CreateAnonymousUser);</code></example>
    public T ValueOrElse(Func<T> fallback) => HasValue ? _value! : fallback();

    /// <summary>Returns the present value or lazily creates a fallback with caller-owned state.</summary>
    public T ValueOrElse<TState>(TState state, Func<TState, T> fallback) =>
        HasValue ? _value! : fallback(state);

    /// <summary>Converts a value to <c>Some</c>, or null to <c>None</c>.</summary>
    /// <example><code>Option&lt;User&gt; option = nullableUser;</code></example>
    public static implicit operator Option<T>(T value) =>
        value is null ? None : Some(value);

    /// <summary>Deconstructs presence and value for positional pattern matching.</summary>
    /// <example><code>string text = option switch { (true, User user) =&gt; user.Name, _ =&gt; "Missing" };</code></example>
    /// <param name="hasValue">Receives true for Some and false for None.</param>
    /// <param name="value">Receives the contained value, or default for None.</param>
    public void Deconstruct(out bool hasValue, [MaybeNull] out T value)
    {
        hasValue = HasValue;
        value = _value;
    }
}
