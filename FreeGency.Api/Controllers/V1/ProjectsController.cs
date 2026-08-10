namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/projects")]
    public class ProjectsController(
        IProjectService _projectService,
        IProposalRankingService _proposalRankingService,
        IProposalAssistantService _proposalAssistantService
    ) : BaseApiController
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
        public async Task<IActionResult> ReplaceSkills([FromRoute] Guid id, [FromBody] IEnumerable<Guid> skillsIds, CancellationToken ct)
            => HandleResult(await _projectService.ReplaceSkillsAsync(id, skillsIds, ct));

        [Authorize]
        [HttpPost("{id}/proposal-assistant")]
        public async Task<IActionResult> AskProposalAssistant(
            [FromRoute] Guid id,
            [FromBody] FreeGency.Application.Features.ProposalAssistant.Dtos.ProposalAssistantRequestDto request,
            CancellationToken ct)
            => HandleResult(await _proposalAssistantService.AskAsync(id, request, ct));
        #endregion


        #region Queries
        [HttpGet]
        public async Task<IActionResult> Browse([FromQuery] FilterProjectsRequestDto request, CancellationToken ct)
            => HandleResult(await _projectService.BrowseAsync(request, ct));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _projectService.GetDetailsAsync(id, ct));

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyProjects([FromQuery] MyProjectsRequestDto request, CancellationToken ct)
            => HandleResult(await _projectService.GetMyProjectsAsync(request, ct));

        [Authorize]
        [HttpGet("mine/summary")]
        public async Task<IActionResult> GetMyProjectsSummary(
            [FromQuery] string role,
            [FromQuery] Guid? teamId,
            CancellationToken ct)
            => HandleResult(await _projectService.GetMyProjectsSummaryAsync(role, teamId, ct));

        [Authorize]
        [HttpGet("saved")]
        public async Task<IActionResult> GetMySavedProjects(CancellationToken ct)
            => HandleResult(await _projectService.GetSavedProjectsAsync(ct));

        [Authorize]
        [HttpGet("{id}/proposal-ranking")]
        public async Task<IActionResult> GetProposalRanking(
            [FromRoute] Guid id,
            [FromQuery] int topK = 50,
            CancellationToken ct = default)
            => HandleResult(await _proposalRankingService.RankAsync(id, topK, ct));


        #endregion
    }
}
