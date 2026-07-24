using FluentValidation;
using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Features.Proposals.Validators;

public sealed class UpdateProposalValidator : AbstractValidator<UpdateProposalDto>
{
    public UpdateProposalValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.CoverLetter)
            .MaximumLength(5000)
            .When(x => x.CoverLetter is not null);

        RuleFor(x => x.ProposedBudget)
            .GreaterThan(0)
            .When(x => x.ProposedBudget.HasValue)
            .WithMessage("Proposed budget must be greater than zero.");
    }
}
