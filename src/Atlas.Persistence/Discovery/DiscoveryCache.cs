using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.Extensions.Caching.Memory;

namespace Atlas.Persistence.Discovery;

/// <summary>
/// Keeps bounded, process-local caches for ranked pages and public display cards.
/// Pages depend on filters and ordering; cards depend only on identity and display data.
/// Database revision checks, rather than expiration alone, prevent reuse after writes.
/// </summary>
public sealed class DiscoveryCache : IDisposable
{
    private readonly MemoryCache _pages;
    private readonly MemoryCache _details;
    private readonly TimeSpan _expiration;
    private readonly long _detailBudget;
    private readonly long _pageBudget;
    private string? _database;

    // SqlDiscoveryReadQuery holds this gate while reading or publishing cache entries.
    // It coordinates misses and counter updates; no DbContext is stored in the cache.
    internal object Gate { get; } = new();
    public long PageHits { get; private set; }
    public long PageMisses { get; private set; }
    public long DetailHits { get; private set; }
    public long DetailMisses { get; private set; }
    public long RejectedEntries { get; private set; }

    /// <summary>Creates independent memory budgets and a fixed lifetime for each entry.</summary>
    public DiscoveryCache(
        long pageBudgetBytes = 1024 * 1024,
        long detailBudgetBytes = 8 * 1024 * 1024,
        TimeSpan? expiration = null)
    {
        if (pageBudgetBytes < 1024 || detailBudgetBytes < 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(pageBudgetBytes));
        }

        _detailBudget = detailBudgetBytes;
        _pageBudget = pageBudgetBytes;
        _expiration = expiration ?? TimeSpan.FromSeconds(30);
        if (_expiration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiration));
        }

        _pages = new MemoryCache(new MemoryCacheOptions { SizeLimit = pageBudgetBytes });
        _details = new MemoryCache(new MemoryCacheOptions { SizeLimit = detailBudgetBytes });
    }

    /// <summary>Prevents identical IDs or revision numbers from mixing different databases.</summary>
    internal void BindDatabase(string connectionString)
    {
        if (_database is not null && _database != connectionString)
        {
            throw new InvalidOperationException("A Discovery cache cannot be shared across different databases.");
        }

        _database = connectionString;
    }

    /// <summary>
    /// Identifies one requested page: its filters, authored scope, visibility policy,
    /// page number, page size, ordering policy and committed page revision.
    /// This is a cache lookup key, not a SQL paging cursor or an authorization token.
    /// </summary>
    internal static string PageKey(
        DiscoveryQuery query,
        IReadOnlyCollection<Guid>? scope,
        long version,
        bool includeHidden)
    {
        // Set-like inputs are sorted and deduplicated so their input order cannot
        // create different keys. Read() has already trimmed the search text.
        // Preserve text case/accents here: SQL owns linguistic comparison rules.
        var request = new
        {
            query.SearchText,
            query.CommunityId,
            query.NodeTypeId,
            query.AuthorParticipantId,
            query.IsArchived,
            Reactions = query.ReactionDefinitionIds?.Distinct().Order().ToArray(),
            query.MinimumVoteCount,
            query.MaximumVoteCount,
            query.MinimumAverageVote,
            query.MaximumAverageVote,
            query.CreatedFrom,
            query.CreatedThrough,
            query.IncludeArchived,
            query.Page,
            PageSize = IDiscoveryService.PageSize,
            IncludeHidden = includeHidden,
            Scope = scope?.Distinct().Order().ToArray()
        };

        // Hashing keeps potentially large filter/scope payloads out of cache keys.
        // order-v2 names the ordering contract; version changes after relevant writes,
        // so entries from an older committed revision can no longer be found.
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        return "order-v2:" + version + ":" + hash;
    }

    /// <summary>Looks up ranked IDs and totals; public cards are resolved separately.</summary>
    internal bool TryPage(string key, out DiscoveryPage page)
    {
        if (_pages.TryGetValue(key, out DiscoveryPage? value))
        {
            PageHits++;
            page = value!;
            return true;
        }

        PageMisses++;
        page = null!;
        return false;
    }

    /// <summary>Stores a completed page with an estimated size charged to its budget.</summary>
    internal void PutPage(string key, DiscoveryPage page)
    {
        // Estimates cover projection/object overhead, not an exact managed-heap measurement.
        var size = 512 + page.Items.Count * 128;
        if (size > _pageBudget)
        {
            RejectedEntries++;
            return;
        }

        _pages.Set(key, page, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expiration,
            Size = size
        });
    }

    /// <summary>Reuses a public card across filters/pages within the same detail revision.</summary>
    internal bool TryDetail(long version, Guid id, out DiscoveryCard card)
    {
        if (_details.TryGetValue((version, id), out DiscoveryCard? value))
        {
            DetailHits++;
            card = value!;
            return true;
        }

        DetailMisses++;
        card = null!;
        return false;
    }

    /// <summary>Stores public display text without any participant-specific ratings.</summary>
    internal void PutDetail(long version, DiscoveryCard card)
    {
        // Strings use approximately two bytes per character. Include object/key overhead;
        // oversized content still reaches the caller, but does not occupy the cache.
        var size = 512L + 2L * (
            card.Title.Length +
            card.TypeName.Length +
            card.AuthorName.Length +
            card.Status.Length +
            card.Description.Length +
            card.Breadcrumb.Length +
            card.SubNodes.Length +
            card.Reactions.Length +
            card.Communities.Length);
        if (size > Math.Min(128 * 1024, _detailBudget))
        {
            RejectedEntries++;
            return;
        }

        _details.Set((version, card.NodeId), card, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expiration,
            Size = size
        });
    }

    /// <summary>Releases both caches when the host shuts down.</summary>
    public void Dispose()
    {
        _pages.Dispose();
        _details.Dispose();
    }
}
