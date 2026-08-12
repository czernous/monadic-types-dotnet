using System.Runtime.CompilerServices;

namespace MonadicTypes.Tests;

public class ErrorTests
{
    [Fact]
    public void BindWidened_ConvertsOnlyContinuationFailure()
    {
        Result<int, Error> source = Result<int, Error>.Ok(42);

        Result<long, Error> result = source.BindWidened(
            static value => Result<long, DomainError>.Fail(new DomainError($"E{value}")));

        Assert.Equal("E42", result.Error.Code);
    }

    [Fact]
    public void ResultStoresErrorAsOneReferenceOnSuccess()
    {
        Assert.Equal(IntPtr.Size, Unsafe.SizeOf<Error>());
    }

    [Fact]
    public void Constructor_RequiresCodeAndMessage()
    {
        Assert.Throws<ArgumentException>(() => new Error("", "message"));
        Assert.Throws<ArgumentNullException>(() => new Error("CODE", null!));
    }

    [Fact]
    public void TryFormat_WritesWithoutCreatingAString()
    {
        Error error = Error.Validation("invalid value");
        Span<char> destination = stackalloc char[64];

        bool formatted = error.TryFormat(destination, out int written, default, null);

        Assert.True(formatted);
        Assert.Equal("[VALIDATION_FAILURE] invalid value", destination[..written]);
    }

    [Fact]
    public void TryFormat_ReturnsFalseWhenDestinationIsTooSmall()
    {
        Error error = Error.IO("failed");
        Span<char> destination = stackalloc char[4];

        Assert.False(error.TryFormat(destination, out int written, default, null));
        Assert.Equal(0, written);
    }

    [Fact]
    public void ToString_UsesGeneralFormat()
    {
        Error error = Error.System("unavailable");

        Assert.Equal("[SYSTEM_FAILURE] unavailable", error.ToString());
        Assert.Equal(
            error.ToString(),
            error.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
        Assert.Throws<FormatException>(() => error.ToString("X", null));
    }

    [Fact]
    public void Unexpected_RetainsCauseWithoutMakingMessagePublic()
    {
        InvalidOperationException cause = new("database unavailable");

        Error error = Error.Unexpected(cause);

        Assert.Equal(ErrorType.Unexpected, error.Type);
        Assert.Same(cause, error.Cause);
        Assert.False(error.IsMessagePublic);
    }

    [Fact]
    public void Constructor_PreservesCustomMessageAndCause()
    {
        InvalidOperationException cause = new("internal detail");

        Error error = new(ErrorType.Unexpected, "PUBLIC_CODE", "safe detail", cause: cause);

        Assert.Equal("safe detail", error.Message);
        Assert.Same(cause, error.Cause);
    }

    [Fact]
    public void CategoryFactory_PreservesCauseWithoutChangingClassification()
    {
        TimeoutException cause = new("gateway timeout");

        Error error = Error.Unavailable(
            "PAYMENT_UNAVAILABLE",
            "Payment is temporarily unavailable.",
            cause: cause);

        Assert.Equal(ErrorType.Unavailable, error.Type);
        Assert.Same(cause, error.Cause);
        Assert.False(error.IsMessagePublic);
    }

    [Fact]
    public void Custom_PreservesApplicationDefinedNumericType()
    {
        Error error = Error.Custom(1001, "PAYMENT_DECLINED", "Payment declined", true);

        Assert.Equal(ErrorType.Custom, error.Type);
        Assert.Equal(1001, error.NumericType);
        Assert.True(error.IsMessagePublic);
    }

    [Fact]
    public void ThrowCause_PreservesOriginalExceptionAndStack()
    {
        Error error;
        try
        {
            ThrowOriginalException();
            throw new Xunit.Sdk.XunitException("Expected exception was not thrown.");
        }
        catch (InvalidOperationException cause)
        {
            error = Error.Unexpected(cause);
        }

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(error.ThrowCause);

        Assert.Same(error.Cause, rethrown);
        Assert.Contains(nameof(ThrowOriginalException), rethrown.StackTrace, StringComparison.Ordinal);
    }

    private static void ThrowOriginalException() => throw new InvalidOperationException("failure");

    private readonly record struct DomainError(string Code) : IErrorConvertible<Error>
    {
        public Error ToError() => Error.Failure(Code, "Domain failure");
    }
}
