using FreeGency.Api.Extensions;
using FreeGency.Application.Features.TeamJoinRequests.Commands;
using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using FreeGency.Domain.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamJoinRequestController(ITeamJoinRequestService teamJoinRequestService) : ControllerBase
    {
        [HttpPut("join-requests")]
        public async Task<IActionResult> ApplyTeamJobAsync([FromBody]ApplyToTeamJobCommand applyToTeamJobCommand)
        {
            var result = await teamJoinRequestService.ApplyToTeamJobAsync(applyToTeamJobCommand);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpPost("join-by-code")]
        public async Task<IActionResult> JoinByCodeAsync([FromBody] JoinTeamByCodeCommand joinTeamByCode)
        {
            var result = await teamJoinRequestService.JoinByCodeAsync(joinTeamByCode);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpGet("Get-Team-Join-Request")]
        public async Task<IActionResult> GetJoinRequests([FromQuery] TeamJoinRequestSpecificationParam param)
        {
            var result = await teamJoinRequestService.GetJoinRequestAsync(param);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetMyJoinRequests(
                                        [FromQuery] UserJoinRequestSpecificationParam param)
        {
            var result = await teamJoinRequestService.GetUserJoinRequestsAsync(param);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }
        [HttpPatch("{requestId:guid}/accept")]
        public async Task<IActionResult> AcceptJoinRequest(Guid requestId)
        {
            var result = await teamJoinRequestService.AcceptJoinRequestAsync(requestId);

            return result.IsSuccess
                ? NoContent()
                : result.ToProblem();
        }
        [HttpPatch("{requestId:guid}/reject")]
        public async Task<IActionResult> RejectJoinRequest(Guid requestId)
        {
            var result = await teamJoinRequestService.RejectJoinRequestAsync(requestId);

            return result.IsSuccess
                ? NoContent()
                : result.ToProblem();
        }
    }
}
