namespace FreeGency.Application.Features.Proposals.Dtos;

public sealed class FilterProposalDto : PagedQuery
{
    public Guid? ProjectId { get; init; }
    public ApplicantType? ApplicantType { get; init; }
    public Guid? UserId { get; init; }
    public Guid? TeamId { get; init; }
    public ProposalStatus? Status { get; init; }
    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }
    public string? Search { get; init; }
    public string SortBy { get; init; } = "AppliedAt";
    public string SortDirection { get; init; } = "desc";
}