using FoundIt.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class ExternalErrors
    {
        public static readonly Error ExternalLoginFailed = new(
                          "External.LoginFailed",
                          "Failed to retrieve external login information.",
                          StatusCodes.Status400BadRequest);

        public static readonly Error ExternalEmailNotFound = new(
                "External.EmailNotFound",
                "The external provider did not return an email address.",
                StatusCodes.Status400BadRequest);
    }
}
