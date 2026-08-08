
namespace FreeGency.Application.Features.Tasks.Validators;

public class LogTimeValidator : AbstractValidator<LogTimeDto>
{
    public LogTimeValidator()
    {
        RuleFor(x => x.Hours)
            .GreaterThan(0).WithMessage("Hours must be greater than zero.")
            .LessThanOrEqualTo(168).WithMessage("Hours per entry cannot exceed 168.");
    }
}
