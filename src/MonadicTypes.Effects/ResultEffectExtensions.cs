using System.Runtime.CompilerServices;

namespace MonadicTypes.Effects;

/// <summary>Provides explicit exception-catching composition for result pipelines.</summary>
public static class ResultEffectExtensions
{
    extension<T, TError>(in Result<T, TError> result) where TError : notnull
    {
        /// <summary>Maps success while converting recoverable callback exceptions to failures.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Potentially throwing callback invoked only for success.</param>
        /// <param name="mapException">Maps a caught exception to the result error type.</param>
        /// <returns>The mapped success, original failure, or mapped exception failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TError> TryMap<TResult>(
            Func<T, TResult> map,
            Func<Exception, TError> mapException)
        {
            if (result.IsFailure)
            {
                return Result<TResult, TError>.Fail(result.Error);
            }

            try
            {
                return Result<TResult, TError>.Ok(map(result.Value));
            }
            catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
            {
                return Result<TResult, TError>.Fail(mapException(exception));
            }
        }

        /// <summary>Binds success while converting recoverable callback exceptions to failures.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Potentially throwing continuation invoked only for success.</param>
        /// <param name="mapException">Maps a caught exception to the result error type.</param>
        /// <returns>The continuation result, original failure, or mapped exception failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TError> TryBind<TResult>(
            Func<T, Result<TResult, TError>> bind,
            Func<Exception, TError> mapException)
        {
            if (result.IsFailure)
            {
                return Result<TResult, TError>.Fail(result.Error);
            }

            try
            {
                return bind(result.Value);
            }
            catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
            {
                return Result<TResult, TError>.Fail(mapException(exception));
            }
        }

        /// <summary>Runs a success side effect while converting recoverable callback exceptions to failures.</summary>
        /// <param name="action">Potentially throwing action invoked only for success.</param>
        /// <param name="mapException">Maps a caught exception to the result error type.</param>
        /// <returns>The original result or a mapped exception failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T, TError> TryTap(Action<T> action, Func<Exception, TError> mapException)
        {
            if (result.IsFailure)
            {
                return result;
            }

            try
            {
                action(result.Value);
                return result;
            }
            catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
            {
                return Result<T, TError>.Fail(mapException(exception));
            }
        }

        /// <summary>Maps success asynchronously while converting recoverable exceptions to failures.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Potentially throwing asynchronous callback invoked only for success.</param>
        /// <param name="mapException">Maps a caught exception to the result error type.</param>
        /// <returns>An awaitable containing the mapped success, original failure, or exception failure.</returns>
        public ValueTask<Result<TResult, TError>> TryMapAsync<TResult>(
            Func<T, ValueTask<TResult>> map,
            Func<Exception, TError> mapException)
        {
            if (result.IsFailure)
            {
                return ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));
            }

            try
            {
                ValueTask<TResult> pending = map(result.Value);
                if (pending.IsCompletedSuccessfully)
                {
                    return ValueTask.FromResult(Result<TResult, TError>.Ok(pending.Result));
                }

                return AwaitMap(pending, mapException);
            }
            catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
            {
                return ValueTask.FromResult(Result<TResult, TError>.Fail(mapException(exception)));
            }
        }

        /// <summary>Runs an asynchronous success side effect and converts recoverable exceptions to failures.</summary>
        /// <param name="action">Potentially throwing action invoked only for success.</param>
        /// <param name="mapException">Maps a caught exception to the result error type.</param>
        /// <returns>An awaitable containing the original result or mapped exception failure.</returns>
        public ValueTask<Result<T, TError>> TryTapAsync(
            Func<T, ValueTask> action,
            Func<Exception, TError> mapException)
        {
            if (result.IsFailure)
            {
                return ValueTask.FromResult(result);
            }

            try
            {
                ValueTask pending = action(result.Value);
                if (pending.IsCompletedSuccessfully)
                {
                    pending.GetAwaiter().GetResult();
                    return ValueTask.FromResult(result);
                }

                return AwaitTap(result, pending, mapException);
            }
            catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
            {
                return ValueTask.FromResult(Result<T, TError>.Fail(mapException(exception)));
            }
        }
    }

    private static async ValueTask<Result<TResult, TError>> AwaitMap<TResult, TError>(
        ValueTask<TResult> pending,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            return Result<TResult, TError>.Ok(await pending.ConfigureAwait(false));
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return Result<TResult, TError>.Fail(mapException(exception));
        }
    }

    private static async ValueTask<Result<T, TError>> AwaitTap<T, TError>(
        Result<T, TError> result,
        ValueTask pending,
        Func<Exception, TError> mapException)
        where TError : notnull
    {
        try
        {
            await pending.ConfigureAwait(false);
            return result;
        }
        catch (Exception exception) when (ExceptionFilter.IsRecoverable(exception))
        {
            return Result<T, TError>.Fail(mapException(exception));
        }
    }
}
