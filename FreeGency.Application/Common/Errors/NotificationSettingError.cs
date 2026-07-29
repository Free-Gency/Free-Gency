using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class NotificationSettingError
    {
        public static Error SettingNotFound=
          new("Notification.SettingNotFound", "Notification Setting Not Found", StatusCodes.Status404NotFound);

    }
}
