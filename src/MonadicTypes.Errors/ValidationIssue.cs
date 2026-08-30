namespace MonadicTypes;

/// <summary>Describes the diagnostic severity of a validation issue.</summary>
public enum ValidationSeverity : byte
{
    /// <summary>The input is invalid and processing cannot continue.</summary>
    /// <example><code>ValidationSeverity severity = ValidationSeverity.Error;</code></example>
    Error,
    /// <summary>The input is accepted but potentially problematic.</summary>
    /// <example><code>ValidationSeverity severity = ValidationSeverity.Warning;</code></example>
    Warning,
    /// <summary>Informational validation feedback.</summary>
    /// <example><code>ValidationSeverity severity = ValidationSeverity.Information;</code></example>
    Information
}

/// <summary>A public-safe validation failure associated with an input path.</summary>
public readonly record struct ValidationIssue
{
    /// <summary>Creates a validation issue with stable machine and human-readable fields.</summary>
    /// <example><code>ValidationIssue issue = new("email", "EMAIL_INVALID", "Email is invalid.");</code></example>
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
    /// <example><code>string path = issue.Path;</code></example>
    public string Path { get; }
    /// <summary>Gets the stable machine-readable issue code.</summary>
    /// <example><code>string code = issue.Code;</code></example>
    public string Code { get; }
    /// <summary>Gets the human-readable validation message.</summary>
    /// <example><code>string message = issue.Message;</code></example>
    public string Message { get; }
    /// <summary>Gets the issue severity.</summary>
    /// <example><code>ValidationSeverity severity = issue.Severity;</code></example>
    public ValidationSeverity Severity { get; }
}
