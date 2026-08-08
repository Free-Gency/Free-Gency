

namespace FreeGency.Application.Features.Tasks.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be greater than zero.");
    }
}
