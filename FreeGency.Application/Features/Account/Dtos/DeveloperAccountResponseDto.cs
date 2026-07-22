using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Features.Account.Dtos;

public class DeveloperAccountResponseDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    public decimal AverageRating { get; set; }

    public int RatingCount { get; set; }

    public string Country { get; set; } = string.Empty;

    public List<CategoryDto> Interests { get; set; } = [];

    public List<SpecialtyWithSkillsDto> Specialties { get; set; } = [];

    public List<SkillDto> Skills { get; set; } = [];
}
