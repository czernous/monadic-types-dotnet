namespace MonadicTypes;

/// <summary>
/// Defines an allocation-free callable value that can be constrained and
/// inlined by the runtime. Implementations should normally be readonly structs.
/// </summary>
public interface IValueFunction<in TIn, out TOut>
{
    /// <summary>Transforms <paramref name="value"/> into an output value.</summary>
    /// <param name="value">Input value.</param>
    /// <returns>The transformed output.</returns>
    /// <example>
    /// <code>
    /// public readonly struct GetId : IValueFunction&lt;User, int&gt;
    /// {
    ///     public int Invoke(User value) =&gt; value.Id;
    /// }
    /// </code>
    /// </example>
    TOut Invoke(TIn value);
}
