
namespace FreeGency.Api.Controllers.V1;


[Route("api/v1")]
[Authorize]
public class TasksController(ITaskService taskService) : BaseApiController
{
    // Tasks
    [HttpGet("milestones/{milestoneId:guid}/tasks")]
    public async Task<IActionResult> GetByMilestone([FromRoute] Guid milestoneId, CancellationToken ct)
        => HandleResult(await taskService.GetByMilestoneAsync(milestoneId, ct));

    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> GetByProject([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await taskService.GetByProjectAsync(projectId, ct));

    [HttpGet("tasks/mine")]
    public async Task<IActionResult> GetMine([FromQuery] Guid? teamId, CancellationToken ct)
        => HandleResult(await taskService.GetMyTasksAsync(teamId, ct));

    [HttpGet("tasks/{taskId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetByIdAsync(taskId, ct));

    [HttpPost("milestones/{milestoneId:guid}/tasks")]
    public async Task<IActionResult> Create(
        [FromRoute] Guid milestoneId,
        [FromBody] CreateTaskDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.CreateAsync(milestoneId, dto, ct));

    [HttpPut("tasks/{taskId:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid taskId,
        [FromBody] UpdateTaskDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.UpdateAsync(taskId, dto, ct));

    [HttpDelete("tasks/{taskId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.DeleteAsync(taskId, ct));

    [HttpPatch("tasks/{taskId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid taskId,
        [FromBody] ChangeTaskStatusDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.ChangeStatusAsync(taskId, dto, ct));

    [HttpPatch("tasks/{taskId:guid}/assignee")]
    public async Task<IActionResult> Assign(
        [FromRoute] Guid taskId,
        [FromBody] AssignTaskDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.AssignAsync(taskId, dto, ct));

    [HttpGet("tasks/{taskId:guid}/assignees")]
    public async Task<IActionResult> GetAssignees([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetAssigneesAsync(taskId, ct));




    // Comments
    [HttpGet("tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> GetComments([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetCommentsAsync(taskId, ct));

    [HttpPost("tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> AddComment(
        [FromRoute] Guid taskId,
        [FromBody] CreateTaskCommentDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.AddCommentAsync(taskId, dto, ct));

    [HttpDelete("tasks/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment([FromRoute] Guid commentId, CancellationToken ct)
        => HandleResult(await taskService.DeleteCommentAsync(commentId, ct));



    // Checklist
    [HttpGet("tasks/{taskId:guid}/checklist")]
    public async Task<IActionResult> GetChecklist([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetChecklistAsync(taskId, ct));

    [HttpPost("tasks/{taskId:guid}/checklist")]
    public async Task<IActionResult> AddChecklistItem(
        [FromRoute] Guid taskId,
        [FromBody] CreateChecklistItemDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.AddChecklistItemAsync(taskId, dto, ct));

    [HttpPatch("tasks/checklist/{itemId:guid}/toggle")]
    public async Task<IActionResult> ToggleChecklistItem(
        [FromRoute] Guid itemId,
        [FromBody] ToggleChecklistItemDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.ToggleChecklistItemAsync(itemId, dto.IsCompleted, ct));

    [HttpDelete("tasks/checklist/{itemId:guid}")]
    public async Task<IActionResult> DeleteChecklistItem([FromRoute] Guid itemId, CancellationToken ct)
        => HandleResult(await taskService.DeleteChecklistItemAsync(itemId, ct));



    // Subtasks
    [HttpGet("tasks/{taskId:guid}/subtasks")]
    public async Task<IActionResult> GetSubtasks([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetSubtasksAsync(taskId, ct));

    [HttpPost("tasks/{taskId:guid}/subtasks")]
    public async Task<IActionResult> AddSubtask(
        [FromRoute] Guid taskId,
        [FromBody] CreateSubtaskDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.AddSubtaskAsync(taskId, dto, ct));

    [HttpPatch("tasks/subtasks/{subtaskId:guid}/status")]
    public async Task<IActionResult> ChangeSubtaskStatus(
        [FromRoute] Guid subtaskId,
        [FromBody] ChangeSubtaskStatusDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.ChangeSubtaskStatusAsync(subtaskId, dto, ct));

    [HttpDelete("tasks/subtasks/{subtaskId:guid}")]
    public async Task<IActionResult> DeleteSubtask([FromRoute] Guid subtaskId, CancellationToken ct)
        => HandleResult(await taskService.DeleteSubtaskAsync(subtaskId, ct));



    // Time logs
    [HttpGet("tasks/{taskId:guid}/time-logs")]
    public async Task<IActionResult> GetTimeLogs([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetTimeLogsAsync(taskId, ct));

    [HttpPost("tasks/{taskId:guid}/time-log")]
    public async Task<IActionResult> LogTime(
        [FromRoute] Guid taskId,
        [FromBody] LogTimeDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.LogTimeAsync(taskId, dto, ct));



    // Attachments
    [HttpGet("tasks/{taskId:guid}/attachments")]
    public async Task<IActionResult> GetAttachments([FromRoute] Guid taskId, CancellationToken ct)
        => HandleResult(await taskService.GetAttachmentsAsync(taskId, ct));

    [HttpPost("tasks/{taskId:guid}/attachments")]
    public async Task<IActionResult> UploadAttachments(
        [FromRoute] Guid taskId,
        [FromForm] UploadTaskAttachmentsDto dto,
        CancellationToken ct)
        => HandleResult(await taskService.UploadAttachmentsAsync(taskId, dto, ct));

    [HttpDelete("tasks/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment([FromRoute] Guid attachmentId, CancellationToken ct)
        => HandleResult(await taskService.DeleteAttachmentAsync(attachmentId, ct));
}