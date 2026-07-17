using FluentValidation;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Validations.AuthenticationValidator
{
    public class RefreshTokenRequestValidatior:AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestValidatior()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}
