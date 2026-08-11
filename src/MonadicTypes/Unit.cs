namespace MonadicTypes;

/// <summary>
/// Represents the single possible value of a successful operation that does
/// not return data. It is the C# equivalent of Rust's unit value, <c>()</c>.
/// </summary>
public readonly record struct Unit
{
    /// <summary>Gets the sole unit value.</summary>
    public static Unit Value => default;

    /// <summary>Returns the canonical unit representation.</summary>
    public override string ToString() => "()";
}
