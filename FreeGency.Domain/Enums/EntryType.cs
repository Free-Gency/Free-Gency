using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Enums
{
    public enum EntryType
    {
        TopUp=0, EscrowLock, EscrowRelease, PendingCredit, AvailableCredit, Withdrawal, PlatformFee, TeamSplit, Refund
    }
}
