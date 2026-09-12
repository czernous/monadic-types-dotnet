using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Converts structured errors to default RFC 9457 problem details.</summary>
public static class ErrorProblemDetails
{
    /// <summary>Creates problem details with an explicit HTTP error status and the existing visibility and trace policy.</summary>
    /// <example><code>ProblemDetails problem = ErrorProblemDetails.Create(error, 422, httpContext);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599, independent of the error's numeric category.</param>
    /// <param name="httpContext">Optional request context supplying a fallback trace identifier.</param>
    /// <returns>Problem details with a status-specific title and type URI.</returns>
    /// <remarks>Protocol-specific headers remain the endpoint's responsibility.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The status is outside the HTTP error range.</exception>
    public static ProblemDetails Create(in Error error, int statusCode, HttpContext? httpContext = null)
    {
        EnsureInitialized(error);
        ValidateStatusCode(statusCode);
        string? traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        return CreateCore(error, traceId, statusCode);
    }

    /// <summary>Creates a deterministic example with an explicit HTTP error status and no ambient trace data.</summary>
    /// <example><code>ProblemDetails example = ErrorProblemDetails.CreateExample(error, 412);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599.</param>
    /// <returns>Problem details without a trace identifier.</returns>
    public static ProblemDetails CreateExample(in Error error, int statusCode)
    {
        EnsureInitialized(error);
        ValidateStatusCode(statusCode);
        return CreateCore(error, traceId: null, statusCode);
    }

    /// <summary>Creates a typed problem result with an explicit HTTP error status.</summary>
    /// <example><code>var response = ErrorProblemDetails.ToHttpResult(error, 413, httpContext);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <param name="statusCode">HTTP error status from 400 through 599.</param>
    /// <param name="httpContext">Optional request context included in the problem payload.</param>
    /// <returns>A typed problem result preserving the error's code and visibility policy.</returns>
    public static ProblemHttpResult ToHttpResult(in Error error, int statusCode, HttpContext? httpContext = null) =>
        TypedResults.Problem(Create(error, statusCode, httpContext));

