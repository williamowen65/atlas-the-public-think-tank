<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { NodeCardData } from './NodeCard.svelte';
  import type { NodeType, ContentBlock } from '../api/nodePage';
  import ContentBlocks from './ContentBlocks.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  import Breadcrumb from './Breadcrumb.svelte';
  import VotingDial from './VotingDial.svelte';
  let { onOpenProfile, voting, revealChildren = false, node, blocks, types, active, draftTab, onTabChange, onAddChild, onNavigate, hasMoreAncestors = false, loadOlderAncestors, average = null, totalVotes = 0, children }: { voting?: Snippet; revealChildren?: boolean; node: NodeCardData; blocks: ContentBlock[]; types: NodeType[]; active: string; draftTab?: { id: string; name: string }; onTabChange: (id: string) => void; onAddChild: (id?: string) => void; onNavigate?: (id: string) => void; onOpenProfile?: (id: string) => void; hasMoreAncestors?: boolean; loadOlderAncestors?: LoadOlderAncestors; average?: number | null; totalVotes?: number; children: Snippet } = $props();
  const instance = $props.id();
  const tabs = $derived([
    { id: 'comments', name: `${node.commentCount} ${node.commentCount === 1 ? 'Comment' : 'Comments'}` },
    ...(node.childTypes ?? []).map(type => ({ id: type.typeId, name: type.label })),
    ...(draftTab && !(node.childTypes ?? []).some(type => type.typeId === draftTab.id) ? [draftTab] : []),
  ]);
  function reveal(tabs: HTMLDivElement) {
    if (!revealChildren) return;
    const card = tabs.closest<HTMLElement>('.node-page')!;
    let frame = 0;
    let stopped = false;
    const align = () => {
      if (stopped || !tabs.isConnected) return;
      card.style.setProperty('--discussion-tabs-height', `${tabs.getBoundingClientRect().height}px`);
      // A short/empty child feed still needs enough document height to reach this offset.
      const delta = tabs.getBoundingClientRect().top - 20;
      if (Math.abs(delta) > 1) window.scrollBy({ top: delta, behavior: 'instant' });
    };
    const schedule = () => { cancelAnimationFrame(frame); frame = requestAnimationFrame(align); };
    const observer = new ResizeObserver(schedule);
    const events = ['wheel', 'touchstart', 'pointerdown', 'keydown'] as const;
    const stop = () => {
      stopped = true;
      cancelAnimationFrame(frame);
      observer.disconnect();
      for (const event of events) window.removeEventListener(event, stop, true);
      clearTimeout(timeout);
    };
    // Re-align during initial feed/image/font layout, but never fight user interaction.
    observer.observe(card);
    observer.observe(tabs);
    for (const event of events) window.addEventListener(event, stop, { capture: true, passive: true });
    const timeout = setTimeout(stop, 10000);
    frame = requestAnimationFrame(() => {
      tabs.querySelector<HTMLElement>('[aria-selected="true"]')?.focus({ preventScroll: true });
      schedule();
    });
    void document.fonts.ready.then(schedule);
    return { destroy: stop };
  }
  function key(event: KeyboardEvent, index: number) { let next = index; if (event.key === 'ArrowRight') next = (index + 1) % tabs.length; else if (event.key === 'ArrowLeft') next = (index - 1 + tabs.length) % tabs.length; else if (event.key === 'Home') next = 0; else if (event.key === 'End') next = tabs.length - 1; else return; event.preventDefault(); onTabChange(tabs[next].id); document.getElementById(`${instance}-${tabs[next].id}`)?.focus(); }
