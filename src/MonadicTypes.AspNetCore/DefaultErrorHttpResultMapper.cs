using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Maps structured errors to the library's default RFC 9457 HTTP result.</summary>
public readonly struct DefaultErrorHttpResultMapper : IHttpResultMapper<Error, ProblemHttpResult>
{
    /// <inheritdoc />
    public ProblemHttpResult Map(in Error failure, HttpContext? httpContext) =>
        ErrorProblemDetails.ToHttpResult(failure, httpContext);
}
