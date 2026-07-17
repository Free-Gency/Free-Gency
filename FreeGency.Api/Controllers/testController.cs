using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
