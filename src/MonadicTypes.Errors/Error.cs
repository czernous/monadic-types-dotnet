using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace MonadicTypes;

/// <summary>
/// A structured error occurrence. <see cref="Code"/> identifies the failure
/// for machines and telemetry; <see cref="Message"/> is diagnostic text and is
/// exposed to clients only when <see cref="IsMessagePublic"/> is true.
/// </summary>
public sealed record Error : ISpanFormattable
{
    private readonly string? _code;
    private readonly object? _detail;

    private sealed class MessageAndCause(string message, Exception cause)
    {
        public string Message { get; } = message;
        public Exception Cause { get; } = cause;
    }

    /// <summary>Creates an error in a built-in category.</summary>
    public Error(
        ErrorType type,
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null)
        : this(type, (int)type, code, message, isMessagePublic, cause)
    {
        if (type is < ErrorType.Failure or >= ErrorType.Custom)
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    /// <summary>Creates a general failure with a private diagnostic message.</summary>
    public Error(string code, string message)
        : this(ErrorType.Failure, code, message)
    {
    }

    private Error(
        ErrorType type,
        int numericType,
        string code,
        string message,
        bool isMessagePublic,
        Exception? cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        Type = type;
        NumericType = numericType;
        _code = code;
        _detail = cause switch
        {
            null => message,
            _ when message.Equals(cause.Message, StringComparison.Ordinal) => cause,
            _ => new MessageAndCause(message, cause)
        };
        IsMessagePublic = isMessagePublic;
    }

    /// <summary>Gets the broad built-in category.</summary>
    public ErrorType Type { get; }
    /// <summary>Gets the stable numeric category, including custom categories.</summary>
    public int NumericType { get; }
    /// <summary>Gets the stable machine-readable error code.</summary>
    public string Code => _code ?? throw UninitializedError();
    /// <summary>Gets the diagnostic message.</summary>
    public string Message => _detail switch
    {
        string message => message,
        Exception cause => cause.Message,
        MessageAndCause detail => detail.Message,
        _ => throw UninitializedError()
    };
    /// <summary>Gets whether adapters may safely expose <see cref="Message"/> to clients.</summary>
    public bool IsMessagePublic { get; }
    /// <summary>
    /// Retained exception for telemetry. Use <see cref="ThrowCause"/> rather than
    /// throwing this property directly when exception propagation is required.
    /// </summary>
    public Exception? Cause => _detail switch
    {
        Exception cause => cause,
        MessageAndCause detail => detail.Cause,
        _ => null
    };

    /// <summary>Rethrows the retained cause while preserving its original stack trace.</summary>
    /// <exception cref="InvalidOperationException">No cause is retained.</exception>
    [DoesNotReturn]
    public void ThrowCause()
    {
        if (Cause is not { } cause)
        {
            throw new InvalidOperationException("This error does not retain an exception cause.");
        }

        ExceptionDispatchInfo.Throw(cause);
    }

    /// <summary>Creates a general failure with the default code.</summary>
    public static Error Failure(string message) =>
        new(ErrorType.Failure, "FAILURE", message);

    /// <summary>Creates a general failure with a caller-defined code and visibility.</summary>
    public static Error Failure(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.Failure, code, message, isMessagePublic, cause);

    /// <summary>Creates an unexpected failure without a retained exception.</summary>
    public static Error Unexpected(string message) =>
        new(ErrorType.Unexpected, "UNEXPECTED_FAILURE", message);

    /// <summary>Creates an unexpected failure that retains <paramref name="cause"/> for telemetry and rethrow.</summary>
    public static Error Unexpected(Exception cause, string code = "UNEXPECTED_FAILURE")
    {
        ArgumentNullException.ThrowIfNull(cause);
        return new(ErrorType.Unexpected, code, cause.Message, cause: cause);
    }

    /// <summary>Creates a public validation failure with the default code.</summary>
    public static Error Validation(string message) =>
        new(ErrorType.Validation, "VALIDATION_FAILURE", message, isMessagePublic: true);

    /// <summary>Creates a public validation failure with a caller-defined code.</summary>
    public static Error Validation(string code, string message, Exception? cause = null) =>
        new(ErrorType.Validation, code, message, isMessagePublic: true, cause);

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(
        string code,
        string message,
        bool isMessagePublic = true,
        Exception? cause = null) =>
        new(ErrorType.Conflict, code, message, isMessagePublic, cause);

    /// <summary>Creates a resource-not-found error.</summary>
    public static Error NotFound(
        string code,
        string message,
        bool isMessagePublic = true,
        Exception? cause = null) =>
        new(ErrorType.NotFound, code, message, isMessagePublic, cause);

    /// <summary>Creates an authentication-required error.</summary>
    public static Error Unauthorized(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.Unauthorized, code, message, isMessagePublic, cause);

    /// <summary>Creates an authorization-denied error.</summary>
    public static Error Forbidden(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.Forbidden, code, message, isMessagePublic, cause);

    /// <summary>Creates a service-unavailable error.</summary>
    public static Error Unavailable(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.Unavailable, code, message, isMessagePublic, cause);

    /// <summary>Creates a timeout error.</summary>
    public static Error Timeout(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.Timeout, code, message, isMessagePublic, cause);

    /// <summary>Creates a rate-limit error.</summary>
    public static Error RateLimited(
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null) =>
        new(ErrorType.RateLimited, code, message, isMessagePublic, cause);

    /// <summary>Creates a cancellation error.</summary>
    public static Error Cancelled(string code, string message, Exception? cause = null) =>
        new(ErrorType.Cancelled, code, message, cause: cause);

    /// <summary>Creates a consumer-defined error category with a positive numeric identifier.</summary>
    public static Error Custom(
        int numericType,
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numericType);

        return new(ErrorType.Custom, numericType, code, message, isMessagePublic, cause);
    }

