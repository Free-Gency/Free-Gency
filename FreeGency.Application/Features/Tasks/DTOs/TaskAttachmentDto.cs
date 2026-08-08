
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskAttachmentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


public class UploadTaskAttachmentsDto
{
    public List<IFormFile> Files { get; set; } = [];
}
