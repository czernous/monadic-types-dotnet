namespace MonadicTypes;

/// <summary>Broad operational category used by adapters and telemetry policy.</summary>
public enum ErrorType : byte
{
    Uninitialized,
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
    Unavailable,
    Timeout,
    RateLimited,
    Cancelled,
    Custom
}
