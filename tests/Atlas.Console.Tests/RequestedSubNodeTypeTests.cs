using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
[DoNotParallelize] // Console input/output are process-wide; restore them after each interaction.
public sealed class RequestedSubNodeTypeTests
{
    private sealed class Types : INodeTypeRepository
    {
        public List<NodeTypeDefinition> Items { get; } =
        [NodeTypeDefinition.CreateSystemDefined("Idea", "Ideas", DateTimeOffset.UtcNow)];
        public IReadOnlyCollection<NodeTypeDefinition> GetAll() => Items;
        public NodeTypeDefinition? GetById(NodeTypeId id) => Items.SingleOrDefault(type => type.Id == id);
        public void Save(NodeTypeDefinition type) => Items.Add(type);
    }

    private static IReadOnlyCollection<NodeTypeDefinition>? Read(string input)
    {
        var originalIn = System.Console.In;
        var originalOut = System.Console.Out;
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        try
        {
            System.Console.SetIn(reader);
            System.Console.SetOut(writer);
            // The catalog deliberately contains no Comment type, as in the reported crash.
            return ConsoleUi.ReadRequestedSubNodeTypes(new Types(), Guid.NewGuid().ToString());
        }
        finally
        {
            System.Console.SetIn(originalIn);
            System.Console.SetOut(originalOut);
        }
    }

    [TestMethod]
    public void BlankInputRequestsNoTypesWithoutRequiringComment()
    {
        var result = Read("\n");
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ExplicitSelectionContainsOnlySelectedType()
    {
        var result = Read("1,1\n");
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("Idea", result.Single().Name);
    }

    [TestMethod]
    public void CancellationAndInvalidInputAreNotValidEmptySelections()
    {
        Assert.IsNull(Read("0\n"));
        Assert.IsNull(Read("99\n\n"));
        Assert.IsNull(Read(",\n\n"));
        Assert.IsNull(Read(""));
    }
}
