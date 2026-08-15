namespace FreeGency.AI.HirePyInterview.Evaluation;

public interface IHirePyEvaluatorAgent
{
    /// <summary>
    /// Scores ONE candidate across the evaluation dimensions and returns a validated, client-safe
    /// structured result. Returns <see cref="HirePyEvaluationReply.IsValid"/> = false when the model
    /// output cannot be trusted (missing or unbounded scores).
    /// </summary>
    Task<HirePyEvaluationReply> EvaluateAsync(
        HirePyEvaluationContext context,
        CancellationToken cancellationToken = default);
}
