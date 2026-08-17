using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Converts options to nullable application-boundary values.</summary>
public static class OptionNullableExtensions
{
    extension<T>(in Option<T> option) where T : class
    {
        /// <summary>Returns the contained reference, or null for None.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ToNullable() => option.TryGetValue(out T? value) ? value : null;
    }

    extension<T>(in Option<T> option) where T : struct
    {
        /// <summary>Returns the contained nullable value, or null for None.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ToNullableValue() => option.TryGetValue(out T value) ? value : null;
    }
}
