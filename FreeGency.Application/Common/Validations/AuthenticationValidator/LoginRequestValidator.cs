using FluentValidation;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Validations.AuthenticationValidator
{
    public class LoginRequestValidator:AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
