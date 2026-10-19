using Atlas.Comments.Comments;
using Atlas.Contracts.Events;
using Atlas.Contracts.Notifications.V1;
using Atlas.Graph.Nodes;
using Atlas.Moderation;

namespace Atlas.Application.Comments;

/// <summary>Commit comment operations through Comments, then request the appropriate notification.</summary>
public sealed class CommentWorkflowService
{
    private readonly CommentService _service;
    private readonly ICommentRepository _comments;
    private readonly INodeRepository _nodes;
    private readonly IEventPublisher _events;
    public CommentWorkflowService(ICommentRepository comments, INodeRepository nodes, ModerationService moderation, IEventPublisher events)
    {
        _comments = comments; _nodes = nodes; _events = events;
        _service = new CommentService(comments, new NodeCommentTargetAvailability(nodes), new CommentModeratorAuthorization(moderation));
    }
    /// <summary>Read one sibling page; replies are requested separately when the host expands a comment.</summary>
    public Task<CommentPage> GetPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default) =>
        _service.GetPageAsync(request, cancellationToken);
    public Comment AddTopLevel(CommentTarget target, Guid actor, string body, DateTimeOffset at)
    {
        var comment = _service.AddTopLevel(target, actor, body, at); Notify(comment); return comment;
    }
    public Comment Reply(CommentId parent, Guid actor, string body, DateTimeOffset at)
    {
        var comment = _service.Reply(parent, actor, body, at); Notify(comment); return comment;
    }
    public void Edit(CommentId id, Guid actor, string body, DateTimeOffset at) => _service.Edit(id, actor, body, at);
    public void Remove(CommentId id, Guid actor, DateTimeOffset at) => _service.Remove(id, actor, at);
    private void Notify(Comment created)
    {
        var node = _nodes.GetById(new NodeId(created.Target.Id)) ?? throw new InvalidOperationException("Comment target node not found.");
        var parent = created.ParentCommentId is null ? null : _comments.GetById(created.ParentCommentId.Value);
        _events.Publish(new NotificationRequestedV1(created.Id.Value, parent?.AuthorParticipantId ?? node.AuthorId.Value,
            created.AuthorParticipantId, parent is null ? "NodeCommented" : "CommentReplied", "Node", node.Id.Value, created.CreatedAt));
    }
}
