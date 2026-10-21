<script lang="ts">
  import VotingDial from '../VotingDial.svelte';

  import type { NodeCardProps } from './types';
  let { node, value = null, average = null, totalVotes = 0, onVote, onNavigate, onComments }: NodeCardProps = $props();
  let expanded = $state(false);
  const instance = $props.id();
  const nodeId = $derived(node.id);
  $effect(() => { nodeId; expanded = false; });
</script>

<article class="node-card" data-presentation="context-below" aria-labelledby={`${instance}-title`}>
  <header>
      <div class="eyebrow">
        <span class="type">{node.typeName}</span>
        {#each node.communities as community}<span>{community}</span>{/each}
        {#if node.status === 'Archived'}<span>Archived</span>{/if}
      </div>
      <h2 id={`${instance}-title`}>{node.title}</h2>
    <p class="byline">By {node.authorName} <span aria-hidden="true">·</span> <time datetime={node.createdAt}>{new Date(node.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', timeZone: 'UTC' })}</time></p>
  </header>

  <div class="body">
    <aside class="dial" aria-label="Importance rating">
      <VotingDial label="Node importance" {value} {average} {totalVotes} disabled={node.status === 'Archived'} onChange={onVote} />
    </aside>
    <p class="description" class:expanded id={`${instance}-description`}>{node.description}</p>
    <button class="text-action" type="button" aria-expanded={expanded} aria-controls={`${instance}-description`} onclick={() => expanded = !expanded}>{expanded ? 'Show less' : 'Read more'}</button>
    {#if node.requestedTypes.length}
      <p class="invitation">Help develop this node with: <strong>{node.requestedTypes.join(', ')}</strong>.</p>
    {/if}
  </div>

  {#if node.breadcrumb.length}
    <p class="breadcrumb" aria-label="Context">{node.breadcrumb.map(item => item.title).join(' / ')}</p>
  {/if}
    {@render connections()}
    <footer>
      {@render reactions()}
      {@render comments()}
    </footer>
</article>

{#snippet reactions()}
  {#if node.reactions.length}
    <div class="reactions" role="group" aria-label="Community reactions">
      {#each node.reactions as reaction}<span class="reaction-pill">{reaction.label} <strong>{reaction.count}</strong></span>{/each}
    </div>
  {/if}
{/snippet}

{#snippet comments()}
  <button class="text-action" type="button" disabled={!onComments} onclick={() => onComments?.(node.id)}>{node.commentCount} {node.commentCount === 1 ? 'Comment' : 'Comments'}</button>
{/snippet}

{#snippet connections()}
  {#if node.subNodeSummary}<p class="breadcrumb">Connected nodes: {node.subNodeSummary}</p>{/if}
  {#if node.related.length}
    <section class="related" aria-label="Connected nodes">
      <h3>Explore the connections</h3>
      {#each node.related as related}
        <button type="button" disabled={!onNavigate} onclick={() => onNavigate?.(related.id)}><span>{related.typeName}</span> {related.title} <span aria-hidden="true">↗</span></button>
      {/each}
    </section>
  {/if}
{/snippet}
<style lang="scss">
  .node-card {
    max-width: 48rem;
    width: 100%;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    overflow-wrap: anywhere;
    header {
      padding: 1.25rem 1.25rem .5rem;
    }
    h2 {
      margin: .7rem 0 .5rem;
      font-size: clamp(1.35rem, 4vw, 1.9rem);
      line-height: 1.2;
    }
    h3 {
      margin: 0 0 .6rem;
      font-size: .85rem;
      color: var(--atlas-muted);
    }
    p {
      line-height: 1.65;
    }
  }











  .eyebrow {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: .65rem;
    font-size: .8rem;
    color: var(--atlas-muted);
    .type {
      border: 1px solid var(--atlas-accent);
      border-radius: 1rem;
      padding: .15rem .65rem;
      color: var(--atlas-accent);
      font-weight: 700;
    }
  }

  .byline {
    margin: 0;
    font-size: .85rem;
    color: var(--atlas-muted);
  }

  .body {
    padding: .5rem 1.25rem 1rem;
    display: flow-root;
  }

  .dial {
    float: left;
    margin: 0 1rem .6rem -.4rem;
  }

  .description {
    margin: 0 0 .65rem;
    white-space: pre-line;
    max-height: 8.25em;
    overflow: hidden;
    &.expanded {
      max-height: none;
    }
  }

  .invitation {
    font-size: .9rem;
    color: var(--atlas-muted);
  }

  .text-action {
    border: 0;
    padding: .5rem 0;
    background: transparent;
    color: var(--atlas-accent);
    font: inherit;
    font-size: .9rem;
    cursor: pointer;
    text-decoration: underline;
  }

  .breadcrumb {
    margin: 0;
    padding: .7rem 1.25rem;
    border-top: 1px solid var(--atlas-border);
    color: var(--atlas-muted);
    font-size: .8rem;
  }

  .related {
    padding: 1rem 1.25rem;
    border-top: 1px solid var(--atlas-border);
    button {
      display: block;
      width: 100%;
      padding: .65rem 0;
      border: 0;
      text-align: left;
      background: transparent;
      color: var(--atlas-accent);
      font: inherit;
      cursor: pointer;
      overflow-wrap: anywhere;
    }
    span {
      font-size: .8rem;
      color: var(--atlas-muted);
    }
  }

  footer {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
    align-items: center;
    gap: .75rem;
    padding: .75rem 1.25rem;
    border-top: 1px solid var(--atlas-border);
  }

  .reactions {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    font-size: .85rem;
    color: var(--atlas-muted);
  }

  button:disabled {
    cursor: default;
    color: var(--atlas-muted);
    text-decoration: none;
  }

  @media (max-width: 380px) {
    .node-card header, .body, .related, footer, .breadcrumb {
      padding-inline: .75rem;
    }
    .dial {
      margin-right: .6rem;
    }
  }
</style>
