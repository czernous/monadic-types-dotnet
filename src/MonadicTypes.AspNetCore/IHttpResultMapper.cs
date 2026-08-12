using Microsoft.AspNetCore.Http;

namespace MonadicTypes.AspNetCore;

/// <summary>Maps any Result error at the HTTP boundary without DI or reflection.</summary>
public interface IHttpResultMapper<TError, out TResult>
    where TError : notnull
    where TResult : IResult
{
    /// <summary>Maps an error to a strongly typed HTTP result.</summary>
    /// <param name="failure">The failure value to map.</param>
    /// <param name="httpContext">Optional request context for transport-specific metadata.</param>
    /// <returns>The mapped HTTP result.</returns>
    TResult Map(in TError failure, HttpContext? httpContext);
}
