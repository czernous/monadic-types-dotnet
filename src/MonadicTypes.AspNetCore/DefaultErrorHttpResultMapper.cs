using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Maps structured errors to the library's default RFC 9457 HTTP result.</summary>
/// <example><code>ProblemHttpResult response = default(DefaultErrorHttpResultMapper).Map(error, httpContext);</code></example>
public readonly struct DefaultErrorHttpResultMapper : IHttpResultMapper<Error, ProblemHttpResult>
{
    /// <summary>Maps <paramref name="failure"/> to a strongly typed problem result.</summary>
    /// <param name="failure">Error to map.</param>
    /// <param name="httpContext">Optional request context included in the problem payload.</param>
    /// <returns>The mapped problem result.</returns>
    /// <example><code>ProblemHttpResult response = mapper.Map(error, httpContext);</code></example>
    public ProblemHttpResult Map(in Error failure, HttpContext? httpContext) =>
        ErrorProblemDetails.ToHttpResult(failure, httpContext);
}
