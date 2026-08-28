using System.Runtime.CompilerServices;
using MonadicTypes.Collections;

namespace MonadicTypes.Collections.Tests;

public class ResultCollectionTests
{
    [Fact]
    public void TraverseToArray_MapsEverySuccessfulItemOnce()
    {
        IReadOnlyList<int> source = new[] { 1, 2, 3 };

        Result<long[], string> result = source.TraverseToArray(
            static value => Result<long, string>.Ok(value + 1L));

        Assert.Equal([2L, 3L, 4L], result.Value);
    }

    [Fact]
    public void TraverseToArray_StopsAtFirstFailure()
    {
        IReadOnlyList<int> source = new[] { 1, 2, 3 };
        CallCounter counter = new();

        Result<int[], string> result = source.TraverseToArray(
            counter,
            static (value, state) =>
            {
                state.Calls++;
                return value is 2
                    ? Result<int, string>.Fail("invalid")
                    : Result<int, string>.Ok(value);
            });

        Assert.Equal("invalid", result.Error);
        Assert.Equal(2, counter.Calls);
    }

    [Fact]
    public void TraverseToArray_StructCallableAndEmptyInputAreSupported()
    {
        IReadOnlyList<int> source = new[] { 1, 2 };
        IReadOnlyList<int> empty = Array.Empty<int>();

        Result<long[], string> populated = source
            .TraverseToArray<int, long, string, Increment>(default);
        Result<long[], string> absent = empty
            .TraverseToArray(static value => Result<long, string>.Ok(value));

        Assert.Equal([2L, 3L], populated.Value);
        Assert.Same(Array.Empty<long>(), absent.Value);
    }

    [Fact]
    public void SequenceToArray_PreservesOrderAndFirstFailure()
    {
        Result<int, string>[] successful =
        [
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2)
        ];
        Result<int, string>[] failed =
        [
            Result<int, string>.Ok(1),
            Result<int, string>.Fail("first"),
            Result<int, string>.Fail("second")
        ];

        Assert.Equal([1, 2], successful.AsSpan().SequenceToArray().Value);
        Assert.Equal("first", failed.AsSpan().SequenceToArray().Error);
    }

    [Fact]
    public void SpanTraverseToArray_SupportsEveryAllocationFreeDispatchForm()
    {
        int[] source = [1, 2, 3];

        Result<long[], string> delegated = source.AsSpan().TraverseToArray(
            static value => Result<long, string>.Ok(value + 1L));
        Result<long[], string> state = source.AsSpan().TraverseToArray(
            2L,
            static (value, increment) => Result<long, string>.Ok(value + increment));
        Result<long[], string> callable = source.AsSpan()
            .TraverseToArray<int, long, string, Increment>(default);

        Assert.Equal([2L, 3L, 4L], delegated.Value);
        Assert.Equal([3L, 4L, 5L], state.Value);
        Assert.Equal([2L, 3L, 4L], callable.Value);
    }

    [Fact]
    public void SpanTraverseToArray_IsFailFastAndRejectsUninitializedResults()
    {
        int[] source = [1, 2, 3];
        CallCounter counter = new();

        Result<int[], string> failed = source.AsSpan().TraverseToArray(
            counter,
            static (value, state) =>
            {
                state.Calls++;
                return value is 2
                    ? Result<int, string>.Fail("invalid")
                    : Result<int, string>.Ok(value);
            });

        Assert.Equal("invalid", failed.Error);
        Assert.Equal(2, counter.Calls);
        Assert.Throws<InvalidOperationException>(() =>
            source.AsSpan().TraverseToArray(static _ => default(Result<int, string>)));
    }

    [Fact]
    public void TraverseToArray_PropagatesSelectorExceptionUnchanged()
    {
        IReadOnlyList<int> source = new[] { 1 };
        InvalidOperationException expected = new("selector");

        Exception? thrown = Record.Exception(() => source.TraverseToArray<int, int, string>(
            _ => ThrowResult(expected)));

        Assert.Same(expected, thrown);
        Assert.Contains(nameof(ThrowResult), thrown.StackTrace, StringComparison.Ordinal);
    }

    [Fact]
    public void TraverseAndSequence_RejectUninitializedSelectorResults()
    {
        IReadOnlyList<int> source = new[] { 1 };
        Result<int, string>[] results = [default];

        Assert.Throws<InvalidOperationException>(() => source.TraverseToArray(
            static _ => default(Result<int, string>)));
        Assert.Throws<InvalidOperationException>(() => results.AsSpan().SequenceToArray());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<int, string> ThrowResult(Exception exception) => throw exception;

    private readonly struct Increment : IValueFunction<int, Result<long, string>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<long, string> Invoke(int value) => Result<long, string>.Ok(value + 1L);
    }

    private sealed class CallCounter
    {
        public int Calls { get; set; }
    }
}
