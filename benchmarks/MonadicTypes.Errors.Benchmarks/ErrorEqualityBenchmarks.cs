using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

/// <summary>Measures structured-error equality and hashing independently.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ErrorEqualityBenchmarks
{
    private Error _left = null!;
    private Error _equal = null!;
    private Error _equalDistinctText = null!;
    private Error _differentCause = null!;

    /// <summary>Creates errors and retained exceptions outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        InvalidOperationException cause = new("upstream message");
        _left = Error.Unavailable("STORE_UNAVAILABLE", "The store is unavailable.", cause: cause);
        _equal = Error.Unavailable("STORE_UNAVAILABLE", "The store is unavailable.", cause: cause);
        _equalDistinctText = Error.Unavailable(
            new string("STORE_UNAVAILABLE".AsSpan()),
            new string("The store is unavailable.".AsSpan()),
            cause: cause);
        _differentCause = Error.Unavailable(
            "STORE_UNAVAILABLE",
            "The store is unavailable.",
            cause: new InvalidOperationException("upstream message"));
    }

    /// <summary>Compares equal semantic fields and the same retained-cause reference.</summary>
    [Benchmark]
    public bool EqualSameCause() => _left.Equals(_equal);

    /// <summary>Compares equal ordinal text held by distinct string instances.</summary>
    [Benchmark]
    public bool EqualDistinctTextSameCause() => _left.Equals(_equalDistinctText);

    /// <summary>Rejects equal semantic fields that retain different cause instances.</summary>
    [Benchmark]
    public bool NotEqualDifferentCause() => _left.Equals(_differentCause);

    /// <summary>Hashes the semantic fields and retained-cause identity.</summary>
    [Benchmark]
    public int HashCode() => _left.GetHashCode();
}
