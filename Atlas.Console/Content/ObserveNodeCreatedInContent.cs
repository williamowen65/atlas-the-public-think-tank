using Atlas.Content.Documents;
using Atlas.Graph.NodeLifecycle;

namespace Atlas.ConsoleApp.Content;

public sealed class ObserveNodeCreatedInContent
{
    private readonly IDocumentRepository _documents;

    public ObserveNodeCreatedInContent(
        IDocumentRepository documents)
    {
        _documents = documents;
    }

    public void Handle(NodeCreated message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeCreated for node " +
            $"{message.NodeId}.");

        var document = _documents.GetById(
            new DocumentId(message.DescriptionId));

        if (document is null)
        {
            Console.WriteLine(
                $"[ATLAS.CONTENT] Description document " +
                $"{message.DescriptionId} was not found.");
            return;
        }

        Console.WriteLine(
            $"[ATLAS.CONTENT] Confirmed description document " +
            $"{document.Id}.");
    }
}