</script>
<article class="node-page atlas-panel" class:reveal-children={revealChildren}>
  {#if node.breadcrumb.length}<div class="ribbon-context"><Breadcrumb items={node.breadcrumb} {onNavigate} hasMore={hasMoreAncestors} loadOlder={loadOlderAncestors} /></div>{/if}
  <header><h1>{node.typeName}: {node.title}</h1><p class="byline">By {#if node.authorId}<a href={`/profiles?id=${encodeURIComponent(node.authorId)}`} onclick={event => { if (onOpenProfile && node.authorId) { event.preventDefault(); onOpenProfile(node.authorId); } }}>{node.authorName}</a>{:else}{node.authorName}{/if} · <time datetime={node.createdAt}>{new Date(node.createdAt).toLocaleDateString()}</time>{#if node.communities.length} · Community: {node.communities.join(', ')}{/if}</p>{#if node.status === 'Archived'}<p>Archived</p>{/if}</header>
  <div class="post-body">
    <aside class="dial" aria-label="Importance rating">{#if voting}{@render voting()}{:else}<VotingDial label="Node importance" {average} {totalVotes} disabled />{/if}</aside>
    <p class="description">{node.description}</p>
    <ContentBlocks {blocks} />
  </div>
  {#if node.reactions.length}<div class="reactions" role="group" aria-label="Community reactions">{#each node.reactions as reaction}<span>{reaction.label} <strong>{reaction.count}</strong></span>{/each}</div>{/if}
  <div class="add"><button onclick={() => onAddChild(active === 'comments' ? undefined : active)} disabled={node.status === 'Archived'}>Add {active === 'comments' ? 'a child node' : types.find(type => type.id === active)?.name ?? 'a child node'}</button></div>
  <div class="tabs" use:reveal role="tablist" aria-label="Node discussions and child types">{#each tabs as tab, index}<button role="tab" id={`${instance}-${tab.id}`} aria-selected={active === tab.id} aria-controls={`${instance}-panel`} tabindex={active === tab.id ? 0 : -1} onclick={() => onTabChange(tab.id)} onkeydown={event => key(event, index)}>{tab.name}</button>{/each}</div>
  <div role="tabpanel" id={`${instance}-panel`} aria-labelledby={`${instance}-${active}`} tabindex="0">{@render children()}</div>
</article>
<style lang="scss">
  .node-page {
    --card-padding: var(--atlas-card-padding, 1.25rem);
    --dial-text-gap: var(--atlas-dial-text-gap, 1rem);
    --atlas-breadcrumb-bottom-space: 0px;
    max-width: 48rem;
    margin-inline: auto;
    padding: 0;
  }
  .ribbon-context { min-width: 0; padding: .75rem 0 0; margin-bottom: var(--card-padding); }
  header { padding: var(--card-padding) var(--card-padding) .5rem; }
  .ribbon-context + header { padding-top: 0; }
  h1 { font-size: clamp(1.4rem,4vw,2rem); margin: 0 0 .5rem; }
  .byline { font-size: .85rem; color: var(--atlas-muted); margin: 0; }
  .post-body { display: flow-root; padding: .5rem var(--card-padding) 0; line-height: var(--atlas-line-height,1.65); }
  .dial { float: left; margin: 0 var(--dial-text-gap) .6rem -.4rem; }
  .description { margin: 0 0 var(--atlas-paragraph-gap,.65rem); white-space: pre-line; }
  .reactions { display: flex; flex-wrap: wrap; gap: .5rem; padding: .5rem var(--card-padding) 0; font-size: .85rem; color: var(--atlas-muted); }
  .reactions span { border: 1px solid var(--atlas-border); border-radius: 999px; padding: .2rem .6rem; }
  .add { display: flex; justify-content: flex-end; padding: .5rem var(--card-padding); }
  .tabs { scroll-margin-top: 20px; display: flex; flex-wrap: wrap; gap: .4rem; border-bottom: 1px solid var(--atlas-border); padding: 0 var(--card-padding) .75rem; }
  button { font: inherit; padding: .5rem .75rem; cursor: pointer; max-width: 100%; overflow-wrap: anywhere; }
  [aria-selected=true] { font-weight: bold; background: var(--atlas-accent); color: var(--atlas-on-accent); }
  [role=tabpanel] { padding: 1rem var(--card-padding) var(--card-padding); min-width: 0; }
  .reveal-children [role=tabpanel] {
    box-sizing: border-box;
    min-height: calc(100dvh - var(--discussion-tabs-height, 60px) - 20px);
  }
  @media(max-width:380px) {
    .node-page { --card-padding: var(--atlas-card-padding-narrow,.75rem); --dial-text-gap: var(--atlas-dial-text-gap,.6rem); }
  }
</style>
