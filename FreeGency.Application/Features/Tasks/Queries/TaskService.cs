
namespace FreeGency.Application.Features.Tasks.Commands;


public partial class TaskService
{
    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetByMilestoneAsync(Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await MilestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<IEnumerable<TaskDto>>(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await ProjectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<TaskDto>>(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await IsProjectParticipantAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<TaskDto>>(AppError.Forbidden("You cannot view this project."));

        var tasks = await TaskRepo.GetByMilestoneIdAsync(milestoneId, ct);
        var isManager = await IsTaskManagerAsync(project, ct);

        var dtos = new List<TaskDto>();
        foreach (var task in tasks)
            dtos.Add(await MapToDtoAsync(project, task, ct));

        return ApiResponse.Success<IEnumerable<TaskDto>>(dtos);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await ProjectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<TaskDto>>(AppError.NotFound(nameof(Project), projectId));

        if (!await IsProjectParticipantAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<TaskDto>>(AppError.Forbidden("You cannot view this project."));

        var tasks = await TaskRepo.GetByProjectIdAsync(projectId, ct);
        var dtos = new List<TaskDto>();
        foreach (var task in tasks)
            dtos.Add(await MapToDtoAsync(project, task, ct));

        return ApiResponse.Success<IEnumerable<TaskDto>>(dtos);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetMyTasksAsync(
        Guid? teamId = null,
        CancellationToken ct = default)
    {
        var tasks = await TaskRepo.GetAssignedToUserAsync(UserId, ct);

        var dtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            var project = task.Milestone?.Project;
            if (project is null) continue;

            if (teamId.HasValue)
            {
                // Team workspace: only tasks on this team's hired projects.
                if (project.AssignedTeamId != teamId.Value) continue;
            }
            else
            {
                // Manage Work: solo hired projects only.
                if (project.AssignedUserId != UserId) continue;
            }

            dtos.Add(await MapToDtoAsync(project, task, ct));
        }

        return ApiResponse.Success<IEnumerable<TaskDto>>(dtos);
    }

    public async Task<ApiResponse<TaskDto>> GetByIdAsync(Guid taskId, CancellationToken ct = default)
    {
        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsProjectParticipantAsync(project, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Forbidden("You cannot view this project."));

        return ApiResponse.Success(await MapToDtoAsync(project, task, ct));
    }
}