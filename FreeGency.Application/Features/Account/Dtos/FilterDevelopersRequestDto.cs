using FreeGency.Application.Common.Pagination;

namespace FreeGency.Application.Features.Account.Dtos;

public sealed class FilterDevelopersRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SpecialtyId { get; init; }
    public Guid? SkillId { get; init; }
}
