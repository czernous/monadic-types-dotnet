namespace MonadicTypes;

/// <summary>Broad operational category used by adapters and telemetry policy.</summary>
public enum ErrorType : byte
{
    /// <summary>An invalid default value that no constructed error may use.</summary>
    Uninitialized,
    /// <summary>A general expected operational failure.</summary>
    Failure,
    /// <summary>An unexpected or internal failure.</summary>
    Unexpected,
    /// <summary>Invalid input or business-rule validation.</summary>
    Validation,
    /// <summary>A state or concurrency conflict.</summary>
    Conflict,
    /// <summary>A requested resource does not exist.</summary>
    NotFound,
    /// <summary>Authentication is absent or invalid.</summary>
    Unauthorized,
    /// <summary>The authenticated caller lacks permission.</summary>
    Forbidden,
    /// <summary>A dependency or service is temporarily unavailable.</summary>
    Unavailable,
    /// <summary>An operation exceeded its time budget.</summary>
    Timeout,
    /// <summary>A caller exceeded a rate or quota limit.</summary>
    RateLimited,
    /// <summary>An operation was cancelled.</summary>
    Cancelled,
    /// <summary>A consumer-defined category identified by <see cref="Error.NumericType"/>.</summary>
    Custom
}
