using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Carries an action's input type so generated call sites remain inferable.</summary>
/// <param name="action">Callable action value to wrap.</param>
/// <example><code>ValueAction&lt;Error, Observe&gt; action = new(default);</code></example>
public readonly struct ValueAction<T, TAction>(TAction action) : IValueAction<T>
    where TAction : struct, IValueAction<T>
{
    private readonly TAction _action = action;

    /// <summary>Forwards <paramref name="value"/> to the wrapped value action.</summary>
    /// <param name="value">Input value.</param>
    /// <example><code>Operations.Functions.Observe.Invoke(error);</code></example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(T value) => _action.Invoke(value);
}
