
namespace FreeGency.Application.Features.Tasks.Validators;

public class CreateTaskCommentValidator : AbstractValidator<CreateTaskCommentDto>
{
    public CreateTaskCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment cannot be empty.")
            .MaximumLength(4000);
    }
}

