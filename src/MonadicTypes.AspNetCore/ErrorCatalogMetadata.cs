using System.Buffers;
using System.Numerics;

namespace MonadicTypes.AspNetCore;

/// <summary>Owns the immutable error catalog attached to one endpoint.</summary>
/// <example><code>ErrorCatalogMetadata metadata = new([new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.")]);</code></example>
public sealed class ErrorCatalogMetadata
{
    private const int LinearScanThreshold = 8;
    private const int MaxProbeLength = 8;
    private const int MaxStackBuckets = 512;

    private readonly ErrorCatalogEntry[] _entries;

    /// <summary>Copies and validates a non-empty endpoint error catalog.</summary>
    /// <param name="entries">The public errors the endpoint can return.</param>
    /// <example><code>ErrorCatalogMetadata metadata = new(entries);</code></example>
    public ErrorCatalogMetadata(ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        if (entries.IsEmpty)
        {
            throw new ArgumentException("At least one error catalog entry is required.", nameof(entries));
        }

        _entries = entries.ToArray();
        ValidateEntries(_entries);
    }

    internal static void ValidateEntries(ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        if (entries.Length is <= LinearScanThreshold)
        {
            ValidateEntriesLinear(entries);
            return;
        }

        int bucketCount = GetBucketCount(entries.Length);
        int[]? rented = null;
        Span<int> buckets = bucketCount <= MaxStackBuckets
            ? stackalloc int[bucketCount]
            : (rented = ArrayPool<int>.Shared.Rent(bucketCount)).AsSpan(0, bucketCount);
        buckets.Clear();

        try
        {
            int bucketMask = bucketCount - 1;
            for (int index = 0; index < entries.Length; index++)
            {
                entries[index].EnsureInitialized(nameof(entries));
                string code = entries[index].Code;
                int slot = StringComparer.Ordinal.GetHashCode(code) & bucketMask;
                int probes = 0;
                while (buckets[slot] is not 0)
                {
                    int previous = buckets[slot] - 1;
                    if (string.Equals(entries[previous].Code, code, StringComparison.Ordinal))
                    {
                        throw new ArgumentException($"Duplicate error code '{code}'.", nameof(entries));
                    }

                    if (++probes is > MaxProbeLength)
                    {
                        ValidateEntriesLinear(entries);
                        return;
                    }

                    slot = (slot + 1) & bucketMask;
                }

                buckets[slot] = index + 1;
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

    private static void ValidateEntriesLinear(ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        for (int index = 0; index < entries.Length; index++)
        {
            entries[index].EnsureInitialized(nameof(entries));
            string code = entries[index].Code;
            for (int previous = 0; previous < index; previous++)
            {
                if (code.Equals(entries[previous].Code, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Duplicate error code '{code}'.", nameof(entries));
                }
            }
        }
    }

    private static int GetBucketCount(int entryCount)
    {
        uint required = checked((uint)entryCount * 2u);
        uint bucketCount = BitOperations.RoundUpToPowerOf2(required);
        return bucketCount is 0 or > int.MaxValue
            ? throw new ArgumentException("The error catalog is too large.", nameof(entryCount))
            : (int)bucketCount;
    }

    /// <summary>Gets the number of catalog entries.</summary>
    /// <example><code>int count = metadata.Count;</code></example>
    public int Count => _entries.Length;

    /// <summary>Returns a zero-allocation view over the owned entries.</summary>
    /// <example><code>ReadOnlySpan&lt;ErrorCatalogEntry&gt; entries = metadata.AsSpan();</code></example>
    public ReadOnlySpan<ErrorCatalogEntry> AsSpan() => _entries;
}
