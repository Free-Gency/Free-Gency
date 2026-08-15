using FreeGency.Api.Extensions;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[Route("api/v1/profiles")]
public class ProfileController(
    IAccountService accountService,
    IUserProfile _userProfileService,
    ITeamService _teamService) : BaseApiController
{
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
    {
        var result = await accountService.ChangePasswordAsync(dto);

        return result.IsSuccess
            ? Ok(result)
            : result.ToProblem();
    }

    [HttpGet("client/me")]
    public async Task<IActionResult> GetClientProfile()
    {
        var result = await accountService.GetClientProfile();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("client/me/interests")]
    public async Task<IActionResult> GetClientInterests()
    {
        var result = await accountService.GetClientInterests();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("client/me")]
    public async Task<IActionResult> UpdateClientProfile([FromForm] UpdateClientAccountDto dto)
    {
        var result = await accountService.UpdateClientProfileAsync(dto);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("client")]
    public async Task<IActionResult> CreateClientProfile()
    {
        var result = await accountService.CreateProfileClientAsync();
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("developer")]
    public async Task<IActionResult> CreateDeveloperProfile()
    {
        var result = await accountService.CreateProfileDeveloperAsync();
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding()
    {
        var result = await accountService.CompleteOnboardingAsync();
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpGet("modes")]
    public async Task<IActionResult> GetProfileModes()
    {
        var result = await accountService.GetProfileModesAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("switch-profile")]
    public async Task<IActionResult> SwitchProfile([FromQuery] string? targetMode = null)
    {
        var result = await accountService.SwitchModeAsync(targetMode);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("developer/me")]
    public async Task<IActionResult> GetDeveloperProfile()
    {
        var result = await accountService.GetDeveloperProfile();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Browse developers for hire talent discovery.</summary>
    [HttpGet("developers")]
    [AllowAnonymous]
    public async Task<IActionResult> BrowseDevelopers(
        [FromQuery] FilterDevelopersRequestDto filter,
        CancellationToken ct)
    {
        var result = await accountService.BrowseDevelopersAsync(filter, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Public developer portfolio profile by user id.</summary>
    [HttpGet("developers/{userId:guid}")]
    public async Task<IActionResult> GetDeveloperPublicProfile(Guid userId)
    {
        var result = await accountService.GetDeveloperPublicProfileAsync(userId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Marketplace + community reviews for a developer (public portfolio).</summary>
    [HttpGet("developers/{userId:guid}/reviews")]
    public async Task<IActionResult> GetDeveloperReviews(Guid userId, CancellationToken ct)
    {
        var result = await accountService.GetDeveloperReviewsAsync(userId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("developers/{userId:guid}/reviews")]
    public async Task<IActionResult> AddDeveloperReview(
        Guid userId,
        [FromBody] CreateDeveloperReviewRequestDto dto,
        CancellationToken ct)
    {
        var result = await accountService.AddDeveloperReviewAsync(userId, dto, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("developer/me/reviews")]
    public async Task<IActionResult> GetMyDeveloperReviews(CancellationToken ct)
    {
        var me = await accountService.GetDeveloperProfile();
        if (!me.IsSuccess)
            return me.ToProblem();

        var result = await accountService.GetDeveloperReviewsAsync(me.Value.UserId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("developer/me")]
    public async Task<IActionResult> UpdateDeveloperProfile([FromForm] UpdateDeveloperAccountDto dto)
    {
        var result = await accountService.UpdateDeveloperProfileAsync(dto);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("client/me/interests")]
    public async Task<IActionResult> AddClientInterests([FromBody] ProfileInterestsDto dto, CancellationToken ct)
        => HandleResult(await accountService.AddClientInterestsAsync(dto, ct));

    [HttpPut("client/me/interests")]
    public async Task<IActionResult> ReplaceClientInterests([FromBody] ProfileInterestsDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceClientInterestsAsync(dto, ct));

    [HttpPost("developer/me/interests")]
    public async Task<IActionResult> AddDeveloperInterests([FromBody] ProfileInterestsDto dto, CancellationToken ct)
        => HandleResult(await accountService.AddDeveloperInterestsAsync(dto, ct));

    [HttpPut("developer/me/interests")]
    public async Task<IActionResult> ReplaceDeveloperInterests([FromBody] ProfileInterestsDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceDeveloperInterestsAsync(dto, ct));

    [HttpPost("client/me/specialties")]
    public async Task<IActionResult> AddClientSpecialties([FromBody] ProfileSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await accountService.AddClientSpecialtiesAsync(dto, ct));

    [HttpPut("client/me/specialties")]
    public async Task<IActionResult> ReplaceClientSpecialties([FromBody] ProfileSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceClientSpecialtiesAsync(dto, ct));

    [HttpPost("developer/me/specialties")]
    public async Task<IActionResult> AddDeveloperSpecialties([FromBody] ProfileSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await accountService.AddDeveloperSpecialtiesAsync(dto, ct));

    [HttpPut("developer/me/specialties")]
    public async Task<IActionResult> ReplaceDeveloperSpecialties([FromBody] ProfileSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceDeveloperSpecialtiesAsync(dto, ct));

    [HttpPut("client/me/skills")]
    public async Task<IActionResult> ReplaceClientSkills([FromBody] ProfileSkillsDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceClientSkillsAsync(dto, ct));

    [HttpPut("developer/me/skills")]
    public async Task<IActionResult> ReplaceDeveloperSkills([FromBody] ProfileSkillsDto dto, CancellationToken ct)
        => HandleResult(await accountService.ReplaceDeveloperSkillsAsync(dto, ct));





    [HttpGet("client/{id:guid}")]
    public async Task<IActionResult> GetClientProfile([FromRoute] Guid id, CancellationToken ct)
            => HandleResult(await _userProfileService.GetClientProfileAsync(id, ct));


    [HttpGet("team/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => HandleResult(await _teamService.GetByIdAsync(id, ct));

}
