using FreeGency.Application.Features.ProjectFiles.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface IProjectFileService
{
    Task<ApiResponse<IEnumerable<ProjectFileDto>>> ListByProjectAsync(
        Guid projectId,
        FileKind? kind = null,
        CancellationToken ct = default);

    Task<ApiResponse<IEnumerable<ProjectFileDto>>> UploadAsync(
        Guid projectId,
        UploadProjectFilesRequestDto request,
        CancellationToken ct = default);

    Task<ApiResponse> DeleteAsync(Guid fileId, CancellationToken ct = default);
}
