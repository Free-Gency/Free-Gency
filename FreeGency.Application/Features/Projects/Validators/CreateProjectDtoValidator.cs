public sealed class CreateProjectValidator : AbstractValidator<CreateProjectRequestDto>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Currency)
            .Must(c => Currencies.Supported.Contains(c))
            .WithMessage("Unsupported currency.");

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(x => x.BudgetMin);

        RuleFor(x => x.EstimatedDurationDays)
            .GreaterThan(0)
            .When(x => x.EstimatedDurationDays.HasValue);

        //RuleFor(x => x.Deadline)
        //    .GreaterThan(DateTime.UtcNow)
        //    .When(x => x.Deadline.HasValue);

        RuleFor(x => x.SpecialtyIds)
            .NotEmpty()
            .WithMessage("At least one specialty is required.");

        RuleFor(x => x.SkillIds)
            .NotEmpty()
            .WithMessage("At least one skill is required.");
    }
}