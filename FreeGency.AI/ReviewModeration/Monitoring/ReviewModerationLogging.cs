using FreeGency.AI.Moderation.Observability;

namespace FreeGency.AI.ReviewModeration.Monitoring;

/// <summary>
/// Review moderation logging helpers. Reuses the shared
/// <see cref="ModerationLogSanitizer"/> so the review module never logs raw
/// sensitive values, without depending directly on the moderation feature
/// namespace at every call site.
/// </summary>
public static class ReviewModerationLogging
{
    /// <summary>The log source/category name used for review moderation events.</summary>
    public const string Source = "ReviewModeration";

    /// <summary>
    /// Redacts sensitive values (emails, phones, cards, IBANs, secrets, JWTs)
    /// from <paramref name="value"/> before it reaches a log.
    /// </summary>
    public static string Sanitize(string? value) => ModerationLogSanitizer.Sanitize(value);
}
