using System.Text.Json;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonDocumentRepository : IDocumentRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonDocumentRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyCollection<Document> GetAll()
    {
        return ReadStoredDocuments()
            .Select(ToDomain)
            .ToList();
    }

    public Document? GetById(DocumentId id)
    {
        var storedDocument = ReadStoredDocuments()
            .SingleOrDefault(document => document.Id == id.Value);

        return storedDocument is null
            ? null
            : ToDomain(storedDocument);
    }

    public void Save(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var storedDocuments = ReadStoredDocuments();

        var existingIndex = storedDocuments.FindIndex(
            storedDocument => storedDocument.Id == document.Id.Value);

        var replacement = ToStorage(document);

        if (existingIndex >= 0)
        {
            storedDocuments[existingIndex] = replacement;
        }
        else
        {
            storedDocuments.Add(replacement);
        }

        WriteStoredDocuments(storedDocuments);
    }

    private List<StoredDocument> ReadStoredDocuments()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<StoredDocument>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    private void WriteStoredDocuments(
        List<StoredDocument> documents)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(documents, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static StoredDocument ToStorage(Document document)
    {
        return new StoredDocument
        {
            Id = document.Id.Value,
            Content = document.Content,
            CreatedAt = document.CreatedAt
        };
    }

    private static Document ToDomain(StoredDocument storedDocument)
    {
        return Document.Reconstitute(
            new DocumentId(storedDocument.Id),
            storedDocument.Content,
            storedDocument.CreatedAt);
    }
}
