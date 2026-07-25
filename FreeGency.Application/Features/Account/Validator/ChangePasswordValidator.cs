using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Features.Account.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Validator
{
    public class ChangePasswordValidator:AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).Matches(RegexPattern.pattern)
           .WithMessage("Password should be  at least 8 digits and should contains Lowercase,NonAlphanumeric and Uppercase");
            RuleFor(x => x.ConfirmPassword).NotEmpty()
                                 .Equal(x => x.NewPassword).WithMessage("Password and Confirm Password must match");

        }
    }
}
