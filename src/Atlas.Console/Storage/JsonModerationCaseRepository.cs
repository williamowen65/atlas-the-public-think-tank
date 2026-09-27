using System.Text.Json;
using Atlas.Moderation;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Prototype JSON storage for cases; Moderation owns the data, not Graph.</summary>
public sealed class JsonModerationCaseRepository : IModerationCaseRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonModerationCaseRepository(string path) => _path = path;

    public ModerationCase? GetById(Guid caseId) => GetAll().SingleOrDefault(x => x.Id == caseId);

    public IReadOnlyCollection<ModerationCase> GetAll() =>
        (JsonStorage.Read<List<StoredCase>>(_path, _options) ?? []).Select(x =>
            ModerationCase.Reconstitute(x.Id, x.NodeId, x.ReporterId, x.Reason,
                x.Explanation, x.ReportedTitle, x.CreatedAt, x.Status,
                x.ReviewerId, x.DecisionReason, x.DecidedAt, x.PublicReason,
                x.ReviewRequestedAt, x.VisibilityRestoredAt, x.RestoredBy, x.RestorationReason)).ToList();

    public void Save(ModerationCase item)
    {
        var stored = JsonStorage.Read<List<StoredCase>>(_path, _options) ?? [];
        var replacement = new StoredCase(item.Id, item.NodeId, item.ReporterId,
            item.Reason, item.Explanation, item.ReportedTitle, item.CreatedAt,
            item.Status, item.ReviewerId, item.DecisionReason, item.DecidedAt,
            item.PublicReason, item.ReviewRequestedAt, item.VisibilityRestoredAt,
            item.RestoredBy, item.RestorationReason);
        var index = stored.FindIndex(x => x.Id == item.Id);
        if (index < 0) stored.Add(replacement); else stored[index] = replacement;
        JsonStorage.Write(_path, stored, _options);
    }

    private sealed record StoredCase(Guid Id, Guid NodeId, Guid ReporterId,
        string Reason, string? Explanation, string ReportedTitle, DateTimeOffset CreatedAt,
        ModerationStatus Status, Guid? ReviewerId, string? DecisionReason, DateTimeOffset? DecidedAt,
        PublicModerationReason PublicReason, DateTimeOffset? ReviewRequestedAt, DateTimeOffset? VisibilityRestoredAt,
        Guid? RestoredBy, string? RestorationReason);
}
