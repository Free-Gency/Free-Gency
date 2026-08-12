using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Proposals.Dtos;

public class CreateProposalDto
{
    public Guid ProjectId { get; set; }

    public ApplicantType ApplicantType { get; set; }

    public Guid? TeamId { get; set; }

    public string CoverLetter { get; set; } = default!;

    public string Approach { get; set; } = string.Empty;

    public string? ProposedTimeline { get; set; }

    public string? SimilarLinksUrl { get; set; }

    public decimal ProposedBudget { get; set; }

    /// <summary>Optional attachment files (max 10).</summary>
    public IFormFile[]? Attachments { get; set; }
}
