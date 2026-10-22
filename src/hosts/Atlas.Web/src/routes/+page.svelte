<script lang="ts">
  import { goto } from '$app/navigation';
  import { discoveryQuery } from '$lib/state/discoveryQuery.svelte';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
  import { readTypes, type NodeType } from '$lib/api/nodePage';
  import type { DiscoveryFilters } from '$lib/api/discovery';
  let types = $state<NodeType[]>([]);
  const query = $derived(discoveryQuery.value);
  const parsed = $derived.by(() => {
    const filters: DiscoveryFilters = {}; const text: string[] = [];
    for (const part of query.match(/"[^"]*"|\S+/g) ?? []) {
      const m = /^(type|author|community|votes|average|from|through|archived):(.+)$/i.exec(part);
      if (!m) { text.push(part); continue; }
      const key = m[1].toLowerCase(), v = m[2].replace(/^"|"$/g, '');
      if (key === 'type') filters.typeId = types.find(t => t.id.toLowerCase() === v.toLowerCase() || t.name.toLowerCase() === v.toLowerCase())?.id;
      if (key === 'author') filters.authorId = v;
      if (key === 'community') filters.communityId = v;
      if (key === 'votes' || key === 'average') { const range = /^(\d+(?:\.\d+)?)?\.\.(\d+(?:\.\d+)?)?$/.exec(v); if (range) { if (key === 'votes') { filters.minVotes = range[1] ? Number(range[1]) : undefined; filters.maxVotes = range[2] ? Number(range[2]) : undefined; } else { filters.minAverage = range[1] ? Number(range[1]) : undefined; filters.maxAverage = range[2] ? Number(range[2]) : undefined; } } }
      if (key === 'from') filters.from = v;
      if (key === 'through') filters.through = v;
      if (key === 'archived') filters.archived = v === 'true';
    }
    return { text: text.join(' '), filters };
  });
  $effect(() => { const c = new AbortController(); void readTypes('/atlas-preview-api', c.signal).then(t => types = t).catch(() => {}); return () => c.abort(); });
</script>
<svelte:head><title>Discover · Atlas</title><meta name="description" content="Explore connected questions, evidence, and ideas in Atlas." /></svelte:head>
<div class="feed-intro"><h1>Discover</h1><p>Follow a question. Find a new perspective. Build on an idea.</p></div>
{#key query + JSON.stringify(parsed.filters)}
<LiveNodeFeed search={parsed.text} filters={parsed.filters} onOpenProfile={id => void goto('/profiles?id=' + encodeURIComponent(id))} onOpenNode={(id, typeId) => void goto(`/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`)} />
{/key}
<style>.feed-intro{max-width:48rem;margin:2rem auto;}h1{margin-bottom:.5rem;}p{color:var(--atlas-muted);}</style>
