namespace FreeGency.Domain.Enums;

public enum HiringAgentRunStatus
{
    Queued = 0,
    Inviting = 1,
    WaitingAccepts = 2,
    Discussing = 3,
    Ranking = 4,
    ReportReady = 5,
    Hired = 6,
    Cancelled = 7,
    Failed = 8,
    Dismissed = 9
}
