using FluentValidation;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Validator;

public sealed class ProfileInterestsValidator : AbstractValidator<ProfileInterestsDto>
{
    public ProfileInterestsValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one category is required.");
    }
}
