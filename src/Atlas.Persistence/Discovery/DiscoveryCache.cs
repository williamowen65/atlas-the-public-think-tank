using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.Extensions.Caching.Memory;

namespace Atlas.Persistence.Discovery;

/// <summary>Two bounded caches of immutable projections. One gate coordinates concurrent misses.</summary>
public sealed class DiscoveryCache : IDisposable
{
    private readonly MemoryCache _pages;
    private readonly MemoryCache _details;
    private readonly TimeSpan _expiration;
    internal object Gate { get; } = new();
    public long PageHits { get; private set; }
    public long PageMisses { get; private set; }
    public long DetailHits { get; private set; }
    public long DetailMisses { get; private set; }
    public long RejectedEntries { get; private set; }

    public DiscoveryCache(long pageBudgetBytes = 1024 * 1024, long detailBudgetBytes = 8 * 1024 * 1024,
        TimeSpan? expiration = null)
    {
        if (pageBudgetBytes < 1024 || detailBudgetBytes < 1024) throw new ArgumentOutOfRangeException(nameof(pageBudgetBytes));
        _expiration = expiration ?? TimeSpan.FromSeconds(30);
        if (_expiration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(expiration));
        _pages = new MemoryCache(new MemoryCacheOptions { SizeLimit = pageBudgetBytes });
        _details = new MemoryCache(new MemoryCacheOptions { SizeLimit = detailBudgetBytes });
    }

    private string? _database;
    internal void BindDatabase(string connectionString)
    {
        if (_database is not null && _database != connectionString)
            throw new InvalidOperationException("A Discovery cache cannot be shared across different databases.");
        _database = connectionString;
    }

    internal static string PageKey(DiscoveryQuery query, IReadOnlyCollection<Guid>? scope, long version, bool includeHidden) =>
        "order-v2:" + version + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            query.SearchText, query.CommunityId, query.NodeTypeId, query.AuthorParticipantId, query.IsArchived,
            Reactions = query.ReactionDefinitionIds?.Distinct().Order().ToArray(), query.MinimumVoteCount,
            query.MaximumVoteCount, query.MinimumAverageVote, query.MaximumAverageVote, query.CreatedFrom,
            query.CreatedThrough, query.IncludeArchived, query.Page, PageSize = IDiscoveryService.PageSize,
            IncludeHidden = includeHidden, Scope = scope?.Distinct().Order().ToArray()
        }))));

    internal bool TryPage(string key, out DiscoveryPage page)
    {
        if (_pages.TryGetValue(key, out DiscoveryPage? value)) { PageHits++; page = value!; return true; }
        PageMisses++; page = null!; return false;
    }
    internal void PutPage(string key, DiscoveryPage page) => _pages.Set(key, page,
        new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _expiration, Size = 512 + page.Items.Count * 128 });

    internal bool TryDetail(long version, Guid id, out DiscoveryCard card)
    {
        if (_details.TryGetValue((version, id), out DiscoveryCard? value)) { DetailHits++; card = value!; return true; }
        DetailMisses++; card = null!; return false;
    }
    internal void PutDetail(long version, DiscoveryCard card)
    {
        // Include string payloads, key and object overhead; oversized content is returned but not cached.
        var size = 512L + 2L * (card.Title.Length + card.TypeName.Length + card.AuthorName.Length + card.Status.Length +
            card.Description.Length + card.Breadcrumb.Length + card.SubNodes.Length + card.Reactions.Length + card.Communities.Length);
        if (size > 128 * 1024) { RejectedEntries++; return; }
        _details.Set((version, card.NodeId), card, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _expiration, Size = size });
    }
    public void Dispose() { _pages.Dispose(); _details.Dispose(); }
}
