<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { NodeCardData } from './NodeCard.svelte';
  import type { NodeType, ContentBlock } from '../api/nodePage';
  import ContentBlocks from './ContentBlocks.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  import Breadcrumb from './Breadcrumb.svelte';
  import VotingDial from './VotingDial.svelte';
  let { node, blocks, types, active, onTabChange, onAddChild, onNavigate, hasMoreAncestors = false, loadOlderAncestors, average = null, totalVotes = 0, children }: { node: NodeCardData; blocks: ContentBlock[]; types: NodeType[]; active: string; onTabChange: (id: string) => void; onAddChild: (id?: string) => void; onNavigate?: (id: string) => void; hasMoreAncestors?: boolean; loadOlderAncestors?: LoadOlderAncestors; average?: number | null; totalVotes?: number; children: Snippet } = $props();
  const instance = $props.id();
  const tabs = $derived([{ id: 'comments', name: 'Comments' }, ...types]);
  function key(event: KeyboardEvent, index: number) { let next = index; if (event.key === 'ArrowRight') next = (index + 1) % tabs.length; else if (event.key === 'ArrowLeft') next = (index - 1 + tabs.length) % tabs.length; else if (event.key === 'Home') next = 0; else if (event.key === 'End') next = tabs.length - 1; else return; event.preventDefault(); onTabChange(tabs[next].id); document.getElementById(`${instance}-${tabs[next].id}`)?.focus(); }
</script>
<article class="node-page atlas-panel">
  {#if node.breadcrumb.length}<Breadcrumb items={node.breadcrumb} {onNavigate} hasMore={hasMoreAncestors} loadOlder={loadOlderAncestors} />{/if}
  <header><h1>{node.typeName}: {node.title}</h1><p class="byline">By {node.authorName} · <time datetime={node.createdAt}>{new Date(node.createdAt).toLocaleDateString()}</time>{#if node.communities.length} · Community: {node.communities.join(', ')}{/if}</p>{#if node.status === 'Archived'}<p>Archived</p>{/if}</header>
  <div class="intro"><VotingDial label="Node importance" {average} {totalVotes} disabled /><p>{node.description}</p></div>
  {#if node.reactions.length}<div class="reactions" role="group" aria-label="Community reactions">{#each node.reactions as reaction}<span>{reaction.label} <strong>{reaction.count}</strong></span>{/each}</div>{/if}
  <ContentBlocks {blocks} />
  <div class="add"><button onclick={() => onAddChild(active === 'comments' ? undefined : active)} disabled={node.status === 'Archived'}>Add {active === 'comments' ? 'a child node' : types.find(type => type.id === active)?.name}</button></div>
  <div class="tabs" role="tablist" aria-label="Node discussions and child types">{#each tabs as tab, index}<button role="tab" id={`${instance}-${tab.id}`} aria-selected={active === tab.id} aria-controls={`${instance}-panel`} tabindex={active === tab.id ? 0 : -1} onclick={() => onTabChange(tab.id)} onkeydown={event => key(event, index)}>{tab.name}</button>{/each}</div>
  <div role="tabpanel" id={`${instance}-panel`} aria-labelledby={`${instance}-${active}`} tabindex="0">{@render children()}</div>
</article>
<style lang="scss">.node-page{max-width:48rem;margin-inline:auto;padding:var(--atlas-card-padding,1.25rem);}h1{font-size:clamp(1.4rem,4vw,2rem);margin:.5rem 0;}.byline{font-size:.85rem;color:var(--atlas-muted);}.reactions{display:flex;flex-wrap:wrap;gap:.5rem;margin:1rem 0;span{border:1px solid var(--atlas-border);border-radius:999px;padding:.2rem .6rem;}}.intro{display:flex;align-items:start;gap:var(--atlas-dial-text-gap,1rem);p{margin:0;white-space:pre-line;}}.add{display:flex;justify-content:flex-end;margin:1rem 0;}.tabs{display:flex;flex-wrap:wrap;gap:.4rem;border-bottom:1px solid var(--atlas-border);padding-bottom:.75rem;}button{font:inherit;padding:.5rem .75rem;cursor:pointer;max-width:100%;overflow-wrap:anywhere;}[aria-selected=true]{font-weight:bold;background:var(--atlas-accent);color:var(--atlas-on-accent);}[role=tabpanel]{padding-top:1rem;min-width:0;}@media(max-width:380px){.node-page{padding:var(--atlas-card-padding-narrow,.75rem);}}</style>
