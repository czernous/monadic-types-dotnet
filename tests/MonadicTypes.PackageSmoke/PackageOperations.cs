using MonadicTypes;

namespace MonadicTypes.PackageSmoke;

public static partial class PackageOperations
{
    [GenerateValueFunction]
    public static Result<long, Error> Widen(int value) => Result<long, Error>.Ok(value + 1L);

    /// <summary>Returns a value from the XML-projection compatibility endpoint.</summary>
    public static int DocumentedEndpoint() => 1;

    [GenerateValueFunction]
    public static ValueTask<long> IncrementAsync(int value) => ValueTask.FromResult(value + 1L);
}
