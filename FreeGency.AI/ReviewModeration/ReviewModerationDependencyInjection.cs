using FreeGency.AI.Guardrails;
using FreeGency.AI.Monitoring;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.Guardrails;
using FreeGency.AI.ReviewModeration.Intelligence;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Observability;
using FreeGency.AI.ReviewModeration.Providers;
using FreeGency.AI.ReviewModeration.Prompts;
using FreeGency.AI.ReviewModeration.Reliability;
using FreeGency.AI.ReviewModeration.Security;
using FreeGency.AI.ReviewModeration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI.ReviewModeration;

/// <summary>
/// Registers the review moderation module. Call <c>AddReviewModeration(configuration)</c>
/// from the composition root. It registers its own services and reuses everything
/// already present: <c>IChatCompletionService</c> (from <c>AddAI</c>),
/// <c>IAICacheService</c> (from <c>AddAIFoundation</c>), and
/// <c>IOptions&lt;ModerationOptions&gt;</c> (from <c>AddModeration</c>). All three
/// registrations are therefore required before calling this method. The
/// correlation context accessor and the shared monitoring foundation are also
/// registered here so the review module is self-contained.
/// </summary>
public static class ReviewModerationDependencyInjection
{
    public static IServiceCollection AddReviewModeration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGuardrails(configuration);

        services.Configure<ReviewModerationCacheOptions>(
            configuration.GetSection(ReviewModerationCacheOptions.SectionName));

        services.Configure<ReviewModerationReliabilityOptions>(
            configuration.GetSection(ReviewModerationReliabilityOptions.SectionName));

        services.Configure<ReviewModerationMonitoringOptions>(
            configuration.GetSection(ReviewModerationMonitoringOptions.SectionName));

        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddSingleton<ReviewModerationMetrics>();
        services.AddSingleton<IReviewModerationMetrics>(sp => sp.GetRequiredService<ReviewModerationMetrics>());
        services.AddSingleton<IReviewModerationLogger, ReviewModerationLogger>();
        services.AddSingleton<IReviewModerationHealthCheck, ReviewModerationHealthCheck>();
        services.AddSingleton<ReviewModerationCacheKeyBuilder>();
        services.AddSingleton<IReviewModerationCache, ReviewModerationCache>();

        services.AddSingleton<IReviewContentMasker, ReviewContentMasker>();
        services.AddSingleton<IReviewSpamDetector, ReviewSpamDetector>();
        services.AddSingleton<IReviewModerationGuardrails, ReviewModerationGuardrails>();
        services.AddSingleton<IReviewSecurityAnalyzer, ReviewSecurityAnalyzer>();
        services.AddSingleton<IReviewIntelligenceAnalyzer, ReviewIntelligenceAnalyzer>();
        services.AddSingleton<IReviewPromptBuilder, ReviewPromptBuilder>();
        services.AddSingleton<IReviewModerationProvider, ReviewModerationProvider>();

        services.AddScoped<ReviewModerationService>();
        services.AddScoped<IReviewModerationService, MonitoredReviewModerationService>();

        return services;
    }
}
