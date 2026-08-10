using FreeGency.AI.Cache;
using FreeGency.AI.Core;
using FreeGency.AI.Embeddings;
using FreeGency.AI.Interfaces;
using FreeGency.AI.Ranking;
using FreeGency.AI.Ranking.ProposalRanking;
using FreeGency.AI.Services;
using FreeGency.AI.Suggestions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI;

public static class AIDependencyInjection
{
    public static IServiceCollection AddAIFoundation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        services.AddSingleton<IAICacheService, InMemoryAICacheService>();

        var vectorStoreProvider = configuration
            .GetSection(AIOptions.SectionName)
            .GetSection(nameof(AIOptions.VectorStore))
            .GetValue<string>(nameof(VectorStoreOptions.Provider))
            ?? "inmemory";

        if (string.Equals(vectorStoreProvider, "qdrant", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IVectorStore, QdrantVectorStore>();
        else if (string.Equals(vectorStoreProvider, "chroma", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IVectorStore, ChromaVectorStore>();
        else
            services.AddSingleton<IVectorStore, InMemoryVectorStore>();

        var embeddingProvider = configuration
            .GetSection(AIOptions.SectionName)
            .GetValue<string>(nameof(AIOptions.EmbeddingProvider))
            ?? "bedrock";

        if (string.Equals(embeddingProvider, "local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, LocalDeterministicEmbeddingGenerator>();
        }
        else if (string.Equals(embeddingProvider, "gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<GeminiEmbeddingService>();
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, GeminiEmbeddingService>();
        }
        else
        {
            services.AddHttpClient<BedrockEmbeddingService>();
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, BedrockEmbeddingService>();
        }

        services.AddScoped<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IProposalRuleEngine, ProposalRuleEngine>();
        services.AddScoped<IProposalRankingService, ProposalRankingService>();
        services.AddScoped<IAIOrchestrator, AIOrchestrator>();

        services.AddSuggestionServices();

        return services;
    }
}
