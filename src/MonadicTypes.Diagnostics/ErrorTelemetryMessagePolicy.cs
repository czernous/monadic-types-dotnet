namespace MonadicTypes;

/// <summary>Controls whether an error's diagnostic message is written to telemetry.</summary>
/// <remarks>
/// This policy applies only to the <c>error.message</c> activity tag. Retained
/// exception causes are still recorded as exception events.
/// </remarks>
public enum ErrorTelemetryMessagePolicy : byte
{
    /// <summary>Writes only messages marked public by the error.</summary>
    /// <example><code>ErrorTelemetryMessagePolicy policy = ErrorTelemetryMessagePolicy.PublicOnly;</code></example>
    PublicOnly,

    /// <summary>Writes the diagnostic message regardless of its transport visibility.</summary>
    /// <example><code>ErrorTelemetryMessagePolicy policy = ErrorTelemetryMessagePolicy.Include;</code></example>
    Include,

    /// <summary>Omits the diagnostic message from telemetry.</summary>
    /// <example><code>ErrorTelemetryMessagePolicy policy = ErrorTelemetryMessagePolicy.Omit;</code></example>
    Omit
}
