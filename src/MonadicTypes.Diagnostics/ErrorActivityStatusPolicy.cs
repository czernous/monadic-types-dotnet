namespace MonadicTypes;

/// <summary>Controls whether recording an error changes the current activity status.</summary>
/// <example><code>ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Automatic;</code></example>
public enum ErrorActivityStatusPolicy : byte
{
    /// <summary>Marks categories that normally represent server failures as errors.</summary>
    /// <example><code>ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Automatic;</code></example>
    Automatic,

    /// <summary>Records error tags and events without changing the activity status.</summary>
    /// <example><code>ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.Preserve;</code></example>
    Preserve,

    /// <summary>Marks every recorded error category as an activity error.</summary>
    /// <example><code>ErrorActivityStatusPolicy policy = ErrorActivityStatusPolicy.MarkError;</code></example>
    MarkError
}
