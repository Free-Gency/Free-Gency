namespace FreeGency.Application.Features.specialties.Dtos;

public sealed class SpecialtyDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}
