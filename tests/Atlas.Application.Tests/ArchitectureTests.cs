using Atlas.Application.Eventing;
using Atlas.Contracts.Events;
using Atlas.Graph.Nodes;
using Atlas.Content.Documents;

namespace Atlas.Application.Tests;

[TestClass]
public sealed class ArchitectureTests
{
    [TestMethod]
    public void ApplicationAndDomainsPreserveHostAndDomainBoundaries()
    {
        var dependencies = typeof(NodeWorkflowService).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        foreach (var forbidden in new[] { "Atlas.Console", "System.Console" })
            Assert.DoesNotContain(forbidden, dependencies);
        Assert.DoesNotContain("Atlas.Content", typeof(Node).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList());
        Assert.DoesNotContain("Atlas.Graph", typeof(Document).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList());
    }

    [TestMethod]
    public void EventDispatcherWorksWithoutATerminalAndPreservesSubscriberFailure()
    {
        IEventPublisher publisher = new InMemoryEventPublisher();
        var dispatcher = (InMemoryEventPublisher)publisher;
        var seen = new List<string>();
        dispatcher.Subscribe<string>(seen.Add);
        publisher.Publish("event");
        CollectionAssert.AreEqual(new[] { "event" }, seen);
        dispatcher.Subscribe<string>(_ => throw new InvalidOperationException("Subscriber failure"));
        Assert.Throws<InvalidOperationException>(() => publisher.Publish("next"));
    }
}
