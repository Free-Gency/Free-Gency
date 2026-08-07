using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Prompts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Registers the cache-first chat moderation pipeline (Part 5). Call
/// <c>AddChatSecurityCache(configuration)</c> from the composition root instead
/// of <c>AddChatSecurity</c> when you want cache-first behaviour.
///
/// This extension is self-contained: it registers the inner moderation pipeline
/// and its own size-bounded <see cref="IMemoryCache"/>, so no other moderation
/// registration is required. Register it AFTER any other <c>AddMemoryCache</c>
/// call so the LRU-bounded cache is the one that wins.
/// </summary>
public static class ChatSecurityCacheDependencyInjection
{
    public static IServiceCollection AddChatSecurityCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatModerationOptions>(configuration.GetSection(ChatModerationOptions.SectionName));
        services.Configure<ChatModerationCacheOptions>(configuration.GetSection(ChatModerationCacheOptions.SectionName));

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

        // Inner moderation pipeline.
        services.AddSingleton<IChatSecurityPromptBuilder, ChatSecurityPromptBuilder>();
        services.AddSingleton<MessageNormalizer>();
        services.AddSingleton<ContentLanguageDetector>();
        services.AddSingleton<EntityDetector>();
        services.AddSingleton<MessageMasker>();
        services.AddSingleton<ChatSecurityJsonParser>();
        services.AddSingleton<ChatSecurityCache>();
        services.AddSingleton<ChatSecurityMetrics>();

        // Cache infrastructure.
        services.AddSingleton<ChatSecurityCacheKeyBuilder>();
        services.AddSingleton<ChatSecurityResponseSanitizer>();
        services.AddSingleton<IChatModerationCache, ChatModerationCache>();

        // The inner service is resolved as a concrete type so the cache-first
        // facade can be registered against the interface without ambiguity.
        services.AddScoped<ChatModerationService>();
        services.AddScoped<IChatModerationService, CachedChatModerationService>();

        return services;
    }
}
