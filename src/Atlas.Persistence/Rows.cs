namespace Atlas.Persistence;

/// <summary>EF Core persistence rows; domain aggregates and rules remain in their domain projects.</summary>
public sealed class NodeRow
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? DescriptionId { get; set; }
    public Guid? TypeId { get; set; }
    public Guid? AuthorId { get; set; }
    public List<NodeRequestedTypeRow> RequestedTypes { get; set; } = [];
    public List<Guid>? RequestedSubNodeTypeIds
    {
        get => RequestedTypes.OrderBy(row => row.Position).Select(row => row.TypeId).ToList();
        set => RequestedTypes = (value ?? []).Select((id, position) => new NodeRequestedTypeRow { NodeId = Id, Position = position, TypeId = id }).ToList();
    }
    public List<NodeParentRow> Parents { get; set; } = [];
    public List<Guid>? ParentNodeIds
    {
        get => Parents.OrderBy(row => row.Position).Select(row => row.ParentNodeId).ToList();
        set => Parents = (value ?? []).Select((id, position) => new NodeParentRow { NodeId = Id, Position = position, ParentNodeId = id }).ToList();
    }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class NodeTypeRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public bool IsSystemDefined { get; set; }
    public bool IsArchived { get; set; }
    public bool? AutoPluralize { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DocumentRow
{
    public Guid Id { get; set; }
    public List<DocumentBlockRow> Blocks { get; set; } = [];
    public List<Guid> BlockIds
    {
        get => Blocks.OrderBy(row => row.Position).Select(row => row.BlockId).ToList();
        set => Blocks = value.Select((id, position) => new DocumentBlockRow { DocumentId = Id, Position = position, BlockId = id }).ToList();
    }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class BlockRow
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? Markdown { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ParticipantRow
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class VoteRow
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ReactionDefinitionRow
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NormalizedText { get; set; } = string.Empty;
    public Guid CreatedByParticipantId { get; set; }
    public bool IsSuppressed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class NodeReactionRow
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid ReactionDefinitionId { get; set; }
    public Guid AppliedByParticipantId { get; set; }
    public string LifecycleState { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public List<ReactionAuditRow> AuditHistory { get; set; } = [];
}

public sealed class CommunityRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerParticipantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CommunityMembershipRow
{
    public Guid CommunityId { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
}

public sealed class CommunityNodeRow
{
    public Guid CommunityId { get; set; }
    public Guid NodeId { get; set; }
    public Guid AssociatedByParticipantId { get; set; }
    public DateTimeOffset AssociatedAt { get; set; }
}

public sealed class CommentRow
{
    public Guid Id { get; set; }
    public string TargetKind { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid AuthorParticipantId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
}

public sealed class ModerationCaseRow
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid ReporterId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string ReportedTitle { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public int Status { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? DecisionReason { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public int PublicReason { get; set; }
    public DateTimeOffset? ReviewRequestedAt { get; set; }
    public DateTimeOffset? VisibilityRestoredAt { get; set; }
    public Guid? RestoredBy { get; set; }
    public string? RestorationReason { get; set; }
}

public sealed class NotificationRow
{
    public Guid Id { get; set; }
    public Guid OccurrenceId { get; set; }
    public Guid RecipientParticipantId { get; set; }
    public int Category { get; set; }
    public bool InAppVisible { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string SubjectKind { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public Guid ActorParticipantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? DismissedAt { get; set; }
    public List<DeliveryAttemptRow> DeliveryAttempts { get; set; } = [];
}

public sealed class NotificationPreferencesRow
{
    public Guid ParticipantId { get; set; }
    public bool DiscussionInApp { get; set; }
    public bool ModerationInApp { get; set; }
    public bool DiscussionEmail { get; set; }
    public bool ModerationEmail { get; set; }
    public bool DiscussionPush { get; set; }
    public bool ModerationPush { get; set; }
}

public sealed class ReactionAuditRow
{
    public Guid NodeReactionId { get; set; }
    public int Position { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? ActorParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string LifecycleState { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public Guid? RelatedNodeReactionId { get; set; }
}

public sealed class DeliveryAttemptRow
{
    public Guid NotificationId { get; set; }
    public int Position { get; set; }
    public int Channel { get; set; }
    public int Status { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public string? Error { get; set; }
}

public sealed class NodeParentRow
{
    public Guid NodeId { get; set; }
    public int Position { get; set; }
    public Guid ParentNodeId { get; set; }
}
public sealed class NodeRequestedTypeRow
{
    public Guid NodeId { get; set; }
    public int Position { get; set; }
    public Guid TypeId { get; set; }
}
public sealed class DocumentBlockRow
{
    public Guid DocumentId { get; set; }
    public int Position { get; set; }
    public Guid BlockId { get; set; }
}
