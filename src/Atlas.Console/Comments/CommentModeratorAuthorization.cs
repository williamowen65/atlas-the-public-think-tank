using Atlas.Comments.Comments;
using Atlas.Moderation;

namespace Atlas.ConsoleApp.Comments;

/// <summary>Supplies Comments with the authoritative Atlas moderation decision.</summary>
public sealed class CommentModeratorAuthorization(ModerationService moderation)
    : ICommentModeratorAuthorization
{
    public bool CanModerate(Guid participantId) => moderation.CanModerate(participantId);
}