    internal static void ValidateStatusCode(int statusCode)
    {
        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Expected an HTTP error status from 400 through 599.");
        }
    }

    /// <summary>Creates problem details using the built-in category, visibility, and trace policy.</summary>
    /// <example><code>ProblemDetails problem = ErrorProblemDetails.Create(error, httpContext);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <param name="httpContext">An optional context supplying a fallback trace identifier.</param>
    /// <returns>A populated problem-details value.</returns>
    public static ProblemDetails Create(in Error error, HttpContext? httpContext = null)
    {
        EnsureInitialized(error);

        string? traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        return CreateCore(error, traceId);
    }

    /// <summary>
    /// Creates deterministic problem details for documentation without request or activity data.
    /// </summary>
    /// <example><code>ProblemDetails example = ErrorProblemDetails.CreateExample(error);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <returns>Problem details without a trace identifier.</returns>
    public static ProblemDetails CreateExample(in Error error)
    {
        EnsureInitialized(error);
        return CreateCore(error, traceId: null);
    }

    private static ProblemDetails CreateCore(in Error error, string? traceId, int? statusCode = null)
    {
        int effectiveStatus = statusCode ?? GetStatusCodeCore(error);
        bool useHttpIdentity = statusCode is not null || IsCustomHttpStatus(error);
        (string title, string type) = useHttpIdentity
            ? (HttpStatusIdentity.GetTitle(effectiveStatus), HttpStatusIdentity.GetTypeUri(effectiveStatus))
            : (GetTitle(error.Type), GetTypeUri(error.Type));
        ProblemDetails details = new()
        {
            Status = effectiveStatus,
            Title = title,
            Detail = error.IsMessagePublic ? error.Message : null,
            Type = type
        };

        details.Extensions["code"] = error.Code;
        if (traceId is not null)
        {
            details.Extensions["traceId"] = traceId;
        }

        return details;
    }

    /// <summary>Creates a strongly typed problem HTTP result for an error.</summary>
    /// <example><code>ProblemHttpResult result = ErrorProblemDetails.ToHttpResult(error, httpContext);</code></example>
    /// <param name="error">The initialized error to convert.</param>
    /// <param name="httpContext">An optional context supplying a fallback trace identifier.</param>
    /// <returns>A strongly typed problem result.</returns>
    public static ProblemHttpResult ToHttpResult(in Error error, HttpContext? httpContext = null) =>
        TypedResults.Problem(Create(error, httpContext));

    /// <summary>Gets the default HTTP status code for an error category.</summary>
    /// <example><code>int status = ErrorProblemDetails.GetStatusCode(ErrorType.NotFound);</code></example>
    /// <param name="type">The initialized error category.</param>
    /// <returns>The corresponding HTTP status code.</returns>
    public static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.PaymentRequired => StatusCodes.Status402PaymentRequired,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.NotAcceptable => StatusCodes.Status406NotAcceptable,
        ErrorType.RequestTimeout => StatusCodes.Status408RequestTimeout,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Gone => StatusCodes.Status410Gone,
        ErrorType.PreconditionFailed => StatusCodes.Status412PreconditionFailed,
        ErrorType.ContentTooLarge => StatusCodes.Status413PayloadTooLarge,
        ErrorType.UnsupportedMediaType => StatusCodes.Status415UnsupportedMediaType,
        ErrorType.UnprocessableContent => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Locked => 423,
        ErrorType.PreconditionRequired => 428,
        ErrorType.Cancelled => 499,
        ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
        ErrorType.BadGateway => StatusCodes.Status502BadGateway,
        ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorType.NotImplemented => StatusCodes.Status501NotImplemented,
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom => StatusCodes.Status500InternalServerError,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    /// <summary>Gets the HTTP status for an initialized error, honoring valid HTTP numeric custom categories.</summary>
    /// <example><code>int status = ErrorProblemDetails.GetStatusCode(error);</code></example>
    /// <param name="error">The initialized error to map.</param>
    /// <returns>The mapped status, or 500 for a non-HTTP custom category.</returns>
    public static int GetStatusCode(in Error error)
    {
        EnsureInitialized(error);
        return GetStatusCodeCore(error);
    }

    private static int GetStatusCodeCore(in Error error) =>
        IsCustomHttpStatus(error) ? error.NumericType : GetStatusCode(error.Type);

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation failed",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.PaymentRequired => "Payment Required",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Not found",
        ErrorType.NotAcceptable => "Not Acceptable",
        ErrorType.RequestTimeout => "Request Timeout",
        ErrorType.Conflict => "Conflict",
        ErrorType.Gone => "Gone",
        ErrorType.PreconditionFailed => "Precondition Failed",
        ErrorType.ContentTooLarge => "Content Too Large",
        ErrorType.UnsupportedMediaType => "Unsupported Media Type",
        ErrorType.UnprocessableContent => "Unprocessable Content",
        ErrorType.Locked => "Locked",
        ErrorType.PreconditionRequired => "Precondition Required",
        ErrorType.Cancelled => "Request cancelled",
        ErrorType.RateLimited => "Too many requests",
        ErrorType.BadGateway => "Bad Gateway",
        ErrorType.Timeout => "Gateway timeout",
        ErrorType.Unavailable => "Service unavailable",
        ErrorType.NotImplemented => "Not Implemented",
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom => "An unexpected error occurred",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    private static string GetTypeUri(ErrorType type) => type switch
    {
        ErrorType.Validation => "urn:problem-type:validation",
        ErrorType.Unauthorized => "urn:problem-type:unauthorized",
        ErrorType.PaymentRequired => "urn:problem-type:payment-required",
        ErrorType.Forbidden => "urn:problem-type:forbidden",
        ErrorType.NotFound => "urn:problem-type:not-found",
        ErrorType.NotAcceptable => "urn:problem-type:not-acceptable",
        ErrorType.RequestTimeout => "urn:problem-type:request-timeout",
        ErrorType.Conflict => "urn:problem-type:conflict",
        ErrorType.Gone => "urn:problem-type:gone",
        ErrorType.PreconditionFailed => "urn:problem-type:precondition-failed",
        ErrorType.ContentTooLarge => "urn:problem-type:content-too-large",
        ErrorType.UnsupportedMediaType => "urn:problem-type:unsupported-media-type",
        ErrorType.UnprocessableContent => "urn:problem-type:unprocessable-content",
        ErrorType.Locked => "urn:problem-type:locked",
        ErrorType.PreconditionRequired => "urn:problem-type:precondition-required",
        ErrorType.Cancelled => "urn:problem-type:cancelled",
        ErrorType.RateLimited => "urn:problem-type:rate-limited",
        ErrorType.BadGateway => "urn:problem-type:bad-gateway",
        ErrorType.Timeout => "urn:problem-type:timeout",
        ErrorType.Unavailable => "urn:problem-type:unavailable",
        ErrorType.NotImplemented => "urn:problem-type:not-implemented",
        ErrorType.Failure or ErrorType.Unexpected or ErrorType.Custom => "urn:problem-type:unexpected",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };

    private static void EnsureInitialized(in Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
    }

    private static bool IsCustomHttpStatus(in Error error) =>
        error.Type is ErrorType.Custom && error.NumericType is >= 400 and <= 599;
}
