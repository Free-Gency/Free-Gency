using FreeGency.Application.Common.Results;

namespace FreeGency.Api.Controllers.V1
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        public IActionResult HandleResult(ApiResponse result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return StatusCode(result.Error!.statusCode, result);
        }
    }
}
