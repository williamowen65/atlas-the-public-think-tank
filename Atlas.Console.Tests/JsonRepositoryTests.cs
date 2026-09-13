using Atlas.ConsoleApp.Storage;
using Atlas.Voting;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Console.Tests
{
    [TestClass]
    public sealed class JsonRepositoryTests
    {

        /*
         * This test verifies the complete round trip:
                 Vote
                → ToStorage
                → StoredVote
                → JSON file
                → StoredVote
                → ToDomain
                → Vote.Reconstitute
                → reloaded Vote
         */
        [TestMethod]
        public void JsonVoteRepository_SavedVoteCanBeReloaded()
        {
            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "AtlasVotingTests",
                Guid.NewGuid().ToString());

            var filePath = Path.Combine(
                temporaryDirectory,
                "votes.json");

            try
            {
                var repository =
                    new JsonVoteRepository(filePath);

                var target =
                    new NodeVoteTarget(Guid.NewGuid());

                var participantId =
                    new ParticipantId(Guid.NewGuid());

                var originalVote =
                    new Vote(target, participantId, 7);

                repository.Save(originalVote);

                var reloadedVote =
                    repository.GetById(originalVote.Id);

                Assert.IsNotNull(reloadedVote);

                Assert.AreEqual(
                    originalVote.Id,
                    reloadedVote.Id);

                Assert.AreEqual(
                    originalVote.ParticipantId.Id,
                    reloadedVote.ParticipantId.Id);

                Assert.AreEqual(
                    originalVote.Target.Id,
                    reloadedVote.Target.Id);

                Assert.IsInstanceOfType<NodeVoteTarget>(
                    reloadedVote.Target);

                Assert.AreEqual(
                    originalVote.Value.Value,
                    reloadedVote.Value.Value);

                Assert.AreEqual(
                    originalVote.CreatedAt,
                    reloadedVote.CreatedAt);

                Assert.AreEqual(
                    originalVote.UpdatedAt,
                    reloadedVote.UpdatedAt);
            }
            finally
            {
                if (Directory.Exists(temporaryDirectory))
                {
                    Directory.Delete(
                        temporaryDirectory,
                        recursive: true);
                }
            }
        }
    }
}
