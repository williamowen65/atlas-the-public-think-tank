using Atlas.Voting.Votes.Value;

namespace Atlas.Voting.Tests
{
    [TestClass]
    public sealed class VotingTests
    {
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(10)]
        public void NodeRating_CanOnlyBeBetween0to10Inclusive(int value)
        {
            var nodeRating = new NodeRating(value);

            Assert.IsNotNull(nodeRating);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(11)]
        public void NodeRating_FailsOutside0to10Inclusive(int value)
        {
           
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var nodeRating = new NodeRating(value);
            });
        }
    }
}
