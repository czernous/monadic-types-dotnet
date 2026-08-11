namespace MonadicTypes;

/// <summary>Defines a value-type side-effect callback that avoids delegate allocation and dispatch.</summary>
/// <typeparam name="T">Input value type.</typeparam>
public interface IValueAction<in T>
{
    /// <summary>Performs the action for <paramref name="value"/>.</summary>
    /// <param name="value">Input value.</param>
    void Invoke(T value);
}
