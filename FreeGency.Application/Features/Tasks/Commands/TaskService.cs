

namespace FreeGency.Application.Features.Tasks.Commands;


public partial class TaskService : ITaskService
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private readonly INotificationService _notificationService;

    private ITaskRepository TaskRepo;
    private ITaskCommentRepository CommentRepo;
    private ITaskChecklistItemRepository ChecklistRepo;
    private ITaskAttachmentRepository AttachmentRepo;
    private ITaskTimeLogRepository TimeLogRepo;
    private ITaskSubtaskRepository SubtaskRepo;
    private IMilestoneRepository MilestoneRepo;
    private IProjectRepository ProjectRepo;
    private ITeamMemberRepository TeamMemberRepo;
    private IProjectEventRepository EventRepo;
    private IUserRepository UserRepo;

    #endregion

    #region Constructor
    public TaskService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IStorageService storageService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _storageService = storageService;
        _notificationService = notificationService;

        TaskRepo = _unitOfWork.Repository<ITaskRepository, ProjectTask>();
        CommentRepo = _unitOfWork.Repository<ITaskCommentRepository, TaskComment>();
        ChecklistRepo = _unitOfWork.Repository<ITaskChecklistItemRepository, TaskChecklistItem>();
        AttachmentRepo = _unitOfWork.Repository<ITaskAttachmentRepository, TaskAttachment>();
        TimeLogRepo = _unitOfWork.Repository<ITaskTimeLogRepository, TaskTimeLog>();
        SubtaskRepo = _unitOfWork.Repository<ITaskSubtaskRepository, TaskSubtask>();
        MilestoneRepo = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        ProjectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        TeamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        EventRepo = _unitOfWork.Repository<IProjectEventRepository, ProjectEvent>();
        UserRepo = _unitOfWork.Repository<IUserRepository, User>();
    }
    #endregion


    #region Shared Helpers
    private Guid UserId => _currentUser.UserId;

    private async Task<ProjectTask?> GetTaskAsync(Guid taskId, CancellationToken ct)
        => await TaskRepo.GetByIdWithDetailsAsync(taskId, ct);


    /// <summary> 
    /// Can the current user create/edit/delete/assign tasks in this project?
    /// </summary>
    private async Task<bool> IsTaskManagerAsync(Project project, CancellationToken ct)
    {
        if (project.AssignedUserId == UserId)
            return true;

        return project.AssignedTeamId is not null &&
               await TeamMemberRepo.IsLeaderAsync(project.AssignedTeamId.Value, UserId, ct);
    }


    /// <summary>
    /// Can the current user view this project (client, assignee, or team member)?
    /// </summary>
    private async Task<bool> IsProjectParticipantAsync(Project project, CancellationToken ct)
    {
        if (project.ClientId == UserId)
            return true;
        if (project.AssignedUserId == UserId)
            return true;

        return project.AssignedTeamId is not null &&
               await TeamMemberRepo.IsMemberAsync(project.AssignedTeamId.Value, UserId, ct);
    }


    private async Task<bool> IsAssigneeAsync(ProjectTask task, CancellationToken ct)
        => task.AssigneeUserId == UserId;


    private async Task<Project?> LoadProjectForMilestoneAsync(Guid milestoneId, CancellationToken ct)
    {
        var milestone = await MilestoneRepo.GetByIdAsync(milestoneId, ct);
        return milestone is null ? null : await ProjectRepo.GetByIdAsync(milestone.ProjectId, ct);
    }

    private async Task<(Project? Project, ProjectTask? Task)> LoadProjectAndTaskAsync(Guid taskId, CancellationToken ct)
    {
        var task = await GetTaskAsync(taskId, ct);
        if (task is null)
            return (null, null);

        var project = await ProjectRepo.GetByIdAsync(task.Milestone.ProjectId, ct);
        return (project, task);
    }

    private static bool CanTransit(Domain.Enums.TaskStatus from, Domain.Enums.TaskStatus to, bool isManager)
    {
        if (from == to)
            return true;
        if (isManager)
            return true;

        return (from, to) switch
        {
            (Domain.Enums.TaskStatus.Todo, Domain.Enums.TaskStatus.InProgress) => true,
            (Domain.Enums.TaskStatus.InProgress, Domain.Enums.TaskStatus.InReview) => true,
            _ => false
        };
    }

    private static string FullName(User? user)
        => user is null ? string.Empty : $"{user.FristName} {user.LastName}".Trim();

    private async Task RecordEventAsync(Guid projectId, Guid? milestoneId, EventType type, CancellationToken ct)
    {
        await EventRepo.AddAsync(new ProjectEvent
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            MilestoneId = milestoneId,
            ActorUserId = UserId,
            EventType = type
        }, ct);
    }

    private Task NotifyAsync(
        Guid? targetUserId,
        string title,
        string body,
        NotificationType type,
        Guid? projectId = null,
        Guid? milestoneId = null,
        CancellationToken ct = default)
    {
        if (targetUserId is null || targetUserId == UserId)
            return Task.CompletedTask;

        return _notificationService.CreateNotification(new CreateNotificationRequest
        {
            UserId = targetUserId,
            Title = title,
            Body = body,
            Type = type,
            ProjectId = projectId,
            MilestoneId = milestoneId,
            ActionUrl = projectId.HasValue ? $"/projects/{projectId}" : null
        });
    }
    

    private async Task<bool> IsValidAssigneeAsync(Project project, Guid assigneeUserId, CancellationToken ct)
    {
        if (project.AssignedUserId == assigneeUserId)
            return true;

        return project.AssignedTeamId is not null &&
               await TeamMemberRepo.IsMemberAsync(project.AssignedTeamId.Value, assigneeUserId, ct);
    }


    private async Task<TaskDto> MapToDtoAsync(Project project, ProjectTask task, CancellationToken ct)
    {
        var dto = _mapper.Map<TaskDto>(task);
        dto.CanManage = await IsTaskManagerAsync(project, ct);
        return dto;
    }
    #endregion



    #region Tasks Commands
    public async Task<ApiResponse<TaskDto>> CreateAsync(Guid milestoneId, CreateTaskDto dto, CancellationToken ct = default)
    {
        new CreateTaskValidator().ValidateAndThrow(dto);

        var milestone = await MilestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await ProjectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Forbidden("Only the team leader can create tasks."));

        if (!milestone.IsFunded)
            return ApiResponse.Failure<TaskDto>(AppError.Validation("Milestone must be funded before tasks can be created."));

        if (dto.AssigneeUserId is not null && !await IsValidAssigneeAsync(project, dto.AssigneeUserId.Value, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Validation("Assignee must be a member of the assigned team."));

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            MilestoneId = milestoneId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Requirements = dto.Requirements?.Trim() ?? string.Empty,
            Priority = dto.Priority,
            Status = Domain.Enums.TaskStatus.Todo,
            AssigneeUserId = dto.AssigneeUserId,
            CreatedByUserId = UserId,
            DueDate = dto.DueDate,
            EstimatedHours = dto.EstimatedHours,
            SpentHours = 0
        };

        await TaskRepo.AddAsync(task, ct);
        await RecordEventAsync(project.Id, milestoneId, EventType.TaskCreated, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        if (dto.AssigneeUserId.HasValue)
        {
            BackgroundJob.Enqueue(() => NotifyAsync(
                dto.AssigneeUserId.Value,
                "New task assigned",
                $"You were assigned a new task: {task.Title}",
                NotificationType.TaskAssigned,
                project.Id,
                milestoneId));
        }

        var dtoResult = await MapToDtoAsync(project, task, ct);
        return ApiResponse.Success(dtoResult, "Task created.");
    }

    public async Task<ApiResponse<TaskDto>> UpdateAsync(Guid taskId, UpdateTaskDto dto, CancellationToken ct = default)
    {
        new UpdateTaskValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Forbidden("Only the team leader can edit tasks."));

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim() ?? task.Description;
        task.Requirements = dto.Requirements?.Trim() ?? task.Requirements;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.EstimatedHours = dto.EstimatedHours;

        TaskRepo.Update(task);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(await MapToDtoAsync(project, task, ct), "Task updated.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid taskId, CancellationToken ct = default)
    {
        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the team leader can delete tasks."));

        TaskRepo.Delete(task);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Task deleted.");
    }

    public async Task<ApiResponse<TaskDto>> ChangeStatusAsync(Guid taskId, ChangeTaskStatusDto dto, CancellationToken ct = default)
    {
        new ChangeTaskStatusValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        var isManager = await IsTaskManagerAsync(project, ct);
        var isAssignee = await IsAssigneeAsync(task, ct);

        if (!isManager && !isAssignee)
            return ApiResponse.Failure<TaskDto>(AppError.Forbidden("You cannot change the status of this task."));

        if (!CanTransit(task.Status, dto.Status, isManager))
            return ApiResponse.Failure<TaskDto>(AppError.Validation(
                $"Invalid status transition from {task.Status} to {dto.Status}."));

        task.Status = dto.Status;
        if (dto.Status == Domain.Enums.TaskStatus.Done)
            task.CompletedAt = DateTime.UtcNow;
        else if (task.Status == Domain.Enums.TaskStatus.Todo)
            task.CompletedAt = null;

        TaskRepo.Update(task);
        await RecordEventAsync(project.Id, task.MilestoneId, EventType.TaskStatusChanged, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // notify the assignee (skip when assignee is the actor)
        var actorIsAssignee = task.AssigneeUserId == UserId;
        if (!actorIsAssignee && task.AssigneeUserId.HasValue)
        {
            BackgroundJob.Enqueue(() => NotifyAsync(
                task.AssigneeUserId.Value,
                "Task status changed",
                $"Task '{task.Title}' is now {dto.Status}.",
                NotificationType.TaskStatusChanged,
                project.Id,
                task.MilestoneId));
        }
        else if (actorIsAssignee)
        {
            // notify the leader that the task awaits review
            var leaderIds = await TeamMemberRepo.GetLeadersAsync(project.AssignedTeamId ?? Guid.Empty, ct);
            foreach (var leader in leaderIds)
            {
                BackgroundJob.Enqueue(() => NotifyAsync(
                    leader.UserId,
                    "Task awaits review",
                    $"Task '{task.Title}' is now {dto.Status}.",
                    NotificationType.TaskStatusChanged,
                    project.Id,
                    task.MilestoneId));
            }
        }

        return ApiResponse.Success(await MapToDtoAsync(project, task, ct), "Status updated.");
    }

    public async Task<ApiResponse<TaskDto>> AssignAsync(Guid taskId, AssignTaskDto dto, CancellationToken ct = default)
    {
        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Forbidden("Only the team leader can assign tasks."));

        if (dto.AssigneeUserId is not null && !await IsValidAssigneeAsync(project, dto.AssigneeUserId.Value, ct))
            return ApiResponse.Failure<TaskDto>(AppError.Validation("Assignee must be a member of the assigned team."));

        task.AssigneeUserId = dto.AssigneeUserId;
        TaskRepo.Update(task);
        await RecordEventAsync(project.Id, task.MilestoneId, EventType.TaskAssigned, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        if (dto.AssigneeUserId.HasValue)
        {
            BackgroundJob.Enqueue(() => NotifyAsync(
                dto.AssigneeUserId.Value,
                "Task assigned",
                $"You were assigned: {task.Title}",
                NotificationType.TaskAssigned,
                project.Id,
                task.MilestoneId));
        }

        return ApiResponse.Success(await MapToDtoAsync(project, task, ct), "Assignee updated.");
    }
    #endregion



    #region Task Comments

    public async Task<ApiResponse<IEnumerable<TaskCommentDto>>> GetCommentsAsync(Guid taskId, CancellationToken ct = default)
    {
        var (project, _) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<TaskCommentDto>>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsProjectParticipantAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<TaskCommentDto>>(AppError.Forbidden("You cannot view this project."));

        var comments = await CommentRepo.GetByTaskIdAsync(taskId, ct);
        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskCommentDto>>(comments));
    }

    public async Task<ApiResponse<TaskCommentDto>> AddCommentAsync(Guid taskId, CreateTaskCommentDto dto, CancellationToken ct = default)
    {
        new CreateTaskCommentValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskCommentDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsProjectParticipantAsync(project, ct))
            return ApiResponse.Failure<TaskCommentDto>(AppError.Forbidden("You cannot comment on this project."));

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = UserId,
            Content = dto.Content.Trim()
        };

        await CommentRepo.AddAsync(comment, ct);
        await RecordEventAsync(project.Id, task.MilestoneId, EventType.TaskCommentAdded, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var user = await UserRepo.GetByIdAsync(UserId, ct);
        
        if (task.AssigneeUserId.HasValue && task.AssigneeUserId != UserId)
        {

            BackgroundJob.Enqueue(() => NotifyAsync(
                task.AssigneeUserId.Value,
                "New comment on your task",
                $"{FullName(user)} commented on '{task.Title}'.",
                NotificationType.TaskCommentAdded,
                project.Id,
                task.MilestoneId));
        }

        return ApiResponse.Success(new TaskCommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = UserId,
            UserName = FullName(user),
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        }, "Comment added.");
    }

    public async Task<ApiResponse> DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await CommentRepo.GetByIdAsync(commentId, ct);
        if (comment is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TaskComment), commentId));

        var task = await GetTaskAsync(comment.TaskId, ct);
        var project = task is null ? null : await ProjectRepo.GetByIdAsync(task.Milestone.ProjectId, ct);

        if (comment.UserId != UserId &&
            (project is null || !await IsTaskManagerAsync(project, ct)))
            return ApiResponse.Failure(AppError.Forbidden("You cannot delete this comment."));

        CommentRepo.Delete(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Comment deleted.");
    }
    #endregion



    #region Checklist Items
    public async Task<ApiResponse<IEnumerable<TaskChecklistItemDto>>> GetChecklistAsync(Guid taskId, CancellationToken ct = default)
    {
        var items = await ChecklistRepo.GetByTaskIdAsync(taskId, ct);
        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskChecklistItemDto>>(items));
    }

    public async Task<ApiResponse<TaskChecklistItemDto>> AddChecklistItemAsync(Guid taskId, CreateChecklistItemDto dto, CancellationToken ct = default)
    {
        new CreateChecklistItemValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskChecklistItemDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct) && !await IsAssigneeAsync(task, ct))
            return ApiResponse.Failure<TaskChecklistItemDto>(AppError.Forbidden("Only the assignee or team leader can add checklist items."));

        var item = new TaskChecklistItem
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Title = dto.Title.Trim()
        };

        await ChecklistRepo.AddAsync(item, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(_mapper.Map<TaskChecklistItemDto>(item), "Checklist item added.");
    }

    public async Task<ApiResponse<TaskChecklistItemDto>> ToggleChecklistItemAsync(Guid itemId, bool isCompleted, CancellationToken ct = default)
    {
        var item = await ChecklistRepo.GetByIdAsync(itemId, ct);
        if (item is null)
            return ApiResponse.Failure<TaskChecklistItemDto>(AppError.NotFound(nameof(TaskChecklistItem), itemId));

        var (project, task) = await LoadProjectAndTaskAsync(item.TaskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskChecklistItemDto>(AppError.NotFound(nameof(ProjectTask), item.TaskId));

        if (!await IsTaskManagerAsync(project, ct) && !await IsAssigneeAsync(task, ct))
            return ApiResponse.Failure<TaskChecklistItemDto>(AppError.Forbidden("Only the assignee or team leader can update checklist items."));

        item.IsCompleted = isCompleted;
        item.CompletedAt = isCompleted ? DateTime.UtcNow : null;
        item.CompletedByUserId = isCompleted ? UserId : null;

        ChecklistRepo.Update(item);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(_mapper.Map<TaskChecklistItemDto>(item), "Checklist item updated.");
    }

    public async Task<ApiResponse> DeleteChecklistItemAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await ChecklistRepo.GetByIdAsync(itemId, ct);
        if (item is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TaskChecklistItem), itemId));

        var (project, task) = await LoadProjectAndTaskAsync(item.TaskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectTask), item.TaskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the team leader can remove checklist items."));

        ChecklistRepo.Delete(item);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Checklist item removed.");
    }
    #endregion



    #region Subtasks
    public async Task<ApiResponse<IEnumerable<TaskSubtaskDto>>> GetSubtasksAsync(Guid taskId, CancellationToken ct = default)
    {
        var subtasks = await SubtaskRepo.GetByTaskIdAsync(taskId, ct);
        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskSubtaskDto>>(subtasks));
    }

    public async Task<ApiResponse<TaskSubtaskDto>> AddSubtaskAsync(Guid taskId, CreateSubtaskDto dto, CancellationToken ct = default)
    {
        new CreateSubtaskValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.Forbidden("Only the team leader can add subtasks."));

        var subtask = new TaskSubtask
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Title = dto.Title.Trim(),
            Status = Domain.Enums.TaskStatus.Todo,
            AssigneeUserId = dto.AssigneeUserId,
            DueDate = dto.DueDate
        };

        await SubtaskRepo.AddAsync(subtask, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(_mapper.Map<TaskSubtaskDto>(subtask), "Subtask added.");
    }

    public async Task<ApiResponse<TaskSubtaskDto>> ChangeSubtaskStatusAsync(Guid subtaskId, ChangeSubtaskStatusDto dto, CancellationToken ct = default)
    {
        var subtask = await SubtaskRepo.GetByIdAsync(subtaskId, ct);
        if (subtask is null)
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.NotFound(nameof(TaskSubtask), subtaskId));

        var (project, task) = await LoadProjectAndTaskAsync(subtask.TaskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.NotFound(nameof(ProjectTask), subtask.TaskId));

        var isManager = await IsTaskManagerAsync(project, ct);
        var isAssignee = await IsAssigneeAsync(task, ct) || subtask.AssigneeUserId == UserId;

        if (!isManager && !isAssignee)
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.Forbidden("You cannot update this subtask."));

        if (!CanTransit(subtask.Status, dto.Status, isManager))
            return ApiResponse.Failure<TaskSubtaskDto>(AppError.Validation("Invalid subtask status transition."));

        subtask.Status = dto.Status;
        SubtaskRepo.Update(subtask);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(_mapper.Map<TaskSubtaskDto>(subtask), "Subtask updated.");
    }

    public async Task<ApiResponse> DeleteSubtaskAsync(Guid subtaskId, CancellationToken ct = default)
    {
        var subtask = await SubtaskRepo.GetByIdAsync(subtaskId, ct);
        if (subtask is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TaskSubtask), subtaskId));

        var (project, task) = await LoadProjectAndTaskAsync(subtask.TaskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectTask), subtask.TaskId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the team leader can remove subtasks."));

        SubtaskRepo.Delete(subtask);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Subtask removed.");
    }
    #endregion



    #region Time Logs
    public async Task<ApiResponse<IEnumerable<TaskTimeLogDto>>> GetTimeLogsAsync(Guid taskId, CancellationToken ct = default)
    {
        var logs = await TimeLogRepo.GetByTaskIdAsync(taskId, ct);
        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskTimeLogDto>>(logs));
    }

    public async Task<ApiResponse<TaskTimeLogDto>> LogTimeAsync(Guid taskId, LogTimeDto dto, CancellationToken ct = default)
    {
        new LogTimeValidator().ValidateAndThrow(dto);

        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<TaskTimeLogDto>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct) && !await IsAssigneeAsync(task, ct))
            return ApiResponse.Failure<TaskTimeLogDto>(AppError.Forbidden("Only the assignee or team leader can log time."));

        var log = new TaskTimeLog
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = UserId,
            Hours = dto.Hours,
            Note = dto.Note?.Trim(),
            WorkDate = dto.WorkDate ?? DateTime.UtcNow
        };

        task.SpentHours += dto.Hours;
        TaskRepo.Update(task);

        await TimeLogRepo.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var user = await UserRepo.GetByIdAsync(UserId, ct);
        return ApiResponse.Success(new TaskTimeLogDto
        {
            Id = log.Id,
            TaskId = log.TaskId,
            UserId = UserId,
            UserName = FullName(user),
            Hours = log.Hours,
            Note = log.Note,
            WorkDate = log.WorkDate
        }, "Time logged.");
    }
    #endregion



    #region Task Attachments

    public async Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> GetAttachmentsAsync(Guid taskId, CancellationToken ct = default)
    {
        var attachments = await AttachmentRepo.GetByTaskIdAsync(taskId, ct);
        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments));
    }

    public async Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> UploadAttachmentsAsync(
        Guid taskId, UploadTaskAttachmentsDto dto, CancellationToken ct = default)
    {
        var (project, task) = await LoadProjectAndTaskAsync(taskId, ct);
        if (project is null || task is null)
            return ApiResponse.Failure<IEnumerable<TaskAttachmentDto>>(AppError.NotFound(nameof(ProjectTask), taskId));

        if (!await IsTaskManagerAsync(project, ct) && !await IsAssigneeAsync(task, ct))
            return ApiResponse.Failure<IEnumerable<TaskAttachmentDto>>(AppError.Forbidden("Only the assignee or team leader can attach files."));

        if (dto.Files is null || dto.Files.Count == 0)
            return ApiResponse.Failure<IEnumerable<TaskAttachmentDto>>(AppError.Validation("Please select at least one file."));

        var created = new List<TaskAttachment>();

        foreach (var file in dto.Files)
        {
            UploadedAsset uploaded;
            try
            {
                uploaded = await _storageService.UploadAsync(file, StorageFolders.TaskFiles, ct);
            }
            catch (Exception)
            {
                return ApiResponse.Failure<IEnumerable<TaskAttachmentDto>>(AppError.FileUploadFailed(file.FileName));
            }

            var attachment = new TaskAttachment
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                UploadedByUserId = UserId,
                FileName = string.IsNullOrWhiteSpace(uploaded.FileName) ? file.FileName : uploaded.FileName,
                FileUrl = uploaded.Url
            };

            await AttachmentRepo.AddAsync(attachment, ct);
            created.Add(attachment);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(_mapper.Map<IEnumerable<TaskAttachmentDto>>(created), "Files uploaded.");
    }

    public async Task<ApiResponse> DeleteAttachmentAsync(Guid attachmentId, CancellationToken ct = default)
    {
        var attachment = await AttachmentRepo.GetByIdAsync(attachmentId, ct);
        if (attachment is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TaskAttachment), attachmentId));

        var task = await GetTaskAsync(attachment.TaskId, ct);
        var project = task is null ? null : await ProjectRepo.GetByIdAsync(task.Milestone.ProjectId, ct);

        if (attachment.UploadedByUserId != UserId &&
            (project is null || !await IsTaskManagerAsync(project, ct)))
            return ApiResponse.Failure(AppError.Forbidden("You cannot delete this attachment."));

        AttachmentRepo.Delete(attachment);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Attachment removed.");
    }
    #endregion



    #region Assignee Options
    public async Task<ApiResponse<IEnumerable<TaskAssigneeDto>>> GetAssigneesAsync(Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await MilestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<IEnumerable<TaskAssigneeDto>>(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await ProjectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<TaskAssigneeDto>>(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await IsTaskManagerAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<TaskAssigneeDto>>(AppError.Forbidden("Only the team leader can view assignee options."));

        var assignees = new List<TaskAssigneeDto>();

        if (project.AssignedTeamId.HasValue)
        {
            var members = await TeamMemberRepo.GetByTeamIdWithUserAsync(project.AssignedTeamId.Value, ct);
            assignees = members.Select(m => new TaskAssigneeDto
            {
                UserId = m.UserId,
                Name = FullName(m.User),
                ImageUrl = m.User?.DeveloperProfile?.ProfileImage,
                Job = m.Job
            }).ToList();
        }
        else if (project.AssignedUserId.HasValue)
        {
            var user = await UserRepo.GetByIdAsync(project.AssignedUserId.Value, ct);
            assignees.Add(new TaskAssigneeDto
            {
                UserId = project.AssignedUserId.Value,
                Name = FullName(user),
                ImageUrl = user?.DeveloperProfile?.ProfileImage
            });
        }

        return ApiResponse.Success<IEnumerable<TaskAssigneeDto>>(assignees);
    }
    #endregion


}
