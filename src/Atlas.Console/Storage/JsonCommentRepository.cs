using System.Text.Json;
using Atlas.Comments.Comments;

namespace Atlas.ConsoleApp.Storage;

/// <summary>JSON adapter for the Comments boundary while Atlas uses file persistence.</summary>
public sealed class JsonCommentRepository : ICommentRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonCommentRepository(string filePath) => _filePath = filePath;

    public Comment? GetById(CommentId id)
    {
        var stored = Read().SingleOrDefault(x => x.Id == id.Value);
        return stored is null ? null : ToDomain(stored);
    }

    public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
        Read().Where(x => x.TargetKind == target.Kind && x.TargetId == target.Id)
            .Select(ToDomain).ToList();

    public void Save(Comment comment)
    {
        var items = Read();
        var replacement = new StoredComment
        {
            Id = comment.Id.Value,
            TargetKind = comment.Target.Kind,
            TargetId = comment.Target.Id,
            ParentCommentId = comment.ParentCommentId?.Value,
            AuthorParticipantId = comment.AuthorParticipantId,
            Body = comment.Body,
            Status = comment.Status.ToString(),
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            RemovedAt = comment.RemovedAt
        };
        var index = items.FindIndex(x => x.Id == replacement.Id);
        if (index >= 0) items[index] = replacement; else items.Add(replacement);
        JsonStorage.Write(_filePath, items, _options);
    }

    private List<StoredComment> Read() => JsonStorage.Read<List<StoredComment>>(_filePath, _options) ?? [];

    private static Comment ToDomain(StoredComment x) => Comment.Reconstitute(
        new CommentId(x.Id),
        new CommentTarget(x.TargetKind, x.TargetId),
        x.ParentCommentId.HasValue ? new CommentId(x.ParentCommentId.Value) : null,
        x.AuthorParticipantId,
        x.Body,
        Enum.Parse<CommentStatus>(x.Status, true),
        x.CreatedAt,
        x.UpdatedAt,
        x.RemovedAt);

    private sealed class StoredComment
    {
        public Guid Id { get; set; }
        public string TargetKind { get; set; } = string.Empty;
        public Guid TargetId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid AuthorParticipantId { get; set; }
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = nameof(CommentStatus.Active);
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? RemovedAt { get; set; }
    }
}
