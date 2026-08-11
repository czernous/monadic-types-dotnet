using MonadicTypes.Effects;

namespace MonadicTypes.Tests;

public class EffectTests
{
    [Fact]
    public void Try_ReturnsSuccessWithoutInvokingExceptionMapper()
    {
        Result<int, string> result = Effect.Try<int, string>(
            static () => 42,
            static _ => throw new InvalidOperationException());

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_ConvertsRecoverableException()
    {
        InvalidOperationException exception = new("failed");

        Result<int, Exception> result = Effect.Try<int, Exception>(
            () => throw exception,
            static caught => caught);

        Assert.Same(exception, result.Error);
    }

    [Fact]
    public void Try_DoesNotConvertCancellationByDefault()
    {
        Assert.Throws<OperationCanceledException>(() => Effect.Try<int, string>(
            static () => throw new OperationCanceledException(),
            static _ => "cancelled"));
    }

    [Fact]
    public void TypedTry_CanConvertExplicitCancellation()
    {
        Result<int, string> result = Effect.Try<int, string, OperationCanceledException>(
            static () => throw new OperationCanceledException(),
            static _ => "cancelled");

        Assert.Equal("cancelled", result.Error);
    }

    [Fact]
    public void TryMap_SkipsOriginalFailure()
    {
        bool invoked = false;
        Result<long, string> result = Result<int, string>.Fail("original").TryMap(
            _ =>
            {
                invoked = true;
                return 1L;
            },
            static _ => "mapped");

        Assert.Equal("original", result.Error);
        Assert.False(invoked);
    }

    [Fact]
    public async Task TryAsync_ConvertsAsynchronousException()
    {
        Result<int, string> result = await Effect.TryAsync<int, string>(
            static async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("failed");
            },
            static exception => exception.Message);

        Assert.Equal("failed", result.Error);
    }

    [Fact]
    public async Task TypedTryTaskAsync_ConvertsSelectedException()
    {
        Result<int, string> result = await Effect.TryTaskAsync<int, string, TimeoutException>(
            static () => Task.FromException<int>(new TimeoutException("timed out")),
            static exception => exception.Message);

        Assert.Equal("timed out", result.Error);
    }

    [Fact]
    public async Task TypedTryTaskAsync_PassesCallerStateWithoutCapture()
    {
        Task<int> completed = Task.FromResult(42);

        Result<int, string> result = await Effect.TryTaskAsync(
            completed,
            static task => task,
            static (TimeoutException exception) => exception.Message);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TypedTryAsync_PropagatesUnselectedException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await Effect.TryAsync<int, string, TimeoutException>(
                static () => ValueTask.FromException<int>(new InvalidOperationException("failed")),
                static exception => exception.Message));
    }

    [Fact]
    public async Task TryMapAsync_ConvertsSynchronousCallbackException()
    {
        Result<int, string> result = await Result<int, string>.Ok(4).TryMapAsync(
            static _ => ValueTask.FromException<int>(new InvalidOperationException("failed")),
            static exception => exception.Message);

        Assert.Equal("failed", result.Error);
    }

    [Fact]
    public void TryTap_ConvertsSideEffectException()
    {
        Result<int, string> result = Result<int, string>.Ok(4).TryTap(
            static _ => throw new InvalidOperationException("failed"),
            static exception => exception.Message);

        Assert.Equal("failed", result.Error);
    }

    [Fact]
    public async Task TryTapAsync_PreservesSuccessfulSynchronousCompletion()
    {
        int observed = 0;
        Result<int, string> result = await Result<int, string>.Ok(4).TryTapAsync(
            value =>
            {
                observed = value;
                return ValueTask.CompletedTask;
            },
            static exception => exception.Message);

        Assert.Equal(4, result.Value);
        Assert.Equal(4, observed);
    }
}
