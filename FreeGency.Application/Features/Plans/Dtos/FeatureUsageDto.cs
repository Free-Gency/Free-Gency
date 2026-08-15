
namespace FreeGency.Application.Features.Plans.Dtos;

public sealed record FeatureUsageDto(
        bool IsEnabled, int? Limit, int Used, int Remaining);

