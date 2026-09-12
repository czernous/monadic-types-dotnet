using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Adds one structured problem response to controller or endpoint metadata.</summary>
/// <param name="errorType">The initialized error category exposed by the operation.</param>
/// <example><code>[ProducesError(ErrorType.NotFound)]</code></example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ProducesErrorAttribute(ErrorType errorType) : Attribute, IProducesResponseTypeMetadata
{
    private readonly int? _statusCode;

    /// <summary>Documents an explicit HTTP error status for an application category.</summary>
    /// <example><code>[ProducesError(ErrorType.Validation, 422)]</code></example>
    /// <param name="errorType">The initialized application category.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599.</param>
    public ProducesErrorAttribute(ErrorType errorType, int statusCode) : this(errorType)
    {
        ErrorProblemDetails.ValidateStatusCode(statusCode);
        _statusCode = statusCode;
    }

    internal static readonly string[] ProblemContentTypes = ["application/problem+json"];

    /// <summary>Gets the configured error category.</summary>
    /// <example><code>ErrorType type = metadata.ErrorType;</code></example>
    public ErrorType ErrorType { get; } = errorType is < ErrorType.Failure or > ErrorType.NotImplemented
        ? throw new ArgumentOutOfRangeException(nameof(errorType))
        : errorType;

    /// <summary>Gets the documented RFC 9457 response body type.</summary>
    /// <example><code>Type? bodyType = metadata.Type;</code></example>
    public Type? Type => typeof(ProblemDetails);

    /// <summary>Gets the explicit HTTP status, or the default mapping from <see cref="ErrorType"/>.</summary>
    /// <example><code>int status = metadata.StatusCode;</code></example>
    public int StatusCode => _statusCode ?? ErrorProblemDetails.GetStatusCode(ErrorType);

    /// <summary>Gets the optional response description; this attribute leaves it unspecified.</summary>
    /// <example><code>string? description = metadata.Description;</code></example>
    public string? Description => null;

    /// <summary>Gets the supported RFC 9457 response content type.</summary>
    /// <example><code>IEnumerable&lt;string&gt; contentTypes = metadata.ContentTypes;</code></example>
    public IEnumerable<string> ContentTypes => ProblemContentTypes;
}
