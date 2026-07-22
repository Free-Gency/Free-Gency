using FreeGency.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace FreeGency.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class ExternalController(
        IExternalServices externalServices,
        SignInManager<User> signInManager,
        IConfiguration configuration) : ControllerBase
    {
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? intent, [FromQuery] string? mode)
        {
            var redirectUrl = Url.Action(
                nameof(GoogleResponse),
                "External");

            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(
                    "Google",
                    redirectUrl);

            if (!string.IsNullOrWhiteSpace(intent))
                properties.Items["intent"] = intent;

            if (!string.IsNullOrWhiteSpace(mode))
                properties.Items["signupMode"] = mode;

            return Challenge(properties, "Google");
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await externalServices.LoginWithGoogleAsync();
            var frontendUrl = configuration["FrontendUrl"]?.TrimEnd('/')
                ?? "http://localhost:4200";

            if (!result.IsSuccess)
            {
                var error = Uri.EscapeDataString(result.error.Discription);
                return Redirect($"{frontendUrl}/auth/google/callback?error={error}");
            }

            var json = JsonSerializer.Serialize(
                result.Value,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));
            return Redirect($"{frontendUrl}/auth/google/callback?session={encoded}");
        }
    }
}
