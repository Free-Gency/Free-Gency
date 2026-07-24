using FluentValidation;
using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Features.Proposals.Validators;

public sealed class CreateProposalValidator : AbstractValidator<CreateProposalDto>
{
    public CreateProposalValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.ApplicantType)
            .IsInEnum();

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .When(x => x.ApplicantType == ApplicantType.Team)
            .WithMessage("TeamId is required when applying as a team.");

        RuleFor(x => x.CoverLetter)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.ProposedBudget)
            .GreaterThan(0)
            .WithMessage("Proposed budget must be greater than zero.");

        RuleFor(x => x.AttachmentUrls)
            .Must(x => x is null || x.Count() <= 10)
            .WithMessage("A proposal can have at most 10 attachments.");
    }
}
