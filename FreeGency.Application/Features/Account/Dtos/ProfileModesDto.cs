namespace FreeGency.Application.Features.Account.Dtos;

public sealed class ProfileModesDto
{
    public string ActiveProfileMode { get; init; } = string.Empty;
    public bool HasClientProfile { get; init; }
    public bool HasDeveloperProfile { get; init; }
    public Guid? ActiveProfileId { get; init; }
}

public sealed class SwitchProfileResponseDto
{
    public string ActiveProfileMode { get; init; } = string.Empty;
    public Guid? ProfileId { get; init; }
    public bool HasClientProfile { get; init; }
    public bool HasDeveloperProfile { get; init; }
}
