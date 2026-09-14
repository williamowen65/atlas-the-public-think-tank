using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;
using Atlas.Voting.Data;

namespace Atlas.Voting.Tests
{
    [TestClass]
    public sealed class VotingTests
    {
        [TestMethod]
        public void CastVote_SavesValidVote()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = new CastVote(repository);

            var target = new NodeVoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            var vote = castVote.Execute(
                target,
                participantId,
                7);

            Assert.AreSame(
                vote,
                repository.GetById(vote.Id));
        }

        /// <summary>
        /// Verifies that one participant cannot cast multiple votes
        /// against the same target.
        /// </summary>
        [TestMethod]
        public void CastVote_WhenParticipantAlreadyVoted_ThrowsException()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = new CastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                participantId,
                7);

            Assert.Throws<InvalidOperationException>(() =>
            {
                castVote.Execute(
                    target,
                    participantId,
                    9);
            });

            var targetVotes =  repository.GetTargetVotes(target);

            Assert.HasCount(
                1,
                targetVotes);
        }

        /// <summary>
        /// Verifies that different participants can vote on the
        /// same target independently.
        /// </summary>
        [TestMethod]
        public void CastVote_WithDifferentParticipants_SavesBothVotes()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = new CastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var firstParticipantId =
                new ParticipantId(Guid.NewGuid());

            var secondParticipantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                firstParticipantId,
                8);

            castVote.Execute(
                target,
                secondParticipantId,
                4);

            var targetVotes =
                repository.GetTargetVotes(target);

            Assert.HasCount(
                2,
                targetVotes);

            var firstVote =
                repository.GetByParticipantAndTarget(
                    firstParticipantId,
                    target);

            var secondVote =
                repository.GetByParticipantAndTarget(
                    secondParticipantId,
                    target);

            Assert.IsNotNull(firstVote);
            Assert.IsNotNull(secondVote);

            Assert.AreEqual(
                8,
                firstVote.Value.Value);

            Assert.AreEqual(
                4,
                secondVote.Value.Value);
        }

        /// <summary>
        /// Verifies that one participant may vote independently
        /// on different targets.
        /// </summary>
        [TestMethod]
        public void CastVote_SameParticipantDifferentTargets_SavesBothVotes()
        {
            var repository =
                new InMemoryVoteRepository();

            var castVote =
                new CastVote(repository);

            var participantId =
                new ParticipantId(Guid.NewGuid());

            var firstTarget =
                new NodeVoteTarget(Guid.NewGuid());

            var secondTarget =
                new NodeVoteTarget(Guid.NewGuid());

            castVote.Execute(
                firstTarget,
                participantId,
                8);

            castVote.Execute(
                secondTarget,
                participantId,
                6);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(firstTarget));

            Assert.HasCount(
                1,
                repository.GetTargetVotes(secondTarget));
        }

        /// <summary>
        /// Verifies that an unvoted target reports zero votes and no average.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithNoVotes_ReturnsEmptySummary()
        {
            var repository = new InMemoryVoteRepository();
            var getVoteSummary = new GetVoteSummary(repository);

            var summary = getVoteSummary.Execute(
                new NodeVoteTarget(Guid.NewGuid()));

            Assert.AreEqual(
                0,
                summary.VoteCount);

            Assert.IsNull(
                summary.AverageVote);

            Assert.IsNull(
                summary.CurrentParticipantVote);
        }

        /// <summary>
        /// Verifies that the summary includes only votes belonging
        /// to the requested target.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithMultipleTargets_AggregatesRequestedTarget()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = new CastVote(repository);
            var getVoteSummary = new GetVoteSummary(repository);

            var requestedTarget =
                new NodeVoteTarget(Guid.NewGuid());

            var otherTarget =
                new NodeVoteTarget(Guid.NewGuid());

            castVote.Execute(
                requestedTarget,
                new ParticipantId(Guid.NewGuid()),
                8);

            castVote.Execute(
                requestedTarget,
                new ParticipantId(Guid.NewGuid()),
                4);

            castVote.Execute(
                otherTarget,
                new ParticipantId(Guid.NewGuid()),
                10);

            var summary =
                getVoteSummary.Execute(requestedTarget);

            Assert.AreEqual(
                2,
                summary.VoteCount);

            Assert.AreEqual(
                6.0,
                summary.AverageVote);
        }

        /// <summary>
        /// Verifies that the summary reports only the requested
        /// participant's current vote.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithParticipant_ReturnsThatParticipantsVote()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = new CastVote(repository);
            var getVoteSummary = new GetVoteSummary(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var requestedParticipant =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                requestedParticipant,
                8);

            castVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()),
                4);

            var summary = getVoteSummary.Execute(
                target,
                requestedParticipant);

            Assert.AreEqual(
                8,
                summary.CurrentParticipantVote);
        }

        [TestMethod]
        public void Vote_WithNodeTarget_CreatesNodeRating()
        {
            var target = new NodeVoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            var vote = new Vote(target, participantId, 7);

            Assert.IsInstanceOfType<NodeRating>(vote.Value);
            Assert.AreEqual(7, vote.Value.Value);
        }

        [TestMethod]
        public void Vote_WithUnsupportedTarget_ThrowsException()
        {
            var unsupportedTarget = new VoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            Assert.Throws<ArgumentException>(() =>
            {
                new Vote(unsupportedTarget, participantId, 7);
            });
        }


        [TestMethod]
        public void Vote_WithEmptyGuid_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var target = new NodeVoteTarget(Guid.Empty);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var participantId = new ParticipantId(Guid.Empty);
            });
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(10)]
        public void NodeRating_CanOnlyBeBetween0to10Inclusive(int value)
        {
            NodeRating nodeRating = new NodeRating(value);

            Assert.AreEqual(value,nodeRating.Value);

        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(11)]
        public void NodeRating_FailsOutside0to10Inclusive(int value)
        {
           
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                NodeRating nodeRating = new NodeRating(value);
            });
        }

        [TestMethod]
        public void NodeRating_ValueHasNoSetter()
        {
            var valueProperty = typeof(NodeRating)
                .GetProperty(nameof(NodeRating.Value));

            Assert.IsNotNull(valueProperty);
            Assert.IsNull(valueProperty.GetSetMethod(nonPublic: true));
        }

    }
}
