using FreeGency.Api.Extensions;
using FreeGency.Application.Features.Plans.Dtos;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlanController(IPlanService planService,  ICurrentUserService currentUserService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPlans()
        {
            var result = await planService.GetPlans();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        
        [HttpPost("subscribe")]
        public async Task<IActionResult> ChangeSubscription([FromBody] ChangePlanRequestDto request)
        {
            var userId = currentUserService.UserId;
            var result = await planService.ChangeSubscriptionAsync(userId, request);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
