namespace FreeGency.AI.ChatModeration.Enums;

/// <summary>
/// The type of conversation the moderated message belongs to.
/// </summary>
public enum ConversationType
{
    /// <summary>A one-on-one private chat between two users.</summary>
    PrivateChat = 0,

    /// <summary>A chat inside a team or group.</summary>
    TeamChat = 1,

    /// <summary>A chat tied to a project.</summary>
    ProjectChat = 2,

    /// <summary>A chat tied to a proposal negotiation.</summary>
    ProposalChat = 3,

    /// <summary>A chat with customer support.</summary>
    SupportChat = 4
}
