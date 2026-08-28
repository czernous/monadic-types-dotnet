using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>
/// Carries the complete input, output, and implementation types of an
/// allocation-free callable so generic consumers can infer every type.
/// </summary>
/// <param name="function">Callable value to wrap.</param>
public readonly struct ValueFunction<TIn, TOut, TFunction>(TFunction function) : IValueFunction<TIn, TOut>
    where TFunction : struct, IValueFunction<TIn, TOut>
{
    private readonly TFunction _function = function;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TOut Invoke(TIn value) => _function.Invoke(value);
}
