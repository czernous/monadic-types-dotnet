using System.Runtime.CompilerServices;

namespace MonadicTypes.Collections;

/// <summary>Provides fail-fast traversal for count-known collections.</summary>
public static class ResultCollectionExtensions
{
    extension<TSource>(IReadOnlyList<TSource> source)
    {
        /// <summary>Traverses each item once and returns a newly allocated array of successful values.</summary>
        /// <remarks>Empty input reuses <see cref="Array.Empty{T}"/>. Non-empty input allocates exactly one output array, including when a later item fails.</remarks>
        public Result<TResult[], TError> TraverseToArray<TResult, TError>(
            Func<TSource, Result<TResult, TError>> selector)
            where TError : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            int count = source.Count;
            if (count is 0)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[count];
            for (int index = 0; index < count; index++)
            {
                Result<TResult, TError> selected = selector(source[index]);
                if (selected.IsFailure)
                {
                    return Result<TResult[], TError>.Fail(selected.Error);
                }

                output[index] = selected.Value;
            }

            return Result<TResult[], TError>.Ok(output);
        }

        /// <summary>Traverses each item once using caller-owned state and returns a new array.</summary>
        public Result<TResult[], TError> TraverseToArray<TState, TResult, TError>(
            TState state,
            Func<TSource, TState, Result<TResult, TError>> selector)
            where TError : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            int count = source.Count;
            if (count is 0)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[count];
            for (int index = 0; index < count; index++)
            {
                Result<TResult, TError> selected = selector(source[index], state);
                if (selected.IsFailure)
                {
                    return Result<TResult[], TError>.Fail(selected.Error);
                }

                output[index] = selected.Value;
            }

            return Result<TResult[], TError>.Ok(output);
        }

        /// <summary>Traverses each item once using an allocation-free callable and returns a new array.</summary>
        public Result<TResult[], TError> TraverseToArray<TResult, TError, TFunction>(TFunction selector)
            where TError : notnull
            where TFunction : struct, IValueFunction<TSource, Result<TResult, TError>>
        {
            ArgumentNullException.ThrowIfNull(source);

            int count = source.Count;
            if (count is 0)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[count];
            for (int index = 0; index < count; index++)
            {
                Result<TResult, TError> selected = selector.Invoke(source[index]);
                if (selected.IsFailure)
                {
                    return Result<TResult[], TError>.Fail(selected.Error);
                }

                output[index] = selected.Value;
            }

            return Result<TResult[], TError>.Ok(output);
        }
    }

    extension<T, TError>(ReadOnlySpan<Result<T, TError>> source) where TError : notnull
    {
        /// <summary>Converts a span of results to one newly allocated array using fail-fast semantics.</summary>
        /// <remarks>Empty input reuses <see cref="Array.Empty{T}"/>. Non-empty input allocates one array.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T[], TError> SequenceToArray()
        {
            if (source.IsEmpty)
            {
                return Result<T[], TError>.Ok([]);
            }

            T[] output = new T[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                ref readonly Result<T, TError> item = ref source[index];
                if (item.IsFailure)
                {
                    return Result<T[], TError>.Fail(item.Error);
                }

                output[index] = item.Value;
            }

            return Result<T[], TError>.Ok(output);
        }
    }
}
