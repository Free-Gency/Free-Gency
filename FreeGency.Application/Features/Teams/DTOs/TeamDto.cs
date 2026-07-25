namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class TeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public List<TeamCategoryDto> Categories { get; set; } = [];
    public List<TeamSpecialtyDto> Specialties { get; set; } = [];
    public List<TeamSkillDto> Skills { get; set; } = [];
    public int MembersCount { get; set; }
}

public sealed class TeamCategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class TeamSpecialtyDto
{
    public Guid SpecialtyId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}

public sealed class TeamSkillDto
{
    public Guid SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
}