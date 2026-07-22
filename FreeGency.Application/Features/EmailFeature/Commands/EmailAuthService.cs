using FreeGency.Application.Features.EmailFeature.Dtos;

namespace FreeGency.Application.Features.EmailFeature.Commands
{
    public class EmailAuthService(IEmailService emailService) : IEmailAuthService
    {
        public async Task<Result<string>> SendEmail(SendEmailRequestDto dto)
        {
            var response = await emailService.SendMassege(dto.email, dto.message, null);
            if (response) return Result.Success("Message sended");
            return Result.Failure<string>(EmailErrors.MessageNotSend);
        }
    }
}
