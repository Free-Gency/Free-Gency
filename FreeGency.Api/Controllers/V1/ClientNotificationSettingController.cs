using FreeGency.Api.Extensions;
using FreeGency.Application.Features.ClientNotification.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientNotificationSettingController(IClientNotficationService clientNotficationService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetSettingAsycn()
        {
            var result = await clientNotficationService.GetNotifiactionCLientSettingAsync();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPut]
        public async Task<IActionResult> UpdateSettingAsync(UpdateNotificationClientDto dto)
        {
            var result = await clientNotficationService.UpdateNotificationClientSettingAsync(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
    }
}
