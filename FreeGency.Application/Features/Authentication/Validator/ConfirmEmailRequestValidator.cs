using FluentValidation;
using FreeGency.Application.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Validator
{
    public class ConfirmEmailRequestValidator:AbstractValidator<ConfirmEmailRequestDto>
    {
        public ConfirmEmailRequestValidator()
        {
            RuleFor(x => x.code).NotEmpty();
            RuleFor(x => x.userId).NotEmpty();
        }
    }
}