    /// <summary>Creates a general input/output failure with the standard code.</summary>
    public static Error IO(string message) =>
        new(ErrorType.Failure, "IO_FAILURE", message);

    /// <summary>Creates an unexpected system failure with the standard code.</summary>
    public static Error System(string message) =>
        new(ErrorType.Unexpected, "SYSTEM_FAILURE", message);

    /// <summary>Compares semantic fields and retained-cause identity.</summary>
    public bool Equals(Error? other) =>
        ReferenceEquals(this, other)
        || other is not null
        && Type == other.Type
        && NumericType == other.NumericType
        && string.Equals(Code, other.Code, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal)
        && IsMessagePublic == other.IsMessagePublic
        && ReferenceEquals(Cause, other.Cause);

    /// <summary>Hashes the same fields used by <see cref="Equals(Error?)"/>.</summary>
    public override int GetHashCode() => HashCode.Combine(
        (int)Type,
        NumericType,
        StringComparer.Ordinal.GetHashCode(Code),
        StringComparer.Ordinal.GetHashCode(Message),
        IsMessagePublic,
        Cause is null ? 0 : RuntimeHelpers.GetHashCode(Cause));

    /// <inheritdoc />
    public override string ToString() => string.Create(
        GetFormattedLength(),
        this,
        static (destination, error) => error.Format(destination));

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ValidateFormat(format);
        return ToString();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        ValidateFormat(format);
        int length = GetFormattedLength();
        if (destination.Length < length)
        {
            charsWritten = 0;
            return false;
        }

        Format(destination);
        charsWritten = length;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetFormattedLength() => checked(Code.Length + Message.Length + 3);

    private void Format(Span<char> destination)
    {
        destination[0] = '[';
        Code.AsSpan().CopyTo(destination[1..]);
        int separator = Code.Length + 1;
        destination[separator] = ']';
        destination[separator + 1] = ' ';
        Message.AsSpan().CopyTo(destination[(separator + 2)..]);
    }

    private static void ValidateFormat(ReadOnlySpan<char> format)
    {
        if (!format.IsEmpty && !format.Equals("G", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"The '{format.ToString()}' error format is not supported.");
        }
    }

    private static InvalidOperationException UninitializedError() =>
        new("A default Error is uninitialized. Construct it with an error factory or constructor before use.");
}
