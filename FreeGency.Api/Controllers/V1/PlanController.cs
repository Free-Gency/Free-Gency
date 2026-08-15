using FreeGency.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlanController(IPlanService planService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPlans()
        {
            var result = await planService.GetPlans();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
