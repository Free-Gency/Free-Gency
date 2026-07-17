using FreeGency.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
namespace FreeGency.Infrastructure.Services;
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId => Guid.TryParse(
        httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value,out var userId)?userId : Guid.Empty;
    
}