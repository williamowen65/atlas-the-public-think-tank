using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests;

/// <summary>Verifies the node-specific tag voting policy and score.</summary>
[TestClass]
public sealed class NodeTagVotingTests
{
    [TestMethod]
    public void CastVote_OnNodeTag_AcceptsUpvoteAndDownvote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeTagVoteTarget(Guid.NewGuid());

        var upvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            NodeTagVote.Upvote);
        var downvote = castVote.Execute(
            target,
            new ParticipantId(Guid.NewGuid()),
            NodeTagVote.Downvote);

        Assert.IsInstanceOfType<NodeTagVote>(upvote.Value);
        Assert.AreEqual(NodeTagVote.Upvote, upvote.Value.Value);
        Assert.AreEqual(NodeTagVote.Downvote, downvote.Value.Value);
    }

    [TestMethod]
    [DataRow(-2)]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(10)]
    public void CastVote_OnNodeTag_RejectsValuesOutsideUpOrDown(int value)
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            castVote.Execute(
                new NodeTagVoteTarget(Guid.NewGuid()),
                new ParticipantId(Guid.NewGuid()),
                value));
    }

    [TestMethod]
    public void GetNodeTagVoteSummary_ReturnsUpvotesMinusDownvotes()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeTagVoteTarget(Guid.NewGuid());

        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeTagVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeTagVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeTagVote.Upvote);
        castVote.Execute(target, new ParticipantId(Guid.NewGuid()), NodeTagVote.Downvote);

        var summary = new GetNodeTagVoteSummary(repository).Execute(target);

        Assert.AreEqual(2, summary.Score);
    }

    [TestMethod]
    public void CastVote_AgainOnSameNodeTag_ChangesExistingVote()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var target = new NodeTagVoteTarget(Guid.NewGuid());
        var participantId = new ParticipantId(Guid.NewGuid());

        var original = castVote.Execute(target, participantId, NodeTagVote.Upvote);
        var changed = castVote.Execute(target, participantId, NodeTagVote.Downvote);

        Assert.AreEqual(original.Id, changed.Id);
        Assert.HasCount(1, repository.GetTargetVotes(target));
        Assert.AreEqual(-1, new GetNodeTagVoteSummary(repository).Execute(target).Score);
    }

    [TestMethod]
    public void ReplacementNodeTag_StartsWithIndependentScore()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var original = new NodeTagVoteTarget(Guid.NewGuid());
        var replacement = new NodeTagVoteTarget(Guid.NewGuid());

        castVote.Execute(original, new ParticipantId(Guid.NewGuid()), NodeTagVote.Upvote);

        var originalSummary = new GetNodeTagVoteSummary(repository).Execute(original);
        var replacementSummary = new GetNodeTagVoteSummary(repository).Execute(replacement);

        Assert.AreEqual(1, originalSummary.Score);
        Assert.AreEqual(0, replacementSummary.Score);
        Assert.HasCount(1, repository.GetTargetVotes(original));
        Assert.HasCount(0, repository.GetTargetVotes(replacement));
    }

    [TestMethod]
    public void NodeAndNodeTagTargets_WithSameId_RemainIndependent()
    {
        var repository = new InMemoryVoteRepository();
        var castVote = CreateCastVote(repository);
        var sharedId = Guid.NewGuid();
        var participantId = new ParticipantId(Guid.NewGuid());

        castVote.Execute(new NodeVoteTarget(sharedId), participantId, 8);
        castVote.Execute(new NodeTagVoteTarget(sharedId), participantId, NodeTagVote.Upvote);

        Assert.HasCount(1, repository.GetTargetVotes(new NodeVoteTarget(sharedId)));
        Assert.HasCount(1, repository.GetTargetVotes(new NodeTagVoteTarget(sharedId)));
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
