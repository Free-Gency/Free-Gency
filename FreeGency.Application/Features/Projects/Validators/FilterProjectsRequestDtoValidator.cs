namespace FreeGency.Application.Features.Projects.Validators
{
    public sealed class FilterProjectsRequestDtoValidator : AbstractValidator<FilterProjectsRequestDto>
    {
        private static readonly string[] AllowedSortColumns =
        {
            "CreatedAt",
            "Budget",
            "Title"
        };

        public FilterProjectsRequestDtoValidator()
        {
            RuleFor(x => x.BudgetMin)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BudgetMin.HasValue);

            RuleFor(x => x.BudgetMax)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BudgetMax.HasValue);

            RuleFor(x => x)
                .Must(x => !x.BudgetMin.HasValue ||
                           !x.BudgetMax.HasValue ||
                           x.BudgetMin <= x.BudgetMax)
                .WithMessage("BudgetMin must be less than or equal to BudgetMax.");

            RuleFor(x => x.CategoryId)
                .NotEqual(Guid.Empty)
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.SpecialtyId)
                .NotEqual(Guid.Empty)
                .When(x => x.SpecialtyId.HasValue);

            RuleFor(x => x.Currency)
                .Must(c => Currencies.Supported.Contains(c))
                .WithMessage("Unsupported currency.")
                .When(x => !string.IsNullOrWhiteSpace(x.Currency));

            RuleFor(x => x.Search)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x.SortBy)
                .NotEmpty()
                .Must(x => AllowedSortColumns.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortColumns)}");

            RuleFor(x => x.SortDirection)
                .NotEmpty()
                .Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                           x.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDirection must be either 'asc' or 'desc'.");
        }
    }
}