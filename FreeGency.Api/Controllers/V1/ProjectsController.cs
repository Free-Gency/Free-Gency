namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/projects")]
    public class ProjectsController(IProjectService _projectService) : BaseApiController
    {
        #region Commands
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectRequestDto request, CancellationToken ct)
            => HandleResult(await _projectService.CreateAsync(request, ct));

        [Authorize]
        [HttpPost("{id}/save")]
        public async Task<IActionResult> Save([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _projectService.SaveAsync(id, ct));

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _projectService.DeleteAsync(id, ct));

        [Authorize]
        [HttpDelete("{id}/save")]
        public async Task<IActionResult> UnSave([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _projectService.UnSaveAsync(id, ct));

        [Authorize]
        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> Publish([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _projectService.PublishAsync(id, ct));

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(UpdateProjectRequestDto request, CancellationToken ct)
            => HandleResult(await _projectService.EditAsync(request, ct));

        [Authorize]
        [HttpPut("{id}/skills")]
        public async Task<IActionResult> ReplaceSkills([FromQuery] Guid id, IEnumerable<Guid> skillsIds, CancellationToken ct)
            => HandleResult(await _projectService.ReplaceSkillsAsync(id, skillsIds, ct));
        #endregion


        #region Queries

        #endregion
    }
}
