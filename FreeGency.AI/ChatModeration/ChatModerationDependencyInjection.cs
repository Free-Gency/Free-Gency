using FreeGency.AI.ChatModeration.Cache;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Helpers;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Logging;
using FreeGency.AI.ChatModeration.Prompts;
using FreeGency.AI.ChatModeration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI.ChatModeration;

public static class ChatModerationDependencyInjection
{
    public static IServiceCollection AddChatModeration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatModerationOptions>(configuration.GetSection(ChatModerationOptions.SectionName));

        services.AddMemoryCache();

        services.AddSingleton<IChatModerationCache, ChatModerationCache>();
        services.AddSingleton<IChatModerationPromptBuilder, ChatModerationPromptBuilder>();
        services.AddSingleton<IChatModerationJsonParser, ChatModerationJsonParser>();
        services.AddSingleton<IChatModerationLogger, ChatModerationLogger>();
        services.AddSingleton<IChatModerationMetrics, ChatModerationMetrics>();
        services.AddScoped<IChatModerationService, ChatModerationService>();

        return services;
    }
}
