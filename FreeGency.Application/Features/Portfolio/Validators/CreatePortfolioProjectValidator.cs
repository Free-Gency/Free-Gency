namespace FreeGency.Application.Features.Portfolio.Validators
{
    public sealed class CreatePortfolioProjectValidator
        : AbstractValidator<CreatePortfolioProjectRequestDto>
    {
        public CreatePortfolioProjectValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(5000);

            RuleFor(x => x.Budget)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Budget.HasValue);

            RuleFor(x => x.OwnerType)
                .IsInEnum().WithMessage("Invalid owner type.");

            RuleFor(x => x.ProjectUrl)
                .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ProjectUrl));

            RuleFor(x => x.CompletionDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.CompletionDate.HasValue);

            RuleForEach(x => x.SkillIds)
                .NotEmpty();

            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count() <= 10)
                .WithMessage("Maximum 10 images are allowed.");
        }
    }
}
