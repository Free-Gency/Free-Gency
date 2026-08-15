using FreeGency.Domain.Enums;

namespace FreeGency.Application.Features.HirePy.Dtos;

public sealed class HirePySelectedCandidateDto
{
    public Guid ProposalId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int Rank { get; set; }
    public ApplicantType InviteeType { get; set; }
    public Guid InviteeId { get; set; }
}
