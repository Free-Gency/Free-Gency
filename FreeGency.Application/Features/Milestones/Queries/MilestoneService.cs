using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Application.Features.Milestones.Commands;

public partial class MilestoneService
{
    public async Task<ApiResponse<IEnumerable<MilestoneDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<MilestoneDto>>(AppError.NotFound(nameof(Project), projectId));

        var milestones = await _milestoneRepo.GetByProjectIdAsync(projectId, ct);

        return ApiResponse.Success(_mapper.Map<IEnumerable<MilestoneDto>>(milestones));
    }
}
