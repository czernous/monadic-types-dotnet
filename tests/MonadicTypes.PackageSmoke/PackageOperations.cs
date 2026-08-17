using MonadicTypes;

namespace MonadicTypes.PackageSmoke;

public static partial class PackageOperations
{
    /// <summary>Returns a value from the XML-projection compatibility endpoint.</summary>
    public static int DocumentedEndpoint() => 1;

    [GenerateValueFunction]
    public static ValueTask<long> IncrementAsync(int value) => ValueTask.FromResult(value + 1L);
}
