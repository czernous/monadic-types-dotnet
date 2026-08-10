using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

public readonly struct DefaultErrorHttpResultMapper : IHttpResultMapper<Error, ProblemHttpResult>
{
    public ProblemHttpResult Map(in Error error, HttpContext? httpContext) =>
        ErrorProblemDetails.ToHttpResult(error, httpContext);
}
