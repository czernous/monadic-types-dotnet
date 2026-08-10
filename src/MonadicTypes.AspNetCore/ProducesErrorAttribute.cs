using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ProducesErrorAttribute(ErrorType errorType) : Attribute, IProducesResponseTypeMetadata
{
    private static readonly string[] ProblemContentTypes = ["application/problem+json"];

    public ErrorType ErrorType { get; } = errorType is ErrorType.Uninitialized
        ? throw new ArgumentOutOfRangeException(nameof(errorType))
        : errorType;

    public Type? Type => typeof(ProblemDetails);
    public int StatusCode => ErrorProblemDetails.GetStatusCode(ErrorType);
    public string? Description => null;
    public IEnumerable<string> ContentTypes => ProblemContentTypes;
}
