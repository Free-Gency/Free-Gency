namespace FreeGency.AI.ProjectDrafting;

public class ProjectDraftResponse
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid? CategoryId { get; set; }
    public bool NeedsManualCategoryReview { get; set; }
    public List<Guid> SpecialtyIds { get; set; } = new();
    public List<Guid> SkillIds { get; set; } = new();
}