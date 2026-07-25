using FreeGency.Api.Extensions;
using FreeGency.Application.Features.SocialLinks.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SocialLinkController(ISocialLinkService socialLinkService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetUserSocialLink()
        {
            var result = await socialLinkService.GetSocialLinksUser();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("Add-User-Social-Link")]
        public async Task<IActionResult> AddUserSocialLink([FromBody] AddSocialUserLinkRequestDto dto)
        {
            var result = await socialLinkService.AddUserSocialLink(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpPut("Update-User-Social-Link")]
        public async Task<IActionResult> UpdateUserSocialLink([FromBody] UpdateUserSocialLinkDto dto)
        {
            var result = await socialLinkService.UpdateUserSocialLink(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpDelete("Delete-User-Social-Link/{id}")]
        public async Task<IActionResult> DeleteUserSocialLink(Guid id)
        {
            var result = await socialLinkService.DeleteUserSocialLink(id);
            return result.IsSuccess ? Ok() : result.ToProblem();

        }
    }
}
