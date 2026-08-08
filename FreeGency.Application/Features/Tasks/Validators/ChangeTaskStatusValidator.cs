
namespace FreeGency.Application.Features.Tasks.Validators;

public class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusDto>
{
    public ChangeTaskStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}