namespace Atlas.ConsoleApp;

/// <summary>
/// Contains voting information prepared for displaying a node.
/// </summary>
public sealed record NodeVoteSummary(
    int VoteCount,
    double? AverageVote,
    int? CurrentParticipantVote);