using FreeGency.AI.Moderation.Caching;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Prompts;
using FreeGency.AI.Moderation.Providers;
using FreeGency.AI.Moderation.Reliability;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Moderation;

/// <summary>
/// Registers the reusable moderation core. Call <c>AddModeration(configuration)</c>
/// from the composition root. It only adds its own services and never modifies
/// existing registrations. Requires <c>AddAI(configuration)</c> (or any other
/// registration of <c>IChatCompletionService</c>) to be present.
/// </summary>
public static class ModerationDependencyInjection
{
    public static IServiceCollection AddModeration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ModerationOptions>(configuration.GetSection(ModerationOptions.SectionName));

        services.AddMemoryCache();

        services.AddSingleton<ModerationJsonParser>();
        services.AddSingleton<ModerationMetrics>();
        services.AddSingleton<IModerationPromptRegistry, ModerationPromptRegistry>();
        services.AddSingleton<IModerationPromptBuilder, ModerationPromptBuilder>();
        services.AddSingleton<BedrockModerationProvider>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ModerationOptions>>().Value;
            return new ModerationCircuitBreaker(
                options.CircuitBreakerThreshold,
                TimeSpan.FromMinutes(options.CircuitBreakerResetMinutes));
        });

        // Decorator chain: Logging -> Caching -> CircuitBreaking -> Retrying -> Timeout -> Bedrock.
        services.AddSingleton<IModerationProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ModerationOptions>>().Value;
            var metrics = sp.GetRequiredService<ModerationMetrics>();

            IModerationProvider provider = sp.GetRequiredService<BedrockModerationProvider>();

            provider = new TimeoutModerationProvider(
                provider,
                TimeSpan.FromSeconds(options.TimeoutSeconds),
                sp.GetRequiredService<ILogger<TimeoutModerationProvider>>(),
                metrics);

            provider = new RetryingModerationProvider(
                provider,
                options.RetryCount,
                sp.GetRequiredService<ILogger<RetryingModerationProvider>>(),
                metrics);

            provider = new CircuitBreakingModerationProvider(
                provider,
                sp.GetRequiredService<ModerationCircuitBreaker>(),
                sp.GetRequiredService<ILogger<CircuitBreakingModerationProvider>>(),
                metrics);

            if (options.EnableCaching)
            {
                provider = new CachingModerationProvider(
                    provider,
                    sp.GetRequiredService<IMemoryCache>(),
                    options,
                    sp.GetRequiredService<ILogger<CachingModerationProvider>>(),
                    metrics);
            }

            if (options.EnableLogging)
            {
                provider = new LoggingModerationProvider(
                    provider,
                    sp.GetRequiredService<ILogger<LoggingModerationProvider>>(),
                    metrics);
            }

            return provider;
        });

        services.AddScoped<IModerationService, ModerationService>();

        return services;
    }
}
