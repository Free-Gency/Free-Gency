namespace FreeGency.Application.Features.skills.Dtos;

public sealed class UpdateSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
