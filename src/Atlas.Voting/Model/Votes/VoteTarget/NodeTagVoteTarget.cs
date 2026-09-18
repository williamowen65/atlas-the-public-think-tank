namespace Atlas.Voting.Target;

/// <summary>
/// Identifies one node-specific tag association as a voting target.
/// </summary>
public sealed class NodeTagVoteTarget : VoteTarget
{
    public NodeTagVoteTarget(Guid id) : base(id)
    {
    }
}
