using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

public static class ErrorProblemDetails
{
    public static ProblemDetails Create(in Error error, HttpContext? httpContext = null)
    {
        EnsureInitialized(error);

        ProblemDetails details = new()
        {
            Status = GetStatusCode(error.Type),
            Title = GetTitle(error.Type),
            Detail = error.IsMessagePublic ? error.Message : null,
            Type = GetTypeUri(error.Type)
        };

        details.Extensions["code"] = error.Code;
        string? traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (traceId is not null)
        {
            details.Extensions["traceId"] = traceId;
        }

        return details;
    }

    public static ProblemHttpResult ToHttpResult(in Error error, HttpContext? httpContext = null) =>
        TypedResults.Problem(Create(error, httpContext));

    public static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Cancelled => 499,
        ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
        ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom =>
            StatusCodes.Status500InternalServerError,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation failed",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Not found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Cancelled => "Request cancelled",
        ErrorType.RateLimited => "Too many requests",
        ErrorType.Timeout => "Gateway timeout",
        ErrorType.Unavailable => "Service unavailable",
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom => "An unexpected error occurred",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    private static string GetTypeUri(ErrorType type) => type switch
    {
        ErrorType.Validation => "urn:problem-type:validation",
        ErrorType.Unauthorized => "urn:problem-type:unauthorized",
        ErrorType.Forbidden => "urn:problem-type:forbidden",
        ErrorType.NotFound => "urn:problem-type:not-found",
        ErrorType.Conflict => "urn:problem-type:conflict",
        ErrorType.Cancelled => "urn:problem-type:cancelled",
        ErrorType.RateLimited => "urn:problem-type:rate-limited",
        ErrorType.Timeout => "urn:problem-type:timeout",
        ErrorType.Unavailable => "urn:problem-type:unavailable",
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom => "urn:problem-type:unexpected",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    private static void EnsureInitialized(in Error error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }
    }
}
