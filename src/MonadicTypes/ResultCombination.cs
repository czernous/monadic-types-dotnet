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

        return second.IsSuccess ? Result<Unit, TError>.Ok(Unit.Value) : Result<Unit, TError>.Fail(second.Error);
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

        return second.IsSuccess
            ? Result<(TFirst, TSecond), TError>.Ok((first.Value, second.Value))
            : Result<(TFirst, TSecond), TError>.Fail(second.Error);
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

        return second.IsSuccess
            ? Result<TResult, TError>.Ok(map(first.Value, second.Value))
            : Result<TResult, TError>.Fail(second.Error);
    }
}
