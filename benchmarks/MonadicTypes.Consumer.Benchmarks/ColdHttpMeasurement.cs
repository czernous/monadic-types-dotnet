using System.Diagnostics;
using MonadicTypes;
using MonadicTypes.AspNetCore;

namespace Benchmarks;

internal static class ColdHttpMeasurement
{
    internal static void Run()
    {
        Error error = Error.Conflict("STALE", "Reload the resource.");
        _ = Stopwatch.GetTimestamp();
        _ = GC.GetAllocatedBytesForCurrentThread();

        (long defaultBytes, TimeSpan defaultElapsed) = Measure(error, status: null);
        (long firstBytes, TimeSpan firstElapsed) = Measure(error, 412);
        (long secondBytes, TimeSpan secondElapsed) = Measure(error, 412);

        Console.WriteLine($"First default response: {defaultBytes} B, {defaultElapsed.TotalMicroseconds:F3} us");
        Console.WriteLine($"First explicit response: {firstBytes} B, {firstElapsed.TotalMicroseconds:F3} us");
        Console.WriteLine($"Second explicit response: {secondBytes} B, {secondElapsed.TotalMicroseconds:F3} us");
        Console.WriteLine("Single-process cold observations, not steady-state timing baselines. Run the published NativeAOT executable in a fresh process.");
    }

    private static (long Bytes, TimeSpan Elapsed) Measure(Error error, int? status)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        var response = status is { } code
            ? ErrorProblemDetails.CreateExample(error, code)
            : ErrorProblemDetails.CreateExample(error);
        TimeSpan elapsed = Stopwatch.GetElapsedTime(start);
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(response);
        return (bytes, elapsed);
    }
}
