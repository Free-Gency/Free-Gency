namespace FreeGency.Application.Features.Account.Dtos;

public sealed class ProfileInterestDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? ImageCover { get; set; }

    public List<ProfileSpecialtyDto> Specialties { get; set; } = [];
}
