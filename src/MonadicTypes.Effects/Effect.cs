using System.Runtime.CompilerServices;

namespace MonadicTypes.Effects;

/// <summary>Executes explicitly fallible effects and converts selected exceptions into result failures.</summary>
public static class Effect
{
    /// <summary>Executes a synchronous effect and converts recoverable exceptions to failures.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>The effect value or mapped failure.</returns>
    /// <remarks>
    /// Cancellation and fatal runtime exceptions propagate. Use the typed overload when cancellation
    /// or another normally excluded exception must be represented explicitly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Try<T, TError>(
        Func<T> operation,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            return Result<T, TError>.Ok(operation());
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return Result<T, TError>.Fail(mapException(exception));
        }
    }

    /// <summary>Executes a synchronous effect and converts only the selected exception type.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <typeparam name="TException">Exception type to convert.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>The effect value or mapped failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Try<T, TError, TException>(
        Func<T> operation,
        Func<TException, TError> mapException)
        where TError : notnull
        where TException : Exception
    {
        try
        {
            return Result<T, TError>.Ok(operation());
        }
        catch (TException exception)
        {
            return Result<T, TError>.Fail(mapException(exception));
        }
    }

    /// <summary>Executes a ValueTask-producing effect and converts recoverable exceptions to failures.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryAsync<T, TError>(
        Func<ValueTask<T>> operation,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            ValueTask<T> pending = operation();
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect(pending, mapException);
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    /// <summary>Executes a ValueTask-producing effect and converts only the selected exception type.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <typeparam name="TException">Exception type to convert.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryAsync<T, TError, TException>(
        Func<ValueTask<T>> operation,
        Func<TException, TError> mapException)
        where TError : notnull
        where TException : Exception
    {
        try
        {
            ValueTask<T> pending = operation();
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect<T, TError, TException>(pending, mapException);
        }
        catch (TException exception)
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    /// <summary>Executes a Task-producing effect and converts recoverable exceptions to failures.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryTaskAsync<T, TError>(
        Func<Task<T>> operation,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            Task<T> pending = operation();
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect(new ValueTask<T>(pending), mapException);
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    /// <summary>Executes a Task effect with caller-owned state and converts recoverable exceptions.</summary>
    /// <typeparam name="TState">Caller state passed to the operation.</typeparam>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <param name="state">State passed unchanged to <paramref name="operation"/>.</param>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError>(
        TState state,
        Func<TState, Task<T>> operation,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            Task<T> pending = operation(state);
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect(new ValueTask<T>(pending), mapException);
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    /// <summary>Executes a Task-producing effect and converts only the selected exception type.</summary>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <typeparam name="TException">Exception type to convert.</typeparam>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryTaskAsync<T, TError, TException>(
        Func<Task<T>> operation,
        Func<TException, TError> mapException)
        where TError : notnull
        where TException : Exception
    {
        try
        {
            Task<T> pending = operation();
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect<T, TError, TException>(new ValueTask<T>(pending), mapException);
        }
        catch (TException exception)
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    /// <summary>Executes a Task effect with caller-owned state and converts one exception type.</summary>
    /// <typeparam name="TState">Caller state passed to the operation.</typeparam>
    /// <typeparam name="T">Effect value type.</typeparam>
    /// <typeparam name="TError">Failure type.</typeparam>
    /// <typeparam name="TException">Exception type to convert.</typeparam>
    /// <param name="state">State passed unchanged to <paramref name="operation"/>.</param>
    /// <param name="operation">Effect to execute exactly once.</param>
    /// <param name="mapException">Maps a caught exception to a failure.</param>
    /// <returns>An awaitable containing the effect value or mapped failure.</returns>
    public static ValueTask<Result<T, TError>> TryTaskAsync<TState, T, TError, TException>(
        TState state,
        Func<TState, Task<T>> operation,
        Func<TException, TError> mapException)
        where TError : notnull
        where TException : Exception
    {
        try
        {
            Task<T> pending = operation(state);
            if (pending.IsCompletedSuccessfully)
            {
                return ValueTask.FromResult(Result<T, TError>.Ok(pending.Result));
            }

            return AwaitEffect<T, TError, TException>(new ValueTask<T>(pending), mapException);
        }
        catch (TException exception)
        {
            return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
        }
    }

    private static async ValueTask<Result<T, TError>> AwaitEffect<T, TError>(
        ValueTask<T> pending,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            return Result<T, TError>.Ok(await pending.ConfigureAwait(false));
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return Result<T, TError>.Fail(mapException(exception));
        }
    }

    private static async ValueTask<Result<T, TError>> AwaitEffect<T, TError, TException>(
        ValueTask<T> pending,
        Func<TException, TError> mapException)
        where TError : notnull
        where TException : Exception
    {
        try
        {
            return Result<T, TError>.Ok(await pending.ConfigureAwait(false));
        }
        catch (TException exception)
        {
            return Result<T, TError>.Fail(mapException(exception));
        }
    }
}
