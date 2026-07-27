using FreeGency.Application.Features.ProjectEvents.DTOs;

namespace FreeGency.Application.Features.ProjectEvents.Commands;

public partial class ProjectEventService
{
    public async Task<ApiResponse<IEnumerable<ProjectEventDto>>> GetByProjectIdAsync(Guid projectId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<ProjectEventDto>>(AppError.NotFound(nameof(Project), projectId));

        var events = await _eventRepo.GetByProjectIdAsync(projectId, skip, take, ct);

        return ApiResponse.Success(_mapper.Map<IEnumerable<ProjectEventDto>>(events));
    }
}