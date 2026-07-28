
using FreeGency.Application.Features.ProjectEvents.DTOs;

namespace FreeGency.Application.Common.Interfaces;


public interface IProjectEventService
{
    Task<ApiResponse<IEnumerable<ProjectEventDto>>> GetByProjectIdAsync(Guid projectId, int skip = 0, int take = 50, CancellationToken ct = default);
}
