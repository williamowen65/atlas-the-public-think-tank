namespace Atlas.Voting.Target;

/// <summary>Identifies a comment independently of its containing node.</summary>
public sealed class CommentVoteTarget(Guid id) : VoteTarget(id);
