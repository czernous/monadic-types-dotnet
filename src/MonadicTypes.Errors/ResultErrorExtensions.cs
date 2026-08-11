using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Provides composition helpers for errors with compile-time widening conversions.</summary>
public static class ResultErrorExtensions
{
    extension<T, TError>(in Result<T, TError> result) where TError : notnull
    {
        /// <summary>
        /// Composes a successful result and widens the continuation's domain error without
        /// boxing or requiring a conversion on the already-wide failure branch.
        /// </summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TDomainError">Convertible domain error type.</typeparam>
        /// <param name="next">Continuation invoked only for success.</param>
        /// <returns>The continuation result widened to <typeparamref name="TError"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TError> BindWidened<TResult, TDomainError>(
            Func<T, Result<TResult, TDomainError>> next)
            where TDomainError : notnull, IErrorConvertible<TError>
        {
            if (result.IsFailure)
            {
                return Result<TResult, TError>.Fail(result.Error);
            }

            Result<TResult, TDomainError> nextResult = next(result.Value);
            return nextResult.IsSuccess
                ? Result<TResult, TError>.Ok(nextResult.Value)
                : Result<TResult, TError>.Fail(nextResult.Error.ToError());
        }
    }
}
