namespace Atlas.Voting.Value;

/// <summary>
/// Represents support (+1) or opposition (-1) for one node tag.
/// </summary>
public sealed class NodeTagVote : IVoteValue
{
    public const int Downvote = -1;
    public const int Upvote = 1;

    public int Value { get; }

    public NodeTagVote(int value)
    {
        if (value is not Downvote and not Upvote)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "A node-tag vote must be an upvote (+1) or downvote (-1).");
        }

        Value = value;
    }
}
