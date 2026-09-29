namespace Atlas.Voting;

/// <summary>
/// Reports the current aggregate and participant-specific voting state
/// for one vote target.
/// </summary>
public sealed record VoteSummary(
    int VoteCount,
    double? AverageVote,
    int? CurrentParticipantVote);
