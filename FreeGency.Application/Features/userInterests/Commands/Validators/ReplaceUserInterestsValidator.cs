using FreeGency.Application.Features.userInterests.Dtos;

namespace FreeGency.Application.Features.userInterests.Commands.Validators;

public sealed class ReplaceUserInterestsValidator : AbstractValidator<ReplaceUserInterestsDto>
{
    public ReplaceUserInterestsValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotNull();
    }
}
