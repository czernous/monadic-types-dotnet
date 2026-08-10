using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

public readonly record struct Option<T>
{
    private readonly T? _value;

    public bool HasValue { get; }
    public bool IsSome => HasValue;
    public bool IsNone => !HasValue;

    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a None Option.");

    private Option(T value)
    {
        _value = value;
        HasValue = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new(value);
    }

    public static Option<T> None => default;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    public TR Match<TR>(Func<T, TR> some, Func<TR> none) =>
        HasValue ? some(_value!) : none();

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR>(Func<T, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!)) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)
        where TFunction : struct, IValueFunction<T, TR> =>
        HasValue ? Option<TR>.Some(map.Invoke(_value!)) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Map<TState, TR>(TState state, Func<T, TState, TR> map) =>
        HasValue ? Option<TR>.Some(map(_value!, state)) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR>(Func<T, Option<TR>> bind) =>
        HasValue ? bind(_value!) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR, TFunction>(TFunction bind)
        where TFunction : struct, IValueFunction<T, Option<TR>> =>
        HasValue ? bind.Invoke(_value!) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TR, TFunction>(ValueFunction<T, Option<TR>, TFunction> bind)
        where TFunction : struct, IValueFunction<T, Option<TR>> =>
        HasValue ? bind.Invoke(_value!) : Option<TR>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TR> Bind<TState, TR>(TState state, Func<T, TState, Option<TR>> bind) =>
        HasValue ? bind(_value!, state) : Option<TR>.None;

    public Option<T> Filter(Func<T, bool> predicate) =>
        HasValue && predicate(_value!) ? this : None;

    public Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate) =>
        HasValue && predicate(_value!, state) ? this : None;

    public T ValueOr(T fallback) => HasValue ? _value! : fallback;

    public T ValueOrElse(Func<T> fallback) => HasValue ? _value! : fallback();

    public T ValueOrElse<TState>(TState state, Func<TState, T> fallback) =>
        HasValue ? _value! : fallback(state);

    public static implicit operator Option<T>(T value) =>
        value is null ? None : Some(value);
}
