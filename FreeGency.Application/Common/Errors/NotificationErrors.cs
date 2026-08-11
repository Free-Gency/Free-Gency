using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class NotificationErrors
    {
        public static readonly Error SettingNotificationNotFound = new Error("Setting. Setting Notfication Not Found", "Setting Notfication Not Found", StatusCodes.Status404NotFound);
        public static readonly Error NotificationNotFound = new Error("Notfication. Notfication Not Found", " Notfication Not Found", StatusCodes.Status404NotFound);

    }
}
