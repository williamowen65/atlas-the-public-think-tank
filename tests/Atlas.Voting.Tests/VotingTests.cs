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
