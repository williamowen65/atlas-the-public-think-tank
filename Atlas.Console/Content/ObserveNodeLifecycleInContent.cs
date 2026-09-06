using Atlas.Content.Documents;
using Atlas.Graph;

namespace Atlas.ConsoleApp.Content;

public sealed class ObserveNodeLifecycleInContent
{
    private readonly IDocumentRepository _documents;

    public ObserveNodeLifecycleInContent(
        IDocumentRepository documents)
    {
        _documents = documents;
    }

    public void Handle(NodeCreated message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeCreated for node " +
            $"{message.NodeId}.");

        ConfirmDescriptionDocument(message.DescriptionId);
    }

    public void Handle(NodeArchived message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeArchived for node " +
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
