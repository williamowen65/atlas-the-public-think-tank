using Atlas.ConsoleApp;
using Atlas.ConsoleApp.Content;
using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Storage;
using Atlas.Content.Documents;
using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

var dataDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "data"));

var nodeDataFilePath = Path.Combine(
    dataDirectory,
    "nodes.json");

var nodeTypeDataFilePath = Path.Combine(
    dataDirectory,
    "node-types.json");

var documentDataFilePath = Path.Combine(
    dataDirectory,
    "documents.json");

var participantDataFilePath = Path.Combine(
    dataDirectory,
    "participants.json");

INodeTypeRepository nodeTypeRepository =
    new JsonNodeTypeRepository(nodeTypeDataFilePath);

SeedSystemNodeTypes(nodeTypeRepository);

IDocumentRepository documentRepository =
    new JsonDocumentRepository(documentDataFilePath);

IParticipantRepository participantRepository =
    new JsonParticipantRepository(participantDataFilePath);

var legacyParticipant =
    EnsureLegacyParticipant(participantRepository);

INodeRepository nodeRepository =
    new JsonNodeRepository(
        nodeDataFilePath,
        nodeTypeRepository,
        documentRepository,
        new NodeAuthorId(legacyParticipant.Id.Value));

var eventPublisher = new InMemoryEventPublisher();

var contentSubscriber =
    new ObserveNodeLifecycleInContent(documentRepository);

eventPublisher.Subscribe<NodeCreatedV1>(
    contentSubscriber.Handle);

eventPublisher.Subscribe<NodeArchivedV1>(
    contentSubscriber.Handle);

var application = new ConsoleApplication(
    nodeRepository,
    nodeTypeRepository,
    documentRepository,
    participantRepository,
    eventPublisher,
    nodeDataFilePath,
    nodeTypeDataFilePath,
    documentDataFilePath,
    participantDataFilePath,
    legacyParticipant);

application.Run();

static void SeedSystemNodeTypes(
    INodeTypeRepository nodeTypes)
{
    var existingTypes = nodeTypes.GetAll();
    var createdAt = DateTimeOffset.UtcNow;

    var systemTypes = new[]
    {
        ("Issue", "A problem or concern to investigate.", true),
        ("Question", "A question that invites answers.", true),
        ("Idea", "A proposed concept or possibility.", true),
        ("Solution", "A proposed response to a problem.", true),
        ("Evidence", "Information supporting or challenging a claim.", false),
        ("Relationship", "A connection involving multiple nodes.", true),
        ("Comment", "A response or observation about another node.", true),
        ("Location", "A place associated with another node.", true)
    };

    foreach (var (name, description, autoPluralize) in systemTypes)
    {
        var existingType = existingTypes.SingleOrDefault(type =>
            string.Equals(
                type.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (existingType is not null)
        {
            existingType.ChangeAutoPluralize(
                autoPluralize,
                actorId: "system",
                actorIsModerator: true,
                changedAt: createdAt);

            nodeTypes.Save(existingType);
            continue;
        }

        nodeTypes.Save(
            NodeTypeDefinition.CreateSystemDefined(
                name,
                description,
                createdAt,
                autoPluralize));
    }
}

static Participant EnsureLegacyParticipant(
    IParticipantRepository participants)
{
    var existing = participants
        .GetAll()
        .FirstOrDefault(participant =>
            string.Equals(
                participant.DisplayName,
                "Legacy Console User",
                StringComparison.OrdinalIgnoreCase));

    if (existing is not null)
    {
        return existing;
    }

    var participant = new Participant(
        "Legacy Console User",
        DateTimeOffset.UtcNow);

    participants.Save(participant);
    return participant;
}
