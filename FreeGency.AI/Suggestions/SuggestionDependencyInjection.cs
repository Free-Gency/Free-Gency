using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI.Suggestions;

public static class SuggestionDependencyInjection
{
    public static IServiceCollection AddSuggestionServices(this IServiceCollection services)
    {
        services.AddSingleton<SuggestionDocumentBuilder>();
        services.AddScoped<ISuggestionIndexingService, SuggestionIndexingService>();
        services.AddScoped<ISuggestionSearchService, SuggestionSearchService>();
        return services;
    }
}
