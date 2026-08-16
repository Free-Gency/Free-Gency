namespace FreeGency.Application.Common.Helpers;

public sealed class HiringAgentOptions
{
    public const string NameSection = "HiringAgent";

    public int DefaultInviteWindowHours { get; set; } = 24;
    public int DefaultDiscussionWindowHours { get; set; } = 48;
    public int MinInviteWindowHours { get; set; } = 1;
    public int MaxInviteWindowHours { get; set; } = 72;
    public int MinDiscussionWindowHours { get; set; } = 2;
    public int MaxDiscussionWindowHours { get; set; } = 168;
    public int MinAcceptorsToCloseInvites { get; set; } = 2;
}
