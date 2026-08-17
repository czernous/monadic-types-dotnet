using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore;

/// <summary>
/// Adds one stable domain error and its response category to controller or endpoint metadata.
/// </summary>
/// <param name="type">The initialized category that determines the HTTP status.</param>
/// <param name="code">The stable machine-readable error code.</param>
/// <param name="description">The public description exposed in API documentation.</param>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ProducesErrorCatalogAttribute(
    ErrorType type,
    string code,
    string description) : Attribute, IProducesResponseTypeMetadata
{
    /// <summary>Gets the documented error entry.</summary>
    public ErrorCatalogEntry Entry { get; } = new(type, code, description);

    /// <inheritdoc />
    public Type? Type => typeof(ProblemDetails);

    /// <inheritdoc />
    public int StatusCode => ErrorProblemDetails.GetStatusCode(Entry.Type);

    /// <inheritdoc />
    public string? Description => null;

    /// <inheritdoc />
    public IEnumerable<string> ContentTypes => ProducesErrorAttribute.ProblemContentTypes;
}
