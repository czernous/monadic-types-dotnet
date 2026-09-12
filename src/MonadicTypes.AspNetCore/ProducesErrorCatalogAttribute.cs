using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore;

/// <summary>
/// Adds one stable domain error and its response category to controller or endpoint metadata.
/// </summary>
/// <param name="type">The initialized category that determines the HTTP status.</param>
/// <param name="code">The stable machine-readable error code.</param>
/// <param name="description">The public description exposed in API documentation.</param>
/// <example><code>[ProducesErrorCatalog(ErrorType.NotFound, "USER_NOT_FOUND", "User not found.")]</code></example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ProducesErrorCatalogAttribute(
    ErrorType type,
    string code,
    string description) : Attribute, IProducesResponseTypeMetadata
{
    /// <summary>Documents a stable error code with an explicit HTTP error status.</summary>
    /// <example><code>[ProducesErrorCatalog(ErrorType.Conflict, "STALE", "Reload the resource.", 412)]</code></example>
    /// <param name="type">The initialized application category.</param>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="description">The public description exposed in API documentation.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599.</param>
    public ProducesErrorCatalogAttribute(ErrorType type, string code, string description, int statusCode)
        : this(type, code, description)
    {
        Entry = new(type, code, description, statusCode);
    }

    /// <summary>Gets the documented error entry.</summary>
    /// <example><code>ErrorCatalogEntry entry = metadata.Entry;</code></example>
    public ErrorCatalogEntry Entry { get; } = new(type, code, description);

    /// <summary>Gets the documented RFC 9457 response body type.</summary>
    /// <example><code>Type? bodyType = metadata.Type;</code></example>
    public Type? Type => typeof(ProblemDetails);

    /// <summary>Gets the HTTP status mapped from the catalog entry.</summary>
    /// <example><code>int status = metadata.StatusCode;</code></example>
    public int StatusCode => Entry.StatusCode ?? ErrorProblemDetails.GetStatusCode(Entry.Type);

    /// <summary>Gets the optional response description; this attribute leaves it unspecified.</summary>
    /// <example><code>string? description = metadata.Description;</code></example>
    public string? Description => null;

    /// <summary>Gets the supported RFC 9457 response content type.</summary>
    /// <example><code>IEnumerable&lt;string&gt; contentTypes = metadata.ContentTypes;</code></example>
    public IEnumerable<string> ContentTypes => ProducesErrorAttribute.ProblemContentTypes;
}
