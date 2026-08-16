
namespace FreeGency.Application.Features.Plans;

public static class AiFeaturePricing
{
    public static readonly FeatureType[] AiFeatures =
    [
        FeatureType.GenerateProjectDraft,
        FeatureType.TeamSuggestions,
        FeatureType.AIChatProposal,
        FeatureType.ProposalRanking,
        FeatureType.HiringAgent
    ];

    public static bool IsTokenBased(FeatureType feature) => AiFeatures.Contains(feature);
}