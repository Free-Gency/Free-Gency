namespace FreeGency.Application.Features.specialties.Dtos;

public sealed class FilterSpecialtiesRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public string SortBy { get; init; } = "NameEn";
    public string SortDirection { get; init; } = "asc";
}
