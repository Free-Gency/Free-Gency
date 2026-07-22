namespace FreeGency.Application.Features.Projects.Commands.Validators
{
    public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequestDto>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Title)
                .MaximumLength(200)
                .When(x => x.Title is not null);

            RuleFor(x => x.Description)
                .MaximumLength(5000)
                .When(x => x.Description is not null);

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.BudgetMin)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BudgetMin.HasValue);

            RuleFor(x => x.BudgetMax)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BudgetMax.HasValue);

            RuleFor(x => x)
                .Must(x =>
                {
                    if (!x.BudgetMin.HasValue || !x.BudgetMax.HasValue)
                        return true;

                    return x.BudgetMax >= x.BudgetMin;
                })
                .WithMessage("BudgetMax must be greater than or equal to BudgetMin.");

            RuleFor(x => x.Currency)
                .Must(c => Currencies.Supported.Contains(c!))
                .When(x => !string.IsNullOrWhiteSpace(x.Currency))
                .WithMessage("Unsupported currency.");

            RuleFor(x => x.EstimatedDurationDays)
                .GreaterThan(0)
                .When(x => x.EstimatedDurationDays.HasValue);

            RuleFor(x => x.SpecialtyIds)
                .Must(x => x == null || x.Distinct().Count() == x.Count)
                .WithMessage("Duplicate specialties are not allowed.");

            RuleFor(x => x.SkillIds)
                .Must(x => x == null || x.Distinct().Count() == x.Count)
                .WithMessage("Duplicate skills are not allowed.");
        }
    }
}
