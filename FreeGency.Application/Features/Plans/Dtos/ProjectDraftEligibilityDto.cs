namespace FreeGency.Application.Features.Plans.Dtos;

public sealed class ProjectDraftEligibilityDto
{
    public bool CanCreateProject { get; init; }
    public bool CanGenerateDraft { get; init; }
    public EntitlementCheckDto CreateProject { get; init; } = null!;
    public EntitlementCheckDto GenerateProjectDraft { get; init; } = null!;
    public string? Message { get; init; }
}
