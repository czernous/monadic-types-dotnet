namespace MonadicTypes;

public enum ValidationSeverity : byte
{
    Error,
    Warning,
    Information
}

/// <summary>A public-safe validation failure associated with an input path.</summary>
public readonly record struct ValidationIssue
{
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

    public string Path { get; }
    public string Code { get; }
    public string Message { get; }
    public ValidationSeverity Severity { get; }
}
