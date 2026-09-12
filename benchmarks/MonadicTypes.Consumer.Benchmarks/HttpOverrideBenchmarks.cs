using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;
using MonadicTypes.AspNetCore;

namespace Benchmarks;

/// <summary>Measures response construction separately from error construction.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class HttpOverrideBenchmarks
{
    private Error _error = null!;
    private ErrorCatalogEntry[] _entries = null!;

    /// <summary>Prepares errors and metadata outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _error = Error.Conflict("STALE", "Reload the resource.");
        _entries = [new(ErrorType.Conflict, "STALE", "Reload the resource.", 412)];
    }

    /// <summary>Existing problem construction control.</summary>
    [Benchmark]
    public ProblemDetails DefaultResponse() => ErrorProblemDetails.CreateExample(_error);

    /// <summary>Explicit status with consistent problem identity.</summary>
    [Benchmark]
    public ProblemDetails ExplicitResponse() => ErrorProblemDetails.CreateExample(_error, 412);

    /// <summary>Cold metadata ownership and validation including the override.</summary>
    [Benchmark]
    public ErrorCatalogMetadata CatalogConstruction() => new(_entries);
}
