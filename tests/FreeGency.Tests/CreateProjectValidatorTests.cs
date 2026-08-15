using FreeGency.Application.Features.Projects.DTOs;

namespace FreeGency.Tests;

public class CreateProjectValidatorTests
{
    private static readonly CreateProjectValidator Validator = new();

    [Fact]
    public void ValidAiMappedRequest_PassesValidation()
    {
        var result = Validator.Validate(BuildRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UnsupportedCurrency_FailsValidation()
    {
        var result = Validator.Validate(BuildRequest(currency: "BTC"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.Currency));
    }

    [Fact]
    public void BudgetMaxBelowBudgetMin_FailsValidation()
    {
        var result = Validator.Validate(BuildRequest(budgetMin: 800m, budgetMax: 100m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.BudgetMax));
    }

    [Fact]
    public void MissingSkillsOrSpecialties_FailsValidation()
    {
        var result = Validator.Validate(BuildRequest(skillIds: [], specialtyIds: []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.SkillIds));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.SpecialtyIds));
    }

    [Fact]
    public void BlankTitleAndDescription_FailsValidation()
    {
        var result = Validator.Validate(BuildRequest(title: string.Empty, description: "  "));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.Title));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectRequestDto.Description));
    }

    private static CreateProjectRequestDto BuildRequest(
        string? title = null,
        string? description = null,
        string currency = "USD",
        decimal budgetMin = 500m,
        decimal budgetMax = 800m,
        IEnumerable<Guid>? skillIds = null,
        IEnumerable<Guid>? specialtyIds = null)
        => new()
        {
            Title = title ?? "Bakery Website",
            Description = description ?? "A polished marketing site for a bakery.",
            CategoryId = Guid.NewGuid(),
            IsFixedPrice = true,
            BudgetMin = budgetMin,
            BudgetMax = budgetMax,
            Currency = currency,
            EstimatedDurationDays = 30,
            SkillIds = skillIds ?? [Guid.NewGuid()],
            SpecialtyIds = specialtyIds ?? [Guid.NewGuid()],
        };
}
