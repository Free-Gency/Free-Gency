using FluentValidation;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Validator;

public sealed class ProfileSkillsValidator : AbstractValidator<ProfileSkillsDto>
{
    public ProfileSkillsValidator()
    {
        RuleFor(x => x.SkillIds)
            .NotNull();
    }
}
