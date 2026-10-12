using Atlas.ConsoleApp;
using Atlas.ConsoleApp.Storage;
using Atlas.ConsoleApp.Eventing;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
[DoNotParallelize] // One regression test temporarily redirects process-wide Console IO.
public sealed class SqlNodeCreationQuotaTests
{
    private static readonly DateTimeOffset Now = new(
        2026,
        10,
        12,
        15,
        0,
        0,
        TimeSpan.Zero
    );
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static NodeTypeDefinition AddType(SqlTestDatabase database)
    {
        var type = NodeTypeDefinition.CreateSystemDefined(
            "Idea",
            "Ideas",
            Now
        );
        new SqlNodeTypeRepository(database.Open).Save(type);
        return type;
    }

    [TestMethod]
    public void ConsoleCreationWithNoCommentTypeAndNoRequestedTypesSucceeds()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new Clock(Now));
        var actor = database.AddParticipant();
        AddType(database);
        var originalIn = System.Console.In;
        var originalOut = System.Console.Out;
        // Select the sole actual Node type, then leave requested child types blank.
        using var input = new StringReader("TEST NODE\ntest node\n1\n\n\n");
        using var output = new StringWriter();
        try
        {
            System.Console.SetIn(input);
            System.Console.SetOut(output);
            var node = NodeCreationWorkflow.Create(
                new SqlNodeRepository(database.Open),
                new SqlNodeTypeRepository(database.Open),
                new SqlDocumentRepository(database.Open),
                actor,
                new InMemoryEventPublisher());
            Assert.IsNotNull(node);
            Assert.IsEmpty(node.RequestedSubNodeTypes);
            using var check = database.Open();
            Assert.AreEqual(1, check.NodeRows.Count());
            Assert.AreEqual(1, check.NodeCreationQuotaRows.Count());
        }
        finally
        {
            System.Console.SetIn(originalIn);
            System.Console.SetOut(originalOut);
        }
    }

    [TestMethod]
    public void ExhaustionArchiveExpiryAndParticipantIsolation()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new Clock(Now));
        var actor = database.AddParticipant();
        var other = database.AddParticipant();
        var type = AddType(database);
        var nodes = new SqlNodeRepository(database.Open);
        var service = new NodeCreationService(
            nodes,
            new SqlDocumentRepository(database.Open)
        );
        Node? first = null;
        for (var i = 0; i < 3; i++)
        {
            var created = service.Create(
                $"Node {i}",
                "Description",
                type.Id,
                actor,
                []
            );
            first ??= created;
        }

        // Successful commits consume allowance; archiving is a metadata edit, not a refund.
        first!.Archive(
            actor,
            Now
        );
        nodes.SaveMetadata(first);
        Assert.AreEqual(
            0,
            nodes.GetCreationAllowance(actor).Remaining
        );
        Assert.ThrowsExactly<NodeCreationQuotaExceededException>(() =>
            service.Create(
                "Too many",
                "Description",
                type.Id,
                actor,
                []
            ));
        Assert.AreEqual(
            3,
            nodes.GetCreationAllowance(other).Remaining
        );
        using (var check = database.Open())
        {
            Assert.AreEqual(
                3,
                check.NodeCreationQuotaRows.Count()
            );
            Assert.AreEqual(
                3,
                check.BlockRows.Count()
            );
            Assert.AreEqual(
                3,
                check.DocumentRows.Count()
            );
        }

        // At the exact cutoff all three simultaneous entries expire.
        using var later = AtlasTime.Use(new Clock(Now.AddHours(2)));
        Assert.AreEqual(
            3,
            nodes.GetCreationAllowance(actor).Remaining
        );
        service.Create(
            "After expiry",
            "Description",
            type.Id,
            actor,
            []
        );
    }

    [TestMethod]
    public void OuterRollbackRestoresQuotaAndRelatedRows()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new Clock(Now));
        var actor = database.AddParticipant();
        var type = AddType(database);
        var nodes = new SqlNodeRepository(database.Open);
        var service = new NodeCreationService(
            nodes,
            new SqlDocumentRepository(database.Open)
        );

        // Throw after all saves, including the ledger. The outer transaction owns commit.
        Assert.ThrowsExactly<InvalidOperationException>(() => nodes.ExecuteTopology<bool>(() =>
        {
            service.Create(
                "Rolled back",
                "Description",
                type.Id,
                actor,
                []
            );
            throw new InvalidOperationException("Injected later workflow failure");
        }));
        using var check = database.Open();
        Assert.AreEqual(
            0,
            check.NodeCreationQuotaRows.Count()
        );
        Assert.AreEqual(
            0,
            check.NodeRows.Count()
        );
        Assert.AreEqual(
            0,
            check.DocumentRows.Count()
        );
        Assert.AreEqual(
            0,
            check.BlockRows.Count()
        );
        Assert.AreEqual(
            3,
            nodes.GetCreationAllowance(actor).Remaining
        );
    }

    [TestMethod]
    public async Task ConcurrentRequestsCannotBothTakeFinalSlot()
    {
        using var database = SqlTestDatabase.Create();
        var actor = database.AddParticipant();
        var type = AddType(database);
        var policy = new NodeCreationQuotaPolicy(
            1,
            TimeSpan.FromHours(2)
        );
        using var ready = new CountdownEvent(2);
        using var start = new ManualResetEventSlim();

        Task<bool> Request(string title) => Task.Run(() =>
        {
            using var clock = AtlasTime.Use(new Clock(Now));
            // Each task has independent repositories/contexts. The gate starts competing
            // requests; SQL's graph lock coordinates the actual check-and-consume work.
            var nodes = new SqlNodeRepository(
                database.Open,
                policy
            );
            var service = new NodeCreationService(
                nodes,
                new SqlDocumentRepository(database.Open)
            );
            ready.Signal();
            if (!start.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException();
            try { service.Create(
                title,
                "Description",
                type.Id,
                actor,
                []
            ); return true; }
            catch (NodeCreationQuotaExceededException) { return false; }
        });
        var a = Request("A");
        var b = Request("B");
        try { Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(30))); }
        finally { start.Set(); }
        var results = await Task.WhenAll(
            a,
            b
        ).WaitAsync(TimeSpan.FromSeconds(60));
        Assert.AreEqual(
            1,
            results.Count(success => success)
        );
        using var check = database.Open();
        Assert.AreEqual(
            1,
            check.NodeRows.Count()
        );
        Assert.AreEqual(
            1,
            check.NodeCreationQuotaRows.Count()
        );
        Assert.AreEqual(
            1,
            check.BlockRows.Count()
        );
    }

    [TestMethod]
    public void DirectRepositoryCreationUsesServerTimeAndMetadataDoesNotConsume()
    {
        using var database = SqlTestDatabase.Create();
        using var clock = AtlasTime.Use(new Clock(Now));
        var actor = database.AddParticipant();
        var type = AddType(database);
        var nodes = new SqlNodeRepository(
            database.Open,
            new NodeCreationQuotaPolicy(
                1,
                TimeSpan.FromHours(2)
            )
        );
        var docs = new SqlDocumentRepository(database.Open);
        var created = new NodeCreationService(
            nodes,
            docs
        ).Create(
            "Original",
            "Description",
            type.Id,
            actor,
            []
        );
        // A caller-supplied old Node timestamp cannot create an old quota timestamp.
        var next = new Node(
            new NodeTitle("Backdated"),
            created.DescriptionId,
            type.Id,
            new NodeAuthorId(actor),
            [],
            Now.AddDays(-1)
        );
        Assert.ThrowsExactly<NodeCreationQuotaExceededException>(() => nodes.Save(next));
        nodes.SaveMetadata(created);
        using var check = database.Open();
        Assert.AreEqual(
            1,
            check.NodeCreationQuotaRows.Count()
        );
        Assert.AreEqual(
            Now,
            check.NodeCreationQuotaRows.Single().CreatedAt
        );
    }
}
