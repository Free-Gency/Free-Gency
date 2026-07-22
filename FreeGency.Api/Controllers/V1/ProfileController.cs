using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[Route("api/v1/profiles")]
public class ProfileController(IAccountService accountService) : BaseApiController
{
    [HttpGet("client/me")]
    public async Task<IActionResult> GetClientProfile()
    {
        var result = await accountService.GetClientProfile();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("client/me")]
    public async Task<IActionResult> UpdateClientProfile(UpdateClientAccountDto dto)
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

    [HttpPost("Developer")]
    public async Task<IActionResult> CreateDeveloperProfile()
    {
        var result = await accountService.CreateProfileDeveloperAsync();
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("switch-profile")]
    public async Task<IActionResult> SwitchProfile()
    {
        var result = await accountService.SwitchModeAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("Developer/me")]
    public async Task<IActionResult> GetDeveloperProfile()
    {
        var result = await accountService.GetDeveloperProfile();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
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
}
