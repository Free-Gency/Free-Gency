using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendMassege(string Email, string Messege, string? reason);
    }
}
