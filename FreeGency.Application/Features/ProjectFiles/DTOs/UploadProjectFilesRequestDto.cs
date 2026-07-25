namespace FreeGency.Application.Features.ProjectFiles.DTOs;

public sealed class UploadProjectFilesRequestDto
{
    public IFormFile[] Files { get; set; } = [];
    public FileKind FileKind { get; set; } = FileKind.Brief;
    public Guid? MilestoneId { get; set; }
}
