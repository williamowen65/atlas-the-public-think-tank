using Atlas.Content.Blocks;

namespace Atlas.Content.Documents;

public sealed class Document
{
    private readonly List<BlockId> _blockIds;

    public DocumentId Id { get; }
    public IReadOnlyList<BlockId> BlockIds => _blockIds.AsReadOnly();
    public DateTimeOffset CreatedAt { get; }

    public Document(IEnumerable<BlockId> blockIds, DateTimeOffset createdAt)
        : this(DocumentId.New(), blockIds, createdAt)
    {
    }

    private Document(DocumentId id, IEnumerable<BlockId> blockIds, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(blockIds);

        Id = id;
        CreatedAt = createdAt;
        _blockIds = blockIds.ToList();

        if (_blockIds.Count != _blockIds.Distinct().Count())
        {
            throw new ArgumentException("A document cannot contain the same block more than once.", nameof(blockIds));
        }
    }

    public void AddBlock(BlockId blockId, int? index = null)
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
    }

    public void MoveBlock(BlockId blockId, int newIndex)
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

        _blockIds.RemoveAt(currentIndex);
        _blockIds.Insert(newIndex, blockId);
    }

    public void RemoveBlock(BlockId blockId)
    {
        if (!_blockIds.Remove(blockId))
        {
            throw new InvalidOperationException("The document does not contain this block.");
        }
    }

    public static Document Reconstitute(DocumentId id, IEnumerable<BlockId> blockIds, DateTimeOffset createdAt) =>
        new(id, blockIds, createdAt);
}
