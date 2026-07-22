using FreeGency.Application.Common.Models;

public static class EmailErrors
{
    public static readonly Error MessageNotSend =
     new(
         "Email.MessageNotSend",
         "Failed to send email",
         StatusCodes.Status500InternalServerError
     );
}
