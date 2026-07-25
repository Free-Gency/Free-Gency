using FluentValidation;
using FreeGency.Application.Common.Validators;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Validator;

public class UpdateClientAccountValidation : AbstractValidator<UpdateClientAccountDto>
{
    public UpdateClientAccountValidation()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .WithMessage("Bio must not exceed 1000 characters.");

        UploadFileValidator.ApplyRules(this, x => x.ProfileImage, UploadFileKind.Image, maxSizeInMb: 5);
    }
}
