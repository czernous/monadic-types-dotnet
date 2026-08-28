using System.Buffers;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;
using MonadicTypes.AspNetCore;
using MonadicTypes.AspNetCore.OpenApi;

namespace Benchmarks;

/// <summary>Measures duplicate-code validation independently from input construction.</summary>
[SimpleJob(RuntimeMoniker.NativeAot10_0, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ErrorCatalogValidationBenchmarks
{
    private ErrorCatalogEntry[] _smallEntries = null!;
    private ErrorCatalogEntry[] _largeEntries = null!;
    private ErrorCatalogEntry[] _mediumEntries = null!;
    private ErrorCatalogEntry[] _collidingEntries = null!;
    private List<object> _smallMetadata = null!;
    private List<object> _largeMetadata = null!;
    private List<object> _collidingMetadata = null!;

    /// <summary>Creates unique and collision-heavy catalogs outside measured operations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _smallEntries = CreateEntries(8, collisionMask: -1);
        _mediumEntries = CreateEntries(48, collisionMask: -1);
        _largeEntries = CreateEntries(160, collisionMask: -1);
        _collidingEntries = CreateEntries(48, collisionMask: 127);
        _smallMetadata = CreateMetadata(_smallEntries, 2);
        _largeMetadata = CreateMetadata(_largeEntries, 8);
        _collidingMetadata = CreateMetadata(_collidingEntries, 3);
    }

    /// <summary>Runs the established nested-loop validator over a small catalog.</summary>
    [Benchmark(Baseline = true)]
    public void LegacySmallCatalog() => LegacyValidateEntries(_smallEntries);

    /// <summary>Runs the current metadata validator over a small catalog.</summary>
    [Benchmark]
    public void CurrentSmallCatalog() => ErrorCatalogMetadata.ValidateEntries(_smallEntries);

    /// <summary>Runs the established nested-loop validator over a medium catalog.</summary>
    [Benchmark]
    public void LegacyMediumCatalog() => LegacyValidateEntries(_mediumEntries);

    /// <summary>Runs the current metadata validator over a medium catalog.</summary>
    [Benchmark]
    public void CurrentMediumCatalog() => ErrorCatalogMetadata.ValidateEntries(_mediumEntries);

    /// <summary>Runs the established nested-loop validator over a large catalog.</summary>
    [Benchmark]
    public void LegacyLargeCatalog() => LegacyValidateEntries(_largeEntries);

    /// <summary>Runs the current metadata validator over a large catalog.</summary>
    [Benchmark]
    public void CurrentLargeCatalog() => ErrorCatalogMetadata.ValidateEntries(_largeEntries);

    /// <summary>Runs the established nested-loop validator over collision-heavy unique codes.</summary>
    [Benchmark]
    public void LegacyCollidingCatalog() => LegacyValidateEntries(_collidingEntries);

    /// <summary>Runs the current metadata validator over collision-heavy unique codes.</summary>
    [Benchmark]
    public void CurrentCollidingCatalog() => ErrorCatalogMetadata.ValidateEntries(_collidingEntries);

    /// <summary>Runs the former re-enumerating validator over small endpoint metadata.</summary>
    [Benchmark]
    public void LegacySmallMetadata() => LegacyValidateMetadata(_smallMetadata);

    /// <summary>Runs the packed-location validator over small endpoint metadata.</summary>
    [Benchmark]
    public void CurrentSmallMetadata() => ErrorCatalogOpenApiTransformer.ValidateUniqueCodes(_smallMetadata);

    /// <summary>Runs the former re-enumerating validator over large endpoint metadata.</summary>
    [Benchmark]
    public void LegacyLargeMetadata() => LegacyValidateMetadata(_largeMetadata);

    /// <summary>Runs the packed-location validator over large endpoint metadata.</summary>
    [Benchmark]
    public void CurrentLargeMetadata() => ErrorCatalogOpenApiTransformer.ValidateUniqueCodes(_largeMetadata);

    /// <summary>Runs the former re-enumerating validator over collision-heavy endpoint metadata.</summary>
    [Benchmark]
    public void LegacyCollidingMetadata() => LegacyValidateMetadata(_collidingMetadata);

    /// <summary>Runs the packed-location validator over collision-heavy endpoint metadata.</summary>
    [Benchmark]
    public void CurrentCollidingMetadata() => ErrorCatalogOpenApiTransformer.ValidateUniqueCodes(_collidingMetadata);

    private static ErrorCatalogEntry[] CreateEntries(int count, int collisionMask)
    {
        var entries = new ErrorCatalogEntry[count];
        int candidate = 0;
        int index = 0;
        while (index < entries.Length)
        {
            string code = $"ERROR_{candidate++:X8}";
            if (collisionMask >= 0 && (code.GetHashCode(StringComparison.Ordinal) & collisionMask) is not 0)
            {
                continue;
            }

            entries[index++] = new ErrorCatalogEntry(ErrorType.Failure, code, "Documented failure.");
        }

        return entries;
    }

    private static List<object> CreateMetadata(ReadOnlySpan<ErrorCatalogEntry> entries, int entriesPerCatalog)
    {
        int count = (entries.Length + entriesPerCatalog - 1) / entriesPerCatalog;
        var metadata = new List<object>(count);
        for (int index = 0; index < entries.Length; index += entriesPerCatalog)
        {
            metadata.Add(new ErrorCatalogMetadata(entries.Slice(index, Math.Min(entriesPerCatalog, entries.Length - index))));
        }

        return metadata;
    }

    private static void LegacyValidateEntries(ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        for (int index = 0; index < entries.Length; index++)
        {
            entries[index].EnsureInitialized(nameof(entries));
            string code = entries[index].Code;
            for (int previous = 0; previous < index; previous++)
            {
                if (code.Equals(entries[previous].Code, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    private static void LegacyValidateMetadata(IList<object> metadata)
    {
        int entryCount = CountEntries(metadata);
        if (entryCount < 2)
        {
            return;
        }

        int bucketCount = checked((int)BitOperations.RoundUpToPowerOf2((uint)entryCount * 2u));
        int[]? rented = null;
        Span<int> buckets = bucketCount <= 256
            ? stackalloc int[bucketCount]
            : (rented = ArrayPool<int>.Shared.Rent(bucketCount)).AsSpan(0, bucketCount);
        buckets.Clear();

        try
        {
            int entryIndex = 0;
            var entries = new LegacyCatalogEntryEnumerator(metadata);
            while (entries.MoveNext())
            {
                ErrorCatalogEntry entry = entries.Current;
                int slot = StringComparer.Ordinal.GetHashCode(entry.Code) & (bucketCount - 1);
                while (buckets[slot] is not 0)
                {
                    ErrorCatalogEntry previous = GetEntryAt(metadata, buckets[slot] - 1);
                    if (string.Equals(previous.Code, entry.Code, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException();
                    }

                    slot = (slot + 1) & (bucketCount - 1);
                }

                buckets[slot] = ++entryIndex;
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<int>.Shared.Return(rented);
            }
        }
    }

    private static int CountEntries(IList<object> metadata)
    {
        int count = 0;
        for (int index = 0; index < metadata.Count; index++)
        {
            count += metadata[index] is ErrorCatalogMetadata catalog ? catalog.Count : 0;
        }

        return count;
    }

    private static ErrorCatalogEntry GetEntryAt(IList<object> metadata, int targetIndex)
    {
        var entries = new LegacyCatalogEntryEnumerator(metadata);
        for (int index = 0; entries.MoveNext(); index++)
        {
            if (index == targetIndex)
            {
                return entries.Current;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(targetIndex));
    }

    private struct LegacyCatalogEntryEnumerator(IList<object> metadata)
    {
        private int _metadataIndex;
        private int _entryIndex;

        public ErrorCatalogEntry Current { get; private set; }

        public bool MoveNext()
        {
            while (_metadataIndex < metadata.Count)
            {
                if (metadata[_metadataIndex] is ErrorCatalogMetadata catalog
                    && _entryIndex < catalog.Count)
                {
                    Current = catalog.AsSpan()[_entryIndex++];
                    return true;
                }

                _metadataIndex++;
                _entryIndex = 0;
            }

            return false;
        }
    }
}
