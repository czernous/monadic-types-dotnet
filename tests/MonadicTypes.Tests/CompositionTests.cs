namespace MonadicTypes.Tests;

public class CompositionTests
{
    [Fact]
    public void BindError_ComposesOnlyFailureBranch()
    {
        Result<int, int> recovered = Result<int, string>.Fail("missing")
            .BindError(static error => Result<int, int>.Ok(error.Length));
        Result<int, int> unchanged = Result<int, string>.Ok(42)
            .BindError<int>(static _ => throw new InvalidOperationException());

        Assert.Equal(7, recovered.Value);
        Assert.Equal(42, unchanged.Value);
    }

    [Fact]
    public void BiMap_TransformsOnlyActiveCase()
    {
        Result<long, int> success = Result<int, string>.Ok(4)
            .BiMap<long, int>(static value => value + 1L, static _ => throw new InvalidOperationException());
        Result<long, int> failure = Result<int, string>.Fail("bad")
            .BiMap<long, int>(static _ => throw new InvalidOperationException(), static error => error.Length);

        Assert.Equal(5L, success.Value);
        Assert.Equal(3, failure.Error);
    }

    [Fact]
    public void ValueFallback_IsLazyForSuccess()
    {
        int eager = Result<int, string>.Fail("bad").ValueOr(9);
        int lazy = Result<int, string>.Ok(4).ValueOrElse(
            static _ => throw new InvalidOperationException());

        Assert.Equal(9, eager);
        Assert.Equal(4, lazy);
    }

    [Fact]
    public void Combine_IsFailFastInInputOrder()
    {
        Result<Unit, string>[] values =
        [
            Result.Ok<string>(),
            Result.Fail<string>("first"),
            Result.Fail<string>("second")
        ];

        Result<Unit, string> combined = ResultCombination.Combine(values.AsSpan());

        Assert.Equal("first", combined.Error);
    }

    [Fact]
    public void ZipAndMap_CombineHeterogeneousValues()
    {
        Result<int, string> first = Result<int, string>.Ok(4);
        Result<long, string> second = Result<long, string>.Ok(5);

        Result<(int First, long Second), string> zipped = ResultCombination.Zip(first, second);
        Result<string, string> mapped = ResultCombination.Map(
            first,
            second,
            static (left, right) => $"{left}:{right}");

        Assert.Equal((4, 5L), zipped.Value);
        Assert.Equal("4:5", mapped.Value);
    }

    [Fact]
    public void Flatten_RemovesOneResultLayer()
    {
        Result<Result<int, string>, string> nested =
            Result<Result<int, string>, string>.Ok(Result<int, string>.Ok(5));

        Assert.Equal(5, nested.Flatten().Value);
    }

    [Fact]
    public void ResultOptionTranspose_PreservesAllThreeStates()
    {
        Option<Result<int, string>> present =
            Result<Option<int>, string>.Ok(Option<int>.Some(5)).Transpose();
        Option<Result<int, string>> absent =
            Result<Option<int>, string>.Ok(Option<int>.None).Transpose();
        Option<Result<int, string>> failure =
            Result<Option<int>, string>.Fail("bad").Transpose();

        Assert.Equal(5, present.Value.Value);
        Assert.True(absent.IsNone);
        Assert.Equal("bad", failure.Value.Error);
    }

    [Fact]
    public void OptionToResult_AndRequireSome_CreateFailureLazily()
    {
        Result<int, string> present = Option<int>.Some(5)
            .ToResult(static () => "unused");
        Result<int, string> absent = Result<Option<int>, string>.Ok(Option<int>.None)
            .RequireSome(static () => "missing");

        Assert.Equal(5, present.Value);
        Assert.Equal("missing", absent.Error);
    }
}
