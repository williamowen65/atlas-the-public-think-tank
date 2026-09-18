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

        /// <summary>
        /// Verifies that participant-target lookup works after votes
        /// are loaded by a new JSON repository instance.
        /// </summary>
        [TestMethod]
        public void JsonVoteRepository_ReloadedVoteCanBeFoundByParticipantAndTarget()
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
                var participantId =
                    new ParticipantId(Guid.NewGuid());

                var target =
                    new NodeVoteTarget(Guid.NewGuid());

                var originalRepository =
                    new JsonVoteRepository(filePath);

                var originalVote =
                    new Vote(target, participantId, 7);

                originalRepository.Save(originalVote);

                var reloadedRepository =
                    new JsonVoteRepository(filePath);

                var reloadedVote =
                    reloadedRepository.GetByParticipantAndTarget(
                        participantId,
                        target);

                Assert.IsNotNull(reloadedVote);

                Assert.AreEqual(
                    originalVote.Id,
                    reloadedVote.Id);

                Assert.AreEqual(
                    7,
                    reloadedVote.Value.Value);
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

        /// <summary>
        /// Verifies that changing a vote replaces its persisted value
        /// while preserving identity and creation time.
        /// </summary>
        [TestMethod]
        public void JsonVoteRepository_ChangedVoteReplacesStoredRecord()
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
                    new Vote(
                        target,
                        participantId,
                        3);

                repository.Save(originalVote);

                var originalCreatedAt =
                    originalVote.CreatedAt;

                originalVote.ChangeValue(
                    9,
                    originalVote.UpdatedAt.AddMinutes(1));

                repository.Save(originalVote);

                var changedVote =
                    originalVote;

                var reloadedRepository =
                    new JsonVoteRepository(filePath);

                var reloadedVote =
                    reloadedRepository.GetById(
                        changedVote.Id);

                Assert.IsNotNull(reloadedVote);

                Assert.AreEqual(
                    originalVote.Id,
                    reloadedVote.Id);

                Assert.AreEqual(
                    originalCreatedAt,
                    reloadedVote.CreatedAt);

                Assert.AreEqual(
                    9,
                    reloadedVote.Value.Value);

                Assert.IsTrue(
                    reloadedVote.UpdatedAt >
                    originalCreatedAt);

                Assert.HasCount(
                    1,
                    reloadedRepository.GetTargetVotes(target));
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

        /// <summary>
        /// Verifies that undo physically removes the current JSON record.
        /// </summary>
        [TestMethod]
        public void JsonVoteRepository_UndoneVoteIsAbsentAfterReload()
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

                var vote = new Vote(
                    target,
                    participantId,
                    7);

                repository.Save(vote);
                repository.Delete(vote.Id);

                var reloadedRepository =
                    new JsonVoteRepository(filePath);

                Assert.IsNull(
                    reloadedRepository.GetById(vote.Id));

                Assert.HasCount(
                    0,
                    reloadedRepository.GetTargetVotes(target));
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
