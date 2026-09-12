using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using MonadicTypes;
using MonadicTypes.Collections;

namespace Benchmarks;

/// <summary>Compares inferred wrappers with the existing bare callable path.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class TraversalBenchmarks
{
    private int[] _items = null!;
    private IReadOnlyList<int> _list = null!;
    private Option<int> _option;
    private Option<string> _other;
    private int _observed;
    private Action<int> _observe = null!;
    private Observe _structObserve;

    /// <summary>Constructs the eight-item input outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _items = [1, 2, 3, 4, 5, 6, 7, 8];
        _list = _items;
        _option = Option<int>.Some(42);
        _other = Option<string>.Some("value");
        _observe = value => _observed = value;
        _structObserve = new Observe(this);
    }

    /// <summary>Existing bare option traversal.</summary>
    [Benchmark(Baseline = true), BenchmarkCategory("Option")]
    public Result<Option<long>, int> OptionBare() => _option.Traverse<int, long, int, Increment>(default(Increment));

    /// <summary>Inferred generated option traversal.</summary>
    [Benchmark, BenchmarkCategory("Option")]
    public Result<Option<long>, int> OptionGenerated() => _option.Traverse(Projections.Functions.Widen);

    /// <summary>Existing bare list traversal with one output array.</summary>
    [Benchmark(Baseline = true), BenchmarkCategory("List")]
    public Result<long[], int> ListBare() => _list.TraverseToArray<int, long, int, Increment>(default(Increment));

    /// <summary>Inferred generated list traversal with one output array.</summary>
    [Benchmark, BenchmarkCategory("List")]
    public Result<long[], int> ListGenerated() => _list.TraverseToArray(Projections.Functions.Widen);

    /// <summary>Existing bare span traversal with one output array.</summary>
    [Benchmark(Baseline = true), BenchmarkCategory("Span")]
    public Result<long[], int> SpanBare() => _items.AsSpan().TraverseToArray<int, long, int, Increment>(default(Increment));

    /// <summary>Inferred generated span traversal with one output array.</summary>
    [Benchmark, BenchmarkCategory("Span")]
    public Result<long[], int> SpanGenerated() => _items.AsSpan().TraverseToArray(Projections.Functions.Widen);

    /// <summary>Direct branch control for observing an Option.</summary>
    [Benchmark(Baseline = true), BenchmarkCategory("OptionOperations")]
    public Option<int> OptionTapControl()
    {
        if (_option.IsSome)
        {
            _observed = _option.Value;
        }

        return _option;
    }

    /// <summary>Option Tap operation compared with its direct branch control.</summary>
    [Benchmark, BenchmarkCategory("OptionOperations")]
    public Option<int> OptionTap() => _option.Tap(_observe);

    /// <summary>Struct-callable Option Tap operation.</summary>
    [Benchmark, BenchmarkCategory("OptionOperations")]
    public Option<int> OptionTapStruct() => _option.Tap(_structObserve);

    /// <summary>Binary Option Zip operation.</summary>
    [Benchmark, BenchmarkCategory("OptionOperations")]
    public Option<(int First, string Second)> OptionZip() => _option.Zip(_other);

    private readonly struct Observe(TraversalBenchmarks owner) : IValueAction<int>
    {
        public void Invoke(int value) => owner._observed = value;
    }

    private readonly struct Increment : IValueFunction<int, Result<long, int>>
    {
        public Result<long, int> Invoke(int value) => Projections.Widen(value);
    }
}
