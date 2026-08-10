using FluentValidation;
using FreeGency.Application.Features.TeamJobs.Dtos;

namespace FreeGency.Application.Features.TeamJobs.Validators;

public sealed class UpdateTeamJobValidator : AbstractValidator<UpdateTeamJobDto>
{
    public UpdateTeamJobValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required — write a pitch that attracts applicants.")
            .MinimumLength(80).WithMessage("Description must be at least 80 characters.")
            .MaximumLength(2000);
    }
}
