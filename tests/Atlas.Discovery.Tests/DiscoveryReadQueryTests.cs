using Atlas.Discovery;

namespace Atlas.Discovery.Tests;

[TestClass]
public sealed class DiscoveryReadQueryTests
{
    [TestMethod]
    public void Page_requests_use_page_port_without_loading_unpaged_results()
    {
        var port = new PageOnlyQuery();
        var service = new DiscoveryService(port);
        var query = new DiscoveryQuery(Page: 2);
        Assert.AreSame(port.Page, service.DiscoverPage(query));
        Assert.AreSame(query, port.LastQuery);
        Assert.IsNull(port.AuthoredIds);

        Guid[] ids = [Guid.NewGuid()];
        Assert.AreSame(port.Page, service.DiscoverAuthoredPage(query, ids));
        Assert.AreSame(ids, port.AuthoredIds);
    }

    [TestMethod]
    public void Invalid_requests_are_rejected_before_reaching_storage()
    {
        var port = new PageOnlyQuery();
        var service = new DiscoveryService(port);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.DiscoverPage(new DiscoveryQuery(Page: 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.DiscoverAuthoredPage(new DiscoveryQuery(MinimumAverageVote: double.NaN), []));
        Assert.Throws<ArgumentException>(() => service.Discover(new DiscoveryQuery(MinimumVoteCount: 4, MaximumVoteCount: 2)));
        Assert.Throws<ArgumentException>(() => service.DiscoverAuthored(new DiscoveryQuery(MinimumAverageVote: 8, MaximumAverageVote: 2), []));
        Assert.Throws<ArgumentNullException>(() => service.DiscoverAuthoredPage(new DiscoveryQuery(), null!));
        Assert.Throws<ArgumentException>(() => service.DiscoverPage(new DiscoveryQuery(NodeId: Guid.Empty)));
        Assert.IsNull(port.LastQuery);
    }

    private sealed class PageOnlyQuery : IDiscoveryReadQuery
    {
        public DiscoveryPage Page { get; } = new([], 8, 2, IDiscoveryService.PageSize);
        public DiscoveryQuery? LastQuery { get; private set; }
        public IReadOnlyCollection<Guid>? AuthoredIds { get; private set; }

        public DiscoveryPage DiscoverPage(DiscoveryQuery query)
        {
            LastQuery = query;
            AuthoredIds = null;
            return Page;
        }

        public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
        {
            LastQuery = query;
            AuthoredIds = authoredNodeIds;
            return Page;
        }

        public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) =>
            throw new AssertFailedException("Page requests must not materialize all ranked results.");
        public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) =>
            throw new AssertFailedException("Authored page requests must not materialize all ranked results.");
    }
}
