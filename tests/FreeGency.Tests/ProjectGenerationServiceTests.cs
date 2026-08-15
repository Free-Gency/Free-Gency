using FreeGency.AI.ProjectDrafting;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;

namespace FreeGency.Tests;

public class ProjectGenerationServiceTests
{
    private static Mock<IChatCompletionService> ChatWithSequence(
        string step1Json, string step2Json, string metaJson)
    {
        var chat = new Mock<IChatCompletionService>();
        chat.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(),
                It.IsAny<CancellationToken>()))
            .Returns((ChatHistory history, PromptExecutionSettings? _, Kernel? _, CancellationToken _) =>
            {
                var prompt = history.Last().Content ?? string.Empty;
                var json = prompt switch
                {
                    _ when prompt.Contains("Pick CategoryName") => step1Json,
                    _ when prompt.Contains("relevant skill names") => step2Json,
                    _ when prompt.Contains("isFixedPrice") => metaJson,
                    _ => "{}"
                };
                return Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                    [new ChatMessageContent(AuthorRole.Assistant, json)]);
            });
        return chat;
    }

    private static (Category category, Specialty specialty, Skill skill) BuildTaxonomy()
    {
        var skill = new Skill { Id = Guid.NewGuid(), Name = "ASP.NET Core" };
        var specialty = new Specialty { Id = Guid.NewGuid(), NameEn = "Web Development" };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            NameEn = "Software Development",
            CategorySpecialties =
            [
                new CategorySpecialty { Id = Guid.NewGuid(), CategoryId = Guid.Empty, SpecialtyId = specialty.Id, Specialty = specialty }
            ]
        };
        return (category, specialty, skill);
    }

    private static ProjectGenerationService BuildService(
        Mock<IChatCompletionService> chat,
        Category category,
        Skill skill)
    {
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(r => r.GetAllWithSpecialtiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { category });

        var specialtyRepo = new Mock<ISpecialtyRepository>();
        specialtyRepo.Setup(r => r.GetSkillsForSpecialtyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Skill> { skill });

        var draftService = new ProjectDraftService(chat.Object, categoryRepo.Object, specialtyRepo.Object);
        return new ProjectGenerationService(draftService, chat.Object);
    }

    [Fact]
    public async Task GenerateAsync_WithValidResponses_ReturnsStructuredDraft()
    {
        var (category, specialty, skill) = BuildTaxonomy();
        var chat = ChatWithSequence(
            """{"title":"Bakery Website","description":"A polished marketing site for a bakery.","categoryName":"Software Development","specialtyNames":["Web Development"]}""",
            """{"skillNames":["ASP.NET Core"]}""",
            """{"isFixedPrice":true,"budgetMin":500,"budgetMax":800,"currency":"USD","estimatedDurationDays":30,"deadline":"2026-12-31","complexity":"Medium","requirements":["Order form","Menu display"],"features":["Gallery"],"risks":["Late assets"]}""");
        var service = BuildService(chat, category, skill);

        var draft = await service.GenerateAsync("I need a website for my bakery", CancellationToken.None);

        Assert.Equal("Bakery Website", draft.Title);
        Assert.False(draft.NeedsManualCategoryReview);
        Assert.Equal(category.Id, draft.CategoryId);
        Assert.Equal(category.NameEn, draft.CategoryName);
        Assert.Contains(specialty.Id, draft.SpecialtyIds);
        Assert.Contains(skill.Id, draft.SkillIds);
        Assert.True(draft.IsFixedPrice);
        Assert.Equal(500m, draft.BudgetMin);
        Assert.Equal(800m, draft.BudgetMax);
        Assert.Equal("USD", draft.Currency);
        Assert.Equal(30, draft.EstimatedDurationDays);
        Assert.Equal(new DateTime(2026, 12, 31), draft.Deadline);
        Assert.Equal("Medium", draft.Complexity);
        Assert.Equal(2, draft.Requirements.Count);
        Assert.Single(draft.Features);
        Assert.Single(draft.Risks);
    }

    [Fact]
    public async Task GenerateAsync_WithUnsupportedCurrencyAndBadMeta_NormalizesValues()
    {
        var (category, _, skill) = BuildTaxonomy();
        var chat = ChatWithSequence(
            """{"title":"App","description":"An app idea.","categoryName":"Software Development","specialtyNames":["Web Development"]}""",
            """{"skillNames":["ASP.NET Core"]}""",
            """{"isFixedPrice":false,"budgetMin":900,"budgetMax":100,"currency":"BTC","estimatedDurationDays":0,"deadline":"not-a-date","complexity":"Unknown","requirements":[],"features":[""],"risks":[]}""");
        var service = BuildService(chat, category, skill);

        var draft = await service.GenerateAsync("I need an app", CancellationToken.None);

        Assert.Equal("USD", draft.Currency);
        Assert.Equal(900m, draft.BudgetMin);
        Assert.Equal(900m, draft.BudgetMax);
        Assert.Null(draft.EstimatedDurationDays);
        Assert.Null(draft.Deadline);
        Assert.Null(draft.Complexity);
        Assert.Empty(draft.Features);
    }

    [Fact]
    public async Task GenerateAsync_WhenMetaIsMalformed_ThrowsInvalidOperationException()
    {
        var (category, _, skill) = BuildTaxonomy();
        var chat = ChatWithSequence(
            """{"title":"App","description":"An app idea.","categoryName":"Software Development","specialtyNames":["Web Development"]}""",
            """{"skillNames":["ASP.NET Core"]}""",
            "this is not json");
        var service = BuildService(chat, category, skill);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateAsync("I need an app", CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAsync_WhenChatFails_Throws()
    {
        var (category, _, skill) = BuildTaxonomy();
        var chat = new Mock<IChatCompletionService>();
        chat.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Bedrock gateway unreachable"));
        var service = BuildService(chat, category, skill);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GenerateAsync("I need an app", CancellationToken.None));
    }
}
