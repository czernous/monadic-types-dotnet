using System.Diagnostics.Metrics;

namespace MonadicTypes.Tests;

public class ErrorMetricsTests
{
    [Fact]
    public void Disabled_IsAnExplicitNoOp()
    {
        ErrorMetrics metrics = ErrorMetrics.Disabled;

        metrics.Record(default);

        Assert.False(metrics.IsEnabled);
    }

    [Fact]
    public void Record_DisabledInstrumentAllocatesNothing()
    {
        using Meter meter = new("Primitives.Tests.Disabled");
        ErrorMetrics metrics = new(meter);
        Error error = Error.Validation("INVALID", "Invalid input");
        metrics.Record(error);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 10_000; index++)
        {
            metrics.Record(error);
        }

        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void Record_EmitsLowCardinalityCategoryByDefault()
    {
        using Meter meter = new("Primitives.Tests.Enabled");
        using MeterListener listener = new();
        KeyValuePair<string, object?> observedTag = default;
        listener.InstrumentPublished = (instrument, current) =>
        {
            if (string.Equals(instrument.Meter.Name, meter.Name, StringComparison.Ordinal))
            {
                current.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) => observedTag = tags[0]);
        listener.Start();
        ErrorMetrics metrics = new(meter);

        metrics.Record(Error.NotFound("ORDER_MISSING", "Order missing"));

        Assert.Equal("error.category", observedTag.Key);
        Assert.Equal("not_found", observedTag.Value);
    }
}
