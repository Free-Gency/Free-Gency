namespace FreeGency.Application.Features.categories.Dtos;

public sealed class FilterCategoriesRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public string SortBy { get; init; } = "Name";
    public string SortDirection { get; init; } = "asc";
}
