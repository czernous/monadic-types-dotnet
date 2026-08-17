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
    public void CombinationMap_SupportsThreeThroughSixInputs()
    {
        Result<int, string> one = Result<int, string>.Ok(1);
        Result<int, string> two = Result<int, string>.Ok(2);
        Result<int, string> three = Result<int, string>.Ok(3);
        Result<int, string> four = Result<int, string>.Ok(4);
        Result<int, string> five = Result<int, string>.Ok(5);
        Result<int, string> six = Result<int, string>.Ok(6);

        Result<int, string> mapped3 = ResultCombination.Map(one, two, three, static (a, b, c) => a + b + c);
        Result<int, string> mapped4 = ResultCombination.Map(one, two, three, four, static (a, b, c, d) => a + b + c + d);
        Result<int, string> mapped5 = ResultCombination.Map(one, two, three, four, five, static (a, b, c, d, e) => a + b + c + d + e);
        Result<int, string> mapped6 = ResultCombination.Map(one, two, three, four, five, six, static (a, b, c, d, e, f) => a + b + c + d + e + f);

        Assert.Equal(6, mapped3.Value);
        Assert.Equal(10, mapped4.Value);
        Assert.Equal(15, mapped5.Value);
        Assert.Equal(21, mapped6.Value);
    }

    [Fact]
    public void CombinationBind_SupportsTwoThroughSixInputs()
    {
        Result<int, string> one = Result<int, string>.Ok(1);
        Result<int, string> two = Result<int, string>.Ok(2);
        Result<int, string> three = Result<int, string>.Ok(3);
        Result<int, string> four = Result<int, string>.Ok(4);
        Result<int, string> five = Result<int, string>.Ok(5);
        Result<int, string> six = Result<int, string>.Ok(6);

        Result<int, string> bound2 = ResultCombination.Bind(one, two, static (a, b) => Result<int, string>.Ok(a + b));
        Result<int, string> bound3 = ResultCombination.Bind(one, two, three, static (a, b, c) => Result<int, string>.Ok(a + b + c));
        Result<int, string> bound4 = ResultCombination.Bind(one, two, three, four, static (a, b, c, d) => Result<int, string>.Ok(a + b + c + d));
        Result<int, string> bound5 = ResultCombination.Bind(one, two, three, four, five, static (a, b, c, d, e) => Result<int, string>.Ok(a + b + c + d + e));
        Result<int, string> bound6 = ResultCombination.Bind(one, two, three, four, five, six, static (a, b, c, d, e, f) => Result<int, string>.Ok(a + b + c + d + e + f));

        Assert.Equal(3, bound2.Value);
        Assert.Equal(6, bound3.Value);
        Assert.Equal(10, bound4.Value);
        Assert.Equal(15, bound5.Value);
        Assert.Equal(21, bound6.Value);
    }

    [Fact]
    public void Combination_RejectsUninitializedInputBeforeLaterFailure()
    {
        Result<int, string> uninitialized = default;
        Result<int, string> failure = Result<int, string>.Fail("later");
        Result<Unit, string> uninitializedUnit = default;
        Result<Unit, string> failedUnit = Result.Fail<string>("later");

        Assert.Throws<InvalidOperationException>(() => ResultCombination.Combine(uninitializedUnit, failedUnit));
        Assert.Throws<InvalidOperationException>(() => ResultCombination.Zip(uninitialized, failure));
        Assert.Throws<InvalidOperationException>(() => ResultCombination.Map(uninitialized, failure, static (a, b) => a + b));
        Assert.Throws<InvalidOperationException>(() => ResultCombination.Bind(uninitialized, failure, static (a, b) => Result<int, string>.Ok(a + b)));
    }

    [Fact]
    public void Combination_IsFailFastAndDoesNotInvokeProjection()
    {
        Result<int, string> first = Result<int, string>.Ok(1);
        Result<int, string> failure = Result<int, string>.Fail("first-failure");
        Result<int, string> later = Result<int, string>.Fail("later-failure");

        Result<int, string> mapped = ResultCombination.Map<int, int, int, int, string>(
            first,
            failure,
            later,
            static (_, _, _) => throw new InvalidOperationException());
        Result<int, string> bound = ResultCombination.Bind<int, int, int, int, string>(
            first,
            failure,
            later,
            static (_, _, _) => throw new InvalidOperationException());

        Assert.Equal("first-failure", mapped.Error);
        Assert.Equal("first-failure", bound.Error);
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

    [Fact]
    public void OptionTraverse_PreservesSomeNoneAndFailure()
    {
        Result<Option<long>, string> success = Option<int>.Some(5)
            .Traverse(static value => Result<long, string>.Ok(value + 1L));
        Result<Option<long>, string> failure = Option<int>.Some(5)
            .Traverse(static _ => Result<long, string>.Fail("invalid"));
        Result<Option<long>, string> none = Option<int>.None
            .Traverse<int, long, string>(static _ => throw new InvalidOperationException());

        Assert.Equal(6L, success.Value.Value);
        Assert.Equal("invalid", failure.Error);
        Assert.True(none.Value.IsNone);
    }

    [Fact]
    public void OptionTraverse_StateAndStructCallablesAvoidCaptures()
    {
        Result<Option<long>, string> state = Option<int>.Some(5)
            .Traverse(2L, static (value, increment) => Result<long, string>.Ok(value + increment));
        Result<Option<long>, string> callable = Option<int>.Some(5)
            .Traverse<int, long, string, TraverseIncrement>(default);

        Assert.Equal(7L, state.Value.Value);
        Assert.Equal(6L, callable.Value.Value);
    }

    [Fact]
    public void OptionTraverse_PropagatesExceptionsAndRejectsUninitializedResult()
    {
        InvalidOperationException expected = new("selector");

        Exception? thrown = Record.Exception(() => Option<int>.Some(1)
            .Traverse<int, int, string>(_ => ThrowTraverse(expected)));

        Assert.Same(expected, thrown);
        Assert.Contains(nameof(ThrowTraverse), thrown.StackTrace, StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => Option<int>.Some(1)
            .Traverse(static _ => default(Result<int, string>)));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static Result<int, string> ThrowTraverse(Exception exception) => throw exception;

    private readonly struct TraverseIncrement : IValueFunction<int, Result<long, string>>
    {
        public Result<long, string> Invoke(int value) => Result<long, string>.Ok(value + 1L);
    }
}
