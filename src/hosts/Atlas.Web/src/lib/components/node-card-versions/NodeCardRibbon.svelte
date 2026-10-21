<script lang="ts">
  import ActionButton from '../ActionButton.svelte';
  import VotingDial from '../VotingDial.svelte';

  import Breadcrumb from '../Breadcrumb.svelte';
  import type { NodeCardProps } from './types';
  let { node, value = null, average = null, totalVotes = 0, onVote, onNavigate, onComments, hasMoreAncestors = false, loadOlderAncestors }: NodeCardProps = $props();
  let expanded = $state(false);
  const instance = $props.id();
  const nodeId = $derived(node.id);
  const connectionLabels = $derived([
    ...(node.subNodeSummary && node.subNodeSummary !== 'None' ? node.subNodeSummary.split(' · ') : []),
    ...node.related.map(related => `${related.typeName}: ${related.title}`),
  ]);
  $effect(() => { nodeId; expanded = false; });
</script>

<article class="node-card" data-presentation="ribbon" aria-labelledby={`${instance}-title`}>
  {#if node.breadcrumb.length}
    <div class="ribbon-context"><Breadcrumb items={node.breadcrumb} {onNavigate} hasMore={hasMoreAncestors} loadOlder={loadOlderAncestors} /></div>
  {/if}
  <header>
      <h2 id={`${instance}-title`}><span class="title-type">{node.typeName}:</span> {node.title}</h2>
      {#if node.status === 'Archived'}<p class="byline">Archived</p>{/if}
    <p class="byline">By {node.authorName} <span aria-hidden="true">·</span> <time datetime={node.createdAt}>{new Date(node.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', timeZone: 'UTC' })}</time></p>
  </header>

  <div class="body">
    <aside class="dial" aria-label="Importance rating">
      <VotingDial label="Node importance" {value} {average} {totalVotes} disabled={node.status === 'Archived'} onChange={onVote} />
    </aside>
    <div class="content-stack">
      <p class="description" class:expanded id={`${instance}-description`}>{node.description}</p>
      {#if node.requestedTypes.length}
        <p class="invitation">Help develop this node with: <strong>{node.requestedTypes.join(', ')}</strong>.</p>
      {/if}
      {@render reactions()}
    </div>
  </div>
  <div class="read-more">
    <ActionButton label={expanded ? 'Show less' : 'Read more'} {expanded} controls={`${instance}-description`} onActivate={() => expanded = !expanded} />
  </div>
  <div class="card-details">
    {#if node.communities.length}
      <div class="communities" role="group" aria-label="Communities">
        <strong>{node.communities.length === 1 ? 'Community' : 'Communities'}:</strong> {node.communities.join(', ')}
      </div>
    {/if}
    <div class="navigation-details">
      {@render comments()}
      {#each connectionLabels as label}<span class="navigation-separator" aria-hidden="true">·</span><span class="text-action connection-summary">{label}</span>{/each}
    </div>
  </div>

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

<style lang="scss">
  .node-card {
    --card-padding: 1.25rem;
    --dial-text-gap: 1rem;
    --atlas-breadcrumb-bottom-space: 0px;
    max-width: 48rem;
    width: 100%;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    overflow-wrap: anywhere;
    header {
      padding: var(--card-padding) var(--card-padding) .5rem;
    }
    h2 {
      margin: 0 0 .5rem;
      font-size: clamp(1.35rem, 4vw, 1.9rem);
      line-height: 1.2;
    }
    p {
      line-height: 1.65;
    }
  }

  .ribbon-context {
    min-width: 0;
    padding: .75rem 0 0;
    margin-bottom: var(--card-padding);
  }

  .ribbon-context + header {
    padding-top: 0;
    h2 { margin-top: 0; }
  }

  .title-type { color: var(--atlas-accent); }

  .read-more {
    clear: both;
    display: flex;
    justify-content: center;
    padding: 0 var(--card-padding) .75rem;
  }

  [data-presentation] {
    .card-details {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      align-items: baseline;
      gap: .35rem 1rem;
      padding: 0 var(--card-padding) 1rem;
      font-size: .9rem;
      color: var(--atlas-muted);
    }
    .reactions {
      display: block;
      line-height: 2.4;
      span {
        display: inline-block;
        margin: .15rem .35rem .15rem 0;
        padding: .1rem .65rem;
        border: 1px solid var(--atlas-border);
        border-radius: 999px;
        background: var(--atlas-surface);
        line-height: 1.7;
      }
    }
    .communities {
      margin: 0;
      padding: 0;
      border: 0;
    }
    .navigation-details {
      display: flex;
      flex-wrap: wrap;
      justify-content: flex-end;
      align-items: baseline;
      column-gap: .35em;
      font-size: .9rem;
      text-align: right;
      margin-left: auto;
      .text-action { padding-block: .15rem; }
      .navigation-separator { font-size: .9rem; }
      .connection-summary {
        text-decoration: none;
        color: var(--atlas-muted);
        cursor: default;
      }
    }
  }



  .byline {
    margin: 0;
    font-size: .85rem;
    color: var(--atlas-muted);
  }

  .body {
    padding: .5rem var(--card-padding) 1rem;
    display: flow-root;
  }

  .dial {
    float: left;
    margin: 0 var(--dial-text-gap) .6rem -.4rem;
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
    .node-card {
      --card-padding: .75rem;
      --dial-text-gap: .6rem;
    }
    .node-card header, .body {
      padding-inline: var(--card-padding);
    }
  }
</style>
