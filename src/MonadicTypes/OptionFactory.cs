using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Creates options from nullable application-boundary values.</summary>
public static class Option
{
    /// <summary>Converts a nullable reference to Some, or null to None.</summary>
    /// <typeparam name="T">Non-null reference value type.</typeparam>
    /// <param name="value">Nullable value to convert.</param>
    /// <returns>Some for a non-null value; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> FromNullable<T>(T? value) where T : class =>
        value is null ? Option<T>.None : Option<T>.Some(value);

    /// <summary>Converts a nullable value type to Some, or null to None.</summary>
    /// <typeparam name="T">Non-null value type.</typeparam>
    /// <param name="value">Nullable value to convert.</param>
    /// <returns>Some for a present value; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> FromNullable<T>(T? value) where T : struct =>
        value.HasValue ? Option<T>.Some(value.GetValueOrDefault()) : Option<T>.None;
}
