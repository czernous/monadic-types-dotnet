namespace MonadicTypes;

/// <summary>Defines a value-type side-effect callback that avoids delegate allocation and dispatch.</summary>
/// <typeparam name="T">Input value type.</typeparam>
public interface IValueAction<in T>
{
    /// <summary>Performs the action for <paramref name="value"/>.</summary>
    /// <param name="value">Input value.</param>
    /// <example>
    /// <code>
    /// public readonly struct Observe : IValueAction&lt;Error&gt;
    /// {
    ///     public void Invoke(Error value) =&gt; Console.WriteLine(value.Message);
    /// }
    /// </code>
    /// </example>
    void Invoke(T value);
}
