using FreeGency.Api.Extensions;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetNotification([FromQuery]NotificationFilter notificationFilter)
        {
            var result = await notificationService.GetNotificationAsync(notificationFilter);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadNotificationCountDto>> GetUnreadNotificationCount()
        {
            var result = await notificationService.GetNotificationUnreadCount();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
