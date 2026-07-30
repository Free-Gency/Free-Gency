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

        RuleFor(x => x.Approach)
            .MaximumLength(5000);

        RuleFor(x => x.ProposedTimeline)
            .MaximumLength(200)
            .When(x => x.ProposedTimeline is not null);

        RuleFor(x => x.SimilarLinksUrl)
            .MaximumLength(1000)
            .When(x => x.SimilarLinksUrl is not null);

        RuleFor(x => x.ProposedBudget)
            .GreaterThan(0)
            .WithMessage("Proposed budget must be greater than zero.");

        RuleFor(x => x.AttachmentUrls)
            .Must(x => x is null || x.Count() <= 10)
            .WithMessage("A proposal can have at most 10 attachments.");
    }
}
