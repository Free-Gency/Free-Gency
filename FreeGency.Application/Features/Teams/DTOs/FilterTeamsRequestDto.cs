using FreeGency.Application.Common.Pagination;

namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class FilterTeamsRequestDto : PagedQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// When true and the caller is authenticated, exclude teams they own or already belong to.
    /// </summary>
    public bool ExcludeMine { get; init; } = true;
}
