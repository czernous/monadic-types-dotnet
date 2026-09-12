using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>
/// Explicitly records an observed error. Call this once at an application
/// boundary; constructing or propagating an Error has no telemetry side effects.
/// </summary>
public static class ErrorTelemetry
{
    /// <summary>Records an error on a sampled activity without creating an activity.</summary>
    /// <example><code>ErrorTelemetry.Record(Activity.Current, error);</code></example>
    /// <param name="activity">The caller-owned activity, or null to perform no work.</param>
    /// <param name="error">The initialized error to record.</param>
    /// <param name="statusPolicy">The policy controlling activity status mutation.</param>
    /// <remarks>
    /// The default <see cref="ErrorTelemetryMessagePolicy.PublicOnly"/> policy
    /// writes only messages marked public to the <c>error.message</c> activity
    /// tag. <see cref="Error.IsMessagePublic"/> controls HTTP disclosure only;
    /// use the overload with an explicit policy when telemetry has a different
    /// security boundary.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The activity is sampled and <paramref name="error"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="statusPolicy"/> or the error category is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Record(
        Activity? activity,
        Error? error,
        ErrorActivityStatusPolicy statusPolicy = ErrorActivityStatusPolicy.Automatic)
        => Record(activity, error, statusPolicy, ErrorTelemetryMessagePolicy.PublicOnly);

    /// <summary>Records an error with explicit activity status and message policies.</summary>
    /// <param name="activity">The caller-owned activity, or null to perform no work.</param>
    /// <param name="error">The initialized error to record.</param>
    /// <param name="statusPolicy">The policy controlling activity status mutation.</param>
    /// <param name="messagePolicy">The policy controlling the diagnostic message tag.</param>
    /// <exception cref="ArgumentNullException">The activity is sampled and <paramref name="error"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="statusPolicy"/>, <paramref name="messagePolicy"/>, or the error category is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Record(
        Activity? activity,
        Error? error,
        ErrorActivityStatusPolicy statusPolicy,
        ErrorTelemetryMessagePolicy messagePolicy)
    {
        if (activity is null || !activity.IsAllDataRequested)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(error);

        if (messagePolicy is not (ErrorTelemetryMessagePolicy.PublicOnly
            or ErrorTelemetryMessagePolicy.Include
            or ErrorTelemetryMessagePolicy.Omit))
        {
            throw new ArgumentOutOfRangeException(nameof(messagePolicy), messagePolicy, null);
        }

        string category = GetCategoryName(error.Type);
        activity.SetTag("error.type", error.Code);
        activity.SetTag("error.category", category);

        if (messagePolicy is ErrorTelemetryMessagePolicy.Include
            || messagePolicy is ErrorTelemetryMessagePolicy.PublicOnly && error.IsMessagePublic)
        {
            activity.SetTag("error.message", error.Message);
        }

        if (error.Cause is { } cause)
        {
            activity.AddException(cause);
        }
        else
        {
            activity.AddEvent(new ActivityEvent("error"));
        }

        if (ShouldMarkError(error.Type, statusPolicy))
        {
            activity.SetStatus(ActivityStatusCode.Error, error.Code);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldMarkError(ErrorType type, ErrorActivityStatusPolicy statusPolicy) => statusPolicy switch
    {
        ErrorActivityStatusPolicy.Preserve => false,
        ErrorActivityStatusPolicy.MarkError => true,
        ErrorActivityStatusPolicy.Automatic => type is
            ErrorType.Failure or
            ErrorType.Unexpected or
            ErrorType.BadGateway or
            ErrorType.NotImplemented or
            ErrorType.Unavailable or
            ErrorType.Timeout or
            ErrorType.Custom,
        _ => throw new ArgumentOutOfRangeException(nameof(statusPolicy), statusPolicy, null)
    };

    internal static string GetCategoryName(ErrorType type) => type switch
    {
        ErrorType.Failure => "failure",
        ErrorType.Unexpected => "unexpected",
        ErrorType.Validation => "validation",
        ErrorType.Conflict => "conflict",
        ErrorType.NotFound => "not_found",
        ErrorType.PaymentRequired => "payment_required",
        ErrorType.NotAcceptable => "not_acceptable",
        ErrorType.RequestTimeout => "request_timeout",
        ErrorType.Gone => "gone",
        ErrorType.PreconditionFailed => "precondition_failed",
        ErrorType.ContentTooLarge => "content_too_large",
        ErrorType.UnsupportedMediaType => "unsupported_media_type",
        ErrorType.UnprocessableContent => "unprocessable_content",
        ErrorType.Locked => "locked",
        ErrorType.PreconditionRequired => "precondition_required",
        ErrorType.Unauthorized => "unauthorized",
        ErrorType.Forbidden => "forbidden",
        ErrorType.BadGateway => "bad_gateway",
        ErrorType.NotImplemented => "not_implemented",
        ErrorType.Unavailable => "unavailable",
        ErrorType.Timeout => "timeout",
        ErrorType.RateLimited => "rate_limited",
        ErrorType.Cancelled => "cancelled",
        ErrorType.Custom => "custom",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };
}
