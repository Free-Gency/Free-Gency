using FoundIt.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class EmailErrors
    {
        public static readonly Error MessageNotSend =
         new(
             "Email.MessageNotSend",
             "Failed to send email",
             StatusCodes.Status500InternalServerError
         );
    }
}
