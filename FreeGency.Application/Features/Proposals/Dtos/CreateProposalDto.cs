using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Proposals.Dtos;

public class CreateProposalDto
{
    public Guid ProjectId { get; init; }

    public ApplicantType ApplicantType { get; init; }

    public Guid? TeamId { get; init; }

    public string CoverLetter { get; init; } = default!;

    public string Approach { get; init; } = string.Empty;

    public string? ProposedTimeline { get; init; }

    public string? SimilarLinksUrl { get; init; }

    public decimal ProposedBudget { get; init; }

    /// <summary>Optional attachment files (max 10).</summary>
    public IFormFile[]? Attachments { get; init; }
}
