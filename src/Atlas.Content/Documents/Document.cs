using Atlas.Content.Blocks;

namespace Atlas.Content.Documents;

public sealed class Document
{
    private readonly List<BlockId> _blockIds;

    public DocumentId Id { get; }
    public IReadOnlyList<BlockId> BlockIds => _blockIds.AsReadOnly();
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Document(IEnumerable<BlockId> blockIds, DateTimeOffset createdAt)
        : this(DocumentId.New(), blockIds, createdAt, createdAt)
    {
    }

    private Document(
        DocumentId id,
        IEnumerable<BlockId> blockIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(blockIds);

        if (updatedAt < createdAt)
        {
            throw new ArgumentException(
                "Updated time cannot precede created time.",
                nameof(updatedAt));
        }

        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _blockIds = blockIds.ToList();

        if (_blockIds.Count != _blockIds.Distinct().Count())
        {
            throw new ArgumentException("A document cannot contain the same block more than once.", nameof(blockIds));
        }
    }

    public void AddBlock(
        BlockId blockId,
        DateTimeOffset changedAt,
        int? index = null)
    {
        if (_blockIds.Contains(blockId))
        {
            throw new InvalidOperationException("The document already contains this block.");
        }

        var insertionIndex = index ?? _blockIds.Count;

        if (insertionIndex < 0 || insertionIndex > _blockIds.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _blockIds.Insert(insertionIndex, blockId);
        ChangedAt(changedAt);
    }

    public void MoveBlock(
        BlockId blockId,
        int newIndex,
        DateTimeOffset changedAt)
    {
        var currentIndex = _blockIds.IndexOf(blockId);

        if (currentIndex < 0)
        {
            throw new InvalidOperationException("The document does not contain this block.");
        }

        if (newIndex < 0 || newIndex >= _blockIds.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex));
        }

        if (currentIndex == newIndex)
        {
            return;
        }

        _blockIds.RemoveAt(currentIndex);
        _blockIds.Insert(newIndex, blockId);
        ChangedAt(changedAt);
    }

    public void RemoveBlock(BlockId blockId, DateTimeOffset changedAt)
    {
        if (!_blockIds.Remove(blockId))
        {
            throw new InvalidOperationException("The document does not contain this block.");
        }

        ChangedAt(changedAt);
    }

    public static Document Reconstitute(
        DocumentId id,
        IEnumerable<BlockId> blockIds,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(id, blockIds, createdAt, updatedAt);

    private void ChangedAt(DateTimeOffset changedAt)
    {
        if (changedAt < UpdatedAt)
        {
            throw new ArgumentException(
                "Changed time cannot precede the current updated time.",
                nameof(changedAt));
        }

        UpdatedAt = changedAt;
    }
}
