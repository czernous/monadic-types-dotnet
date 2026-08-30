namespace MonadicTypes.AspNetCore;

/// <summary>
/// Describes one stable, publicly documented error returned by an endpoint.
/// This value is metadata only and is never created while handling a request.
/// </summary>
/// <example><code>ErrorCatalogEntry entry = new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.");</code></example>
public readonly record struct ErrorCatalogEntry
{
    /// <summary>Creates one documented error entry.</summary>
    /// <param name="type">The initialized category that determines the HTTP status.</param>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="description">The public description exposed in API documentation.</param>
    /// <example><code>ErrorCatalogEntry entry = new(ErrorType.Conflict, "VERSION_CONFLICT", "Resource changed.");</code></example>
    public ErrorCatalogEntry(ErrorType type, string code, string description)
    {
        if (type is < ErrorType.Failure or > ErrorType.Custom)
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Type = type;
        Code = code;
        Description = description;
    }

    /// <summary>Gets the category that determines the documented HTTP status.</summary>
    /// <example><code>ErrorType type = entry.Type;</code></example>
    public ErrorType Type { get; }

    /// <summary>Gets the stable machine-readable error code.</summary>
    /// <example><code>string code = entry.Code;</code></example>
    public string Code { get; }

    /// <summary>Gets the public description emitted into API documentation.</summary>
    /// <example><code>string description = entry.Description;</code></example>
    public string Description { get; }

    internal void EnsureInitialized(string parameterName)
    {
        if (Type is < ErrorType.Failure or > ErrorType.Custom
            || string.IsNullOrWhiteSpace(Code)
            || string.IsNullOrWhiteSpace(Description))
        {
            throw new ArgumentException(
                "Every error catalog entry must be constructed and fully initialized.",
                parameterName);
        }
    }
}
