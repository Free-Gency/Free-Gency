namespace FreeGency.AI.ProjectDrafting;

public interface IProjectGenerationService
{
    Task<GeneratedProjectDraft> GenerateAsync(string userInput, CancellationToken ct = default);
}
