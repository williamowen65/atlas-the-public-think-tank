using Atlas.Content.Documents;
using Atlas.Contracts.Graph.V1;

namespace Atlas.Application.Content;

/// <summary>Observe Graph events through Content's repository; archiving retains the description.</summary>
public sealed class ObserveNodeLifecycleInContent(IDocumentRepository documents, Action<NodeContentObservation>? observe = null)
{
    public void Handle(NodeCreatedV1 message) => Confirm(message.NodeId, message.DescriptionId, "created");
    public void Handle(NodeArchivedV1 message) => Confirm(message.NodeId, message.DescriptionId, "archived");
    private void Confirm(Guid nodeId, Guid descriptionId, string transition)
    {
        var exists = documents.GetById(new DocumentId(descriptionId)) is not null;
        observe?.Invoke(new(nodeId, descriptionId, transition, exists));
    }
}

public sealed record NodeContentObservation(Guid NodeId, Guid DescriptionId, string Transition, bool DescriptionExists);
