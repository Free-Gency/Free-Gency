using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreeGency.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class ExternalController(IExternalServices externalServices,SignInManager<User> signInManager) : ControllerBase
    {
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(
                nameof(GoogleResponse),
                "External");

            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(
                    "Google",
                    redirectUrl);

            return Challenge(properties, "Google");
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await externalServices.LoginWithGoogleAsync();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
