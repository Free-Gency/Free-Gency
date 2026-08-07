using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Prompts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Registers the AI Chat Security moderation service and its dependencies.
/// Call <c>AddChatSecurity(configuration)</c> from the composition root alongside
/// the existing <c>AddChatModeration</c> / <c>AddAI</c> registrations.
/// </summary>
public static class ChatSecurityDependencyInjection
{
    public static IServiceCollection AddChatSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatModerationOptions>(configuration.GetSection(ChatModerationOptions.SectionName));

        services.AddMemoryCache();

        services.AddSingleton<IChatSecurityPromptBuilder, ChatSecurityPromptBuilder>();
        services.AddSingleton<MessageNormalizer>();
        services.AddSingleton<ContentLanguageDetector>();
        services.AddSingleton<EntityDetector>();
        services.AddSingleton<MessageMasker>();
        services.AddSingleton<ChatSecurityJsonParser>();
        services.AddSingleton<ChatSecurityCache>();
        services.AddSingleton<ChatSecurityMetrics>();

        services.AddScoped<IChatModerationService, ChatModerationService>();

        return services;
    }
}
