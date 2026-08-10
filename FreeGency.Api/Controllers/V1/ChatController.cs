using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Pagination;
using FreeGency.Application.Features.ChatFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetChatRoomsAsync([FromQuery]ChatRoomFilter filter)
        {
            var result = await chatService.GetChatRoomAsync(filter);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("rooms/{roomId}/messages")]
        public async Task<IActionResult> GetRoomMessages([FromRoute] Guid roomId, [FromQuery] RoomMessageFilter pagedQuery)
        {
            var result = await chatService.GetMessageChatRoom(roomId, pagedQuery);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("Start-Discussion")]
        public async Task<IActionResult> StartDiscussion([FromBody]StartDiscussionRequestDto dto)
        {
            var result = await chatService.StartDiscussionAsync(dto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("Send-message/{roomId}")]
        public async Task<IActionResult> SendMessage([FromRoute]Guid roomId, [FromForm] SendMessageRequest sendMessageRequest)
        {
            var result = await chatService.SendMessageAsync(roomId, sendMessageRequest);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPut("rooms/{roomId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid roomId)
        {
            var result = await chatService.MarkAsRead(roomId);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPost("rooms/{roomId}/archive")]
        public async Task<IActionResult> ArchiveRoom(Guid roomId)
        {
            var result = await chatService.ArchiveRoomAsync(roomId);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    }
}
