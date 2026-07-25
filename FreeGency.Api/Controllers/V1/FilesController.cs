using FreeGency.Application.Features.ProjectFiles.DTOs;
using FreeGency.Domain.Enums;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[Route("api/v1")]
public class FilesController(IProjectFileService projectFileService) : BaseApiController
{
    [HttpGet("projects/{projectId:guid}/files")]
    public async Task<IActionResult> List(
        [FromRoute] Guid projectId,
        [FromQuery] FileKind? kind,
        CancellationToken ct)
        => HandleResult(await projectFileService.ListByProjectAsync(projectId, kind, ct));

    [HttpPost("projects/{projectId:guid}/files")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromRoute] Guid projectId,
        [FromForm] UploadProjectFilesRequestDto request,
        CancellationToken ct)
        => HandleResult(await projectFileService.UploadAsync(projectId, request, ct));

    [HttpDelete("files/{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await projectFileService.DeleteAsync(id, ct));
}
