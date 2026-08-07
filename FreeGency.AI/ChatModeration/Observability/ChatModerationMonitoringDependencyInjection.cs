using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Prompts;
using FreeGency.AI.ChatModeration.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModerationInterfaces = FreeGency.AI.ChatModeration.Interfaces;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Registers the fully instrumented moderation pipeline (Parts 4 + 5 + 6):
/// the AI moderation service, the cache-first facade, and the observability
/// decorator with audit, metrics, health, and performance alerts.
///
/// Call <c>AddChatModerationMonitoring(configuration)</c> from the composition
/// root instead of <c>AddChatSecurity</c> / <c>AddChatSecurityCache</c> to get
/// the complete monitored pipeline. Register it AFTER any other
/// <c>AddMemoryCache</c> call so the LRU-bounded cache wins.
/// </summary>
public static class ChatModerationMonitoringDependencyInjection
{
    public static IServiceCollection AddChatModerationMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatModerationOptions>(configuration.GetSection(ChatModerationOptions.SectionName));
        services.Configure<ChatModerationCacheOptions>(configuration.GetSection(ChatModerationCacheOptions.SectionName));
        services.Configure<ChatModerationMonitoringOptions>(configuration.GetSection(ChatModerationMonitoringOptions.SectionName));

        services.AddSingleton<IMemoryCache>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ChatModerationCacheOptions>>().Value;
            return new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = options.MaxEntries,
                CompactionPercentage = 0.05,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1)
            });
        });

        // Inner moderation pipeline (Part 4).
        services.AddSingleton<ModerationInterfaces.IChatSecurityPromptBuilder, ChatSecurityPromptBuilder>();
        services.AddSingleton<MessageNormalizer>();
        services.AddSingleton<ContentLanguageDetector>();
        services.AddSingleton<EntityDetector>();
        services.AddSingleton<MessageMasker>();
        services.AddSingleton<ChatSecurityJsonParser>();
        services.AddSingleton<ChatSecurityCache>();
        services.AddSingleton<ChatSecurityMetrics>();

        // Cache-first pipeline (Part 5).
        services.AddSingleton<ChatSecurityCacheKeyBuilder>();
        services.AddSingleton<ChatSecurityResponseSanitizer>();
        services.AddSingleton<IChatModerationCache, ChatModerationCache>();

        // Observability layer (Part 6).
        services.AddSingleton<IChatModerationLogger, ChatModerationLogger>();
        services.AddSingleton<ChatModerationMetrics>();

        services.AddScoped<ChatModerationService>();
        services.AddScoped<CachedChatModerationService>();

        // The decorator is wired through a factory to avoid resolving itself
        // when the inner service is requested via the same interface.
        services.AddScoped<IChatModerationService>(sp =>
        {
            var inner = sp.GetRequiredService<CachedChatModerationService>();
            return new MonitoredChatModerationService(
                inner,
                sp.GetRequiredService<IChatModerationLogger>(),
                sp.GetRequiredService<ChatModerationMetrics>(),
                sp.GetRequiredService<IOptions<ChatModerationMonitoringOptions>>(),
                sp.GetRequiredService<MessageNormalizer>(),
                sp.GetRequiredService<ContentLanguageDetector>(),
                sp.GetRequiredService<IOptions<ChatModerationCacheOptions>>());
        });

        return services;
    }
}
