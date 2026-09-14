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
