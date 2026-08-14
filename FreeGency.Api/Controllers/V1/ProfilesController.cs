namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/profiles")]
    public class ProfilesController(IUserProfile _userProfileService, ITeamService _teamService)
        : BaseApiController
    {
        [HttpGet("client/{id:guid}")]
        public async Task<IActionResult> GetClientProfile([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _userProfileService.GetClientProfileAsync(id, ct));


        [HttpGet("developer/{id:guid}")]
        public async Task<IActionResult> GetDeveloperProfile([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _userProfileService.GetDeveloperProfileAsync(id, ct));


        [HttpGet("team/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
                => HandleResult(await _teamService.GetByIdAsync(id, ct));
    }
}
