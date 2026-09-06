using Atlas.ConsoleApp;
using Atlas.ConsoleApp.Content;
using Atlas.ConsoleApp.Eventing;
using Atlas.ConsoleApp.Storage;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph;
using Atlas.Participants.Participants;

const string ConsoleActorId = "console-user";

var dataDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
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

eventPublisher.Subscribe<NodeCreated>(
    contentSubscriber.Handle);

eventPublisher.Subscribe<NodeArchived>(
    contentSubscriber.Handle);

var application = new ConsoleApplication(
    nodeRepository,
    nodeTypeRepository,
    documentRepository,
    participantRepository,
    eventPublisher,
    ConsoleActorId,
    nodeDataFilePath,
    nodeTypeDataFilePath,
    documentDataFilePath,
    participantDataFilePath,
    legacyParticipant);

application.Run();

static void SeedSystemNodeTypes(
    INodeTypeRepository nodeTypes)
{
    if (nodeTypes.GetAll().Count > 0)
    {
        return;
    }

    var createdAt = DateTimeOffset.UtcNow;

    var systemTypes = new[]
    {
        ("Issue", "A problem or concern to investigate."),
        ("Question", "A question that invites answers."),
        ("Idea", "A proposed concept or possibility."),
        ("Solution", "A proposed response to a problem."),
        ("Evidence", "Information supporting or challenging a claim."),
        ("Relationship", "A connection involving multiple nodes.")
    };

    foreach (var (name, description) in systemTypes)
    {
        nodeTypes.Save(
            NodeTypeDefinition.CreateSystemDefined(
                name,
                description,
                createdAt));
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
