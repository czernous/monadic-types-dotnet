using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

/// <summary>Compares absence handling with the existing allocation-free composition.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class RequireSomeBenchmarks
{
    private Result<Option<int>, int> _source;
    private Func<int, int> _error = null!;

    /// <summary>Selects the present or absent successful branch.</summary>
    [Params(false, true)]
    public bool Present { get; set; }

    /// <summary>Creates input and callback outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = Result<Option<int>, int>.Ok(Present ? Option<int>.Some(42) : Option<int>.None);
        _error = static id => id;
    }

    /// <summary>Existing caller-state Bind and eager ToResult control.</summary>
    [Benchmark(Baseline = true)]
    public Result<int, int> BindControl() => _source.Bind(7, static (option, error) => option.ToResult(error));

    /// <summary>Named eager absence conversion.</summary>
    [Benchmark]
    public Result<int, int> Eager() => _source.RequireSome(7);

    /// <summary>Named lazy conversion with caller-owned state.</summary>
    [Benchmark]
    public Result<int, int> CallerState() => _source.RequireSome(7, _error);
}
