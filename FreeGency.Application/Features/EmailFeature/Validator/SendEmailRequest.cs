using FluentValidation;
using FreeGency.Application.Features.EmailFeature.Dtos;
namespace FreeGency.Application.Features.EmailFeature.Validator
{
    public class SendEmailRequest:AbstractValidator<SendEmailRequestDto>
    {
        public SendEmailRequest()
        {
            RuleFor(x => x.email).NotEmpty();
            RuleFor(x => x.message).NotEmpty();
        }
    }
}
