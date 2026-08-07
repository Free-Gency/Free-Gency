using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IJwtProvider
    {
        (string token, int expiresIn) GenerateToken(User user);

        /// <param name="validateLifetime">
        /// When false, signature is checked but expiry is ignored (needed for refresh/revoke).
        /// </param>
        Guid? ValidateToken(string token, bool validateLifetime = true);
    }
}
