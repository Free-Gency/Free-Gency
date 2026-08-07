using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class UpdateTeamChatRoomDto
{
    public string? Title { get; set; }
    public IFormFile? Logo { get; set; }
}
