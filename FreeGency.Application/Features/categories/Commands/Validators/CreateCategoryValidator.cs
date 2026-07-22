using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Features.categories.Commands.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NameEn)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ImageCover)
            .MaximumLength(500)
            .When(x => x.ImageCover is not null);
    }
}
