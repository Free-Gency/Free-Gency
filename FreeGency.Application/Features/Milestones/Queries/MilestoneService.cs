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

    public async Task<ApiResponse<IEnumerable<DeveloperMilestoneDto>>> GetMyMilestonesAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var memberships = await _teamMemberRepo.GetByUserIdAsync(userId, ct);
        var myTeamIds = memberships.Select(m => m.TeamId).ToHashSet();
        var myLeaderTeamIds = memberships
            .Where(m => m.TeamRole == Role.TeamLeader)
            .Select(m => m.TeamId)
            .ToHashSet();

        var milestones = await _milestoneRepo.Query()
            .Include(m => m.Project)
            .Where(m => m.Project != null &&
                        (m.Project.AssignedUserId == userId ||
                         (m.Project.AssignedTeamId != null &&
                          myTeamIds.Contains(m.Project.AssignedTeamId.Value))))
            .OrderBy(m => m.DueDate)
            .ToListAsync(ct);

        var dtos = _mapper.Map<IEnumerable<DeveloperMilestoneDto>>(milestones).ToList();

        foreach (var dto in dtos)
        {
            var project = milestones.First(m => m.Id == dto.Id).Project!;
            dto.CanSubmit = project.AssignedUserId == userId ||
                            (project.AssignedTeamId != null &&
                             myLeaderTeamIds.Contains(project.AssignedTeamId.Value));
        }

        return ApiResponse.Success<IEnumerable<DeveloperMilestoneDto>>(dtos);
    }
}
