using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>
/// Carries the complete input, output, and implementation types of an
/// allocation-free callable so generic consumers can infer every type.
/// </summary>
public struct ValueFunction<TIn, TOut, TFunction> : IValueFunction<TIn, TOut>
    where TFunction : struct, IValueFunction<TIn, TOut>
{
    private TFunction _function;

    public ValueFunction(TFunction function) => _function = function;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TOut Invoke(TIn value) => _function.Invoke(value);
}
