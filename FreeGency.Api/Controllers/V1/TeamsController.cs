using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Teams.Dtos;
using FreeGency.Application.Features.Teams.DTOs;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/teams")]
public class TeamsController(ITeamService _teamService) : BaseApiController
{
    [HttpGet("wallet/{teamId}")]
    public async Task<IActionResult> GetWalletTeam([FromRoute]Guid teamId)
    {
        var result = await _teamService.GetTeamWallet(teamId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpGet("Project-Team")]
    [Authorize]
    public async Task<IActionResult> GetTeamProject([FromQuery]TeamProjectsFilter teamProjectsFilter)
    {
        var result = await _teamService.GetTeamProjectEarnings(teamProjectsFilter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] CreateTeamDto dto, CancellationToken ct)
        => HandleResult(await _teamService.CreateAsync(dto, ct));

    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterTeamsRequestDto filter, CancellationToken ct)
        => HandleResult(await _teamService.BrowseAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _teamService.GetByIdAsync(id, ct));

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => HandleResult(await _teamService.GetMineAsync(ct));

    [HttpGet("by-code/{teamCode}")]
    public async Task<IActionResult> GetByTeamCode(string teamCode, CancellationToken ct)
        => HandleResult(await _teamService.GetByTeamCodeAsync(teamCode, ct));

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTeamDto dto, CancellationToken ct)
    {
        dto.Id = id;
        return HandleResult(await _teamService.UpdateAsync(dto, ct));
    }

    [HttpPut("{id:guid}/categories")]
    [Authorize]
    public async Task<IActionResult> ReplaceCategories(Guid id, [FromBody] UpdateTeamCategoriesDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceCategoriesAsync(id, dto, ct));

    [HttpPut("{id:guid}/specialties")]
    [Authorize]
    public async Task<IActionResult> ReplaceSpecialties(Guid id, [FromBody] UpdateTeamSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceSpecialtiesAsync(id, dto, ct));

    [HttpPut("{id:guid}/skills")]
    [Authorize]
    public async Task<IActionResult> ReplaceSkills(Guid id, [FromBody] UpdateTeamSkillsDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceSkillsAsync(id, dto, ct));

    [HttpGet("{id:guid}/members")]
    [Authorize]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
        => HandleResult(await _teamService.GetMembersAsync(id, ct));

    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    [Authorize]
    public async Task<IActionResult> UpdateMemberRole(
        Guid id,
        Guid userId,
        [FromBody] UpdateTeamMemberRoleDto dto,
        CancellationToken ct)
        => HandleResult(await _teamService.UpdateMemberRoleAsync(id, userId, dto, ct));

    [HttpPost("{id:guid}/chat-groups")]
    [Authorize]
    public async Task<IActionResult> CreateChatGroup(
        Guid id,
        [FromBody] CreateTeamGroupDto dto,
        CancellationToken ct)
        => HandleResult(await _teamService.CreateTeamGroupAsync(id, dto, ct));

    [HttpPut("{id:guid}/chat-groups/{roomId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateChatGroup(
        Guid id,
        Guid roomId,
        [FromForm] UpdateTeamChatRoomDto dto,
        CancellationToken ct)
        => HandleResult(await _teamService.UpdateTeamChatRoomAsync(id, roomId, dto, ct));

    [HttpGet("{id:guid}/chat-groups/{roomId:guid}/members")]
    [Authorize]
    public async Task<IActionResult> GetChatGroupMembers(
        Guid id,
        Guid roomId,
        CancellationToken ct)
        => HandleResult(await _teamService.GetTeamChatRoomMembersAsync(id, roomId, ct));

    [HttpPost("{id:guid}/chat-groups/{roomId:guid}/members")]
    [Authorize]
    public async Task<IActionResult> AddChatGroupMembers(
        Guid id,
        Guid roomId,
        [FromBody] AddTeamChatRoomMembersDto dto,
        CancellationToken ct)
        => HandleResult(await _teamService.AddTeamChatRoomMembersAsync(id, roomId, dto, ct));

    [HttpGet("{id:guid}/reviews")]
    public async Task<IActionResult> GetReviews(Guid id, CancellationToken ct)
        => HandleResult(await _teamService.GetReviewsAsync(id, ct));

    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    public async Task<IActionResult> AddReview(
        Guid id,
        [FromBody] CreateTeamFeedbackRequestDto dto,
        CancellationToken ct)
        => HandleResult(await _teamService.AddReviewAsync(id, dto, ct));
}