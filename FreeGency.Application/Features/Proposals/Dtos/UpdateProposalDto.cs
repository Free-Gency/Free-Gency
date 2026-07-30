namespace FreeGency.Application.Features.Proposals.Dtos;

public class UpdateProposalDto
{
    public Guid Id { get; init; }

    public string? CoverLetter { get; init; }

    public string? Approach { get; init; }

    public string? ProposedTimeline { get; init; }

    public string? PortfolioUrl { get; init; }

    public decimal? ProposedBudget { get; init; }
}
