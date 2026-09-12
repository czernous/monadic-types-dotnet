namespace MonadicTypes.AspNetCore;

/// <summary>
/// Describes one stable, publicly documented error returned by an endpoint.
/// This value is metadata only and is never created while handling a request.
/// </summary>
/// <example><code>ErrorCatalogEntry entry = new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.");</code></example>
public readonly record struct ErrorCatalogEntry
{
    // Zero means no override; validated HTTP errors fit alongside the category
    // without increasing the entry's array stride.
    private readonly ushort _statusCode;

    /// <summary>Creates a documented error with an explicit HTTP error status.</summary>
    /// <example><code>ErrorCatalogEntry entry = new(ErrorType.Conflict, "STALE", "Reload the resource.", 412);</code></example>
    /// <param name="type">The initialized application error category.</param>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="description">The public description exposed in API documentation.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599.</param>
    public ErrorCatalogEntry(ErrorType type, string code, string description, int statusCode)
        : this(type, code, description)
    {
        ErrorProblemDetails.ValidateStatusCode(statusCode);
        _statusCode = (ushort)statusCode;
    }

    /// <summary>Gets the explicit HTTP status override, or null to use the category's default mapping.</summary>
    /// <example><code>int status = entry.StatusCode ?? ErrorProblemDetails.GetStatusCode(entry.Type);</code></example>
    public int? StatusCode => _statusCode == 0 ? null : _statusCode;

    /// <summary>Creates one documented error entry.</summary>
    /// <param name="type">The initialized category that determines the HTTP status.</param>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="description">The public description exposed in API documentation.</param>
    /// <example><code>ErrorCatalogEntry entry = new(ErrorType.Conflict, "VERSION_CONFLICT", "Resource changed.");</code></example>
    public ErrorCatalogEntry(ErrorType type, string code, string description)
    {
        if (type is < ErrorType.Failure or > ErrorType.NotImplemented)
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Type = type;
        Code = code;
        Description = description;
    }

    /// <summary>Gets the application category used for the default HTTP mapping when no status override is supplied.</summary>
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
        if (Type is < ErrorType.Failure or > ErrorType.NotImplemented
            || string.IsNullOrWhiteSpace(Code)
            || string.IsNullOrWhiteSpace(Description))
        {
            throw new ArgumentException(
                "Every error catalog entry must be constructed and fully initialized.",
                parameterName);
        }
    }
}
