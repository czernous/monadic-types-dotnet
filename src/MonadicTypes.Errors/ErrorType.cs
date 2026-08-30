namespace MonadicTypes;

/// <summary>Broad operational category used by adapters and telemetry policy.</summary>
/// <example><code>ErrorType category = ErrorType.Validation;</code></example>
public enum ErrorType : byte
{
    /// <summary>An invalid default value that no constructed error may use.</summary>
    /// <example><code>ErrorType category = ErrorType.Uninitialized;</code></example>
    Uninitialized,
    /// <summary>A general expected operational failure.</summary>
    /// <example><code>ErrorType category = ErrorType.Failure;</code></example>
    Failure,
    /// <summary>An unexpected or internal failure.</summary>
    /// <example><code>ErrorType category = ErrorType.Unexpected;</code></example>
    Unexpected,
    /// <summary>Invalid input or business-rule validation.</summary>
    /// <example><code>ErrorType category = ErrorType.Validation;</code></example>
    Validation,
    /// <summary>A state or concurrency conflict.</summary>
    /// <example><code>ErrorType category = ErrorType.Conflict;</code></example>
    Conflict,
    /// <summary>A requested resource does not exist.</summary>
    /// <example><code>ErrorType category = ErrorType.NotFound;</code></example>
    NotFound,
    /// <summary>Authentication is absent or invalid.</summary>
    /// <example><code>ErrorType category = ErrorType.Unauthorized;</code></example>
    Unauthorized,
    /// <summary>The authenticated caller lacks permission.</summary>
    /// <example><code>ErrorType category = ErrorType.Forbidden;</code></example>
    Forbidden,
    /// <summary>A dependency or service is temporarily unavailable.</summary>
    /// <example><code>ErrorType category = ErrorType.Unavailable;</code></example>
    Unavailable,
    /// <summary>An operation exceeded its time budget.</summary>
    /// <example><code>ErrorType category = ErrorType.Timeout;</code></example>
    Timeout,
    /// <summary>A caller exceeded a rate or quota limit.</summary>
    /// <example><code>ErrorType category = ErrorType.RateLimited;</code></example>
    RateLimited,
    /// <summary>An operation was cancelled.</summary>
    /// <example><code>ErrorType category = ErrorType.Cancelled;</code></example>
    Cancelled,
    /// <summary>A consumer-defined category identified by <see cref="Error.NumericType"/>.</summary>
    /// <example><code>ErrorType category = ErrorType.Custom;</code></example>
    Custom
}
