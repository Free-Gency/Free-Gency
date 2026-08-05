using FreeGency.Api.Extensions;
using FreeGency.Application.Features.DeveloperNotification.Commands;
using FreeGency.Application.Features.DeveloperNotification.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class DeveloperNotficationSettingController(IDeveloperNotificationService developerNotificationService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetSettingNotificationSetting()
        {
            var result = await developerNotificationService.GetNotifiactionDeveloperSettingAsync();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPut]
        public async Task<IActionResult> UpdateSettingNotification([FromBody] UpdateNotificationDeveloperDto dto)
        {
            var result = await developerNotificationService.UpdateNotificationDeveloperSettingAsync(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
    }
}
