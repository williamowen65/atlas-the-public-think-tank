using Atlas.ConsoleApp.Storage;
using Atlas.Voting;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;


namespace Atlas.Persistence.Tests;
[TestClass]
public sealed class SqlVoteRepositoryTests
{
    [TestMethod]
    public void SqlVoteRepository_SavedVoteCanBeReloaded()
    {
        using var database = SqlTestDatabase.Create();

                var repository =
                    new SqlVoteRepository(database.Open);

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
    [TestMethod]
    public void SqlVoteRepository_NodeReactionVoteCanBeReloaded()
    {
        using var database = SqlTestDatabase.Create();

                var repository = new SqlVoteRepository(database.Open);
                var original = new Vote(
                    new NodeReactionVoteTarget(Guid.NewGuid()),
                    new ParticipantId(Guid.NewGuid()),
                    1);

                repository.Save(original);
                var reloaded = repository.GetById(original.Id);

                Assert.IsNotNull(reloaded);
                Assert.IsInstanceOfType<NodeReactionVoteTarget>(reloaded.Target);
                Assert.AreEqual(1, reloaded.Value.Value);
            
    }
    [TestMethod]
    public void SqlVoteRepository_ReloadedVoteCanBeFoundByParticipantAndTarget()
    {
        using var database = SqlTestDatabase.Create();

                var participantId =
                    new ParticipantId(Guid.NewGuid());

                var target =
                    new NodeVoteTarget(Guid.NewGuid());

                var originalRepository =
                    new SqlVoteRepository(database.Open);

                var originalVote =
                    new Vote(target, participantId, 7);

                originalRepository.Save(originalVote);

                var reloadedRepository =
                    new SqlVoteRepository(database.Open);

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
    [TestMethod]
    public void SqlVoteRepository_ChangedVoteReplacesStoredRecord()
    {
        using var database = SqlTestDatabase.Create();

                var repository =
                    new SqlVoteRepository(database.Open);

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
                    new SqlVoteRepository(database.Open);

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
    [TestMethod]
    public void SqlVoteRepository_UndoneVoteIsAbsentAfterReload()
    {
        using var database = SqlTestDatabase.Create();

                var repository =
                    new SqlVoteRepository(database.Open);

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
                    new SqlVoteRepository(database.Open);

                Assert.IsNull(
                    reloadedRepository.GetById(vote.Id));

                Assert.HasCount(
                    0,
                    reloadedRepository.GetTargetVotes(target));
            
    }
}