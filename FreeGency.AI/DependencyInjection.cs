using FreeGency.AI.HiringAgent;
using FreeGency.AI.MilestonePlanAssist;
using FreeGency.AI.Moderation;
using FreeGency.AI.ProjectDrafting;
using FreeGency.AI.ProposalAssistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<BedrockGatewayChatService>();

        services.AddSingleton<IChatCompletionService, BedrockGatewayChatService>();

        services.AddKernel();
        
        services.AddScoped<ProjectDraftService>();
        services.AddScoped<ProposalAssistantChatService>();
        services.AddScoped<DiscussionAgentChatService>();
        services.AddScoped<DiscussionRankingChatService>();
        services.AddScoped<MilestonePlanAssistChatService>();
        services.AddScoped<IModerationAgent, ModerationAgent>();

        services.AddAIFoundation(configuration);

        return services;
    }
}