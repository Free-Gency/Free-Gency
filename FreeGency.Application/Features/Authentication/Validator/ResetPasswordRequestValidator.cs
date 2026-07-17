using FluentValidation;
using FreeGency.Application.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Validator
{
    public class ResetPasswordRequestValidator:AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
        }
    }
}
