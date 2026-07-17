using FluentValidation;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Validations.AuthenticationValidator
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName)
               .NotEmpty()
               .WithMessage("First name is required.")
               .MinimumLength(3);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MinimumLength(3);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address.");
            RuleFor(x => x.Password)
                 .NotEmpty()
                 .Matches(RegexPattern.pattern)
                 .WithMessage("Password should be  at least 8 digits and should contains Lowercase,NonAlphanumeric and Uppercase");
            RuleFor(x => x.Mode)
                .NotEmpty()
                .WithMessage("Mode is required.");
        }
    }
}
