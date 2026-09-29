namespace Atlas.Voting;

/// <summary>
/// Reports the net score and participant-specific vote for one node reaction.
/// </summary>
public sealed record NodeReactionVoteSummary(
    int Score,
    int TotalVotes,
    int? CurrentParticipantVote);
