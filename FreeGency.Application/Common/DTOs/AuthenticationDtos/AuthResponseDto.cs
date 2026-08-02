namespace FreeGency.Application.Common.DTOs.AuthenticationDtos;

public class AuthResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ActiveProfileMode { get; set; }
    public bool HasCompletedOnboarding { get; set; }
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiration { get; set; }
    public string ActiveMode { get; set; }
    public Guid? ProfileId { get; set; }
}
