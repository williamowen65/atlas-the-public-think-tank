using System.Text.Json;
using Atlas.Discovery;
using Atlas.Persistence;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Testing;

// Compiled into both the isolated developer host and Persistence tests.
public sealed record RankingScenario
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Query { get; init; } = "";
    public RankingEntry[] Entries { get; init; } = [];
    public RankingPhase[] Phases { get; init; } = [];
}
public sealed record RankingEntry
{
    public string Key { get; init; } = "";
    public string Kind { get; init; } = "node";
    public string Title { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public string[] Blocks { get; init; } = [];
    public string Body { get; init; } = "";
    public string? SameTextAs { get; init; }
    public int[] Votes { get; init; } = [];
    public DateTimeOffset At { get; init; } = DateTimeOffset.Parse("2026-01-01T12:00:00Z");
}
public sealed record RankingPhase
{
    public string Name { get; init; } = "";
    public Dictionary<string, int[]> Votes { get; init; } = [];
    public string[] ExpectedOrder { get; init; } = [];
    public string? SameScoresAs { get; init; }
}
public sealed record RankingCheck(string Description, bool Passed);
public sealed record RankingRow(int Position, int Page, string Key, string SearchText, int[] Votes,
    double? Average, int NetScore, SearchRankEvidence Evidence);
public sealed record RankingPhaseResult(string Name, RankingRow[] Rows, RankingCheck[] Checks);
public sealed record RankingResult(string Policy, string Corpus, RankingPhaseResult[] Phases);

public static class SearchRankingFixture
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static RankingScenario[] Load(string path) => JsonSerializer.Deserialize<RankingScenario[]>(File.ReadAllText(path), Json)!;

    public static void Validate(RankingScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (string.IsNullOrWhiteSpace(scenario.Query) || scenario.Query.Length > 200 || scenario.Entries.Length is < 1 or > 12 || scenario.Phases.Length is < 1 or > 8)
            throw new ArgumentException("Use a search term, 1–12 entries and 1–8 phases.");
        if (scenario.Entries.Select(e => e.Key).Distinct().Count() != scenario.Entries.Length || scenario.Entries.Any(e => string.IsNullOrWhiteSpace(e.Key)))
            throw new ArgumentException("Entry keys must be unique and nonempty.");
        if (scenario.Phases.Select(p => p.Name).Distinct().Count() != scenario.Phases.Length)
            throw new ArgumentException("Phase names must be unique.");
        foreach (var entry in scenario.Entries)
        {
            if (entry.Kind is not ("node" or "comment") || entry.Title.Length > 200 || entry.ShortDescription.Length > 280 || entry.Body.Length > 10000 || entry.Blocks.Length > 8 || entry.Blocks.Any(b => b.Length > 1000))
                throw new ArgumentException("An entry exceeds the fixture limits.");
            if (entry.Kind == "node" && string.IsNullOrWhiteSpace(entry.Title) || entry.Kind == "comment" && string.IsNullOrWhiteSpace(entry.Body) && string.IsNullOrEmpty(entry.SameTextAs))
                throw new ArgumentException("Nodes need titles; comments need bodies or a matching node.");
            if (!string.IsNullOrEmpty(entry.SameTextAs) && (entry.Kind != "comment" || !scenario.Entries.Any(e => e.Key == entry.SameTextAs && e.Kind == "node")))
                throw new ArgumentException("Identical-text comments must reference a node in the same fixture.");
            ValidateVotes(entry, entry.Votes);
        }
        var previous = new HashSet<string>();
        foreach (var phase in scenario.Phases)
        {
            foreach (var (key, votes) in phase.Votes)
                ValidateVotes(scenario.Entries.SingleOrDefault(e => e.Key == key) ?? throw new ArgumentException("Unknown vote target."), votes);
            if (phase.ExpectedOrder.Distinct().Count() != phase.ExpectedOrder.Length || phase.ExpectedOrder.Any(key => !scenario.Entries.Any(e => e.Key == key)))
                throw new ArgumentException("Expected order must reference unique fixture keys.");
            if (phase.SameScoresAs is { } baseline && !previous.Contains(baseline)) throw new ArgumentException("Score comparisons need an earlier phase.");
            previous.Add(phase.Name);
        }
    }

