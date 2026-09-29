namespace Atlas.Voting.Target;

/// <summary>
/// Identifies one node-specific tag association as a voting target.
/// </summary>
public sealed class NodeReactionVoteTarget : VoteTarget
{
    public NodeReactionVoteTarget(Guid id) : base(id)
    {
    }
}
