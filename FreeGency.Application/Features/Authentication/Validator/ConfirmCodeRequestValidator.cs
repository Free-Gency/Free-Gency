using FluentValidation;
using FreeGency.Application.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Validator
{
    public class ConfirmCodeRequestValidator:AbstractValidator<ConfirmCodeRequestDto>
    {
        public ConfirmCodeRequestValidator()
        {
            RuleFor(x => x.code).NotEmpty();
            RuleFor(x => x.email).NotEmpty();
        }
    }
}
