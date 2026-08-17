using MonadicTypes.Linq;

namespace MonadicTypes.Linq.Tests;

public class QueryTests
{
    [Fact]
    public void FluentResultOperators_MapBindAndProjectSuccesses()
    {
        Result<int, string> result = Result<int, string>.Ok(2)
            .Select(static value => value + 1)
            .SelectMany(
                static value => Result<int, string>.Ok(value * 2),
                static (left, right) => left + right);

        Assert.Equal(9, result.Value);
    }

    [Fact]
    public void ResultQuery_MapsAndBindsSuccesses()
    {
        Result<int, string> first = Result<int, string>.Ok(2);

        Result<int, string> result =
            from left in first
            from right in Result<int, string>.Ok(3)
            select left + right;

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void ResultQuery_ShortCircuitsFailure()
    {
        Result<int, string> first = Result<int, string>.Fail("invalid");

        Result<int, string> result =
            from left in first
            from right in ThrowingResult(left)
            select left + right;

        Assert.Equal("invalid", result.Error);
    }

    [Fact]
    public void OptionQuery_MapsBindsAndFiltersPresence()
    {
        Option<int> result =
            from left in Option<int>.Some(2)
            where left > 1
            from right in Option<int>.Some(3)
            select left + right;
        Option<int> filtered =
            from value in Option<int>.Some(1)
            where value > 1
            select value;

        Assert.Equal(5, result.Value);
        Assert.True(filtered.IsNone);
    }

    [Fact]
    public void QueryOperators_PropagateUserExceptionsUnchanged()
    {
        InvalidOperationException expected = new("projector");
        Result<int, string> source = Result<int, string>.Ok(1);

        Exception? thrown = Record.Exception(() => source.Select(_ => ThrowValue(expected)));

        Assert.Same(expected, thrown);
        Assert.Contains(nameof(ThrowValue), thrown.StackTrace, StringComparison.Ordinal);
    }

    [Fact]
    public void QueryOperators_RejectUninitializedResults()
    {
        Result<int, string> source = default;

        Assert.Throws<InvalidOperationException>(() => source.Select(static value => value + 1));
    }

    private static Result<int, string> ThrowingResult(int _) => throw new InvalidOperationException();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int ThrowValue(Exception exception) => throw exception;
}
