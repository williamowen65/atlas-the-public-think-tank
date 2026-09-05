using Atlas.ConsoleApp;
using Atlas.ConsoleApp.Storage;
using Atlas.Graph.Nodes;

var dataFilePath = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "data",
        "nodes.json"));

INodeRepository nodeRepository =
    new JsonNodeRepository(dataFilePath);

var application = new ConsoleApplication(
    nodeRepository,
    dataFilePath);

application.Run();
