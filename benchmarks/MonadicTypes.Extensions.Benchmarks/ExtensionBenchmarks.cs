using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;
using MonadicTypes.Collections;
using MonadicTypes.Linq;

namespace Benchmarks;

/// <summary>Measures newly added APIs against direct, allocation-equivalent controls.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ExtensionBenchmarks
{
    private IReadOnlyList<int> _source = null!;
    private Result<int, BenchmarkError> _first;
    private Result<int, BenchmarkError> _second;
    private Result<int, BenchmarkError> _third;
    private Result<int, BenchmarkError> _fourth;
    private Result<int, BenchmarkError> _fifth;
    private Result<int, BenchmarkError> _sixth;
    private Func<int, Result<long, BenchmarkError>> _traverse = null!;
    private Func<int, int, Result<long, BenchmarkError>> _traverseState = null!;
    private Func<int, long> _select = null!;
    private Func<int, Result<int, BenchmarkError>> _bind = null!;
    private Func<int, int, int> _project = null!;

    /// <summary>Creates inputs and delegates outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        _first = Result<int, BenchmarkError>.Ok(1);
        _second = Result<int, BenchmarkError>.Ok(2);
        _third = Result<int, BenchmarkError>.Ok(3);
        _fourth = Result<int, BenchmarkError>.Ok(4);
        _fifth = Result<int, BenchmarkError>.Ok(5);
        _sixth = Result<int, BenchmarkError>.Ok(6);
        _traverse = static value => Result<long, BenchmarkError>.Ok(value + 1L);
        _traverseState = static (value, increment) => Result<long, BenchmarkError>.Ok(value + increment);
        _select = static value => value + 1L;
        _bind = static value => Result<int, BenchmarkError>.Ok(value + 1);
        _project = static (left, right) => left + right;
    }

    /// <summary>Manual count-known traversal control with the same one-array ownership contract.</summary>
    [Benchmark(Baseline = true)]
    public Result<long[], BenchmarkError> ManualTraverse()
    {
        int count = _source.Count;
        long[] output = new long[count];
        for (int index = 0; index < count; index++)
        {
            Result<long, BenchmarkError> selected = _traverse(_source[index]);
            if (selected.IsFailure)
            {
                return Result<long[], BenchmarkError>.Fail(selected.Error);
            }

            output[index] = selected.Value;
        }

        return Result<long[], BenchmarkError>.Ok(output);
    }

    /// <summary>Measures ergonomic delegate traversal.</summary>
    [Benchmark]
    public Result<long[], BenchmarkError> DelegateTraverse() => _source.TraverseToArray(_traverse);

    /// <summary>Measures caller-state traversal.</summary>
    [Benchmark]
    public Result<long[], BenchmarkError> StateTraverse() => _source.TraverseToArray(1, _traverseState);

    /// <summary>Measures allocation-free callable dispatch; only the owned output array may allocate.</summary>
    [Benchmark]
    public Result<long[], BenchmarkError> CallableTraverse() =>
        _source.TraverseToArray<int, long, BenchmarkError, Increment>(default);

    /// <summary>Measures direct six-value projection as the combination control.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> DirectMapSix() => Result<int, BenchmarkError>.Ok(
        _first.Value + _second.Value + _third.Value + _fourth.Value + _fifth.Value + _sixth.Value);

    /// <summary>Measures six-input fail-fast projection.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> CombinationMapSix() => ResultCombination.Map(
        _first,
        _second,
        _third,
        _fourth,
        _fifth,
        _sixth,
        static (a, b, c, d, e, f) => a + b + c + d + e + f);

    /// <summary>Measures direct core map as the single-stage LINQ control.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> DirectMap() => _first.Map(_select);

    /// <summary>Measures fluent LINQ method syntax.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> FluentSelect() => _first.Select(_select);

    /// <summary>Measures query-expression Select syntax.</summary>
    [Benchmark]
    public Result<long, BenchmarkError> QuerySelect() =>
        from value in _first
        select value + 1L;

    /// <summary>Measures fluent multi-stage LINQ method syntax with pre-created delegates.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> FluentSelectMany() => _first.SelectMany(_bind, _project);

    /// <summary>Measures equivalent query-expression syntax.</summary>
    [Benchmark]
    public Result<int, BenchmarkError> QuerySelectMany() =>
        from left in _first
        from right in Bind(left)
        select left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Result<int, BenchmarkError> Bind(int value) => Result<int, BenchmarkError>.Ok(value + 1);

    private readonly struct Increment : IValueFunction<int, Result<long, BenchmarkError>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<long, BenchmarkError> Invoke(int value) => Result<long, BenchmarkError>.Ok(value + 1L);
    }

    /// <summary>Compact benchmark-only error.</summary>
    /// <param name="Code">Stable error code.</param>
    public readonly record struct BenchmarkError(int Code);
}
