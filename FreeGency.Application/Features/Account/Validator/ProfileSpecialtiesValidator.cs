using FluentValidation;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Validator;

public sealed class ProfileSpecialtiesValidator : AbstractValidator<ProfileSpecialtiesDto>
{
    public ProfileSpecialtiesValidator()
    {
        RuleFor(x => x.SpecialtyIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one specialty is required.");
    }
}
