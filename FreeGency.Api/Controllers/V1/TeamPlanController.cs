using FreeGency.Api.Extensions;
using FreeGency.Application.Features.PlanTeamFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TeamPlanController(IPlanTeamService planTeamService) : ControllerBase
    {

        [HttpGet("TeamPlans")]
        public async Task<IActionResult> GetTeamPlansAsync()
        {
            var result = await planTeamService.GetPlansTeam();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("{teamId}/subscription")]
        public async Task<IActionResult> GetMySubscription([FromRoute]Guid teamId)
        {
            var result = await planTeamService.GetMySubscription(teamId);

            return Ok(result);
        }
        [HttpPost("ToggleAutoRenew/{SubscriptionId}")]
        public async Task<IActionResult> ToggleAutoRenewAsync([FromRoute]Guid SubscriptionId)
        {
            var result = await planTeamService.ToggleAutoRenew(SubscriptionId);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("ChangePlanTeam")]
        public async Task<IActionResult> ChangePlanTeamAsync([FromBody]ChangeTeamPlanDto dto)
        {
            var result = await planTeamService.ChangeMyPlan(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
    }
}
