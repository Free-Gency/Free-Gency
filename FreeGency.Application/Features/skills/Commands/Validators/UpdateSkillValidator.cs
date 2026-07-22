using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Features.skills.Commands.Validators;

public sealed class UpdateSkillValidator : AbstractValidator<UpdateSkillDto>
{
    public UpdateSkillValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
