namespace FreeGency.Application.Features.ProjectFiles.DTOs;

public sealed class ProjectFileDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? MilestoneId { get; init; }
    public Guid UploadedByUserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public FileKind FileKind { get; init; }
    public DateTime CreatedAt { get; init; }
}
