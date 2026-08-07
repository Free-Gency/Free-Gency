using FreeGency.AI.Guardrails.Detection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreeGency.AI.Guardrails;

/// <summary>
/// Registers the shared guardrail subsystem. Call <c>AddGuardrails(configuration)</c>
/// from the composition root before any module that consumes the engine. It binds
/// <see cref="GuardrailOptions"/> to <c>AI:Guardrails</c>, registers every
/// deterministic detector, and registers the engine as a singleton.
/// </summary>
public static class GuardrailDependencyInjection
{
    public static IServiceCollection AddGuardrails(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GuardrailOptions>(configuration.GetSection(GuardrailOptions.SectionName));

        services.AddSingleton<IGuardrailDetector, LanguageDetector>();
        services.AddSingleton<IGuardrailDetector, PromptInjectionDetector>();
        services.AddSingleton<IGuardrailDetector, LlmAbuseDetector>();
        services.AddSingleton<IGuardrailDetector, SensitiveDataDetector>();
        services.AddSingleton<IGuardrailDetector, ProfanityDetector>();
        services.AddSingleton<IGuardrailDetector, ToxicityDetector>();
        services.AddSingleton<IGuardrailDetector, ScamDetector>();
        services.AddSingleton<IGuardrailDetector, AdvertisementDetector>();
        services.AddSingleton<IGuardrailDetector, SpamDetector>();

        services.AddSingleton<IGuardrailEngine, GuardrailEngine>();

        return services;
    }
}
