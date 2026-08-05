using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Account.Dtos;

public class UpdateDeveloperAccountDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Country { get; set; }

    public IFormFile? ProfileImage { get; set; }

    public string? Bio { get; set; }
}
