using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Features.categories.Commands.Validators;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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
