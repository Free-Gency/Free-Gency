
namespace FreeGency.Application.Features.Plans.Dtos;

public sealed record PlanSnapshotDto(
    string PlanName,
    bool IsSubscribed,
    DateTime? RenewsAt,
    IReadOnlyDictionary<FeatureType, FeatureUsageDto> Usage,
    IReadOnlyList<TokenUsageDto> TokenUsage,
    long AllowedTokens,
    long TokensUsedThisMonth,
    long TokensRemainingThisMonth);