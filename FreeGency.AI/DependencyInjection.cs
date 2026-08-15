using FreeGency.AI.HirePyInterview;
using FreeGency.AI.HirePyInterview.Evaluation;
using FreeGency.AI.HirePyInterview.MilestonePlanning;
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
        services.AddScoped<IProjectGenerationService, ProjectGenerationService>();
        services.AddScoped<ProposalAssistantChatService>();
        services.AddScoped<IModerationAgent, ModerationAgent>();
        services.AddScoped<IHirePyInterviewAgent, HirePyInterviewAgent>();
        services.AddScoped<IHirePyMilestonePlannerAgent, HirePyMilestonePlannerAgent>();
        services.AddScoped<IHirePyEvaluatorAgent, HirePyEvaluatorAgent>();

        services.AddAIFoundation(configuration);

        return services;
    }
}