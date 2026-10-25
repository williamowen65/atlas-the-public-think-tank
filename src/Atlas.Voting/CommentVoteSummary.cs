namespace Atlas.Voting;

public sealed record CommentVoteSummary(int Score, int TotalVotes, int? CurrentParticipantVote);
