namespace Atlas.Moderation;

public enum ModerationStatus { Submitted, Dismissed, Actioned }
public enum ModerationDecision { Dismiss, HideNode }
public enum PublicModerationReason { Other, Spam, Harassment, UnsafeContent, OffTopic }

/// <summary>A report and its immutable final decision about a Node, identified across boundaries by ID.</summary>
public sealed class ModerationCase
{
    public Guid Id { get; }
    public Guid NodeId { get; }
    public Guid ReporterId { get; }
    public string Reason { get; }
    public string? Explanation { get; }
    public string ReportedTitle { get; }
    public DateTimeOffset CreatedAt { get; }
    public ModerationStatus Status { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public PublicModerationReason PublicReason { get; private set; }
    public DateTimeOffset? ReviewRequestedAt { get; private set; }
    public DateTimeOffset? VisibilityRestoredAt { get; private set; }
    public Guid? RestoredBy { get; private set; }
    public string? RestorationReason { get; private set; }
    public bool IsHidden => Status == ModerationStatus.Actioned && VisibilityRestoredAt is null;

    public ModerationCase(Guid nodeId, Guid reporterId, string reason, string? explanation,
        string reportedTitle, DateTimeOffset createdAt)
        : this(Guid.NewGuid(), nodeId, reporterId, reason, explanation, reportedTitle,
            createdAt, ModerationStatus.Submitted, null, null, null) { }

    private ModerationCase(Guid id, Guid nodeId, Guid reporterId, string reason, string? explanation,
        string reportedTitle, DateTimeOffset createdAt, ModerationStatus status,
        Guid? reviewerId, string? decisionReason, DateTimeOffset? decidedAt,
        PublicModerationReason publicReason = PublicModerationReason.Other,
        DateTimeOffset? reviewRequestedAt = null, DateTimeOffset? visibilityRestoredAt = null,
        Guid? restoredBy = null, string? restorationReason = null)
    {
        if (id == Guid.Empty || nodeId == Guid.Empty || reporterId == Guid.Empty)
            throw new ArgumentException("Case, Node, and reporter identifiers are required.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 100)
            throw new ArgumentException("A reason of at most 100 characters is required.", nameof(reason));
        if (explanation?.Length > 2000) throw new ArgumentException("Explanation is too long.", nameof(explanation));
        if (string.IsNullOrWhiteSpace(reportedTitle)) throw new ArgumentException("A reported title is required.", nameof(reportedTitle));
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (!Enum.IsDefined(publicReason)) throw new ArgumentOutOfRangeException(nameof(publicReason));
        if (status != ModerationStatus.Submitted &&
            (reviewerId is null || reviewerId == Guid.Empty || string.IsNullOrWhiteSpace(decisionReason) || decidedAt < createdAt))
            throw new ArgumentException("Decided cases require a reviewer, rationale, and valid decision time.");
        if (status == ModerationStatus.Submitted && (reviewerId is not null || decidedAt is not null || decisionReason is not null))
            throw new ArgumentException("Pending cases cannot have a decision.");
        if (reviewRequestedAt is not null && (status != ModerationStatus.Actioned || reviewRequestedAt <= decidedAt))
            throw new ArgumentException("Review requests require an upheld decision.");
        if (visibilityRestoredAt is not null &&
            (reviewRequestedAt is null || visibilityRestoredAt < reviewRequestedAt ||
             restoredBy is null || restoredBy == Guid.Empty || string.IsNullOrWhiteSpace(restorationReason)))
            throw new ArgumentException("Restoration requires a review request and moderator rationale.");
        Id = id; NodeId = nodeId; ReporterId = reporterId; Reason = reason.Trim();
        Explanation = explanation?.Trim(); ReportedTitle = reportedTitle.Trim(); CreatedAt = createdAt;
        Status = status; ReviewerId = reviewerId; DecisionReason = decisionReason; DecidedAt = decidedAt;
        PublicReason = publicReason; ReviewRequestedAt = reviewRequestedAt;
        VisibilityRestoredAt = visibilityRestoredAt;
        RestoredBy = restoredBy; RestorationReason = restorationReason;
    }

    public static ModerationCase Reconstitute(Guid id, Guid nodeId, Guid reporterId, string reason,
        string? explanation, string reportedTitle, DateTimeOffset createdAt, ModerationStatus status,
        Guid? reviewerId, string? decisionReason, DateTimeOffset? decidedAt,
        PublicModerationReason publicReason = PublicModerationReason.Other,
        DateTimeOffset? reviewRequestedAt = null, DateTimeOffset? visibilityRestoredAt = null,
        Guid? restoredBy = null, string? restorationReason = null) =>
        new(id, nodeId, reporterId, reason, explanation, reportedTitle, createdAt,
            status, reviewerId, decisionReason, decidedAt, publicReason, reviewRequestedAt,
            visibilityRestoredAt, restoredBy, restorationReason);

    public void Decide(Guid reviewerId, ModerationDecision decision, string rationale, DateTimeOffset decidedAt,
        PublicModerationReason publicReason = PublicModerationReason.Other)
    {
        if (Status != ModerationStatus.Submitted) throw new InvalidOperationException("Case already decided.");
        if (reviewerId == Guid.Empty) throw new ArgumentException("Reviewer is required.", nameof(reviewerId));
        if (string.IsNullOrWhiteSpace(rationale) || rationale.Trim().Length > 2000)
            throw new ArgumentException("A rationale of at most 2000 characters is required.", nameof(rationale));
        if (decidedAt < CreatedAt) throw new ArgumentException("Decision cannot precede report.", nameof(decidedAt));
        if (!Enum.IsDefined(decision)) throw new ArgumentOutOfRangeException(nameof(decision));
        if (!Enum.IsDefined(publicReason)) throw new ArgumentOutOfRangeException(nameof(publicReason));
        ReviewerId = reviewerId; DecisionReason = rationale.Trim(); DecidedAt = decidedAt;
        PublicReason = publicReason;
        Status = decision == ModerationDecision.Dismiss ? ModerationStatus.Dismissed : ModerationStatus.Actioned;
    }

    public void RequestReview(DateTimeOffset requestedAt)
    {
        if (!IsHidden || ReviewRequestedAt is not null || requestedAt <= DecidedAt)
            throw new InvalidOperationException("This Node is not eligible for review.");
        ReviewRequestedAt = requestedAt;
    }

    public void RestoreVisibility(Guid moderatorId, string rationale, DateTimeOffset restoredAt)
    {
        if (!IsHidden || ReviewRequestedAt is null || restoredAt < ReviewRequestedAt)
            throw new InvalidOperationException("A pending review request is required.");
        if (moderatorId == Guid.Empty || string.IsNullOrWhiteSpace(rationale) || rationale.Length > 2000)
            throw new ArgumentException("A moderator and restoration rationale are required.");
        VisibilityRestoredAt = restoredAt;
        RestoredBy = moderatorId; RestorationReason = rationale.Trim();
    }
}
