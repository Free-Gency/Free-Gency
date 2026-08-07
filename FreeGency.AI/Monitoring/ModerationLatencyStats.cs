namespace FreeGency.AI.Monitoring;

/// <summary>
/// Thread-safe latency tracker used by the AI module metrics. Samples are stored
/// in a bounded rolling window (oldest evicted first) so memory stays flat under
/// high throughput while percentiles remain meaningful. The average is exact and
/// includes every sample; the percentiles are computed from the window.
/// </summary>
public sealed class ModerationLatencyStats
{
    /// <summary>The maximum number of samples kept for percentile computation.</summary>
    private const int MaxSamples = 2048;

    private readonly object _sync = new();
    private readonly List<double> _samples = new();
    private long _count;
    private long _totalTicks;

    /// <summary>Records one latency sample. Thread-safe.</summary>
    public void Add(TimeSpan latency)
    {
        lock (_sync)
        {
            _count++;
            _totalTicks += latency.Ticks;
            _samples.Add(latency.TotalMilliseconds);

            if (_samples.Count > MaxSamples)
                _samples.RemoveAt(0);
        }
    }

    /// <summary>Builds an immutable latency summary. Thread-safe.</summary>
    public LatencySummary GetSummary()
    {
        lock (_sync)
        {
            if (_samples.Count == 0)
            {
                return new LatencySummary(
                    SampleCount: _count,
                    AverageMs: _count == 0 ? 0 : _totalTicks / (double)TimeSpan.TicksPerMillisecond / _count,
                    MinMs: 0,
                    MaxMs: 0,
                    P50Ms: 0,
                    P95Ms: 0,
                    P99Ms: 0);
            }

            var sorted = _samples.ToArray();
            Array.Sort(sorted);

            return new LatencySummary(
                SampleCount: _count,
                AverageMs: _totalTicks / (double)TimeSpan.TicksPerMillisecond / _count,
                MinMs: sorted[0],
                MaxMs: sorted[^1],
                P50Ms: Percentile(sorted, 0.50),
                P95Ms: Percentile(sorted, 0.95),
                P99Ms: Percentile(sorted, 0.99));
        }
    }

    /// <summary>Resets all samples and counters. Used by tests and operations tooling.</summary>
    public void Reset()
    {
        lock (_sync)
        {
            _samples.Clear();
            _count = 0;
            _totalTicks = 0;
        }
    }

    private static double Percentile(double[] sorted, double percentile)
    {
        var index = (sorted.Length - 1) * percentile;
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
            return sorted[lower];

        var fraction = index - lower;
        return sorted[lower] + (sorted[upper] - sorted[lower]) * fraction;
    }
}
