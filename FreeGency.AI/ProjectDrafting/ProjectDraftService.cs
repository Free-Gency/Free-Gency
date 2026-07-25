using System.Text.Json;
using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ProjectDrafting;

public class ProjectDraftService
{
    private readonly IChatCompletionService _chat;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    
    public ProjectDraftService(
        IChatCompletionService chat,
        ICategoryRepository categoryRepo,
        ISpecialtyRepository specialtyRepo)
    {
        _chat = chat;
        _categoryRepository = categoryRepo;
        _specialtyRepository = specialtyRepo;
    }

    public async Task<ProjectDraftResponse> GenerateDraftAsync(string userInput)
    {
        var categories = (await _categoryRepository.GetAllWithSpecialtiesAsync()).ToList();
        var taxonomyBlock = string.Join("\n", categories.Select(c =>
            $"- {c.NameEn} → specialties: [{string.Join(", ", c.CategorySpecialties.Select(cs => cs.Specialty.NameEn))}]"));

        var step1Json = await AskAsync($$"""
            You turn a client's rough project idea into a structured job post.
            Pick CategoryName from this exact list only, copied exactly:
            {{taxonomyBlock}}
            Pick 1-3 SpecialtyNames that belong to that category's list above.
            Write a polished 2-3 paragraph Description and a short Title.
            Respond ONLY as JSON: {"title": "", "description": "", "categoryName": "", "specialtyNames": []}

            Client input: {{userInput}}
            """);
        
        var step1 = JsonSerializer.Deserialize<Step1Result>(step1Json, JsonOpts)!;
        
        var matchedCategory = categories.FirstOrDefault(c => c.NameEn == step1.CategoryName);
        var matchedSpecialties = matchedCategory?.CategorySpecialties
            .Select(cs => cs.Specialty)
            .Where(s => step1.SpecialtyNames.Contains(s.NameEn))
            .ToList() ?? new List<Domain.Entities.Specialty>();
        
        if (!matchedSpecialties.Any())
        {
            return new ProjectDraftResponse
            {
                Title = step1.Title,
                Description = step1.Description,
                CategoryId = matchedCategory?.Id,
                NeedsManualCategoryReview = matchedCategory is null,
                SpecialtyIds = new List<Guid>(),
                SkillIds = new List<Guid>()
            };
        }

        var specialtySkillLists = new List<Domain.Entities.Skill>();
        foreach (var specialty in matchedSpecialties)
        {
            var skills = await _specialtyRepository.GetSkillsForSpecialtyAsync(specialty.Id);
            specialtySkillLists.AddRange(skills);
        }
        var availableSkills = specialtySkillLists.DistinctBy(s => s.Id).ToList();

        var skillsBlock = string.Join(", ", availableSkills.Select(s => s.Name));

        var step2Json = await AskAsync($$"""
                                         Given this project idea, category "{{matchedCategory?.NameEn}}", and specialties "{{string.Join(", ", matchedSpecialties.Select(s => s.NameEn))}}",
                                         pick 3-6 relevant skill names from this exact list only, copied exactly:
                                         {{skillsBlock}}
                                         Respond ONLY as JSON: {"skillNames": []}

                                         Client input: {{userInput}}
                                         """);

        var step2 = JsonSerializer.Deserialize<Step2Result>(step2Json, JsonOpts)!;

        var matchedSkillIds = availableSkills
            .Where(s => step2.SkillNames.Contains(s.Name, StringComparer.OrdinalIgnoreCase))
            .Select(s => s.Id)
            .ToList();

        return new ProjectDraftResponse
        {
            Title = step1.Title,
            Description = step1.Description,
            CategoryId = matchedCategory?.Id,
            NeedsManualCategoryReview = matchedCategory is null,
            SpecialtyIds = matchedSpecialties.Select(s => s.Id).ToList(),
            SkillIds = matchedSkillIds
        };
    }
    
    private async Task<string> AskAsync(string prompt)
    {
        var history = new ChatHistory();
        history.AddUserMessage(prompt);
        var result = await _chat.GetChatMessageContentsAsync(history);
        var raw = result[0].Content ?? "{}";
        return StripJsonFences(raw);
    }

    private static string StripJsonFences(string raw)
    {
        var text = raw.Trim();

        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline != -1)
                text = text[(firstNewline + 1)..];

            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence != -1)
                text = text[..lastFence];
        }

        return text.Trim();
    }
}