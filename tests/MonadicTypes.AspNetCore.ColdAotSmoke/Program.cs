using Microsoft.AspNetCore.Mvc;
using MonadicTypes;
using MonadicTypes.AspNetCore;

const long MaxColdStatusIdentityBytes = 4_096;

Error error = Error.Conflict("STALE", "Reload the resource.");
_ = Measure(error, statusCode: null);

long defaultBytes = Measure(error, statusCode: null);
long firstExplicitBytes = Measure(error, 412);
long secondExplicitBytes = Measure(error, 412);
long coldIdentityBytes = firstExplicitBytes - defaultBytes;

Console.WriteLine($"Default response: {defaultBytes} B");
Console.WriteLine($"First explicit response: {firstExplicitBytes} B ({coldIdentityBytes} B cold identity cost)");
Console.WriteLine($"Second explicit response: {secondExplicitBytes} B");

if (coldIdentityBytes > MaxColdStatusIdentityBytes)
{
    throw new InvalidOperationException(
        $"Cold HTTP identity allocation was {coldIdentityBytes} B; expected at most {MaxColdStatusIdentityBytes} B.");
}

if (secondExplicitBytes != defaultBytes)
{
    throw new InvalidOperationException(
        $"Steady explicit response allocated {secondExplicitBytes} B; expected the default response's {defaultBytes} B.");
}

static long Measure(Error error, int? statusCode)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    ProblemDetails response = statusCode is { } value
        ? ErrorProblemDetails.CreateExample(error, value)
        : ErrorProblemDetails.CreateExample(error);
    long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    GC.KeepAlive(response);
    return allocated;
}
