using FreeGency.Application.Features.PayoutSplits.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface IPayoutSplitService
{
    Task<ApiResponse<PayoutSplitsDto>> GetTeamDefaultsAsync(Guid teamId, CancellationToken ct = default);
    Task<ApiResponse<PayoutSplitsDto>> ReplaceTeamDefaultsAsync(Guid teamId, ReplacePayoutSplitsDto dto, CancellationToken ct = default);

    Task<ApiResponse<PayoutSplitsDto>> GetProjectSplitsAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse<PayoutSplitsDto>> ReplaceProjectSplitsAsync(Guid projectId, ReplacePayoutSplitsDto dto, CancellationToken ct = default);
}
