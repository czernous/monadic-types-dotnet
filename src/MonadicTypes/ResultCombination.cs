using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Combines independent results using fail-fast semantics.</summary>
public static class ResultCombination
{
    /// <summary>Combines two unit results and returns the first failure in argument order.</summary>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="first">First result.</param>
    /// <param name="second">Second result.</param>
    /// <returns>Success when both inputs succeed; otherwise the first failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, TError> Combine<TError>(
        in Result<Unit, TError> first,
        in Result<Unit, TError> second)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return first;
        }

        _ = first.Value;
        return second.IsFailure
            ? Result<Unit, TError>.Fail(second.Error)
            : Result<Unit, TError>.Ok(second.Value);
    }

    /// <summary>Combines a span of unit results and returns the first failure in span order.</summary>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="results">Results to inspect exactly once.</param>
    /// <returns>Success when every input succeeds; otherwise the first failure.</returns>
    public static Result<Unit, TError> Combine<TError>(ReadOnlySpan<Result<Unit, TError>> results)
        where TError : notnull
    {
        foreach (ref readonly Result<Unit, TError> result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }

            _ = result.Value;
        }

        return Result<Unit, TError>.Ok(Unit.Value);
    }

    /// <summary>Combines two success values into a value tuple and returns the first failure.</summary>
    /// <typeparam name="TFirst">First success type.</typeparam>
    /// <typeparam name="TSecond">Second success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">First result.</param>
    /// <param name="second">Second result.</param>
    /// <returns>A tuple of both values or the first failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(TFirst First, TSecond Second), TError> Zip<TFirst, TSecond, TError>(
        in Result<TFirst, TError> first,
        in Result<TSecond, TError> second)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<(TFirst, TSecond), TError>.Fail(first.Error);
        }

        TFirst firstValue = first.Value;
        if (second.IsFailure)
        {
            return Result<(TFirst, TSecond), TError>.Fail(second.Error);
        }

        return Result<(TFirst, TSecond), TError>.Ok((firstValue, second.Value));
    }

    /// <summary>Projects two success values directly and returns the first failure.</summary>
    /// <typeparam name="TFirst">First success type.</typeparam>
    /// <typeparam name="TSecond">Second success type.</typeparam>
    /// <typeparam name="TResult">Projected success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">First result.</param>
    /// <param name="second">Second result.</param>
    /// <param name="map">Projection invoked only when both inputs succeed.</param>
    /// <returns>The projected success or the first failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<TFirst, TSecond, TResult, TError>(
        in Result<TFirst, TError> first,
        in Result<TSecond, TError> second,
        Func<TFirst, TSecond, TResult> map)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        TFirst firstValue = first.Value;
        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        return Result<TResult, TError>.Ok(map(firstValue, second.Value));
    }

    /// <summary>Binds two independent success values and returns the first failure.</summary>
    /// <typeparam name="TFirst">First success type.</typeparam>
    /// <typeparam name="TSecond">Second success type.</typeparam>
    /// <typeparam name="TResult">Bound success type.</typeparam>
    /// <typeparam name="TError">Shared failure type.</typeparam>
    /// <param name="first">First result.</param>
    /// <param name="second">Second result.</param>
    /// <param name="bind">Binding function invoked only when both inputs succeed.</param>
    /// <returns>The bound result or the first input failure in argument order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<TFirst, TSecond, TResult, TError>(
        in Result<TFirst, TError> first,
        in Result<TSecond, TError> second,
        Func<TFirst, TSecond, Result<TResult, TError>> bind)
        where TError : notnull
    {
        if (first.IsFailure)
        {
            return Result<TResult, TError>.Fail(first.Error);
        }

        TFirst firstValue = first.Value;
        if (second.IsFailure)
        {
            return Result<TResult, TError>.Fail(second.Error);
        }

        return bind(firstValue, second.Value);
    }

    /// <summary>Projects three independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        Func<T1, T2, T3, TResult> map)
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

        return Result<TResult, TError>.Ok(map(value1, value2, third.Value));
    }

    /// <summary>Binds three independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        Func<T1, T2, T3, Result<TResult, TError>> bind)
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

        return bind(value1, value2, third.Value);
    }

    /// <summary>Projects four independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        Func<T1, T2, T3, T4, TResult> map)
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

        return Result<TResult, TError>.Ok(map(value1, value2, value3, fourth.Value));
    }

    /// <summary>Binds four independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        Func<T1, T2, T3, T4, Result<TResult, TError>> bind)
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

        return bind(value1, value2, value3, fourth.Value);
    }

    /// <summary>Projects five independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, T5, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        Func<T1, T2, T3, T4, T5, TResult> map)
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

        return Result<TResult, TError>.Ok(map(value1, value2, value3, value4, fifth.Value));
    }

    /// <summary>Binds five independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, T5, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        Func<T1, T2, T3, T4, T5, Result<TResult, TError>> bind)
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

        return bind(value1, value2, value3, value4, fifth.Value);
    }

    /// <summary>Projects six independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T1, T2, T3, T4, T5, T6, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        in Result<T6, TError> sixth,
        Func<T1, T2, T3, T4, T5, T6, TResult> map)
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

        return Result<TResult, TError>.Ok(map(value1, value2, value3, value4, value5, sixth.Value));
    }

    /// <summary>Binds six independent success values and returns the first failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T1, T2, T3, T4, T5, T6, TResult, TError>(
        in Result<T1, TError> first,
        in Result<T2, TError> second,
        in Result<T3, TError> third,
        in Result<T4, TError> fourth,
        in Result<T5, TError> fifth,
        in Result<T6, TError> sixth,
        Func<T1, T2, T3, T4, T5, T6, Result<TResult, TError>> bind)
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

        return bind(value1, value2, value3, value4, value5, sixth.Value);
    }
}
