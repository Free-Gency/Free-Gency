using FreeGency.AI.ChatModeration.Observability;
using FreeGency.AI.ChatModeration.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FreeGency.Api.Extensions;

/// <summary>
/// Registers everything the AI chat security API needs (Part 7): the fully
/// instrumented moderation pipeline (Parts 4 + 5 + 6) and the per-user /
/// per-conversation rate limiter used by the <c>moderate</c> endpoints.
///
/// Call <c>AddChatModerationApi(configuration)</c> from the composition root as
/// a single replacement for <c>AddChatSecurity</c> / <c>AddChatSecurityCache</c>
/// / <c>AddChatModerationMonitoring</c>.
/// </summary>
public static class ChatModerationApiDependencyInjection
{
    public static IServiceCollection AddChatModerationApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddChatModerationMonitoring(configuration);
        services.Configure<ChatModerationRateLimitOptions>(
            configuration.GetSection(ChatModerationRateLimitOptions.SectionName));
        services.AddSingleton<ChatModerationRateLimiter>();
        return services;
    }
}
