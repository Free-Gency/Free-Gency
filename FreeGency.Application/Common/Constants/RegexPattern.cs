using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Constants
{
    public static class RegexPattern
    {
        public const string pattern = "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[@#$%^&*!])[A-Za-z\\d@#$%^&*!]{8,}$";
    }
}
