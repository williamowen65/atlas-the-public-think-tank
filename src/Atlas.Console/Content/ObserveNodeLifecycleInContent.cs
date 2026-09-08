using Atlas.Content.Documents;
using Atlas.Contracts.Graph.V1;

namespace Atlas.ConsoleApp.Content;

/// <summary>Observes Graph lifecycle events from the console host and checks related Content state.</summary>
public sealed class ObserveNodeLifecycleInContent
{
    private readonly IDocumentRepository _documents;

    /// <summary>Creates a validated observe node lifecycle in content instance.</summary>
    public ObserveNodeLifecycleInContent(
        IDocumentRepository documents)
    {
        _documents = documents;
    }

    /// <summary>Handles node creation by confirming that the referenced Content document already exists.</summary>
    public void Handle(NodeCreatedV1 message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeCreatedV1 for node " +
            $"{message.NodeId}.");

        ConfirmDescriptionDocument(message.DescriptionId);
    }

    /// <summary>Handles node archival by reporting the related Content reference retained by the host.</summary>
    public void Handle(NodeArchivedV1 message)
    {
        Console.WriteLine(
            $"[ATLAS.CONTENT] Heard NodeArchivedV1 for node " +
            $"{message.NodeId}; no Content state change required.");

        ConfirmDescriptionDocument(message.DescriptionId);
    }

    /// <summary>Confirms that the Content document referenced by a Graph event already exists.</summary>
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
