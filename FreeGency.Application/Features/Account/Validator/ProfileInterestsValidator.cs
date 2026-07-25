using FluentValidation;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Validator;

public sealed class ProfileInterestsValidator : AbstractValidator<ProfileInterestsDto>
{
    public ProfileInterestsValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotNull()
            .WithMessage("CategoryIds is required.");
    }
}
