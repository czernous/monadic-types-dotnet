using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>
/// Carries the complete input, output, and implementation types of an
/// allocation-free callable so generic consumers can infer every type.
/// </summary>
/// <param name="function">Callable value to wrap.</param>
/// <example><code>ValueFunction&lt;User, int, GetId&gt; function = new(default);</code></example>
public readonly struct ValueFunction<TIn, TOut, TFunction>(TFunction function) : IValueFunction<TIn, TOut>
    where TFunction : struct, IValueFunction<TIn, TOut>
{
    private readonly TFunction _function = function;

    /// <summary>Forwards <paramref name="value"/> to the wrapped value function.</summary>
    /// <param name="value">Input value.</param>
    /// <returns>The transformed output.</returns>
    /// <example><code>int id = Operations.Functions.GetId.Invoke(user);</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TOut Invoke(TIn value) => _function.Invoke(value);
}
