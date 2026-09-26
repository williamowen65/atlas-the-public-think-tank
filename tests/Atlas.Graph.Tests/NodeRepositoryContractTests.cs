using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

[TestClass]
public sealed class NodeRepositoryContractTests
{
    [TestMethod]
    public void Repository_does_not_expose_an_unrestricted_all_nodes_query()
    {
        Assert.IsNull(typeof(INodeRepository).GetMethod("GetAll"));
    }

    [TestMethod]
    public void Repository_exposes_only_purpose_specific_collection_queries()
    {
        var collectionQueries = typeof(INodeRepository).GetMethods()
            .Where(method => method.ReturnType.IsGenericType &&
                method.ReturnType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
            .Select(method => method.Name)
            .OrderBy(name => name)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { "GetByAuthor", "GetChildren", "GetParentCandidates" },
            collectionQueries);
    }
}
