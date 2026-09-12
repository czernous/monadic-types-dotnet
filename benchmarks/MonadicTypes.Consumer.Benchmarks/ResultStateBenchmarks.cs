using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

/// <summary>Compares caller-state Result recovery and fallback with direct branches and captured delegates.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ResultStateBenchmarks
{
    private Result<int, string> _source;
    private int _state;

    /// <summary>Selects the successful or failed branch.</summary>
    [Params(false, true)]
    public bool Failure { get; set; }

    /// <summary>Creates inputs outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = Failure
            ? Result<int, string>.Fail("missing")
            : Result<int, string>.Ok(7);
        _state = 42;
    }

    /// <summary>Direct branch control for Result-returning recovery.</summary>
    [Benchmark(Baseline = true)]
    public Result<int, string> RecoverBranchControl() => _source.IsSuccess
        ? _source
        : Result<int, string>.Ok(_source.Error.Length + _state);

    /// <summary>Recovery using explicit caller state and a static callback.</summary>
    [Benchmark]
    public Result<int, string> RecoverCallerState() => _source.Recover(
        _state,
        static (error, state) => Result<int, string>.Ok(error.Length + state));

    /// <summary>Recovery using a callback that captures this benchmark instance.</summary>
    [Benchmark]
    public Result<int, string> RecoverCaptured() => _source.Recover(
        error => Result<int, string>.Ok(error.Length + _state));

    /// <summary>Direct branch control for reducing a failure to a value.</summary>
    [Benchmark]
    public int ValueOrElseBranchControl() => _source.IsSuccess
        ? _source.Value
        : _source.Error.Length + _state;

    /// <summary>Fallback using explicit caller state and a static callback.</summary>
    [Benchmark]
    public int ValueOrElseCallerState() => _source.ValueOrElse(
        _state,
        static (error, state) => error.Length + state);

    /// <summary>Fallback using a callback that captures this benchmark instance.</summary>
    [Benchmark]
    public int ValueOrElseCaptured() => _source.ValueOrElse(error => error.Length + _state);
}
