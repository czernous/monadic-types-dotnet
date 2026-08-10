using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Carries an action's input type so generated call sites remain inferable.</summary>
public readonly struct ValueAction<T, TAction> : IValueAction<T>
    where TAction : struct, IValueAction<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(T value) => default(TAction).Invoke(value);
}
