using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
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

    public Error(
        ErrorType type,
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null)
        : this(type, (int)type, code, message, isMessagePublic, cause)
    {
        if (type is ErrorType.Uninitialized or ErrorType.Custom)
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

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

    public ErrorType Type { get; }
    public int NumericType { get; }
    public string Code => _code ?? throw UninitializedError();
    public string Message => _detail switch
    {
        string message => message,
        Exception cause => cause.Message,
        MessageAndCause detail => detail.Message,
        _ => throw UninitializedError()
    };
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

    [DoesNotReturn]
    public void ThrowCause()
    {
        if (Cause is not { } cause)
        {
            throw new InvalidOperationException("This error does not retain an exception cause.");
        }

        ExceptionDispatchInfo.Throw(cause);
    }

    public static Error Failure(string message) =>
        new(ErrorType.Failure, "FAILURE", message);

    public static Error Failure(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.Failure, code, message, isMessagePublic);

    public static Error Unexpected(string message) =>
        new(ErrorType.Unexpected, "UNEXPECTED_FAILURE", message);

    public static Error Unexpected(Exception cause, string code = "UNEXPECTED_FAILURE")
    {
        ArgumentNullException.ThrowIfNull(cause);
        return new(ErrorType.Unexpected, code, cause.Message, cause: cause);
    }

    public static Error Validation(string message) =>
        new(ErrorType.Validation, "VALIDATION_FAILURE", message, isMessagePublic: true);

    public static Error Validation(string code, string message) =>
        new(ErrorType.Validation, code, message, isMessagePublic: true);

    public static Error Conflict(string code, string message, bool isMessagePublic = true) =>
        new(ErrorType.Conflict, code, message, isMessagePublic);

    public static Error NotFound(string code, string message, bool isMessagePublic = true) =>
        new(ErrorType.NotFound, code, message, isMessagePublic);

    public static Error Unauthorized(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.Unauthorized, code, message, isMessagePublic);

    public static Error Forbidden(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.Forbidden, code, message, isMessagePublic);

    public static Error Unavailable(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.Unavailable, code, message, isMessagePublic);

    public static Error Timeout(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.Timeout, code, message, isMessagePublic);

    public static Error RateLimited(string code, string message, bool isMessagePublic = false) =>
        new(ErrorType.RateLimited, code, message, isMessagePublic);

    public static Error Cancelled(string code, string message) =>
        new(ErrorType.Cancelled, code, message);

    public static Error Custom(
        int numericType,
        string code,
        string message,
        bool isMessagePublic = false,
        Exception? cause = null)
    {
        if (numericType <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numericType));
        }

        return new(ErrorType.Custom, numericType, code, message, isMessagePublic, cause);
    }

    public static Error IO(string message) =>
        new(ErrorType.Failure, "IO_FAILURE", message);

    public static Error System(string message) =>
        new(ErrorType.Unexpected, "SYSTEM_FAILURE", message);

    public override string ToString() => string.Create(
        GetFormattedLength(),
        this,
        static (destination, error) => error.Format(destination));

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ValidateFormat(format);
        return ToString();
    }

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
