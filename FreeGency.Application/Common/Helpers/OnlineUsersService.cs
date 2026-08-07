using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Helpers
{
    public class OnlineUsersService
    {
        public ConcurrentDictionary<Guid, HashSet<string>> Users = new();
    }
}
