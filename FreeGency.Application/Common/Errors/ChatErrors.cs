using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class ChatErrors
    {
        public static readonly Error ProposalNotFound =
        new(
            "Chat.ProposalNotFound",
            "Proposal not found.",
            StatusCodes.Status404NotFound
        );
        public static readonly Error OnlyProjectClientCanStartDiscussion =
            new(
                "Chat.OnlyProjectClientCanStartDiscussion",
                "Only the project owner can start a discussion.",
                StatusCodes.Status403Forbidden
            );
        public static readonly Error ChatRoomNotFound =
            new(
                "Chat.ChatRoomNotFound",
                "Chat room not found.",
                StatusCodes.Status404NotFound
            );

        public static readonly Error MessageNotFound =
            new(
                "Chat.MessageNotFound",
                "Message not found.",
                StatusCodes.Status404NotFound
            );

        public static readonly Error DiscussionAlreadyExists =
            new(
                "Chat.DiscussionAlreadyExists",
                "A discussion already exists for this proposal.",
                StatusCodes.Status409Conflict
            );

        public static readonly Error DiscussionNotAllowed =
            new(
                "Chat.DiscussionNotAllowed",
                "You are not allowed to start a discussion for this proposal.",
                StatusCodes.Status403Forbidden
            );

        public static readonly Error UserNotMember =
            new(
                "Chat.UserNotMember",
                "You are not a member of this chat room.",
                StatusCodes.Status403Forbidden
            );

        public static readonly Error CannotSendMessage =
            new(
                "Chat.CannotSendMessage",
                "You are not allowed to send messages in this chat room.",
                StatusCodes.Status403Forbidden
            );

        public static readonly Error ChatRoomArchived =
            new(
                "Chat.ChatRoomArchived",
                "This chat room has been archived.",
                StatusCodes.Status400BadRequest
            );
        public static readonly Error InvalidProposalStatus =
    new(
        "Chat.InvalidProposalStatus",
        "Discussion can only be started for pending proposals.",
        StatusCodes.Status400BadRequest
    );
        public static readonly Error TeamHasNoLeaders =
    new(
        "Chat.TeamHasNoLeaders",
        "The team has no leaders.",
        StatusCodes.Status400BadRequest
    );
        public static readonly Error MessageCannotBeEmpty =
    new(
        "Chat.MessageCannotBeEmpty",
        "Message must contain text or a file.",
        StatusCodes.Status400BadRequest
    );
    }
}
