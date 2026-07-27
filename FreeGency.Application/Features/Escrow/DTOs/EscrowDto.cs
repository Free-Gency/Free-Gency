
namespace FreeGency.Application.Features.Escrow.DTOs;

public sealed class EscrowDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal TotalReleased { get; init; }
    public decimal Remaining => TotalAmount - TotalReleased;
    public string FundingStatus { get; init; } = default!;
    public string PlanStatus { get; init; } = default!;
    public DateTime? LockedAt { get; init; }
    public DateTime? PlanAgreedAt { get; init; }
}
