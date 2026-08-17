using System.Runtime.CompilerServices;

namespace MonadicTypes.Linq;

/// <summary>Provides opt-in C# query-expression operators for results and options.</summary>
public static class QueryExtensions
{
    extension<T, TError>(in Result<T, TError> source) where TError : notnull
    {
        /// <summary>Projects a successful result; this is query syntax's map operation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector) => source.Map(selector);

        /// <summary>Binds and projects successful results for multi-from query expressions.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TError> SelectMany<TIntermediate, TResult>(
            Func<T, Result<TIntermediate, TError>> bind,
            Func<T, TIntermediate, TResult> project)
        {
            if (source.IsFailure)
            {
                return Result<TResult, TError>.Fail(source.Error);
            }

            T value = source.Value;
            Result<TIntermediate, TError> intermediate = bind(value);
            return intermediate.IsSuccess
                ? Result<TResult, TError>.Ok(project(value, intermediate.Value))
                : Result<TResult, TError>.Fail(intermediate.Error);
        }
    }

    extension<T>(in Option<T> source)
    {
        /// <summary>Projects a present option; this is query syntax's map operation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TResult> Select<TResult>(Func<T, TResult> selector) => source.Map(selector);

        /// <summary>Binds and projects present options for multi-from query expressions.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TResult> SelectMany<TIntermediate, TResult>(
            Func<T, Option<TIntermediate>> bind,
            Func<T, TIntermediate, TResult> project)
        {
            if (source.IsNone)
            {
                return Option<TResult>.None;
            }

            T value = source.Value;
            Option<TIntermediate> intermediate = bind(value);
            return intermediate.HasValue
                ? Option<TResult>.Some(project(value, intermediate.Value))
                : Option<TResult>.None;
        }

        /// <summary>Keeps a present option only when its predicate succeeds.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> Where(Func<T, bool> predicate) => source.Filter(predicate);
    }
}
