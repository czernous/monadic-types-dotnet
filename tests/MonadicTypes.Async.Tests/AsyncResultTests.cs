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

    [Fact]
    public async Task GeneratedCallables_SupportEveryAsyncOperationKind()
    {
        Result<int, string> success = Result<int, string>.Ok(4);
        Result<int, string> failure = Result<int, string>.Fail("bad");

        Result<long, string> mapped = await success
            .MapAsync(GeneratedAsyncOperations.Functions.MapValueTask)
            .MapTaskAsync(GeneratedAsyncOperations.Functions.MapTask);
        Result<long, string> bound = await success
            .BindAsync(GeneratedAsyncOperations.Functions.BindValueTask)
            .BindTaskAsync(GeneratedAsyncOperations.Functions.BindTask);
        Result<int, int> recovered = await ValueTask.FromResult(failure)
            .BindErrorAsync(GeneratedAsyncOperations.Functions.BindErrorValueTask)
            .BindErrorTaskAsync(GeneratedAsyncOperations.Functions.BindErrorTask);

        Assert.Equal(6L, mapped.Value);
        Assert.Equal(6L, bound.Value);
        Assert.Equal(3, recovered.Value);
    }

    [Fact]
    public async Task GeneratedCallable_ComposesFromTaskReceiver()
    {
        Task<Result<int, string>> source = Task.FromResult(Result<int, string>.Ok(4));

        Result<long, string> result = await source
            .MapAsync(GeneratedAsyncOperations.Functions.MapValueTask)
            .MapTaskAsync(GeneratedAsyncOperations.Functions.MapTask);

        Assert.Equal(6L, result.Value);
    }
}

public static partial class GeneratedAsyncOperations
{
    [GenerateValueFunction]
    public static ValueTask<long> MapValueTask(int value) => ValueTask.FromResult(value + 1L);

    [GenerateValueFunction]
    public static Task<long> MapTask(long value) => Task.FromResult(value + 1L);

    [GenerateValueFunction]
    public static ValueTask<Result<long, string>> BindValueTask(int value) =>
        ValueTask.FromResult(Result<long, string>.Ok(value + 1L));

    [GenerateValueFunction]
    public static Task<Result<long, string>> BindTask(long value) =>
        Task.FromResult(Result<long, string>.Ok(value + 1L));

    [GenerateValueFunction]
    public static ValueTask<Result<int, int>> BindErrorValueTask(string error) =>
        ValueTask.FromResult(Result<int, int>.Fail(error.Length));

    [GenerateValueFunction]
    public static Task<Result<int, int>> BindErrorTask(int error) =>
        Task.FromResult(Result<int, int>.Ok(error));
}
