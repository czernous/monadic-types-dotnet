namespace MonadicTypes;

/// <summary>
/// Defines an allocation-free callable value that can be constrained and
/// inlined by the runtime. Implementations should normally be readonly structs.
/// </summary>
public interface IValueFunction<in TIn, out TOut>
{
    TOut Invoke(TIn value);
}