    private static void ValidateVotes(RankingEntry entry, int[] votes)
    {
        if (votes.Length > 50 || votes.Any(v => entry.Kind == "node" ? v is < 0 or > 10 : v is not (-1 or 1)))
            throw new ArgumentException("Use at most 50 votes: nodes 0–10; comments -1 or +1.");
    }

    public static RankingResult Run(Func<AtlasDataContext> open, RankingScenario scenario)
    {
        Validate(scenario);
        var at = DateTimeOffset.Parse("2026-01-01T12:00:00Z");
        var author = new Guid("10000000-0000-0000-0000-000000000001");
        var container = new Guid("10000000-0000-0000-0000-000000000002");
        var type = new Guid("10000000-0000-0000-0000-000000000003");
        var ids = scenario.Entries.Select((e, i) => (e.Key, Id: new Guid($"20000000-0000-0000-0000-{i + 1:000000000000}"))).ToDictionary(e => e.Key, e => e.Id);
        using (var db = open())
        {
            // Refuse to seed any populated database, even if a caller passes the wrong factory.
            if (db.NodeRows.Any() || db.ParticipantRows.Any()) throw new InvalidOperationException("Ranking fixtures require an empty disposable database.");
            db.ParticipantRows.Add(new ParticipantRow { Id = author, DisplayName = "Fixture author", Bio = "", IsActive = true, CreatedAt = at, UpdatedAt = at });
            for (var i = 0; i < 50; i++) db.ParticipantRows.Add(new ParticipantRow { Id = Voter(i), DisplayName = $"Voter {i}", Bio = "", IsActive = true, CreatedAt = at, UpdatedAt = at });
            db.NodeTypeRows.Add(new NodeTypeRow { Id = type, Name = "Idea", Description = "", IsSystemDefined = true, CreatedAt = at, UpdatedAt = at });
            db.NodeRows.Add(new NodeRow { Id = container, Title = "Discussion container", TypeId = type, AuthorId = author, Status = "Active", CreatedAt = at, UpdatedAt = at });
            foreach (var e in scenario.Entries.Where(e => e.Kind == "node"))
            {
                var document = Guid.NewGuid();
                db.DocumentRows.Add(new DocumentRow { Id = document, CreatedAt = e.At, UpdatedAt = e.At });
                for (var i = 0; i < e.Blocks.Length; i++)
                {
                    var block = Guid.NewGuid();
                    db.BlockRows.Add(new BlockRow { Id = block, Kind = "markdown", Markdown = e.Blocks[i], CreatedAt = e.At, UpdatedAt = e.At });
                    db.Set<DocumentBlockRow>().Add(new DocumentBlockRow { DocumentId = document, BlockId = block, Position = i });
                }
                db.NodeRows.Add(new NodeRow { Id = ids[e.Key], Title = e.Title, ShortDescription = e.ShortDescription, TypeId = type, AuthorId = author,
                    DescriptionId = document, Status = "Active", CreatedAt = e.At, UpdatedAt = e.At });
            }
            db.SaveChanges();
            var documents = ReadDocuments(db);
            foreach (var e in scenario.Entries.Where(e => e.Kind == "comment"))
                db.CommentRows.Add(new CommentRow { Id = ids[e.Key], TargetKind = "Node", TargetId = container, AuthorParticipantId = author,
                    Body = string.IsNullOrEmpty(e.SameTextAs) ? e.Body : documents[ids[e.SameTextAs]].SearchText, Status = "Active", CreatedAt = e.At, UpdatedAt = e.At });
            db.SaveChanges();
        }
        // Only the isolated test/workbench waits. Normal search never waits for population.
        using (var db = open()) SqlFullTextSupport.WaitForIndexing(db.Database.GetConnectionString()!);
        var activeVotes = scenario.Entries.ToDictionary(e => e.Key, e => e.Votes);
        var results = new List<RankingPhaseResult>();
        foreach (var phase in scenario.Phases)
        {
            foreach (var (key, votes) in phase.Votes) activeVotes[key] = votes;
            using (var db = open())
            {
                db.VoteRows.RemoveRange(db.VoteRows); db.SaveChanges();
                foreach (var e in scenario.Entries)
                    for (var i = 0; i < activeVotes[e.Key].Length; i++)
                        db.VoteRows.Add(new VoteRow { Id = Guid.NewGuid(), TargetType = e.Kind == "node" ? "Node" : "Comment", TargetId = ids[e.Key], ParticipantId = Voter(i),
                            Value = activeVotes[e.Key][i], CreatedAt = at, UpdatedAt = at });
                db.SaveChanges();
            }
            var search = new SqlPublicSearch(open);
            var rows = new List<RankingRow>();
            using var read = open();
            var documents = ReadDocuments(read);
            for (var page = 1; ; page++)
            {
                var explained = search.Explain(new(SearchText: scenario.Query, Page: page));
                foreach (var evidence in explained.Evidence)
                {
                    var key = ids.SingleOrDefault(pair => pair.Value == evidence.Id).Key;
                    if (key is null) throw new InvalidOperationException("The container unexpectedly matched; use a different search term.");
                    var entry = scenario.Entries.Single(e => e.Key == key);
                    var votes = read.VoteRows.Where(v => v.TargetType == (entry.Kind == "node" ? "Node" : "Comment") && v.TargetId == evidence.Id).OrderBy(v => v.ParticipantId).Select(v => v.Value).ToArray();
                    rows.Add(new(rows.Count + 1, page, key, documents[evidence.Id].SearchText, votes,
                        entry.Kind == "node" && votes.Length > 0 ? votes.Average() : null, entry.Kind == "comment" ? votes.Sum() : 0, evidence));
                }
                if (!explained.Page.HasNextPage) break;
            }
            var checks = new List<RankingCheck>
            {
                new("Every fixture result appears exactly once across all pages", rows.Count == scenario.Entries.Length && rows.Select(r => r.Key).Distinct().Count() == scenario.Entries.Length)
            };
            foreach (var entry in scenario.Entries.Where(e => !string.IsNullOrEmpty(e.SameTextAs)))
            {
                var comment = rows.SingleOrDefault(r => r.Key == entry.Key);
                var node = rows.SingleOrDefault(r => r.Key == entry.SameTextAs);
                checks.Add(new($"Identical text has equal raw relevance: {entry.Key} / {entry.SameTextAs}", comment is not null && node is not null && comment.Evidence.TextRank == node.Evidence.TextRank));
            }
            if (phase.ExpectedOrder.Length > 0) checks.Add(new("Expected order: " + string.Join(" → ", phase.ExpectedOrder), rows.Select(r => r.Key).SequenceEqual(phase.ExpectedOrder)));
            else checks.Add(new("SQL results follow descending final score", rows.Zip(rows.Skip(1)).All(pair => pair.First.Evidence.FinalScore >= pair.Second.Evidence.FinalScore)));
            if (phase.SameScoresAs is { } reference)
            {
                var baseline = results.Single(r => r.Name == reference);
                checks.Add(new("Scores and order restored to " + reference, rows.Select(r => (r.Key, r.Evidence.TextRank, r.Evidence.FinalScore)).SequenceEqual(baseline.Rows.Select(r => (r.Key, r.Evidence.TextRank, r.Evidence.FinalScore)))));
            }
            results.Add(new(phase.Name, rows.ToArray(), checks.ToArray()));
        }
        return new(SqlPublicSearch.RankingPolicyVersion, "Isolated fixture corpus; full-text ranks can differ from a larger Atlas database.", results.ToArray());
    }
    private static Guid Voter(int i) => new($"30000000-0000-0000-0000-{i + 1:000000000000}");
    private static Dictionary<Guid, Document> ReadDocuments(AtlasDataContext db) => db.Database.SqlQueryRaw<Document>("SELECT ResultId, SearchText FROM dbo.SearchDocuments").ToDictionary(d => d.ResultId);
    public sealed class Document { public Guid ResultId { get; set; } public string SearchText { get; set; } = ""; }
}
