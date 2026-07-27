namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/profiles")]
    public class PortfoliosController(IPortfolioService _portfolioService)
        : BaseApiController
    {
        #region Developer Portfolio

        [Authorize]
        [HttpPost("developer/me/portfolio-projects")]
        public async Task<IActionResult> Create(
            [FromForm] CreatePortfolioProjectRequestDto request,
            CancellationToken ct)
            => HandleResult(await _portfolioService.CreateAsync(request, ct));

        [Authorize]
        [HttpPut("developer/me/portfolio-projects/{id:guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromForm] UpdatePortfolioProjectRequestDto request,
            CancellationToken ct)
            => HandleResult(await _portfolioService.UpdateAsync(id, request, ct));

        [Authorize]
        [HttpDelete("developer/me/portfolio-projects/{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken ct)
            => HandleResult(await _portfolioService.DeleteAsync(id, ct));

        [Authorize]
        [HttpGet("developer/me/portfolio-projects")]
        public async Task<IActionResult> GetMine(CancellationToken ct)
            => HandleResult(await _portfolioService.GetMineAsync(ct));

        [HttpGet("developers/{developerId:guid}/portfolio-projects")]
        public async Task<IActionResult> GetDeveloperPortfolio(
            [FromRoute] Guid developerId,
            CancellationToken ct)
            => HandleResult(await _portfolioService.GetDeveloperPortfolioAsync(developerId, ct));

        /// <summary>Public Inspiration feed for client home.</summary>
        [HttpGet("portfolio-projects/inspiration")]
        public async Task<IActionResult> GetInspiration(
            [FromQuery] FilterInspirationRequestDto request,
            CancellationToken ct = default)
            => HandleResult(await _portfolioService.GetInspirationAsync(request, ct));

        /// <summary>Public portfolio details for Inspiration / client browse.</summary>
        [HttpGet("portfolio-projects/{id:guid}")]
        public async Task<IActionResult> GetPublicDetails(
            [FromRoute] Guid id,
            CancellationToken ct)
            => HandleResult(await _portfolioService.GetPublicDetailsAsync(id, ct));

        /// <summary>Record that the current user viewed an inspiration portfolio item.</summary>
        [Authorize]
        [HttpPost("portfolio-projects/{id:guid}/view")]
        public async Task<IActionResult> RecordView(
            [FromRoute] Guid id,
            CancellationToken ct)
            => HandleResult(await _portfolioService.RecordViewAsync(id, ct));

        /// <summary>Leave feedback on a public portfolio project.</summary>
        [Authorize]
        [HttpPost("portfolio-projects/{id:guid}/feedback")]
        public async Task<IActionResult> AddFeedback(
            [FromRoute] Guid id,
            [FromBody] CreatePortfolioFeedbackRequestDto request,
            CancellationToken ct)
            => HandleResult(await _portfolioService.AddFeedbackAsync(id, request, ct));

        /// <summary>Recently viewed inspiration items for the current user.</summary>
        [Authorize]
        [HttpGet("me/portfolio-projects/recently-viewed")]
        public async Task<IActionResult> GetRecentlyViewed(
            [FromQuery] int take = 5,
            CancellationToken ct = default)
            => HandleResult(await _portfolioService.GetRecentlyViewedAsync(take, ct));

        [Authorize]
        [HttpGet("developer/me/portfolio-projects/{id:guid}")]
        public async Task<IActionResult> GetDetails(
            [FromRoute] Guid id,
            CancellationToken ct)
            => HandleResult(await _portfolioService.GetDetailsAsync(id, ct));

        [Authorize]
        [HttpPut("developer/me/portfolio-projects/{id:guid}/skills")]
        public async Task<IActionResult> ReplaceSkills(
            [FromRoute] Guid id,
            [FromBody] IEnumerable<Guid> skillIds,
            CancellationToken ct)
            => HandleResult(await _portfolioService.ReplaceSkillsAsync(id, skillIds, ct));

        [Authorize]
        [HttpPost("developer/me/portfolio-projects/{id:guid}/images")]
        public async Task<IActionResult> UploadImages(
            [FromRoute] Guid id,
            [FromForm] IEnumerable<IFormFile> images,
            CancellationToken ct)
            => HandleResult(await _portfolioService.UploadImagesAsync(id, images, ct));

        [Authorize]
        [HttpDelete("developer/me/portfolio-projects/{portfolioId:guid}/images/{imageId:guid}")]
        public async Task<IActionResult> DeleteImage(
            [FromRoute] Guid portfolioId,
            [FromRoute] Guid imageId,
            CancellationToken ct)
            => HandleResult(await _portfolioService.DeleteImageAsync(portfolioId, imageId, ct));

        #endregion


        #region Team Portfolio

        [Authorize]
        [HttpPost("teams/{teamId:guid}/portfolio-projects")]
        public async Task<IActionResult> CreateForTeam(
            [FromRoute] Guid teamId,
            [FromForm] CreatePortfolioProjectRequestDto request,
            CancellationToken ct)
            => HandleResult(await _portfolioService.CreateForTeamAsync(teamId, request, ct));

        [Authorize]
        [HttpPut("teams/{teamId:guid}/portfolio-projects/{id:guid}")]
        public async Task<IActionResult> UpdateForTeam(
            [FromRoute] Guid teamId,
            [FromRoute] Guid id,
            [FromForm] UpdatePortfolioProjectRequestDto request,
            CancellationToken ct)
            => HandleResult(await _portfolioService.UpdateForTeamAsync(teamId, id, request, ct));

        [Authorize]
        [HttpDelete("teams/{teamId:guid}/portfolio-projects/{id:guid}")]
        public async Task<IActionResult> DeleteForTeam(
            [FromRoute] Guid teamId,
            [FromRoute] Guid id,
            CancellationToken ct)
            => HandleResult(await _portfolioService.DeleteForTeamAsync(teamId, id, ct));

        [HttpGet("teams/{teamId:guid}/portfolio-projects")]
        public async Task<IActionResult> GetTeamPortfolio(
            [FromRoute] Guid teamId,
            CancellationToken ct)
            => HandleResult(await _portfolioService.GetTeamPortfolioAsync(teamId, ct));

        [Authorize]
        [HttpPut("teams/{teamId:guid}/portfolio-projects/{id:guid}/skills")]
        public async Task<IActionResult> ReplaceTeamSkills(
            [FromRoute] Guid teamId,
            [FromRoute] Guid id,
            [FromBody] IEnumerable<Guid> skillIds,
            CancellationToken ct)
            => HandleResult(await _portfolioService.ReplaceTeamSkillsAsync(teamId, id, skillIds, ct));

        [Authorize]
        [HttpPost("teams/{teamId:guid}/portfolio-projects/{id:guid}/images")]
        public async Task<IActionResult> UploadTeamImages(
            [FromRoute] Guid teamId,
            [FromRoute] Guid id,
            [FromForm] IEnumerable<IFormFile> images,
            CancellationToken ct)
            => HandleResult(await _portfolioService.UploadTeamImagesAsync(teamId, id, images, ct));

        [Authorize]
        [HttpDelete("teams/{teamId:guid}/portfolio-projects/images/{imageId:guid}")]
        public async Task<IActionResult> DeleteTeamImage(
            [FromRoute] Guid teamId,
            [FromRoute] Guid imageId,
            CancellationToken ct)
            => HandleResult(await _portfolioService.DeleteTeamImageAsync(teamId, imageId, ct));

        #endregion
    }
}
