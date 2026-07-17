using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IJwtProvider
    {
        (string token, int expiresIn) GenerateToken(User user);
        Guid? ValidateToken(string token);
    }
}
