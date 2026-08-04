namespace FreeGency.Application.Features.Portfolio.Validators
{
    public sealed class UpdatePortfolioProjectValidator
     : AbstractValidator<UpdatePortfolioProjectRequestDto>
    {
        public UpdatePortfolioProjectValidator()
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

            RuleFor(x => x.ProjectUrl)
                .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ProjectUrl));

            RuleFor(x => x.PrototypeUrl)
                .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.PrototypeUrl));

            RuleFor(x => x.Challenge).MaximumLength(8000);
            RuleFor(x => x.Solution).MaximumLength(8000);
            RuleFor(x => x.DurationLabel).MaximumLength(100);
            RuleFor(x => x.Industry).MaximumLength(120);
            RuleFor(x => x.TeamLeads).MaximumLength(200);
            RuleFor(x => x.TestimonialQuote).MaximumLength(4000);

            RuleFor(x => x.CompletionDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.CompletionDate.HasValue);
        }
    }
}
