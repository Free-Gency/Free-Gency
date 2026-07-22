using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class testController : ControllerBase
    {
        [HttpGet]
        public IActionResult asd()
        {
            return Ok("asd");
        }
        
        // [HttpGet("test-ai")]
        // [AllowAnonymous]
        // public async Task<IActionResult> TestAi([FromServices] IChatCompletionService chat)
        // {
        //     var history = new ChatHistory();
        //     history.AddSystemMessage("You are a concise teaching assistant.");
        //     history.AddUserMessage("Explain binary search in simple terms in a sentence containing maximum 20 words.");
        //
        //     var result = await chat.GetChatMessageContentsAsync(history);
        //
        //     return Ok(result[0].Content);
        // }
    }
}
