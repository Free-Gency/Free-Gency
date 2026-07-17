using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.EmailFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class EmailsController(IEmailAuthService emailAuthService) : ControllerBase
    {
        [HttpPost("Send-Email")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto dto)
        {
            var result = await emailAuthService.SendEmail(dto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
