using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

[SimpleJob(
    RuntimeMoniker.NativeAot10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 10,
    invocationCount: 100_000_000)]
[MemoryDiagnoser]
public class SwitchBenchmarks
{
    private Result<int, BenchmarkError> _successful;
    private Result<int, BenchmarkError> _failed;
    private Option<int> _some;
    private Option<int> _none;
    private int _sink;
    private Action<int> _success = null!;
    private Action<BenchmarkError> _failure = null!;
    private Action _noneAction = null!;

    [GlobalSetup]
    public void Setup()
    {
        _successful = Result<int, BenchmarkError>.Ok(42);
        _failed = Result<int, BenchmarkError>.Fail(new BenchmarkError(7));
        _some = Option<int>.Some(42);
        _none = Option<int>.None;
        _success = value => _sink = value;
        _failure = error => _sink = error.Code;
        _noneAction = () => _sink = 0;
    }

    [Benchmark(Baseline = true)]
    public int ResultSuccess()
    {
        _successful.Switch(_success, _failure);
        return _sink;
    }

    [Benchmark]
    public int ResultFailure()
    {
        _failed.Switch(_success, _failure);
        return _sink;
    }

    [Benchmark]
    public int OptionSome()
    {
        _some.Switch(_success, _noneAction);
        return _sink;
    }

    [Benchmark]
    public int OptionNone()
    {
        _none.Switch(_success, _noneAction);
        return _sink;
    }

    public readonly record struct BenchmarkError(int Code);
}
