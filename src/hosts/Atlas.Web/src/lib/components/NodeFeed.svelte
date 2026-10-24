<!--
  NodeFeed: Presentational card list with automatic/manual pagination and empty/error states.
  The parent owns data; optional snippets replace voting/reactions for each item.
-->
<script lang="ts" module>
  import type { NodeCardData } from './NodeCard.svelte';
  export interface FeedItem {
    node: NodeCardData;
    average: number | null;
    totalVotes: number;
    reactionError?: boolean;
    hasMoreAncestors?: boolean;
  }
</script>

<script lang="ts">
  import type { Snippet } from 'svelte';
  import NodeCard, { type NodeCardPresentation } from './NodeCard.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  let {
    renderVoting,
    renderReactions,
    items,
    loading = false,
    hasMore = false,
    error = '',
    presentation = 'ribbon-corner',
    onLoadMore,
    onNavigate,
    onOpenProfile,
    onReadMore,
    onComments,
    onChildType,
    loadOlderAncestors,
    emptyTypeName,
    onCreateFirst,
    apiBase,
  }: {
    /** Svelte snippet receiving a FeedItem, used to supply voting controls for each card. */
    renderVoting?: Snippet<[FeedItem]>;
    /** Svelte snippet receiving a FeedItem, used to supply reaction controls for each card. */
    renderReactions?: Snippet<[FeedItem]>;
    /** Loaded FeedItem records to display; the parent owns fetching and accumulation. */
    items: FeedItem[];
    /** Whether a page request is in progress; shows loading feedback and restricts paging. */
    loading?: boolean;
    /** Whether more feed pages remain. Enables automatic loading and the Load more button. */
    hasMore?: boolean;
    /** Error text for feed paging; a nonempty value exposes retry feedback. */
    error?: string;
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
    /**
     * Callback: Requests the next page or retries the failed page; parent owns fetching and page
     * state.
     */
    onLoadMore?: () => void;
    /**
     * Callback receiving a node ID from a card’s ancestor or related-node links. The parent
     * chooses the destination/navigation mechanism.
     */
    onNavigate?: (id: string) => void;
    /** Callback: Receives the author/participant ID to open in a profile page. */
    onOpenProfile?: (id: string) => void;
    /** Callback: Receives the node ID when Read more is activated; absence disables that action. */
    onReadMore?: (id: string) => void;
    /**
     * Async ancestor loader forwarded to Breadcrumb; follow the LoadOlderAncestors type definition
     * for its fields.
     */
    loadOlderAncestors?: LoadOlderAncestors;
    /** Forwarded to card breadcrumb readers; NodeFeed itself does not fetch discovery pages. */
    apiBase?: string;
    /**
     * Callback: Receives node ID and contribution type ID when a contribution-summary button is
     * activated.
     */
    onChildType?: (id: string, typeId: string) => void;
    /** Callback: Receives the node ID when its comment-count button is activated. */
    onComments?: (id: string) => void;
    /** Singular contribution type name used in the empty-feed invitation. */
    emptyTypeName?: string;
    /**
     * Callback: Called by the empty-feed invitation to open creation for that contribution type.
     */
    onCreateFirst?: () => void;
  } = $props();
  let tail: HTMLDivElement;
  /**
   * Trimmed contribution type label used by the empty-feed invitation.
   */
  const singularType = $derived((emptyTypeName ?? '').trim());
  /**
   * English indefinite article selected from the invitation type’s initial letter.
   */
  const article = $derived(/^[aeiou]/i.test(singularType) ? 'an' : 'a');
  // Observe the tail only when another page can be requested; the manual button remains the fallback.
  $effect(() => {
    items.length;
    if (
      typeof IntersectionObserver === 'undefined' ||
      !tail ||
      loading ||
      error ||
      !hasMore ||
      !onLoadMore
    )
      return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) onLoadMore?.();
      },
      { rootMargin: '0px 0px 240px 0px' },
    );
    observer.observe(tail);
    return () => observer.disconnect();
  });
</script>

<!--
  Layout: A semantic list contains node cards; a separate tail region handles paging, retry,
  loading and the invitation to create the first contribution.
-->

<section class="node-feed" aria-label="Node feed" aria-busy={loading}>
  <ul aria-label="Nodes">
    {#each items as item (item.node.id)}
      <!--
        Per-item closures adapt the parent’s renderers into the zero-argument snippets accepted
        by NodeCard.
      -->
      {#snippet voting()}{#if renderVoting}{@render renderVoting(item)}{/if}{/snippet}
      {#snippet reactions()}{#if renderReactions}{@render renderReactions(item)}{/if}{/snippet}
      <li>
        <NodeCard
          reactionControls={renderReactions ? reactions : undefined}
          voting={renderVoting ? voting : undefined}
          node={item.node}
          {presentation}
          {apiBase}
          average={item.average}
          totalVotes={item.totalVotes}
          {onNavigate}
          {onOpenProfile}
          {onReadMore}
          {onComments}
          {onChildType}
          {loadOlderAncestors}
          hasMoreAncestors={item.hasMoreAncestors ?? false}
        />
        {#if item.reactionError}<p class="note">
            Reaction totals are unavailable for this node.
          </p>{/if}
      </li>
    {/each}
  </ul>
  <!--
    This wrapper is also the intersection-observer target for automatic loading; the button
    remains the manual/retry path.
  -->
  <div class="feed-tail" bind:this={tail}>
    {#if error}<p role="alert">{error}</p>{/if}
    {#if !loading && !error && !items.length && !hasMore && emptyTypeName && onCreateFirst}
      <div class="empty-invitation">
        <h3>Be the first to create {article} {singularType.toLowerCase()}</h3>
        <p>
          There aren't any {singularType.toLowerCase()} contributions here yet. Start the conversation
          by adding one.
        </p>
        <button type="button" onclick={onCreateFirst}
          >Create {article} {singularType.toLowerCase()}</button
        >
      </div>
    {:else}<p role="status">
        {loading
          ? 'Loading nodes…'
          : !items.length
            ? error
              ? 'No nodes loaded.'
              : 'No nodes found.'
            : `${items.length} nodes shown${hasMore ? '.' : ' · You’re all caught up.'}`}
      </p>{/if}
    {#if hasMore || error}<button
        type="button"
        disabled={loading || !onLoadMore}
        onclick={onLoadMore}>{error ? 'Retry loading nodes' : 'Load more nodes'}</button
      >{/if}
  </div>
</section>

<style lang="scss">
  .node-feed {
    max-width: 48rem;
    margin-inline: auto;
    min-width: 0;
  }
  ul {
    list-style: none;
    padding: 0;
    margin: 0;
    display: grid;
    gap: 1.25rem;
  }
  li {
    min-width: 0;
  }
  .feed-tail {
    text-align: center;
    padding: 1.5rem 0.5rem;
    color: var(--atlas-muted);
  }
  button {
    font: inherit;
    padding: 0.6rem 1rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-accent);
    cursor: pointer;
  }
  .note {
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }
  .empty-invitation {
    padding: 2rem 1rem;
    border: 1px dashed var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
  }
  .empty-invitation h3 {
    margin: 0 0 0.5rem;
    color: var(--atlas-text);
    font-size: 1.2rem;
  }
  .empty-invitation p {
    margin: 0.5rem 0 1rem;
  }
</style>
