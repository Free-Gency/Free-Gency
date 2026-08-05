using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FreeGency.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public Guid UserId
        {
            get
            {
                var principal = httpContextAccessor.HttpContext?.User;
                if (principal is null) return Guid.Empty;

                var raw =
                    principal.FindFirst("uid")?.Value
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(raw, out var userId) ? userId : Guid.Empty;
            }
        }

        public string FirstName =>
            httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName)!;

        public string LastName =>
            httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Surname)!;

        public string origin =>
            $"{httpContextAccessor.HttpContext?.Request.Scheme}://" +
            $"{httpContextAccessor.HttpContext?.Request.Host}";
    }
}
