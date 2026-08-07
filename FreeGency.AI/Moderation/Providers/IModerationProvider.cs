using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Providers;

/// <summary>
/// The core moderation provider abstraction. Implementations analyse a
/// <see cref="ModerationRequest"/> and return a verdict. The reliability,
/// caching, and observability layers are decorators over this interface, so each
/// concern can be unit-tested independently with a mocked inner provider.
/// </summary>
public interface IModerationProvider
{
    /// <summary>
    /// Analyses the given content and returns a <see cref="ModerationResult"/>.
    /// Providers throw <see cref="Reliability.ModerationProviderException"/> on
    /// failure so decorators can retry, trip the circuit breaker, or fall back.
    /// </summary>
    Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default);
}
