using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Content;

public sealed class CreateDocumentWhenNodeCreated
{
    private readonly IDocumentRepository _documents;

    public CreateDocumentWhenNodeCreated(
        IDocumentRepository documents)
    {
        _documents = documents;
    }

    public void Handle(NodeCreated message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeCreated for node " +
            $"{message.NodeId}.");

        if (_documents.GetByNodeId(message.NodeId) is not null)
        {
            Console.WriteLine(
                "[ATLAS.CONTENT] A document already exists; event ignored.");
            return;
        }

        var document = new Document(
            message.NodeId,
            message.InitialDescription,
            message.OccurredAt);

        _documents.Save(document);

        Console.WriteLine(
            $"[ATLAS.CONTENT] Created document {document.Id}.");
    }
}
