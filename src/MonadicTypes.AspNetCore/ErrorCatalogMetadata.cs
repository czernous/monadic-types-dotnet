namespace MonadicTypes.AspNetCore;

/// <summary>Owns the immutable error catalog attached to one endpoint.</summary>
public sealed class ErrorCatalogMetadata
{
    private readonly ErrorCatalogEntry[] _entries;

    /// <summary>Copies and validates a non-empty endpoint error catalog.</summary>
    /// <param name="entries">The public errors the endpoint can return.</param>
    public ErrorCatalogMetadata(ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        if (entries.IsEmpty)
        {
            throw new ArgumentException("At least one error catalog entry is required.", nameof(entries));
        }

        _entries = entries.ToArray();
        for (int index = 0; index < _entries.Length; index++)
        {
            _entries[index].EnsureInitialized(nameof(entries));
            string code = _entries[index].Code;
            for (int previous = 0; previous < index; previous++)
            {
                if (code.Equals(_entries[previous].Code, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Duplicate error code '{code}'.", nameof(entries));
                }
            }
        }
    }

    /// <summary>Gets the number of catalog entries.</summary>
    public int Count => _entries.Length;

    /// <summary>Returns a zero-allocation view over the owned entries.</summary>
    public ReadOnlySpan<ErrorCatalogEntry> AsSpan() => _entries;
}
