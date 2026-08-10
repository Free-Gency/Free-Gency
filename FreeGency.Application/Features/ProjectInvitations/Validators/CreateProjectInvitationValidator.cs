using FluentValidation;
using FreeGency.Application.Features.ProjectInvitations.Dtos;

namespace FreeGency.Application.Features.ProjectInvitations.Validators;

public sealed class CreateProjectInvitationValidator : AbstractValidator<CreateProjectInvitationDto>
{
    public CreateProjectInvitationValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.InviteeType).IsInEnum();
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.InviteeUserId)
            .NotEmpty()
            .When(x => x.InviteeType == ApplicantType.User)
            .WithMessage("InviteeUserId is required when inviting a developer.");

        RuleFor(x => x.InviteeTeamId)
            .NotEmpty()
            .When(x => x.InviteeType == ApplicantType.Team)
            .WithMessage("InviteeTeamId is required when inviting a team.");
    }
}
