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
    /// <param name="activity">The caller-owned activity, or null to perform no work.</param>
    /// <param name="error">The initialized error to record.</param>
    /// <param name="statusPolicy">The policy controlling activity status mutation.</param>
    /// <exception cref="ArgumentNullException">The activity is sampled and <paramref name="error"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="statusPolicy"/> or the error category is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Record(
        Activity? activity,
        in Error? error,
        ErrorActivityStatusPolicy statusPolicy = ErrorActivityStatusPolicy.Automatic)
    {
        if (activity is null || !activity.IsAllDataRequested)
        {
            return;
        }

        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        string category = GetCategoryName(error.Type);
        activity.SetTag("error.type", error.Code);
        activity.SetTag("error.category", category);
        activity.SetTag("error.message", error.Message);

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
        ErrorType.Unauthorized => "unauthorized",
        ErrorType.Forbidden => "forbidden",
        ErrorType.Unavailable => "unavailable",
        ErrorType.Timeout => "timeout",
        ErrorType.RateLimited => "rate_limited",
        ErrorType.Cancelled => "cancelled",
        ErrorType.Custom => "custom",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Uninitialized error type.")
    };
}
