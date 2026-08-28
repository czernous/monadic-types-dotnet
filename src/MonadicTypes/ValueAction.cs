using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Carries an action's input type so generated call sites remain inferable.</summary>
/// <param name="action">Callable action value to wrap.</param>
public readonly struct ValueAction<T, TAction>(TAction action) : IValueAction<T>
    where TAction : struct, IValueAction<T>
{
    private readonly TAction _action = action;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(T value) => _action.Invoke(value);
}
