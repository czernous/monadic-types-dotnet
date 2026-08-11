using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Provides composition between nested <see cref="Result{T, E}"/> and <see cref="Option{T}"/> values.</summary>
public static class ResultCompositionExtensions
{
    extension<T, TError>(in Result<Result<T, TError>, TError> result) where TError : notnull
    {
        /// <summary>Removes one result layer while preserving the first failure encountered.</summary>
        /// <returns>The nested success result or the outer failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T, TError> Flatten() => result.IsSuccess
            ? result.Value
            : Result<T, TError>.Fail(result.Error);
    }

    extension<T, TError>(in Result<Option<T>, TError> result) where TError : notnull
    {
        /// <summary>Exchanges the result and option layers without losing a failure.</summary>
        /// <returns>
        /// None for a successful absent value, Some containing success for a present value,
        /// or Some containing the original failure.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<Result<T, TError>> Transpose()
        {
            if (result.IsFailure)
            {
                return Option<Result<T, TError>>.Some(Result<T, TError>.Fail(result.Error));
            }

            Option<T> option = result.Value;
            return option.HasValue
                ? Option<Result<T, TError>>.Some(Result<T, TError>.Ok(option.Value))
                : Option<Result<T, TError>>.None;
        }

        /// <summary>Requires a successful option to contain a value.</summary>
        /// <param name="whenNone">Error factory invoked only for a successful absent option.</param>
        /// <returns>The contained value, the original failure, or the generated absence failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T, TError> RequireSome(Func<TError> whenNone)
        {
            if (result.IsFailure)
            {
                return Result<T, TError>.Fail(result.Error);
            }

            Option<T> option = result.Value;
            return option.HasValue
                ? Result<T, TError>.Ok(option.Value)
                : Result<T, TError>.Fail(whenNone());
        }
    }

    extension<T, TError>(in Option<Result<T, TError>> option) where TError : notnull
    {
        /// <summary>Exchanges the option and result layers while treating absence as a successful absence.</summary>
        /// <returns>The contained result with its success wrapped in an option, or a successful None.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<Option<T>, TError> Transpose()
        {
            if (option.IsNone)
            {
                return Result<Option<T>, TError>.Ok(Option<T>.None);
            }

            Result<T, TError> result = option.Value;
            return result.IsSuccess
                ? Result<Option<T>, TError>.Ok(Option<T>.Some(result.Value))
                : Result<Option<T>, TError>.Fail(result.Error);
        }
    }

    extension<T>(in Option<T> option)
    {
        /// <summary>Converts an option to a result using an eagerly supplied absence error.</summary>
        /// <typeparam name="TError">Failure type.</typeparam>
        /// <param name="whenNone">Failure returned for None.</param>
        /// <returns>Success containing the present value or the supplied failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T, TError> ToResult<TError>(TError whenNone) where TError : notnull =>
            option.HasValue ? Result<T, TError>.Ok(option.Value) : Result<T, TError>.Fail(whenNone);

        /// <summary>Converts an option to a result using a lazy absence error factory.</summary>
        /// <typeparam name="TError">Failure type.</typeparam>
        /// <param name="whenNone">Factory invoked only for None.</param>
        /// <returns>Success containing the present value or the generated failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T, TError> ToResult<TError>(Func<TError> whenNone) where TError : notnull =>
            option.HasValue ? Result<T, TError>.Ok(option.Value) : Result<T, TError>.Fail(whenNone());
    }
}
