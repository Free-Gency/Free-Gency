using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface IMilestoneService
{
    Task<ApiResponse<IEnumerable<MilestoneDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<DeveloperMilestoneDto>>> GetMyMilestonesAsync(CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<MilestonePlanVersionDto>>> GetPlanVersionsAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse<MilestonePlanVersionDto>> GetLatestPlanAsync(Guid projectId, CancellationToken ct = default);

    Task<ApiResponse<MilestonePlanVersionDto>> ProposePlanAsync(ProposeMilestonePlanDto dto, CancellationToken ct = default);
    Task<ApiResponse> RequestPlanChangesAsync(RequestPlanChangesDto dto, CancellationToken ct = default);
    Task<ApiResponse> AcceptPlanAsync(Guid planVersionId, CancellationToken ct = default);

    Task<ApiResponse> FundNextMilestoneAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse> SubmitMilestoneAsync(Guid milestoneId, CancellationToken ct = default);
    Task<ApiResponse> ApproveAndReleaseAsync(Guid milestoneId, CancellationToken ct = default);
    Task<ApiResponse> RequestMilestoneWorkChangesAsync(Guid milestoneId, string comment, CancellationToken ct = default);

    /// <summary>Called by background worker for auto-release after review timeout.</summary>
    Task<int> AutoReleaseDueMilestonesAsync(CancellationToken ct = default);
}
