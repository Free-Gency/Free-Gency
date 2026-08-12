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

            // Case-study wizard may leave short description empty; Challenge/Solution carry the story.
            RuleFor(x => x.Description)
                .MaximumLength(5000);

            RuleFor(x => x.Budget)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Budget.HasValue);

            RuleFor(x => x.OwnerType)
                .IsInEnum().WithMessage("Invalid owner type.");

            RuleFor(x => x.Challenge).MaximumLength(8000);
            RuleFor(x => x.Solution).MaximumLength(8000);
            RuleFor(x => x.DurationLabel).MaximumLength(100);
            RuleFor(x => x.Industry).MaximumLength(120);
            RuleFor(x => x.TeamLeads).MaximumLength(2000);
            RuleFor(x => x.TestimonialQuote).MaximumLength(4000);
            RuleFor(x => x.TestimonialAuthorName).MaximumLength(150);
            RuleFor(x => x.TestimonialAuthorTitle).MaximumLength(200);

            // Allow "today" in local timezones slightly ahead of UTC.
            RuleFor(x => x.CompletionDate)
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                .When(x => x.CompletionDate.HasValue);

            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count <= 10)
                .WithMessage("Maximum 10 images are allowed.");
        }
    }
}
