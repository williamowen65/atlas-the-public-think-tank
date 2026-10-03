using System.Data.Common;
using Atlas.ConsoleApp.Storage;
using Atlas.Participants.Participants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlValidationTests
{
    [TestMethod]
    public void SqlLookingTextIsStoredAndQueriedAsParameterData()
    {
        using var database = SqlTestDatabase.Create();
        using var context = database.Open();
        const string name = "O'Brien'; DROP TABLE ParticipantRows;--";
        var probe = new ParameterProbe(name);
        var options = new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer(context.Database.GetConnectionString()!)
            .AddInterceptors(probe)
            .Options;
        AtlasDataContext Open() => new(options);
        var participants = new SqlParticipantRepository(Open);
        var participant = new Participant(name, "Unicode 水; SELECT * FROM Nodes;", DateTimeOffset.UtcNow);
        participants.Save(participant);

        probe.Reset();
        var found = new QueryProbe(Open).FindByName(name);
        Assert.AreEqual(participant.Id.Value, found.Single().Id);
        Assert.IsTrue(probe.SawParameterizedValue, "The SQL query must bind supplied text as a parameter.");
        var loaded = participants.GetById(participant.Id)!;
        Assert.AreEqual(name, loaded.DisplayName);
        Assert.AreEqual(participant.Bio, loaded.Bio);
        participants.Save(new Participant("Normal participant", DateTimeOffset.UtcNow));
        Assert.AreEqual(2, participants.GetAll().Count);
    }

    // Exercises the same expression-based query helper used by production SQL repositories.
    private sealed class QueryProbe(Func<AtlasDataContext> open) : SqlRepository(open)
    {
        public IReadOnlyList<ParticipantRow> FindByName(string name) =>
            QueryRows<ParticipantRow>(row => row.DisplayName == name);
    }

    private sealed class ParameterProbe(string suppliedText) : DbCommandInterceptor
    {
        public bool SawParameterizedValue { get; private set; }
        public void Reset() => SawParameterizedValue = false;

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Assert.IsFalse(command.CommandText.Contains(suppliedText, StringComparison.Ordinal),
                "User text must not be concatenated into a SQL command.");
            if (command.Parameters.Cast<DbParameter>().Any(parameter => Equals(parameter.Value, suppliedText)))
                SawParameterizedValue = true;
            return result;
        }
    }
}
