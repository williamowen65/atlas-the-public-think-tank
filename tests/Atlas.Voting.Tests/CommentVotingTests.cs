using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests;

/// <summary>Verifies the comment voting policy and score.</summary>
[TestClass]
public sealed class CommentVotingTests
{
    [TestMethod]
    public void CastVote_OnComment_AcceptsUpvoteAndDownvote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new CommentVoteTarget(Guid.NewGuid());

        var upvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Upvote
        );
        var downvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Downvote
        );

        Assert.IsInstanceOfType<CommentVote>(upvote.Value);
        Assert.AreEqual(
            CommentVote.Upvote,
            upvote.Value.Value
        );
        Assert.AreEqual(
            CommentVote.Downvote,
            downvote.Value.Value
        );
    }

    [TestMethod]
    [DataRow(-2)]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(10)]
    public void CastVote_OnComment_RejectsValuesOutsideUpOrDown(int value)
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            castVote.Execute(
                new CommentVoteTarget(Guid.NewGuid()),
                new ParticipantId(Guid.NewGuid()),
                value
            ));
    }

    [TestMethod]
    public void GetCommentVoteSummary_ReturnsUpvotesMinusDownvotes()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new CommentVoteTarget(Guid.NewGuid());

        castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Upvote
        );
        castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Upvote
        );
        castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Upvote
        );
        castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Downvote
        );

        var summary = new GetCommentVoteSummary(repository).Execute(target);

        Assert.AreEqual(
            2,
            summary.Score
        );
    }

    [TestMethod]
    public void CastVote_AgainOnSameComment_ChangesExistingVote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new CommentVoteTarget(Guid.NewGuid());
        var participantId = new ParticipantId(Guid.NewGuid());

        var original = castVote.Execute(
            target,
            participantId,
            CommentVote.Upvote
        );
        var changed = castVote.Execute(
            target,
            participantId,
            CommentVote.Downvote
        );

        Assert.AreEqual(
            original.Id,
            changed.Id
        );
        Assert.HasCount(
            1,
            repository.GetTargetVotes(target)
        );
        Assert.AreEqual(
            -1,
            new GetCommentVoteSummary(repository).Execute(target).Score
        );
    }

    [TestMethod]
    public void ReplacementComment_StartsWithIndependentScore()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var original = new CommentVoteTarget(Guid.NewGuid());
        var replacement = new CommentVoteTarget(Guid.NewGuid());

        castVote.Execute(
            original,
            new ParticipantId(Guid.NewGuid()),
            CommentVote.Upvote
        );

        var originalSummary = new GetCommentVoteSummary(repository).Execute(original);
        var replacementSummary = new GetCommentVoteSummary(repository).Execute(replacement);

        Assert.AreEqual(
            1,
            originalSummary.Score
        );
        Assert.AreEqual(
            0,
            replacementSummary.Score
        );
        Assert.HasCount(
            1,
            repository.GetTargetVotes(original)
        );
        Assert.HasCount(
            0,
            repository.GetTargetVotes(replacement)
        );
    }

    [TestMethod]
    public void NodeAndCommentTargets_WithSameId_RemainIndependent()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var sharedId = Guid.NewGuid();
        var participantId = new ParticipantId(Guid.NewGuid());

        castVote.Execute(
            new NodeVoteTarget(sharedId),
            participantId,
            8
        );
        castVote.Execute(
            new CommentVoteTarget(sharedId),
            participantId,
            CommentVote.Upvote
        );

        Assert.HasCount(
            1,
            repository.GetTargetVotes(new NodeVoteTarget(sharedId))
        );
        Assert.HasCount(
            1,
            repository.GetTargetVotes(new CommentVoteTarget(sharedId))
        );
    }

    private static CastVote CreateCastVote(IVoteRepository repository)
    {
        return new CastVote(
            repository,
            new VoteMutationPolicy(new AlwaysEligibleVoting())
        );
    }

    private sealed class AlwaysEligibleVoting : IVotingEligibility
    {
        public bool IsEligible(ParticipantId participantId) => true;

        public bool IsAvailable(VoteTarget target) => true;
    }
}
