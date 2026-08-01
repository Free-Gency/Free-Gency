using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using FreeGency.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace FreeGency.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor): ICurrentUserService
    {
      
        public Guid UserId => Guid.TryParse(
      httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value, out var userId) ? userId : Guid.Empty;
        public string FirstName => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName)!;
        public string LastName=> httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Surname)!;
        public string origin =>
                    $"{httpContextAccessor.HttpContext?.Request.Scheme}://" +
                    $"{httpContextAccessor.HttpContext?.Request.Host}";

    }
}

