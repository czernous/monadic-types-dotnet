using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Represents either one non-null value or no value without heap allocation.</summary>
/// <typeparam name="T">Contained value type.</typeparam>
public readonly record struct Option<T>
{
    private readonly T? _value;

    /// <summary>Gets whether this option contains a value.</summary>
    public bool HasValue { get; }
    /// <summary>Gets whether this option is the <c>Some</c> case.</summary>
    public bool IsSome => HasValue;
    /// <summary>Gets whether this option is the <c>None</c> case.</summary>
    public bool IsNone => !HasValue;

    /// <summary>Gets the contained value.</summary>
    /// <exception cref="InvalidOperationException">The option is <c>None</c>.</exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a None Option.");

    private Option(T value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>Creates an option containing a non-null value.</summary>
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
    public static Option<T> None => default;

    /// <summary>Attempts to retrieve the contained value.</summary>
    /// <param name="value">Receives the value when present.</param>
    /// <returns><see langword="true"/> when populated; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    /// <summary>Folds the active case into one output value.</summary>
    /// <typeparam name="TR">Output type.</typeparam>
    /// <param name="some">Function invoked for a populated option.</param>
    /// <param name="none">Function invoked for an empty option.</param>
    /// <returns>The selected function's output.</returns>
    public TR Match<TR>(Func<T, TR> some, Func<TR> none) =>
        HasValue ? some(_value!) : none();

    /// <summary>Executes exactly one action for the active case.</summary>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR>(Func<T, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value through an allocation-free callable and propagates <c>None</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value through a generated callable wrapper and propagates <c>None</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    /// <summary>Maps a present value while passing caller-owned state to a non-capturing function.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TState, TR>(TState state, Func<T, TState, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!, state)) : Option<TR>.None;

    /// <summary>Composes a present value with another optional operation and propagates <c>None</c>.</summary>
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

    /// <summary>Retains a present value only when <paramref name="predicate"/> returns true.</summary>
    public Option<T> Filter(Func<T, bool> predicate) =>
        HasValue && predicate(_value!) ? this : None;

    /// <summary>Filters a present value while passing caller-owned state to the predicate.</summary>
    public Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate) =>
        HasValue && predicate(_value!, state) ? this : None;

    /// <summary>Returns the present value or an eagerly supplied fallback.</summary>
    public T ValueOr(T fallback) => HasValue ? _value! : fallback;

    /// <summary>Returns the present value or lazily creates a fallback.</summary>
    public T ValueOrElse(Func<T> fallback) => HasValue ? _value! : fallback();

    /// <summary>Returns the present value or lazily creates a fallback with caller-owned state.</summary>
    public T ValueOrElse<TState>(TState state, Func<TState, T> fallback) =>
        HasValue ? _value! : fallback(state);

    /// <summary>Converts a value to <c>Some</c>, or null to <c>None</c>.</summary>
    public static implicit operator Option<T>(T value) =>
        value is null ? None : Some(value);

    /// <summary>Deconstructs presence and value for positional pattern matching.</summary>
    /// <param name="hasValue">Receives true for Some and false for None.</param>
    /// <param name="value">Receives the contained value, or default for None.</param>
    public void Deconstruct(out bool hasValue, [MaybeNull] out T value)
    {
        hasValue = HasValue;
        value = _value;
    }
}
