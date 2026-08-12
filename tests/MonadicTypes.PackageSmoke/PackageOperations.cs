using MonadicTypes;

namespace MonadicTypes.PackageSmoke;

public static partial class PackageOperations
{
    [GenerateValueFunction]
    public static ValueTask<long> IncrementAsync(int value) => ValueTask.FromResult(value + 1L);
}
