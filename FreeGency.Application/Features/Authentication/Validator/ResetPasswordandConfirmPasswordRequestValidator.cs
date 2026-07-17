using FluentValidation;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Validator
{
    public class ResetPasswordandConfirmPasswordRequestValidator:AbstractValidator<ResetPasswordandConfirmPasswordRequestDto>
    {
        public ResetPasswordandConfirmPasswordRequestValidator()
        {
            RuleFor(x => x.email).NotEmpty();
            RuleFor(x => x.password)
                .NotEmpty()
                .Matches(RegexPattern.pattern)
                .WithMessage("Password should be  at least 8 digits and should contains Lowercase,NonAlphanumeric and Uppercase");
            RuleFor(x => x.confirmPassword).NotEmpty()
                                   .Equal(x => x.password).WithMessage("Password and Confirm Password must match");

        }
    }
}
