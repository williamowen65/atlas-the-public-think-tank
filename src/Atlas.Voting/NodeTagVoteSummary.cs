namespace Atlas.Voting;

/// <summary>
/// Reports the net score and participant-specific vote for one node tag.
/// </summary>
public sealed record NodeTagVoteSummary(
    int Score,
    int? CurrentParticipantVote);
