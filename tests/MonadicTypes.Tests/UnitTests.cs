namespace MonadicTypes.Tests;

public class UnitTests
{
    [Fact]
    public void Unit_HasOneValue()
    {
        Assert.Equal(Unit.Value, default);
        Assert.Equal("()", Unit.Value.ToString());
        Assert.Equal(0, Unit.Value.GetHashCode());
    }

    [Fact]
    public void ResultFactory_CreatesUnitSuccessAndFailure()
    {
        Result<Unit, string> success = Result.Ok<string>();
        Result<Unit, string> failure = Result.Fail("invalid");

        Assert.Equal(Unit.Value, success.Value);
        Assert.Equal("invalid", failure.Error);
    }
}
