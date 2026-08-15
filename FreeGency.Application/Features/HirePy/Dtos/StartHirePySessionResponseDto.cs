namespace FreeGency.Application.Features.HirePy.Dtos;

public sealed class StartHirePySessionResponseDto
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = "Processing";
    public string Stage { get; set; } = "GeneratingProject";
}
