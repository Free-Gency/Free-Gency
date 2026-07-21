using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController (IAccountService accountService): ControllerBase
    {
        [HttpGet("Get-Client-Profile")]
        public async Task<IActionResult> GetClientProfile()
        {
            var result = await accountService.GetClientProfile();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
