namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

/// <summary>
/// A single milestone proposed by the AI, shaped so it can be handed straight to
/// the existing milestone-plan flow (title, DefinitionOfDone, Amount, DueDate).
/// </summary>
public sealed class ProposedMilestone
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Composite of description + deliverables + dependencies + acceptance criteria.</summary>
    public string DefinitionOfDone { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime? DueDate { get; set; }

    public int SortOrder { get; set; }
}
