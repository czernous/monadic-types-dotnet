using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>
/// Vendor-neutral error counter backed by a caller-owned Meter. Export through
/// OpenTelemetry, Prometheus, or any System.Diagnostics.Metrics listener.
/// </summary>
public readonly struct ErrorMetrics
{
    private readonly Counter<long> _counter;
    private readonly bool _includeErrorCode;

    public static ErrorMetrics Disabled => default;
    public bool IsEnabled => _counter?.Enabled is true;

    public ErrorMetrics(
        Meter meter,
        bool includeErrorCode = false,
        string instrumentName = "monadic.errors")
    {
        ArgumentNullException.ThrowIfNull(meter);
        ArgumentException.ThrowIfNullOrWhiteSpace(instrumentName);
        _counter = meter.CreateCounter<long>(
            instrumentName,
            description: "Number of observed Result errors.");
        _includeErrorCode = includeErrorCode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Record(in Error? error)
    {
        Counter<long>? counter = _counter;
        if (counter is null || !counter.Enabled)
        {
            return;
        }

        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        string category = ErrorTelemetry.GetCategoryName(error.Type);
        if (_includeErrorCode)
        {
            counter.Add(
                1,
                new KeyValuePair<string, object?>("error.category", category),
                new KeyValuePair<string, object?>("error.type", error.Code));
            return;
        }

        counter.Add(1, new KeyValuePair<string, object?>("error.category", category));
    }
}
