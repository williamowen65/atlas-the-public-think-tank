namespace Atlas.Content.Blocks;

/// <summary>A stable reference to poll content that will be modeled separately.</summary>
public sealed class PollReferenceBlock : ContentBlock
{
    public override string Kind => "poll-reference";
    public Guid PollId { get; private set; }

    public PollReferenceBlock(Guid pollId, DateTimeOffset createdAt)
        : this(BlockId.New(), pollId, createdAt, createdAt) { }

    public PollReferenceBlock(DateTimeOffset createdAt)
        : this(BlockId.New(), Guid.NewGuid(), createdAt, createdAt) { }

    private PollReferenceBlock(BlockId id, Guid pollId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        PollId = pollId != Guid.Empty ? pollId : throw new ArgumentException("A poll ID is required.", nameof(pollId));
    }

    public void UpdateReference(Guid pollId, DateTimeOffset changedAt)
    {
        PollId = pollId != Guid.Empty ? pollId : throw new ArgumentException("A poll ID is required.", nameof(pollId));
        ChangedAt(changedAt);
    }

    public static PollReferenceBlock Reconstitute(BlockId id, Guid pollId, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, pollId, createdAt, updatedAt);
}
