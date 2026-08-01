using FreeGency.Application.Features.ChatFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Validation
{
    public class SendMessageRequestValidator:AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Text) || x.File != null)
                .WithMessage("Either text or file must be provided");
        }
    }
}
