namespace MonadicTypes.Testing;

/// <summary>Provides framework-neutral assertions for tests of monadic values.</summary>
public static class MonadicAssertions
{
    /// <summary>Returns the successful value or throws a test assertion exception.</summary>
    /// <typeparam name="T">Success value type.</typeparam>
    /// <typeparam name="TError">Error type.</typeparam>
    /// <param name="result">Result to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The successful value.</returns>
    public static T ValueOrFail<T, TError>(this Result<T, TError> result, string? message = null)
        where TError : notnull => result.IsSuccess
            ? result.Value
            : throw Failure(message, $"Expected a successful Result, but received {Describe(result)}.");

    /// <summary>Returns the failure error or throws a test assertion exception.</summary>
    /// <typeparam name="T">Success value type.</typeparam>
    /// <typeparam name="TError">Error type.</typeparam>
    /// <param name="result">Result to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The failure error.</returns>
    public static TError ErrorOrFail<T, TError>(this Result<T, TError> result, string? message = null)
        where TError : notnull => result.IsFailure
            ? result.Error
            : throw Failure(message, $"Expected a failed Result, but received {Describe(result)}.");

    /// <summary>Returns the present value or throws a test assertion exception.</summary>
    /// <typeparam name="T">Contained value type.</typeparam>
    /// <param name="option">Option to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The present value.</returns>
    public static T ValueOrFail<T>(this Option<T> option, string? message = null) => option.IsSome
        ? option.Value
        : throw Failure(message, "Expected a Some Option, but received None.");

    /// <summary>Asserts that a result is successful and returns it for further checks.</summary>
    /// <typeparam name="T">Success value type.</typeparam>
    /// <typeparam name="TError">Error type.</typeparam>
    /// <param name="result">Result to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The unchanged result.</returns>
    public static Result<T, TError> ShouldBeOk<T, TError>(this Result<T, TError> result, string? message = null)
        where TError : notnull
    {
        if (!result.IsSuccess)
        {
            throw Failure(message, $"Expected a successful Result, but received {Describe(result)}.");
        }

        return result;
    }

    /// <summary>Asserts that a result is failed and returns it for further checks.</summary>
    /// <typeparam name="T">Success value type.</typeparam>
    /// <typeparam name="TError">Error type.</typeparam>
    /// <param name="result">Result to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The unchanged result.</returns>
    public static Result<T, TError> ShouldBeError<T, TError>(this Result<T, TError> result, string? message = null)
        where TError : notnull
    {
        if (!result.IsFailure)
        {
            throw Failure(message, $"Expected a failed Result, but received {Describe(result)}.");
        }

        return result;
    }

    /// <summary>Asserts that an option is present and returns it for further checks.</summary>
    /// <typeparam name="T">Contained value type.</typeparam>
    /// <param name="option">Option to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The unchanged option.</returns>
    public static Option<T> ShouldBeSome<T>(this Option<T> option, string? message = null)
    {
        if (option.IsNone)
        {
            throw Failure(message, "Expected a Some Option, but received None.");
        }

        return option;
    }

    /// <summary>Asserts that an option is empty and returns it for further checks.</summary>
    /// <typeparam name="T">Contained value type.</typeparam>
    /// <param name="option">Option to inspect.</param>
    /// <param name="message">Optional context included in the failure message.</param>
    /// <returns>The unchanged option.</returns>
    public static Option<T> ShouldBeNone<T>(this Option<T> option, string? message = null)
    {
        if (option.IsSome)
        {
            throw Failure(message, "Expected a None Option, but received Some.");
        }

        return option;
    }

    private static string Describe<T, TError>(Result<T, TError> result) where TError : notnull => result.IsInitialized
        ? result.IsSuccess ? "Success" : "Failure"
        : "Uninitialized";

    private static MonadicAssertionException Failure(string? message, string detail) =>
        new(string.IsNullOrWhiteSpace(message) ? detail : $"{message}: {detail}");
}
