using System.Runtime.CompilerServices;

namespace MonadicTypes.Collections;

/// <summary>Provides fail-fast traversal for count-known collections.</summary>
public static class ResultCollectionExtensions
{
    extension<TSource>(IReadOnlyList<TSource> source)
    {
        /// <summary>Traverses each item through a generated callable wrapper with inferred result types.</summary>
        /// <example><code>var result = rows.TraverseToArray(Projections.Functions.ToDomain);</code></example>
        /// <remarks>Preserves fail-fast order. Empty input reuses an empty array; non-empty input allocates one output array.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult[], TError> TraverseToArray<TResult, TError, TFunction>(
            ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)
            where TError : notnull
            where TFunction : struct, IValueFunction<TSource, Result<TResult, TError>> =>
            ResultCollectionExtensions.TraverseToArray<TSource, TResult, TError,
                ValueFunction<TSource, Result<TResult, TError>, TFunction>>(source, selector);

        /// <summary>Traverses each item once and returns a newly allocated array of successful values.</summary>
        /// <example><code>Result&lt;User[], LoadError&gt; users = ids.TraverseToArray(LoadUser);</code></example>
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

    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <summary>Traverses each span item through a generated callable wrapper with inferred result types.</summary>
        /// <remarks>Preserves fail-fast order. Empty input reuses an empty array; non-empty input allocates one output array.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult[], TError> TraverseToArray<TResult, TError, TFunction>(
            ValueFunction<TSource, Result<TResult, TError>, TFunction> selector)
            where TError : notnull
            where TFunction : struct, IValueFunction<TSource, Result<TResult, TError>> =>
            ResultCollectionExtensions.TraverseToArray<TSource, TResult, TError,
                ValueFunction<TSource, Result<TResult, TError>, TFunction>>(source, selector);

        /// <summary>Traverses each span item once and returns a newly allocated array of successful values.</summary>
        /// <remarks>Empty input reuses <see cref="Array.Empty{T}"/>. Non-empty input allocates exactly one output array, including when a later item fails.</remarks>
        public Result<TResult[], TError> TraverseToArray<TResult, TError>(
            Func<TSource, Result<TResult, TError>> selector)
            where TError : notnull
        {
            ArgumentNullException.ThrowIfNull(selector);
            if (source.IsEmpty)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[source.Length];
            for (int index = 0; index < source.Length; index++)
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

        /// <summary>Traverses each span item once using caller-owned state and returns a new array.</summary>
        public Result<TResult[], TError> TraverseToArray<TState, TResult, TError>(
            TState state,
            Func<TSource, TState, Result<TResult, TError>> selector)
            where TError : notnull
        {
            ArgumentNullException.ThrowIfNull(selector);
            if (source.IsEmpty)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[source.Length];
            for (int index = 0; index < source.Length; index++)
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

        /// <summary>Traverses each span item once using an allocation-free callable and returns a new array.</summary>
        public Result<TResult[], TError> TraverseToArray<TResult, TError, TFunction>(TFunction selector)
            where TError : notnull
            where TFunction : struct, IValueFunction<TSource, Result<TResult, TError>>
        {
            if (source.IsEmpty)
            {
                return Result<TResult[], TError>.Ok([]);
            }

            TResult[] output = new TResult[source.Length];
            for (int index = 0; index < source.Length; index++)
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
        /// <example><code>Result&lt;User[], LoadError&gt; users = results.AsSpan().SequenceToArray();</code></example>
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
