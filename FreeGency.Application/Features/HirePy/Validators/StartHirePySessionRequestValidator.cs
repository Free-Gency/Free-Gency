using FluentValidation;
using FreeGency.Application.Features.HirePy.Dtos;

namespace FreeGency.Application.Features.HirePy.Validators;

public sealed class StartHirePySessionRequestValidator : AbstractValidator<StartHirePySessionRequestDto>
{
    public StartHirePySessionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A project description is required.")
            .MaximumLength(5000)
            .WithMessage("Description must be at most 5000 characters.");
    }
}
