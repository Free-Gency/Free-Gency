
namespace FreeGency.Application.Common.Interfaces;


public interface ITaskService
{
    Task<ApiResponse<IEnumerable<TaskDto>>> GetByMilestoneAsync(Guid milestoneId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetMyTasksAsync(Guid? teamId = null, CancellationToken ct = default);
    Task<ApiResponse<TaskDto>> GetByIdAsync(Guid taskId, CancellationToken ct = default);

    Task<ApiResponse<TaskDto>> CreateAsync(Guid milestoneId, CreateTaskDto dto, CancellationToken ct = default);
    Task<ApiResponse<TaskDto>> UpdateAsync(Guid taskId, UpdateTaskDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<TaskDto>> ChangeStatusAsync(Guid taskId, ChangeTaskStatusDto dto, CancellationToken ct = default);
    Task<ApiResponse<TaskDto>> AssignAsync(Guid taskId, AssignTaskDto dto, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskCommentDto>>> GetCommentsAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<TaskCommentDto>> AddCommentAsync(Guid taskId, CreateTaskCommentDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteCommentAsync(Guid commentId, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskChecklistItemDto>>> GetChecklistAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<TaskChecklistItemDto>> AddChecklistItemAsync(Guid taskId, CreateChecklistItemDto dto, CancellationToken ct = default);
    Task<ApiResponse<TaskChecklistItemDto>> ToggleChecklistItemAsync(Guid itemId, bool isCompleted, CancellationToken ct = default);
    Task<ApiResponse> DeleteChecklistItemAsync(Guid itemId, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskSubtaskDto>>> GetSubtasksAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<TaskSubtaskDto>> AddSubtaskAsync(Guid taskId, CreateSubtaskDto dto, CancellationToken ct = default);
    Task<ApiResponse<TaskSubtaskDto>> ChangeSubtaskStatusAsync(Guid subtaskId, ChangeSubtaskStatusDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteSubtaskAsync(Guid subtaskId, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskTimeLogDto>>> GetTimeLogsAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<TaskTimeLogDto>> LogTimeAsync(Guid taskId, LogTimeDto dto, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> GetAttachmentsAsync(Guid taskId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> UploadAttachmentsAsync(Guid taskId, UploadTaskAttachmentsDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteAttachmentAsync(Guid attachmentId, CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<TaskAssigneeDto>>> GetAssigneesAsync(Guid milestoneId, CancellationToken ct = default);
}
