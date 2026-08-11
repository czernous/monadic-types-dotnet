namespace MonadicTypes.Tests;

using System.Runtime.CompilerServices;

public class ResultTests
{
    [Fact]
    public void Default_IsUninitializedRatherThanFailure()
    {
        Result<int, string> result = default;

        Assert.False(result.IsInitialized);
        Assert.False(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Match(static value => value, static _ => 0));
    }

    [Fact]
    public void Ok_ExposesOnlyValue()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Assert.True(result.IsInitialized);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Fail_ExposesOnlyError()
    {
        Result<int, string> result = Result<int, string>.Fail("invalid");

        Assert.True(result.IsFailure);
        Assert.Equal("invalid", result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void MapBindEnsure_ComposeWithoutChangingFailureType()
    {
        Result<string, string> result = Result<int, string>.Ok(20)
            .Map(static value => value * 2)
            .Ensure(static value => value > 10, static _ => "too-small")
            .Bind(static value => Result<string, string>.Ok(value.ToString()));

        Assert.Equal("40", result.Value);
    }

    [Fact]
    public void MapError_ChangesOnlyFailureType()
    {
        Result<int, int> result = Result<int, string>.Fail("invalid")
            .MapError(static error => error.Length);

        Assert.Equal(7, result.Error);
    }

    [Fact]
    public void Recover_ReplacesFailure()
    {
        Result<int, string> result = Result<int, string>.Fail("missing")
            .Recover(static _ => Result<int, string>.Ok(7));

        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void TryGetMethods_ReturnOnlyActiveCase()
    {
        Result<int, string> result = Result<int, string>.Ok(5);

        Assert.True(result.TryGetValue(out int value));
        Assert.Equal(5, value);
        Assert.False(result.TryGetError(out string? error));
        Assert.Null(error);
    }

    [Fact]
    public void StructCallableMap_SupportsSameAndDifferentResultTypes()
    {
        Result<int, string> source = Result<int, string>.Ok(5);

        Result<int, string> sameType = source.Map(default(Increment));
        Result<long, string> differentType = source.Map<long, Widen>(default(Widen));

        Assert.Equal(6, sameType.Value);
        Assert.Equal(6L, differentType.Value);
    }

    [Fact]
    public void StructCallableMap_DoesNotInvokeForFailure()
    {
        Result<int, string> source = Result<int, string>.Fail("invalid");

        Result<int, string> result = source.Map(default(ThrowingMap));

        Assert.Equal("invalid", result.Error);
    }

    [Fact]
    public void StructCallableBind_SupportsSameAndDifferentResultTypes()
    {
        Result<int, string> source = Result<int, string>.Ok(5);

        Result<int, string> sameType = source.Bind(default(IncrementResult));
        Result<long, string> differentType = source.Bind<long, WidenResult>(default(WidenResult));

        Assert.Equal(6, sameType.Value);
        Assert.Equal(6L, differentType.Value);
    }

    [Fact]
    public void StructCallableMatch_SelectsActiveCase()
    {
        Result<int, string> success = Result<int, string>.Ok(5);
        Result<int, string> failure = Result<int, string>.Fail("invalid");

        Assert.Equal(6, success.Match<int, Increment, ErrorLength>(default, default));
        Assert.Equal(7, failure.Match<int, Increment, ErrorLength>(default, default));
    }

    [Fact]
    public void Switch_InvokesExactlyOneActiveBranch()
    {
        int successes = 0;
        int failures = 0;

        Result<int, string>.Ok(5).Switch(
            _ => successes++,
            _ => failures++);
        Result<int, string>.Fail("invalid").Switch(
            _ => successes++,
            _ => failures++);

        Assert.Equal(1, successes);
        Assert.Equal(1, failures);
    }

    [Fact]
    public void Switch_ThrowsForUninitializedResult()
    {
        Result<int, string> result = default;

        Assert.Throws<InvalidOperationException>(() =>
            result.Switch(static _ => { }, static _ => { }));
    }

    [Fact]
    public void StateOverloads_PassStateWithoutCapturedClosures()
    {
        Result<int, string> source = Result<int, string>.Ok(5);

        Result<long, string> mapped = source.Map(2L, static (value, state) => value + state);
        Result<long, string> bound = source.Bind(2L, static (value, state) => Result<long, string>.Ok(value + state));
        Result<int, string> ensured = source.Ensure(
            4,
            static (value, minimum) => value > minimum,
            static (_, minimum) => $"less-than-{minimum}");
        Result<int, int> mappedError = Result<int, string>.Fail("invalid")
            .MapError(2, static (error, multiplier) => error.Length * multiplier);
        int matchedSuccess = source.Match(
            2,
            static (value, increment) => value + increment,
            static (error, increment) => error.Length + increment);
        int matchedFailure = Result<int, string>.Fail("invalid").Match(
            2,
            static (value, increment) => value + increment,
            static (error, increment) => error.Length + increment);

        Assert.Equal(7L, mapped.Value);
        Assert.Equal(7L, bound.Value);
        Assert.Equal(5, ensured.Value);
        Assert.Equal(14, mappedError.Error);
        Assert.Equal(7, matchedSuccess);
        Assert.Equal(9, matchedFailure);
    }

    [Fact]
    public void TapError_StateOverloadPassesObserverWithoutClosure()
    {
        Result<int, string> source = Result<int, string>.Fail("invalid");
        List<string> observed = [];

        Result<int, string> returned = source.TapError(
            observed,
            static (error, errors) => errors.Add(error));

        Assert.Equal(["invalid"], observed);
        Assert.Equal(source, returned);
    }

    private readonly struct Increment : IValueFunction<int, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Invoke(int value) => value + 1;
    }

    private readonly struct Widen : IValueFunction<int, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Invoke(int value) => value + 1L;
    }

    private readonly struct ThrowingMap : IValueFunction<int, int>
    {
        public int Invoke(int value) => throw new InvalidOperationException();
    }

    private readonly struct IncrementResult : IValueFunction<int, Result<int, string>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<int, string> Invoke(int value) => Result<int, string>.Ok(value + 1);
    }

    private readonly struct WidenResult : IValueFunction<int, Result<long, string>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<long, string> Invoke(int value) => Result<long, string>.Ok(value + 1L);
    }

    private readonly struct ErrorLength : IValueFunction<string, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Invoke(string value) => value.Length;
    }

}
