using MonadicTypes;

namespace Benchmarks;

/// <summary>Real generated-callable inputs for consumer syntax measurements.</summary>
public static partial class Projections
{
    /// <summary>Transforms one value with the same semantics as the bare control.</summary>
    [GenerateValueFunction]
    public static Result<long, int> Widen(int value) => Result<long, int>.Ok(value + 1L);
}
