using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;
using MonadicTypes.Async;
using MonadicTypes.Effects;

namespace Benchmarks;

/// <summary>Measures additive result composition without including setup in measured operations.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class CompositionBenchmarks
{
    private Result<int, BenchmarkError> _success;
    private Result<int, BenchmarkError> _failure;
    private Result<Option<int>, BenchmarkError> _optionalSuccess;
    private Result<Unit, BenchmarkError> _unitSuccess;
    private Result<long, BenchmarkError> _longSuccess;
    private ValueTask<Result<int, BenchmarkError>> _completedValueTask;
    private Task<Result<int, BenchmarkError>> _completedTask = null!;
    private Func<int, long> _map = null!;
    private Func<long, long> _mapLong = null!;
    private Func<int, ValueTask<long>> _mapAsync = null!;
    private Func<BenchmarkError, Result<int, BenchmarkError>> _bindError = null!;
    private Func<BenchmarkError, Task<Result<int, BenchmarkError>>> _bindErrorTask = null!;
    private Func<int> _effect = null!;
    private Func<ValueTask<int>> _valueTaskEffect = null!;
    private Func<Task<int>> _taskEffect = null!;
    private Task<int> _completedEffect = null!;
    private Func<Exception, BenchmarkError> _mapException = null!;
    private Func<TimeoutException, BenchmarkError> _mapTimeout = null!;

    /// <summary>Initializes every benchmark input and callback outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _success = Result<int, BenchmarkError>.Ok(42);
        _failure = Result<int, BenchmarkError>.Fail(new BenchmarkError(7));
        _optionalSuccess = Result<Option<int>, BenchmarkError>.Ok(Option<int>.Some(42));
        _unitSuccess = Result.Ok<BenchmarkError>();
        _longSuccess = Result<long, BenchmarkError>.Ok(43L);
        _completedValueTask = ValueTask.FromResult(_success);
        _completedTask = Task.FromResult(_success);
        _map = static value => value + 1L;
        _mapLong = static value => value + 1L;
        _mapAsync = static value => ValueTask.FromResult(value + 1L);
        _bindError = static error => Result<int, BenchmarkError>.Ok(error.Code);
        Task<Result<int, BenchmarkError>> recoveredTask =
            Task.FromResult(Result<int, BenchmarkError>.Ok(_failure.Error.Code));
        _bindErrorTask = _ => recoveredTask;
        _effect = static () => 42;
        _valueTaskEffect = static () => ValueTask.FromResult(42);
        Task<int> completedEffect = Task.FromResult(42);
        _completedEffect = completedEffect;
        _taskEffect = () => completedEffect;
        _mapException = static exception => new BenchmarkError(exception.HResult);
        _mapTimeout = static exception => new BenchmarkError(exception.HResult);
    }

    /// <summary>Measures a direct synchronous transform used as the local control.</summary>
    [Benchmark(Baseline = true)]
    public long DirectMap() => _map(42);

    /// <summary>Measures failure-side bind on the active failure branch.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> BindErrorFailure() => _failure.BindError(_bindError);

    /// <summary>Measures failure-side bind bypass on the dominant success branch.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> BindErrorSuccess() => _success.BindError(_bindError);

    /// <summary>Measures Task-returning failure recovery using a pre-existing completed Task.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> CompletedTaskBindError() =>
        _failure.BindErrorTaskAsync(_bindErrorTask).Result;

    /// <summary>Measures two-result fail-fast combination.</summary>
    [Benchmark]
    public Result<Unit, BenchmarkError> CombineTwo() => ResultCombination.Combine(_unitSuccess, _unitSuccess);

    /// <summary>Measures heterogeneous tuple combination.</summary>
    [Benchmark]
    public Result<(int First, long Second), BenchmarkError> ZipTwo() =>
        ResultCombination.Zip(_success, _longSuccess);

    /// <summary>Measures result-option transposition for a present value.</summary>
    [Benchmark]
    public Option<Result<int, BenchmarkError>> TransposePresent() => _optionalSuccess.Transpose();

    /// <summary>Measures a completed ValueTask map without awaiting or allocating a Task.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> CompletedValueTaskMap() => _completedValueTask.Map(_map).Result;

    /// <summary>Measures an asynchronous callback that completes synchronously through ValueTask.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> CompletedAsyncMap() => _success.MapAsync(_mapAsync).Result;

    /// <summary>Measures the generated callable equivalent of the completed asynchronous map.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> GeneratedCompletedAsyncMap() =>
        _success.MapAsync(GeneratedBenchmarkOperations.Functions.MapAsync).Result;

    /// <summary>Measures a mixed asynchronous then synchronous fluent pipeline.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> MixedCompletedPipeline() =>
        _success.MapAsync(_mapAsync).Map(_mapLong).Result;

    /// <summary>Measures lifting a pre-existing completed Task into the ValueTask pipeline.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> CompletedTaskReceiverMap() => _completedTask.Map(_map).Result;

    /// <summary>Measures an explicit exception boundary when the operation succeeds.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> EffectSuccess() => Effect.Try(_effect, _mapException);

    /// <summary>Measures a typed ValueTask effect that completes synchronously.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> TypedCompletedValueTaskEffectSuccess() =>
        Effect.TryAsync<int, BenchmarkError, TimeoutException>(_valueTaskEffect, _mapTimeout).Result;

    /// <summary>Measures a typed Task effect using a pre-existing completed Task.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> TypedCompletedTaskEffectSuccess() =>
        Effect.TryTaskAsync<int, BenchmarkError, TimeoutException>(_taskEffect, _mapTimeout).Result;

    /// <summary>Measures the typed caller-state Task effect without a capturing operation delegate.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> TypedCallerStateCompletedTaskEffectSuccess() =>
        Effect.TryTaskAsync(
            _completedEffect,
            static task => task,
            _mapTimeout).Result;

    /// <summary>Compact benchmark error carried entirely by value.</summary>
    /// <param name="Code">Stable error code.</param>
    public readonly record struct BenchmarkError(int Code);
}

/// <summary>Hosts generated callables used by the composition benchmarks.</summary>
public static partial class GeneratedBenchmarkOperations
{
    /// <summary>Returns a synchronously completed asynchronous transform.</summary>
    [GenerateValueFunction]
    public static ValueTask<long> MapAsync(int value) => ValueTask.FromResult(value + 1L);
}
