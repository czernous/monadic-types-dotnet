using Microsoft.AspNetCore.Http;

namespace MonadicTypes.AspNetCore;

/// <summary>Maps any Result error at the HTTP boundary without DI or reflection.</summary>
public interface IHttpResultMapper<TError, out TResult>
    where TError : notnull
    where TResult : IResult
{
    TResult Map(in TError error, HttpContext? httpContext);
}
