using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Storage;

/// <summary>
/// JSON adapter for the Content repository boundary. Documents store ordered
/// references; blocks are stored separately as discriminated records.
/// </summary>
public sealed class JsonDocumentRepository : IDocumentRepository
{
    private readonly string _documentFilePath;
    private readonly string _blockFilePath;
    // Omitting nulls prevents one block kind from storing empty fields that
    // belong only to other block kinds.
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public JsonDocumentRepository(string documentFilePath, string blockFilePath)
    {
        _documentFilePath = documentFilePath;
        _blockFilePath = blockFilePath;
    }

    public IReadOnlyCollection<Document> GetAll() =>
        Read<List<StoredDocument>>(_documentFilePath)
            .Select(ToDomain)
            .ToList();

    public Document? GetById(DocumentId id)
    {
        var stored = Read<List<StoredDocument>>(_documentFilePath)
            .SingleOrDefault(document => document.Id == id.Value);
        return stored is null ? null : ToDomain(stored);
    }

    public void Save(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var stored = Read<List<StoredDocument>>(_documentFilePath);
        Upsert(stored, document.Id.Value, ToStorage(document), item => item.Id);
        Write(_documentFilePath, stored);
    }

    public ContentBlock? GetBlockById(BlockId id)
    {
        var stored = Read<List<StoredBlock>>(_blockFilePath)
            .SingleOrDefault(block => block.Id == id.Value);
        return stored is null ? null : ToDomain(stored);
    }

    public IReadOnlyCollection<ContentBlock> GetBlocks(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        // Resolve through BlockIds rather than file order because Document is
        // the authority for presentation order.
        return document.BlockIds
            .Select(id => GetBlockById(id) ??
                throw new InvalidOperationException($"Content block {id} was not found."))
            .ToList();
    }

    public void SaveBlock(ContentBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        var stored = Read<List<StoredBlock>>(_blockFilePath);
        Upsert(stored, block.Id.Value, ToStorage(block), item => item.Id);
        Write(_blockFilePath, stored);
    }

    private T Read<T>(string path) where T : new()
    {
        if (!File.Exists(path) || string.IsNullOrWhiteSpace(File.ReadAllText(path)))
        {
            return new T();
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _jsonOptions) ?? new T();
    }

    private void Write<T>(string path, T value)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions));
    }

    private static void Upsert<T>(List<T> items, Guid id, T replacement, Func<T, Guid> idSelector)
    {
        var index = items.FindIndex(item => idSelector(item) == id);
        if (index >= 0) items[index] = replacement;
        else items.Add(replacement);
    }

    private static StoredDocument ToStorage(Document document) => new()
    {
        Id = document.Id.Value,
        BlockIds = document.BlockIds.Select(id => id.Value).ToList(),
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private static Document ToDomain(StoredDocument document) =>
        Document.Reconstitute(
            new DocumentId(document.Id),
            document.BlockIds.Select(id => new BlockId(id)),
            document.CreatedAt,
            document.UpdatedAt);

    private static StoredBlock ToStorage(ContentBlock block)
    {
        // Kind is the discriminator; the switch adds only the selected
        // subtype's payload to the common storage metadata.
        var stored = new StoredBlock
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

    private static ContentBlock ToDomain(StoredBlock block)
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
