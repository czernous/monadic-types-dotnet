namespace MonadicTypes;

/// <summary>Describes the diagnostic severity of a validation issue.</summary>
public enum ValidationSeverity : byte
{
    /// <summary>The input is invalid and processing cannot continue.</summary>
    Error,
    /// <summary>The input is accepted but potentially problematic.</summary>
    Warning,
    /// <summary>Informational validation feedback.</summary>
    Information
}

/// <summary>A public-safe validation failure associated with an input path.</summary>
public readonly record struct ValidationIssue
{
    /// <summary>Creates a validation issue with stable machine and human-readable fields.</summary>
    public ValidationIssue(
        string path,
        string code,
        string message,
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        Path = path;
        Code = code;
        Message = message;
        Severity = severity;
    }

    /// <summary>Gets the input path or member associated with the issue.</summary>
    public string Path { get; }
    /// <summary>Gets the stable machine-readable issue code.</summary>
    public string Code { get; }
    /// <summary>Gets the human-readable validation message.</summary>
    public string Message { get; }
    /// <summary>Gets the issue severity.</summary>
    public ValidationSeverity Severity { get; }
}
