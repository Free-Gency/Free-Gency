using FluentValidation;
using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Features.Proposals.Validators;

public sealed class FilterProposalValidator : AbstractValidator<FilterProposalDto>
{
    private static readonly string[] AllowedSortColumns = ["AppliedAt", "ProposedBudget", "CreatedAt"];

    public FilterProposalValidator()
    {
        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetMin.HasValue);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.BudgetMin.HasValue || !x.BudgetMax.HasValue || x.BudgetMin <= x.BudgetMax)
            .WithMessage("BudgetMin must be less than or equal to BudgetMax.");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .When(x => x.TeamId.HasValue);

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
