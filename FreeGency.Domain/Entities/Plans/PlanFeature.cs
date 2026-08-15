
namespace FreeGency.Domain.Entities.Plans;

public class PlanFeature : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid PlanId { get; set; }
    [ForeignKey(nameof(PlanId))]
    public Plan Plan { get; set; } = null!;

    public FeatureType Feature { get; set; }

    public int? Limit { get; set; }
    public bool IsEnabled { get; set; }
}
public enum FeatureType
{
    CreateProject,
    SendProposal,
    TeamMembers,
    ActiveProjects,
    FileStorage,
    SendInvitation,
    GenerateProjectDraft,
    TeamSuggestions,
    AIChat,
    HiringAgent
}
