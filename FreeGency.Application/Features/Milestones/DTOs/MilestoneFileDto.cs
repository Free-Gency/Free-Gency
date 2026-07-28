
namespace FreeGency.Application.Features.Milestones.DTOs;

public sealed class MilestoneFileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string FileKind { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}