using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests;

/// <summary>Verifies the node-specific tag voting policy and score.</summary>
[TestClass]
public sealed class NodeReactionVotingTests
{
    [TestMethod]
    public void CastVote_OnNodeReaction_AcceptsUpvoteAndDownvote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeReactionVoteTarget(Guid.NewGuid());

        var upvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            NodeReactionVote.Upvote);
        var downvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            NodeReactionVote.Downvote);

        Assert.IsInstanceOfType<NodeReactionVote>(upvote.Value);
        Assert.AreEqual(NodeReactionVote.Upvote, upvote.Value.Value);
        Assert.AreEqual(NodeReactionVote.Downvote, downvote.Value.Value);
    }

    [TestMethod]
    [DataRow(-2)]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(10)]
    public void CastVote_OnNodeReaction_RejectsValuesOutsideUpOrDown(int value)
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            castVote.Execute(
                new NodeReactionVoteTarget(Guid.NewGuid()),
                new ParticipantId(Guid.NewGuid()),
                value));
    }

    [TestMethod]
    public void GetNodeReactionVoteSummary_ReturnsUpvotesMinusDownvotes()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeReactionVoteTarget(Guid.NewGuid());

        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeReactionVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeReactionVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeReactionVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeReactionVote.Downvote);

        var summary = new GetNodeReactionVoteSummary(repository).Execute(target);

        Assert.AreEqual(2, summary.Score);
    }

    [TestMethod]
    public void CastVote_AgainOnSameNodeReaction_ChangesExistingVote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeReactionVoteTarget(Guid.NewGuid());
        var participantId = new ParticipantId(Guid.NewGuid());

        var original = castVote.Execute(target, participantId, NodeReactionVote.Upvote);
        var changed = castVote.Execute(target, participantId, NodeReactionVote.Downvote);

        Assert.AreEqual(original.Id, changed.Id);
        Assert.HasCount(1, repository.GetTargetVotes(target));
        Assert.AreEqual(-1, new GetNodeReactionVoteSummary(repository).Execute(target).Score);
    }

    [TestMethod]
    public void ReplacementNodeReaction_StartsWithIndependentScore()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var original = new NodeReactionVoteTarget(Guid.NewGuid());
        var replacement = new NodeReactionVoteTarget(Guid.NewGuid());

        castVote.Execute(original, new ParticipantId(Guid.NewGuid()), NodeReactionVote.Upvote);

        var originalSummary = new GetNodeReactionVoteSummary(repository).Execute(original);
        var replacementSummary = new GetNodeReactionVoteSummary(repository).Execute(replacement);

        Assert.AreEqual(1, originalSummary.Score);
        Assert.AreEqual(0, replacementSummary.Score);
        Assert.HasCount(1, repository.GetTargetVotes(original));
        Assert.HasCount(0, repository.GetTargetVotes(replacement));
    }

    [TestMethod]
    public void NodeAndNodeReactionTargets_WithSameId_RemainIndependent()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var sharedId = Guid.NewGuid();
        var participantId = new ParticipantId(Guid.NewGuid());

        castVote.Execute(new NodeVoteTarget(sharedId), participantId, 8);
        castVote.Execute(new NodeReactionVoteTarget(sharedId), participantId, NodeReactionVote.Upvote);

        Assert.HasCount(1, repository.GetTargetVotes(new NodeVoteTarget(sharedId)));
        Assert.HasCount(1, repository.GetTargetVotes(new NodeReactionVoteTarget(sharedId)));
    }

    private static CastVote CreateCastVote(IVoteRepository repository)
    {
        return new CastVote(
            repository,
            new VoteMutationPolicy(new AlwaysEligibleVoting()));
    }

    private sealed class AlwaysEligibleVoting : IVotingEligibility
    {
        public bool IsEligible(ParticipantId participantId) => true;

        public bool IsAvailable(VoteTarget target) => true;
    }
}
