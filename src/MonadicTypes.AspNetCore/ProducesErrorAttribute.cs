using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Adds one structured problem response to controller or endpoint metadata.</summary>
/// <param name="errorType">The initialized error category exposed by the operation.</param>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ProducesErrorAttribute(ErrorType errorType) : Attribute, IProducesResponseTypeMetadata
{
    private static readonly string[] ProblemContentTypes = ["application/problem+json"];

    /// <summary>Gets the configured error category.</summary>
    public ErrorType ErrorType { get; } = errorType is ErrorType.Uninitialized
        ? throw new ArgumentOutOfRangeException(nameof(errorType))
        : errorType;

    /// <inheritdoc />
    public Type? Type => typeof(ProblemDetails);

    /// <inheritdoc />
    public int StatusCode => ErrorProblemDetails.GetStatusCode(ErrorType);

    /// <inheritdoc />
    public string? Description => null;

    /// <inheritdoc />
    public IEnumerable<string> ContentTypes => ProblemContentTypes;
}
