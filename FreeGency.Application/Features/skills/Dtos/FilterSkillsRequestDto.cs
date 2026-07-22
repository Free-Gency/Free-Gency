namespace FreeGency.Application.Features.skills.Dtos;

public sealed class FilterSkillsRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SpecialtyId { get; init; }
    public string SortBy { get; init; } = "Name";
    public string SortDirection { get; init; } = "asc";
}
