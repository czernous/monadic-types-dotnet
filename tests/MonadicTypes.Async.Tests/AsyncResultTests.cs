using MonadicTypes.Async;

namespace MonadicTypes.Tests;

public class AsyncResultTests
{
    [Fact]
    public async Task Pipeline_MixesSynchronousAndAsynchronousOperations()
    {
        Result<string, string> result = await Result<int, string>.Ok(4)
            .Map(static value => value + 1)
            .BindAsync(static value => ValueTask.FromResult(Result<long, string>.Ok(value + 1L)))
            .Map(static value => value * 2)
            .Bind(static value => Result<string, string>.Ok(value.ToString()));

        Assert.Equal("12", result.Value);
    }

    [Fact]
    public async Task TaskReturningCallback_IsSupportedWithoutAdapter()
    {
        Result<int, string> result = await Result<int, string>.Ok(4)
            .MapTaskAsync(static value => Task.FromResult(value + 1));

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task TaskReturningCallback_ComposesAfterValueTaskOperation()
    {
        Result<long, string> result = await Result<int, string>.Ok(4)
            .MapAsync(static value => ValueTask.FromResult(value + 1))
            .MapTaskAsync(static value => Task.FromResult(value + 1L));

        Assert.Equal(6L, result.Value);
    }

    [Fact]
    public async Task AsyncLambda_IsNotAmbiguous()
    {
        Result<int, string> result = await Result<int, string>.Ok(4)
            .MapAsync(static async value =>
            {
                await Task.Yield();
                return value + 1;
            });

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task TaskReceiver_ContinuesWithSynchronousOperation()
    {
        Task<Result<int, string>> source = Task.FromResult(Result<int, string>.Ok(4));

        Result<long, string> result = await source.Map(static value => value + 1L);

        Assert.Equal(5L, result.Value);
    }

    [Fact]
    public async Task Failure_SkipsEverySuccessCallback()
    {
        bool invoked = false;
        Result<long, string> result = await ValueTask.FromResult(Result<int, string>.Fail("bad"))
            .Map(_ =>
            {
                invoked = true;
                return 1L;
            })
            .BindAsync(static value => ValueTask.FromResult(Result<long, string>.Ok(value)));

        Assert.Equal("bad", result.Error);
        Assert.False(invoked);
    }

    [Fact]
    public async Task BindErrorAsync_RecoversPendingFailure()
    {
        Result<int, int> result = await Result<int, string>.Fail("bad")
            .BindErrorAsync(static async error =>
            {
                await Task.Yield();
                return Result<int, int>.Ok(error.Length);
            });

        Assert.Equal(3, result.Value);
    }

    [Fact]
    public async Task BindErrorTaskAsync_ComposesAfterValueTaskOperation()
    {
        Result<int, int> result = await ValueTask.FromResult(Result<int, string>.Fail("bad"))
            .BindErrorTaskAsync(static error => Task.FromResult(Result<int, int>.Ok(error.Length)));

        Assert.Equal(3, result.Value);
    }

    [Fact]
    public async Task TaskReceiver_SupportsEveryFailureContinuationKind()
    {
        Task<Result<int, string>> synchronousSource = Task.FromResult(Result<int, string>.Fail("bad"));
        Task<Result<int, string>> valueTaskSource = Task.FromResult(Result<int, string>.Fail("bad"));
        Task<Result<int, string>> taskSource = Task.FromResult(Result<int, string>.Fail("bad"));

        Result<int, int> synchronous = await synchronousSource
            .BindError(static error => Result<int, int>.Ok(error.Length));
        Result<int, int> valueTask = await valueTaskSource
            .BindErrorAsync(static error => ValueTask.FromResult(Result<int, int>.Ok(error.Length)));
        Result<int, int> task = await taskSource
            .BindErrorTaskAsync(static error => Task.FromResult(Result<int, int>.Ok(error.Length)));

        Assert.Equal(3, synchronous.Value);
        Assert.Equal(3, valueTask.Value);
        Assert.Equal(3, task.Value);
    }
}
