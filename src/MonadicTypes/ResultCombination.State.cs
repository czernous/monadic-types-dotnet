using System.Runtime.CompilerServices;

namespace MonadicTypes;

public static partial class ResultCombination
{
    /// <summary>Projects 2 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="state">State passed unchanged to the map function.</param>
    /// <param name="map">Function invoked only when every input succeeds.</param>
    /// <returns>The projected result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        TState state,
        Func<T1, T2, TState, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        return Result<TResult, TError>.Ok(map(value1, value2, state));
    }

    /// <summary>Binds 2 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="state">State passed unchanged to the bind function.</param>
    /// <param name="bind">Function invoked only when every input succeeds.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        TState state,
        Func<T1, T2, TState, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        return bind(value1, value2, state);
    }

    /// <summary>Projects 3 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="state">State passed unchanged to the map function.</param>
    /// <param name="map">Function invoked only when every input succeeds.</param>
    /// <returns>The projected result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        TState state,
        Func<T1, T2, T3, TState, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        return Result<TResult, TError>.Ok(map(value1, value2, value3, state));
    }

    /// <summary>Binds 3 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="state">State passed unchanged to the bind function.</param>
    /// <param name="bind">Function invoked only when every input succeeds.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        TState state,
        Func<T1, T2, T3, TState, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        return bind(value1, value2, value3, state);
    }

    /// <summary>Projects 4 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="state">State passed unchanged to the map function.</param>
    /// <param name="map">Function invoked only when every input succeeds.</param>
    /// <returns>The projected result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        TState state,
        Func<T1, T2, T3, T4, TState, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        return Result<TResult, TError>.Ok(map(value1, value2, value3, value4, state));
    }

    /// <summary>Binds 4 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="state">State passed unchanged to the bind function.</param>
    /// <param name="bind">Function invoked only when every input succeeds.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        TState state,
        Func<T1, T2, T3, T4, TState, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        return bind(value1, value2, value3, value4, state);
    }

    /// <summary>Projects 5 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="T5">Input 5 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="fifth">Input 5 result.</param>
    /// <param name="state">State passed unchanged to the map function.</param>
    /// <param name="map">Function invoked only when every input succeeds.</param>
    /// <returns>The projected result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, T5, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        TState state,
        Func<T1, T2, T3, T4, T5, TState, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        if (fifth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fifth.Error);
        }

        T5 value5 = fifth.Value;

        return Result<TResult, TError>.Ok(map(value1, value2, value3, value4, value5, state));
    }

    /// <summary>Binds 5 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="T5">Input 5 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="fifth">Input 5 result.</param>
    /// <param name="state">State passed unchanged to the bind function.</param>
    /// <param name="bind">Function invoked only when every input succeeds.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        TState state,
        Func<T1, T2, T3, T4, T5, TState, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        if (fifth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fifth.Error);
        }

        T5 value5 = fifth.Value;

        return bind(value1, value2, value3, value4, value5, state);
    }

    /// <summary>Projects 6 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="T5">Input 5 success type.</typeparam>
    /// <typeparam name="T6">Input 6 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="fifth">Input 5 result.</param>
    /// <param name="sixth">Input 6 result.</param>
    /// <param name="state">State passed unchanged to the map function.</param>
    /// <param name="map">Function invoked only when every input succeeds.</param>
    /// <returns>The projected result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        in Result<T6, TError> sixth,
        TState state,
        Func<T1, T2, T3, T4, T5, T6, TState, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        if (fifth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fifth.Error);
        }

        T5 value5 = fifth.Value;

        if (sixth.IsFailure)
        {
            return Result<TResult, TError>.Fail(sixth.Error);
        }

        T6 value6 = sixth.Value;

        return Result<TResult, TError>.Ok(map(value1, value2, value3, value4, value5, value6, state));
    }

    /// <summary>Binds 6 independent success values with caller-owned state and returns the first failure.</summary>
    /// <typeparam name="T1">Input 1 success type.</typeparam>
    /// <typeparam name="T2">Input 2 success type.</typeparam>
    /// <typeparam name="T3">Input 3 success type.</typeparam>
    /// <typeparam name="T4">Input 4 success type.</typeparam>
    /// <typeparam name="T5">Input 5 success type.</typeparam>
    /// <typeparam name="T6">Input 6 success type.</typeparam>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">Input 1 result.</param>
    /// <param name="second">Input 2 result.</param>
    /// <param name="third">Input 3 result.</param>
    /// <param name="fourth">Input 4 result.</param>
    /// <param name="fifth">Input 5 result.</param>
    /// <param name="sixth">Input 6 result.</param>
    /// <param name="state">State passed unchanged to the bind function.</param>
    /// <param name="bind">Function invoked only when every input succeeds.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TState, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        in Result<T6, TError> sixth,
        TState state,
        Func<T1, T2, T3, T4, T5, T6, TState, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        T1 value1 = first.Value;

        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        T2 value2 = second.Value;

        if (third.IsFailure)
        {
            return Result<TResult, TError>.Fail(third.Error);
        }

        T3 value3 = third.Value;

        if (fourth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fourth.Error);
        }

        T4 value4 = fourth.Value;

        if (fifth.IsFailure)
        {
            return Result<TResult, TError>.Fail(fifth.Error);
        }

        T5 value5 = fifth.Value;

        if (sixth.IsFailure)
        {
            return Result<TResult, TError>.Fail(sixth.Error);
        }

        T6 value6 = sixth.Value;

        return bind(value1, value2, value3, value4, value5, value6, state);
    }

}
