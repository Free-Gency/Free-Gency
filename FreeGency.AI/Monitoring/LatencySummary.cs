namespace FreeGency.AI.Monitoring;

/// <summary>
/// A point-in-time latency summary derived from the samples collected by
/// <see cref="ModerationLatencyStats"/>. All values are in milliseconds.
/// </summary>
/// <param name="SampleCount">The number of samples the summary is based on.</param>
/// <param name="AverageMs">The arithmetic mean of all samples.</param>
/// <param name="MinMs">The fastest observed sample.</param>
/// <param name="MaxMs">The slowest observed sample.</param>
/// <param name="P50Ms">The 50th percentile (median).</param>
/// <param name="P95Ms">The 95th percentile.</param>
/// <param name="P99Ms">The 99th percentile.</param>
public sealed record LatencySummary(
    long SampleCount,
    double AverageMs,
    double MinMs,
    double MaxMs,
    double P50Ms,
    double P95Ms,
    double P99Ms);
