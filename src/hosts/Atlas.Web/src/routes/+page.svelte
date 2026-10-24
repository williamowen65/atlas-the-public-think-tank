<script lang="ts">
  import { goto } from '$app/navigation';
  import { discoveryQuery } from '$lib/state/discoveryQuery.svelte';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
  import { readTypes, type NodeType } from '$lib/api/nodePage';
  import type { DiscoveryFilters } from '$lib/api/discovery';
  // This root-route page becomes the children rendered by +layout.svelte.
  let types = $state<NodeType[]>([]);
  // The header and page share the search query; derive API filters from it.
  const query = $derived(discoveryQuery.value);
  // Recompute when the query or loaded type catalog changes. This only derives data;
  // LiveNodeFeed owns discovery requests. The result separates free text from API filters.
  const parsed = $derived.by(() => {
    const filters: DiscoveryFilters = {};
    const text: string[] = [];
    // Split into quoted phrases or non-space tokens. This is a small token parser,
    // not a full query language: multiword values after a prefix are not grouped here.
    for (const part of query.match(/"[^"]*"|\S+/g) ?? []) {
      // RegExp.exec matches text; it never runs a shell command. Capture 1 is the
      // recognized filter name, capture 2 its value; null means ordinary search text.
      const m = /^(type|author|community|votes|average|from|through|archived):(.+)$/i.exec(part);
      if (!m) {
        text.push(part);
        continue;
      }
      const key = m[1].toLowerCase(),
        v = m[2].replace(/^"|"$/g, '');
      // Resolve type IDs or names against the catalog; an unknown name leaves typeId unset.
      if (key === 'type')
        filters.typeId = types.find(
          (t) => t.id.toLowerCase() === v.toLowerCase() || t.name.toLowerCase() === v.toLowerCase(),
        )?.id;
      if (key === 'author') filters.authorId = v;
      if (key === 'community') filters.communityId = v;
      if (key === 'votes' || key === 'average') {
        // Accept min..max, min.. or ..max; missing bounds remain undefined.
        const range = /^(\d+(?:\.\d+)?)?\.\.(\d+(?:\.\d+)?)?$/.exec(v);
        if (range) {
          if (key === 'votes') {
            filters.minVotes = range[1] ? Number(range[1]) : undefined;
            filters.maxVotes = range[2] ? Number(range[2]) : undefined;
          } else {
            filters.minAverage = range[1] ? Number(range[1]) : undefined;
            filters.maxAverage = range[2] ? Number(range[2]) : undefined;
          }
        }
      }
      // Recognized tokens are consumed even if their value is invalid. Dates are
      // forwarded for API validation; archived:true includes active AND archived nodes.
      if (key === 'from') filters.from = v;
      if (key === 'through') filters.through = v;
      if (key === 'archived') filters.archived = v === 'true';
    }
    return { text: text.join(' '), filters };
  });
  // $effect runs in the browser after mount, not during server-side rendering.
  // No reactive value is read synchronously here, so typing does not rerun this effect.
  // The loaded catalog updates `types`, which recalculates the derived filter above.
  $effect(() => {
    // Pass a cancellation signal through readTypes/readJson to fetch.
    const c = new AbortController();
    void readTypes('/atlas-public-api', c.signal)
      .then((t) => (types = t))
      // Cancellation and load failures are currently ignored; types remains empty
      // on failure, so type-name filters cannot resolve until a later page mount.
      .catch(() => {});
    // Svelte calls this cleanup when the page is destroyed (or before a rerun).
    // Abort an unfinished fetch/body read; this does not undo server-side work.
    return () => c.abort();
  });
</script>

<svelte:head>
  <title>Discover · Atlas</title>
  <meta name="description" content="Explore connected questions, evidence, and ideas in Atlas." />
</svelte:head>
<div class="feed-intro">
  <h1>Discover</h1>
  <p>Follow a question. Find a new perspective. Build on an idea.</p>
</div>
<!-- A changed search remounts the feed so paging starts again at page one. -->
{#key query + JSON.stringify(parsed.filters)}
  <LiveNodeFeed
    search={parsed.text}
    filters={parsed.filters}
    onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
    onOpenNode={(id, typeId) =>
      void goto(
        `/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`,
      )}
  />
{/key}

<style>
  .feed-intro {
    max-width: 48rem;
    margin: 2rem auto;
  }
  h1 {
    margin-bottom: 0.5rem;
  }
  p {
    color: var(--atlas-muted);
  }
</style>
