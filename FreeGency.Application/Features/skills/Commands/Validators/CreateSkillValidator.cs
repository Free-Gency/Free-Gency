using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Features.skills.Commands.Validators;

public sealed class CreateSkillValidator : AbstractValidator<CreateSkillDto>
{
    public CreateSkillValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
