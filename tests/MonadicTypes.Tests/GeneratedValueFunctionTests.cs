namespace MonadicTypes.Tests;

public class GeneratedValueFunctionTests
{
    [Fact]
    public void GeneratedToken_InfersOptionTraverseAndPreservesBranches()
    {
        var success = Option<int>.Some(42).Traverse(GeneratedOperations.Functions.Validate);
        var failure = Option<int>.Some(-1).Traverse(GeneratedOperations.Functions.Validate);
        var none = Option<int>.None.Traverse(GeneratedOperations.Functions.Validate);

        Assert.Equal(43L, success.Value.Value);
        Assert.Equal("negative", failure.Error);
        Assert.True(none.Value.IsNone);
    }

    [Fact]
    public void AnnotatedMethod_RemainsDirectlyCallable()
    {
        Assert.Equal(43L, GeneratedOperations.Widen(42));
    }

    [Fact]
    public void GeneratedToken_InfersTypeChangingResultMap()
    {
        Result<int, string> source = Result<int, string>.Ok(42);

        Result<long, string> result = source.Map(GeneratedOperations.Functions.Widen);

        Assert.Equal(43L, result.Value);
    }

    [Fact]
    public void GeneratedToken_WorksOutsideResult()
    {
        long result = GeneratedOperations.Functions.Widen.Invoke(42);

        Assert.Equal(43L, result);
    }

    [Fact]
    public void GeneratedName_DisambiguatesOperations()
    {
        Option<int> source = Option<int>.Some(42);

        Option<int> result = source.Map(GeneratedOperations.Functions.Increment);

        Assert.Equal(43, result.Value);
    }

    [Fact]
    public void GeneratedActionToken_ObservesResultErrorWithoutADelegate()
    {
        GeneratedOperations.LastObserved = null;
        Result<int, string> source = Result<int, string>.Fail("failure");

        source.TapError(GeneratedOperations.Functions.Observe);

        Assert.Equal("failure", GeneratedOperations.LastObserved);
    }
}

public static partial class GeneratedOperations
{
    [GenerateValueFunction]
    public static Result<long, string> Validate(int value) => value < 0
        ? Result<long, string>.Fail("negative")
        : Result<long, string>.Ok(value + 1L);

    public static string? LastObserved { get; set; }

    [GenerateValueFunction]
    public static long Widen(int value) => value + 1L;

    [GenerateValueFunction("Increment")]
    public static int AddOne(int value) => value + 1;

    [GenerateValueFunction]
    public static void Observe(string error) => LastObserved = error;
}
