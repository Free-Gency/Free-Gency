using FreeGency.Application.Features.EmailFeature.Dtos;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IEmailAuthService
    {
        Task<Result<string>> SendEmail(SendEmailRequestDto dto);
    }
}
