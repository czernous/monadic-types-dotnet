using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;
using MonadicTypes.AspNetCore;

namespace Benchmarks;

/// <summary>Measures error-catalog value, ownership, and read costs independently.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ErrorCatalogBenchmarks
{
    private ErrorCatalogEntry[] _entries = null!;
    private ErrorCatalogMetadata _metadata = null!;
    private string _code = null!;
    private string _description = null!;

    /// <summary>Creates strings, source entries, and owned metadata outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _code = "ITEM_NOT_FOUND";
        _description = "The item was not found.";
        _entries =
        [
            new ErrorCatalogEntry(ErrorType.NotFound, _code, _description),
            new ErrorCatalogEntry(
                ErrorType.Unavailable,
                "STORE_UNAVAILABLE",
                "The item store is unavailable.")
        ];
        _metadata = new ErrorCatalogMetadata(_entries);
    }

    /// <summary>Constructs the immutable entry value without allocating owned data.</summary>
    [Benchmark]
    public ErrorCatalogEntry EntryConstruction() =>
        new(ErrorType.NotFound, _code, _description);

    /// <summary>Copies the source entries, establishing the minimum endpoint ownership allocation.</summary>
    [Benchmark]
    public ErrorCatalogEntry[] OwnedArrayCopy() => [.. _entries];

    /// <summary>Copies and validates entries into endpoint metadata during route registration.</summary>
    [Benchmark]
    public ErrorCatalogMetadata MetadataConstruction() => new(_entries);

    /// <summary>Reads owned metadata through its zero-allocation span view.</summary>
    [Benchmark]
    public int MetadataRead() => _metadata.Count + _metadata.AsSpan()[0].Code.Length;
}
