using Atlas.Voting.Value;
using Newtonsoft.Json.Linq;

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
