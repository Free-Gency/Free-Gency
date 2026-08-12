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
                .MaximumLength(5000);

            RuleFor(x => x.Budget)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Budget.HasValue);

            RuleFor(x => x.Challenge).MaximumLength(8000);
            RuleFor(x => x.Solution).MaximumLength(8000);
            RuleFor(x => x.DurationLabel).MaximumLength(100);
            RuleFor(x => x.Industry).MaximumLength(120);
            RuleFor(x => x.TeamLeads).MaximumLength(2000);
            RuleFor(x => x.TestimonialQuote).MaximumLength(4000);

            RuleFor(x => x.CompletionDate)
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                .When(x => x.CompletionDate.HasValue);
        }
    }
}
