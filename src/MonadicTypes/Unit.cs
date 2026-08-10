namespace MonadicTypes;

/// <summary>
/// Represents the single possible value of a successful operation that does
/// not return data. It is the C# equivalent of Rust's unit value, <c>()</c>.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value => default;

    [Obsolete("Use Unit.Value.")]
    public static Unit Default => default;

    public override string ToString() => "()";
}
