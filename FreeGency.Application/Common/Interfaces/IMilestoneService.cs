
using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface IMilestoneService
{
    Task<ApiResponse<IEnumerable<MilestoneDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
}