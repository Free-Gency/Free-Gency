namespace FreeGency.Application.Features.Plans.Dtos;

public sealed class EntitlementCheckDto
{
    public string Feature { get; init; } = string.Empty;
    public bool IsAllowed { get; init; }
    public bool IsEnabled { get; init; }
    public int? Limit { get; init; }
    public int Used { get; init; }
    public int Remaining { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public string? Message { get; init; }
}
