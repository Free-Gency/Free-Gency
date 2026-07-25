namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class FilterTeamJobsRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public Guid? TeamId { get; init; }
    public string SortBy { get; init; } = "Title";
    public string SortDirection { get; init; } = "asc";
}
