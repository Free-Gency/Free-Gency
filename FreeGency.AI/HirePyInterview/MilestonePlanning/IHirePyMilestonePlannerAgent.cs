namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

public interface IHirePyMilestonePlannerAgent
{
    /// <summary>
    /// Reviews the candidate's latest milestone plan and decides the next step:
    /// ask for a revision or finalize the plan for persistence.
    /// </summary>
    Task<MilestonePlanningReply> PlanNextStepAsync(
        MilestonePlanningContext context,
        CancellationToken cancellationToken = default);
}
