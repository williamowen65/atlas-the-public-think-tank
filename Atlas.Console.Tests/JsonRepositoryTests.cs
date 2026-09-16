using Atlas.ConsoleApp.Storage;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
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


        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task JsonVoteRepository_SeparateAdapters_StoreOneCurrentVote()
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
                var firstRepository =
                    new JsonVoteRepository(filePath);

                var secondRepository =
                    new JsonVoteRepository(filePath);

                var target =
                    new NodeVoteTarget(Guid.NewGuid());

                var participantId =
                    new ParticipantId(Guid.NewGuid());

                await CastCompetingVotes(
                    CreateCastVote(firstRepository),
                    CreateCastVote(secondRepository),
                    target,
                    participantId);

                var reloadedRepository =
                    new JsonVoteRepository(filePath);

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

        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task JsonVoteRepository_AfterSeparateAdapterWrites_CountsParticipantOnce()
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
                var firstRepository =
                    new JsonVoteRepository(filePath);

                var secondRepository =
                    new JsonVoteRepository(filePath);

                var target =
                    new NodeVoteTarget(Guid.NewGuid());

                var participantId =
                    new ParticipantId(Guid.NewGuid());

                await CastCompetingVotes(
                    CreateCastVote(firstRepository),
                    CreateCastVote(secondRepository),
                    target,
                    participantId);

                var summary =
                    new GetVoteSummary(
                        new JsonVoteRepository(filePath))
                        .Execute(target);

                Assert.AreEqual(
                    1,
                    summary.VoteCount);

                Assert.IsTrue(
                    summary.AverageVote is 4.0 or 10.0);
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

        private static CastVote CreateCastVote(
            IVoteRepository repository)
        {
            return new CastVote(
                repository,
                new VoteMutationPolicy(
                    new AlwaysEligibleVotingContext()));
        }

        private static async Task CastCompetingVotes(
            CastVote firstCastVote,
            CastVote secondCastVote,
            VoteTarget target,
            ParticipantId participantId)
        {
            using var ready =
                new CountdownEvent(2);

            using var startGate =
                new ManualResetEventSlim(false);

            var firstRequest = Task.Run(() =>
            {
                ready.Signal();
                startGate.Wait();

                return firstCastVote.Execute(
                    target,
                    participantId,
                    4);
            });

            var secondRequest = Task.Run(() =>
            {
                ready.Signal();
                startGate.Wait();

                return secondCastVote.Execute(
                    target,
                    participantId,
                    10);
            });

            if (!ready.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException(
                    "Both JSON vote tasks did not become ready.");
            }

            startGate.Set();

            await Task.WhenAll(
                firstRequest,
                secondRequest);
        }

        private sealed class AlwaysEligibleVotingContext :
            IVotingEligibility
        {
            public bool IsEligible(
                ParticipantId participantId)
            {
                return true;
            }

            public bool IsAvailable(
                VoteTarget target)
            {
                return true;
            }
        }

    }
}
