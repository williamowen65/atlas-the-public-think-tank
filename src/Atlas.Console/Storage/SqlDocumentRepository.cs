using Atlas.Persistence;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlDocumentRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IDocumentRepository
{
    public IReadOnlyCollection<Document> GetAll() => ReadRows<DocumentRow>().Select(ToDomain).ToList();
    public Document? GetById(DocumentId id) => FindRow<DocumentRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Document document) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(document);
        foreach (var block in document.BlockIds) ReferenceValidation.Require(this, "Block", block.Value, false);
        SaveRow(ToStorage(document));
        return true;
    });
    public ContentBlock? GetBlockById(BlockId id) => FindRow<BlockRow>(id.Value) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<ContentBlock> GetBlocks(Document document) => document.BlockIds.Select(id =>
        GetBlockById(id) ?? throw new InvalidOperationException($"Content block {id} was not found.")).ToList();
    public void SaveBlock(ContentBlock block) { ArgumentNullException.ThrowIfNull(block); SaveRow(ToStorage(block)); }

    private static DocumentRow ToStorage(Document document) => new()
    {
        Id = document.Id.Value,
        BlockIds = document.BlockIds.Select(id => id.Value).ToList(),
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private static Document ToDomain(DocumentRow document) =>
        Document.Reconstitute(
            new DocumentId(document.Id),
            document.BlockIds.Select(id => new BlockId(id)),
            document.CreatedAt,
            document.UpdatedAt);

    private static BlockRow ToStorage(ContentBlock block)
    {
        // Kind is the discriminator; the switch adds only the selected
        // subtype's payload to the common storage metadata.
        var stored = new BlockRow
        {
            Id = block.Id.Value,
            Kind = block.Kind,
            CreatedAt = block.CreatedAt,
            UpdatedAt = block.UpdatedAt
        };

        switch (block)
        {
            case MarkdownTextBlock text:
                stored.Markdown = text.Markdown;
                break;
            case ImageBlock image:
                stored.Url = image.Url;
                stored.AltText = image.AltText;
                stored.Caption = ForStorage(image.Caption);
                break;
            case VideoBlock video:
                stored.Url = video.Url;
                stored.Caption = ForStorage(video.Caption);
                break;
            case LinkPreviewBlock link:
                stored.Url = link.Url.ToString();
                stored.Title = link.Title;
                stored.Description = ForStorage(link.Description);
                break;
            case PollReferenceBlock poll:
                stored.ReferenceId = poll.PollId;
                break;
            case ChartReferenceBlock chart:
                stored.ReferenceId = chart.ChartId;
                stored.Title = ForStorage(chart.Title);
                break;
            default:
                throw new InvalidOperationException($"Unsupported block type {block.GetType().Name}.");
        }

        return stored;
    }

    private static string? ForStorage(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static ContentBlock ToDomain(BlockRow block)
    {
        var id = new BlockId(block.Id);

        // Reconstitution preserves persisted identity and timestamps rather
        // than running a public constructor that represents new creation.
        return block.Kind switch
        {
            "markdown" => MarkdownTextBlock.Reconstitute(id, block.Markdown ?? string.Empty, block.CreatedAt, block.UpdatedAt),
            "image" => ImageBlock.Reconstitute(id, block.Url ?? string.Empty, block.AltText ?? string.Empty, block.Caption, block.CreatedAt, block.UpdatedAt),
            "video" => VideoBlock.Reconstitute(id, block.Url ?? string.Empty, block.Caption, block.CreatedAt, block.UpdatedAt),
            "link-preview" => LinkPreviewBlock.Reconstitute(id, block.Url ?? string.Empty, block.Title ?? string.Empty, block.Description, block.CreatedAt, block.UpdatedAt),
            "poll-reference" => PollReferenceBlock.Reconstitute(id, block.ReferenceId ?? Guid.Empty, block.CreatedAt, block.UpdatedAt),
            "chart-reference" => ChartReferenceBlock.Reconstitute(id, block.ReferenceId ?? Guid.Empty, block.Title, block.CreatedAt, block.UpdatedAt),
            _ => throw new InvalidOperationException($"Unknown Content block kind '{block.Kind}'.")
        };
    }
}
