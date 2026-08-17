using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

/// <summary>Compares positional patterns with direct branching and delegate-based Match.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class PatternBenchmarks
{
    private Result<int, PatternError> _success;
    private Result<int, PatternError> _failure;
    private Option<int> _some;
    private Option<int> _none;
    private Func<int, int> _resultValue = null!;
    private Func<PatternError, int> _resultError = null!;
    private Func<int> _optionNone = null!;

    /// <summary>Creates inputs and callbacks outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _success = Result<int, PatternError>.Ok(42);
        _failure = Result<int, PatternError>.Fail(new PatternError(7));
        _some = Option<int>.Some(42);
        _none = Option<int>.None;
        _resultValue = static value => value;
        _resultError = static error => -error.Code;
        _optionNone = static () => -1;
    }

    /// <summary>Direct Result success branch control.</summary>
    [Benchmark]
    public int ResultDirectSuccess() => _success.IsSuccess ? _success.Value : -_success.Error.Code;

    /// <summary>Positional Result success pattern.</summary>
    [Benchmark]
    public int ResultPatternSuccess() => _success switch
    {
        (true, int value, _) => value,
        (false, _, PatternError error) => -error.Code
    };

    /// <summary>Delegate Result success fold.</summary>
    [Benchmark]
    public int ResultMatchSuccess() => _success.Match(_resultValue, _resultError);

    /// <summary>Direct Result failure branch control.</summary>
    [Benchmark]
    public int ResultDirectFailure() => _failure.IsSuccess ? _failure.Value : -_failure.Error.Code;

    /// <summary>Positional Result failure pattern.</summary>
    [Benchmark]
    public int ResultPatternFailure() => _failure switch
    {
        (true, int value, _) => value,
        (false, _, PatternError error) => -error.Code
    };

    /// <summary>Delegate Result failure fold.</summary>
    [Benchmark]
    public int ResultMatchFailure() => _failure.Match(_resultValue, _resultError);

    /// <summary>Direct Option Some branch control.</summary>
    [Benchmark]
    public int OptionDirectSome() => _some.HasValue ? _some.Value : -1;

    /// <summary>Positional Option Some pattern.</summary>
    [Benchmark]
    public int OptionPatternSome() => _some switch
    {
        (true, int value) => value,
        (false, _) => -1
    };

    /// <summary>Delegate Option Some fold.</summary>
    [Benchmark]
    public int OptionMatchSome() => _some.Match(_resultValue, _optionNone);

    /// <summary>Direct Option None branch control.</summary>
    [Benchmark]
    public int OptionDirectNone() => _none.HasValue ? _none.Value : -1;

    /// <summary>Positional Option None pattern.</summary>
    [Benchmark]
    public int OptionPatternNone() => _none switch
    {
        (true, int value) => value,
        (false, _) => -1
    };

    /// <summary>Delegate Option None fold.</summary>
    [Benchmark]
    public int OptionMatchNone() => _none.Match(_resultValue, _optionNone);

    /// <summary>Compact pattern benchmark error.</summary>
    /// <param name="Code">Failure code.</param>
    public readonly record struct PatternError(int Code);
}
