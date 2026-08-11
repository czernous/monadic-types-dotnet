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

    /// <summary>Gets a recorder that performs no work and creates no instrument.</summary>
    public static ErrorMetrics Disabled => default;

    /// <summary>Gets whether the counter currently has an enabled listener.</summary>
    public bool IsEnabled => _counter?.Enabled is true;

    /// <summary>Creates an error counter on a caller-owned meter.</summary>
    /// <param name="meter">The meter through which consumers export measurements.</param>
    /// <param name="includeErrorCode">Whether to add the potentially high-cardinality error code tag.</param>
    /// <param name="instrumentName">The counter instrument name.</param>
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

    /// <summary>Records one observed error when the counter has an enabled listener.</summary>
    /// <param name="error">The initialized error to categorize and count.</param>
    /// <exception cref="ArgumentNullException">The counter is enabled and <paramref name="error"/> is null.</exception>
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
