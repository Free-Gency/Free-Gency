
namespace FreeGency.Application.Features.Proposals.Dtos;

public class ProposalDto
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    public string ProjectTitle { get; init; } = string.Empty;

    public string ApplicantType { get; init; } = default!;

    public Guid? TeamId { get; init; }

    public string? TeamName { get; init; }

    public Guid? UserId { get; init; }

    public string? ApplicantName { get; init; }

    public string? ApplicantAvatarUrl { get; init; }

    public string CoverLetter { get; init; } = string.Empty;

    public decimal ProposedBudget { get; init; }

    public string Status { get; init; } = default!;

    public DateTime AppliedAt { get; init; }

    public DateTime? ResponseAt { get; init; }

    public Guid? ChatRoomId { get; init; }

    public IEnumerable<string> AttachmentUrls { get; init; } = [];
}