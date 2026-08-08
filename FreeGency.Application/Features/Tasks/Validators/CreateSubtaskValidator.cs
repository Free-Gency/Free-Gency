
namespace FreeGency.Application.Features.Tasks.Validators;

public class CreateSubtaskValidator : AbstractValidator<CreateSubtaskDto>
{
    public CreateSubtaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Subtask title is required.")
            .MaximumLength(300);
    }
}
