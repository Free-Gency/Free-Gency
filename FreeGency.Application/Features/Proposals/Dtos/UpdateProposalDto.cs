
namespace FreeGency.Application.Features.Proposals.Dtos;

public class UpdateProposalDto
{
    public Guid Id { get; init; }

    public string? CoverLetter { get; init; }

    public decimal? ProposedBudget { get; init; }

    public ProposalStatus? Status { get; init; }
}
