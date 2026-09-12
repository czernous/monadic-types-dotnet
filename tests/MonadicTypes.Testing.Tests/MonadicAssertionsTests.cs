using MonadicTypes.Testing;

namespace MonadicTypes.Testing.Tests;

public sealed class MonadicAssertionsTests
{
    [Fact]
    public void ResultAssertionsReturnValuesAndErrors()
    {
        Result<int, string> success = Result<int, string>.Ok(42);
        Result<int, string> failure = Result<int, string>.Fail("bad");

        Assert.Equal(42, success.ShouldBeOk().ValueOrFail());
        Assert.Equal("bad", failure.ShouldBeError().ErrorOrFail());
    }

    [Fact]
    public void OptionAssertionsReturnValueAndPreserveNone()
    {
        Option<string> some = Option<string>.Some("value");

        Assert.Equal("value", some.ShouldBeSome().ValueOrFail());
        Assert.True(Option<int>.None.ShouldBeNone().IsNone);
    }

    [Fact]
    public void FailedAssertionsIncludeContextAndState()
    {
        MonadicAssertionException exception = Assert.Throws<MonadicAssertionException>(
            () => Result<int, string>.Fail("bad").ValueOrFail("load user"));

        Assert.Contains("load user", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Failure", exception.Message, StringComparison.Ordinal);
    }
}
