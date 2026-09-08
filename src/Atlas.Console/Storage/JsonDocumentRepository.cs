using System.Text.Json;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists Content documents as JSON and reconstitutes them as domain objects.</summary>
public sealed class JsonDocumentRepository : IDocumentRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>Initializes the JSON adapter and ensures its backing file is available.</summary>
    public JsonDocumentRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all persisted domain objects.</summary>
    public IReadOnlyCollection<Document> GetAll()
    {
        return ReadStoredDocuments()
            .Select(ToDomain)
            .ToList();
    }

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    public Document? GetById(DocumentId id)
    {
        var storedDocument = ReadStoredDocuments()
            .SingleOrDefault(document => document.Id == id.Value);

        return storedDocument is null
            ? null
            : ToDomain(storedDocument);
    }

    /// <summary>Persists the current domain-object state.</summary>
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

    /// <summary>Reads document persistence records from JSON.</summary>
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

    /// <summary>Writes document persistence records to JSON.</summary>
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

    /// <summary>Maps a domain object to its data-only persistence representation.</summary>
    private static StoredDocument ToStorage(Document document)
    {
        return new StoredDocument
        {
            Id = document.Id.Value,
            Content = document.Content,
            CreatedAt = document.CreatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private static Document ToDomain(StoredDocument storedDocument)
    {
        return Document.Reconstitute(
            new DocumentId(storedDocument.Id),
            storedDocument.Content,
            storedDocument.CreatedAt);
    }
}
