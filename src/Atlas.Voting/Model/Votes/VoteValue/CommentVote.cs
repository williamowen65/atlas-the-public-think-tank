namespace Atlas.Voting.Value;

/// <summary>Support or opposition for one comment.</summary>
public sealed class CommentVote : IVoteValue
{
    public const int Downvote = -1;
    public const int Upvote = 1;
    public int Value { get; }
    public CommentVote(int value)
    {
        if (value is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(value), "Use +1 or -1.");
        Value = value;
    }
}
