namespace MonadicTypes;

/// <summary>Controls whether recording an error changes the current activity status.</summary>
public enum ErrorActivityStatusPolicy : byte
{
    /// <summary>Marks categories that normally represent server failures as errors.</summary>
    Automatic,

    /// <summary>Records error tags and events without changing the activity status.</summary>
    Preserve,

    /// <summary>Marks every recorded error category as an activity error.</summary>
    MarkError
}
