using FreeGency.Api.Extensions;
using FreeGency.Application.Features.ExternalFeature.Dtos;
using FreeGency.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FreeGency.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class ExternalController(
        IExternalServices externalServices,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        IOptions<LinkedInOptions> linkedInOptions,
        IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly LinkedInOptions _linkedIn = linkedInOptions.Value;
        [HttpGet("linkedin-login")]
        public IActionResult LinkedinLogin(
    [FromQuery] string? intent,
    [FromQuery] string? mode)
        {
            var state = Guid.NewGuid().ToString("N");

            HttpContext.Session.SetString(
                $"LinkedIn:Intent:{state}",
                intent ?? "login");

            HttpContext.Session.SetString(
                $"LinkedIn:Mode:{state}",
                mode ?? "Client");

            var parameters = new Dictionary<string, string?>
            {
                ["response_type"] = "code",
                ["client_id"] = _linkedIn.ClientId,
                ["redirect_uri"] = _linkedIn.RedirectUri,
                ["scope"] = "openid profile email",
                ["state"] = state
            };

            var url = QueryHelpers.AddQueryString(
                "https://www.linkedin.com/oauth/v2/authorization",
                parameters);

            return Redirect(url);
        }
        [HttpGet("linkedin-callback")]
        public async Task<IActionResult> LinkedinCallback(
     [FromQuery] string? code,
     [FromQuery] string? state,
     [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return Unauthorized();

            if (string.IsNullOrEmpty(code))
                return BadRequest("LinkedIn authorization code is missing.");

            if (string.IsNullOrEmpty(state))
                return BadRequest("LinkedIn state is missing.");

            var intent =
                HttpContext.Session.GetString($"LinkedIn:Intent:{state}")
                ?? "login";

            var mode =
                HttpContext.Session.GetString($"LinkedIn:Mode:{state}")
                ?? "Client";

            var token = await ExchangeLinkedInCodeAsync(code);

            if (token is null)
                return Unauthorized();

            var linkedInUser =
                await GetLinkedInUserAsync(token.AccessToken);

            if (linkedInUser is null)
                return Unauthorized();

            var result = await externalServices.LoginWithLinkedInAsync(
                linkedInUser,
                intent,
                mode);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    error = result.error
                });
            }

            return Ok(result.Value);
        }
        private async Task<LinkedInTokenResponse?> ExchangeLinkedInCodeAsync(
     string code)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.linkedin.com/oauth/v2/accessToken");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["client_id"] = configuration["LinkedIn:ClientId"]!,
                    ["client_secret"] = configuration["LinkedIn:ClientSecret"]!,
                    ["redirect_uri"] = configuration["LinkedIn:RedirectUri"]!
                });

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<LinkedInTokenResponse>();
        }
        private async Task<LinkedInUserInfo?> GetLinkedInUserAsync(
    string accessToken)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            return await client.GetFromJsonAsync<LinkedInUserInfo>(
                "https://api.linkedin.com/v2/userinfo");
        }
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
            var frontendUrl = configuration["FrontendUrl"]?.TrimEnd('/')
                ?? "http://localhost:4200";

            try
            {
                var result = await externalServices.LoginWithGoogleAsync();

                if (!result.IsSuccess)
                {
                    var error = Uri.EscapeDataString(result.error.Discription ?? "Google sign-in failed.");
                    return Redirect($"{frontendUrl}/auth/google/callback?error={error}");
                }

                var json = JsonSerializer.Serialize(
                    result.Value,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));
                return Redirect($"{frontendUrl}/auth/google/callback?session={encoded}");
            }
            catch (Exception ex)
            {
                var error = Uri.EscapeDataString(ex.Message);
                return Redirect($"{frontendUrl}/auth/google/callback?error={error}");
            }
        }
    }
}
