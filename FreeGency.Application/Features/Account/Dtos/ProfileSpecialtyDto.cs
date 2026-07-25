namespace FreeGency.Application.Features.Account.Dtos;

public sealed class ProfileSpecialtyDto
{
    public Guid Id { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public List<SkillDto> Skills { get; set; } = [];
}
