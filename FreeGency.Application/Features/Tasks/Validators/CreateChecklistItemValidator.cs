
namespace FreeGency.Application.Features.Tasks.Validators;

public class CreateChecklistItemValidator : AbstractValidator<CreateChecklistItemDto>
{
    public CreateChecklistItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Checklist item title is required.")
            .MaximumLength(300);
    }
}
