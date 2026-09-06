using Atlas.Content.Documents;
using Atlas.Contracts.Graph.V1;

namespace Atlas.ConsoleApp.Content;

public sealed class ObserveNodeLifecycleInContent
{
    private readonly IDocumentRepository _documents;

    public ObserveNodeLifecycleInContent(
        IDocumentRepository documents)
    {
        _documents = documents;
    }

    public void Handle(NodeCreatedV1 message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeCreatedV1 for node " +
            $"{message.NodeId}.");

        ConfirmDescriptionDocument(message.DescriptionId);
    }

    public void Handle(NodeArchivedV1 message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeArchivedV1 for node " +
            $"{message.NodeId}; no Content state change required.");

        ConfirmDescriptionDocument(message.DescriptionId);
    }

    private void ConfirmDescriptionDocument(Guid descriptionId)
    {
        var document = _documents.GetById(
            new DocumentId(descriptionId));

        if (document is null)
        {
            Console.WriteLine(
                $"[ATLAS.CONTENT] Description document " +
                $"{descriptionId} was not found.");
            return;
        }

        Console.WriteLine(
            $"[ATLAS.CONTENT] Confirmed description document " +
            $"{document.Id}.");
    }
}
