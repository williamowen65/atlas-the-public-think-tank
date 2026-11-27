<!--
  @component
  NodeFeed: Presentational card list with automatic/manual pagination and empty/error states.
  The parent owns data; optional snippets replace voting/reactions for each item.

  **Props**
  - `renderVoting` (`Snippet<[FeedItem]>`; optional): Svelte snippet receiving a FeedItem, used to
    supply voting controls for each card.
  - `renderReactions` (`Snippet<[FeedItem]>`; optional): Svelte snippet receiving a FeedItem, used
    to supply reaction controls for each card.
  - `items` (`FeedItem[]`; required): Loaded FeedItem records to display; the parent owns fetching
    and accumulation.
  - `loading` (`boolean`; optional; default `false`): Whether a page request is in progress; shows
    loading feedback and restricts paging.
  - `hasMore` (`boolean`; optional; default `false`): Whether more feed pages remain. Enables
    automatic loading and the Load more button.
  - `error` (`string`; optional; default `''`): Error text for feed paging; a nonempty value exposes
    retry feedback.
  - `presentation` (`NodeCardPresentation`; optional; default `'ribbon-corner'`): Selects a
    NodeCardPresentation layout; it does not change the underlying node data.
  - `onLoadMore` (`() => void`; optional): Callback: Requests the next page or retries the failed
    page; parent owns fetching and page state.
  - `onNavigate` (`(id: string) => void`; optional): Callback receiving a node ID from a card’s
    ancestor or related-node links. The parent chooses the destination/navigation mechanism.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
  - `onReadMore` (`(id: string) => void`; optional): Callback: Receives the node ID when Read more
    is activated; absence disables that action.
  - `loadOlderAncestors` (`LoadOlderAncestors`; optional): Async ancestor loader forwarded to
    Breadcrumb; follow the LoadOlderAncestors type definition for its fields.
  - `apiBase` (`string`; optional): Forwarded to card breadcrumb readers; NodeFeed itself does not
    fetch discovery pages.
  - `onChildType` (`(id: string, typeId: string) => void`; optional): Callback: Receives node ID and
    contribution type ID when a contribution-summary button is activated.
  - `onComments` (`(id: string) => void`; optional): Callback: Receives the node ID when its
    comment-count button is activated.
  - `emptyTypeName` (`string`; optional): Singular contribution type name used in the empty-feed
    invitation.
  - `onCreateFirst` (`() => void`; optional): Callback: Called by the empty-feed invitation to open
    creation for that contribution type.
-->
<script lang="ts" module>
  import type { NodeCardData } from './NodeCard.svelte';
  export interface FeedItem {
    node: NodeCardData;
    average: number | null;
    totalVotes: number;
    reactionError?: boolean;
    hasMoreAncestors?: boolean;
    page?: number;
  }
</script>

<script lang="ts">
  import LoadingPlaceholder from './LoadingPlaceholder.svelte';
  import type { Snippet } from 'svelte';
  import NodeCard, { type NodeCardPresentation } from './NodeCard.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  let {
    renderVoting,
    renderReactions,
    items,
    loading = false,
    loadingNotice = '',
    hasMore = false,
    hasPrevious = false,
    error = '',
    errorRole = 'alert',
    presentation = 'ribbon-corner',
    onLoadMore,
    onLoadPrevious,
    onRetry,
    onNavigate,
    onOpenProfile,
    onReadMore,
    onComments,
    onChildType,
    loadOlderAncestors,
    emptyTypeName,
    onCreateFirst,
    apiBase,
    connected = false,
    inline = false,
    inlineRelationships = false,
  }: {
    /** Svelte snippet receiving a FeedItem, used to supply voting controls for each card. */
    renderVoting?: Snippet<[FeedItem, compact?: boolean]>;
    /** Svelte snippet receiving a FeedItem, used to supply reaction controls for each card. */
    renderReactions?: Snippet<[FeedItem]>;
    /** Loaded FeedItem records to display; the parent owns fetching and accumulation. */
    items: FeedItem[];
    /** Only direct child feeds draw sibling branches. */
    connected?: boolean;
    /** Embedded feeds use manual paging and do not participate in route scroll anchors. */
    inline?: boolean;
    inlineRelationships?: boolean;
    /** Whether a page request is in progress; shows loading feedback and restricts paging. */
    loading?: boolean;
    /** Slow-request feedback, announced outside the busy list and above placeholders. */
    loadingNotice?: string;
    /** Whether more feed pages remain. Enables automatic loading and the Load more button. */
    hasMore?: boolean;
    /** Whether an earlier discovery page can be prepended while the visitor scrolls upward. */
    hasPrevious?: boolean;
    /** Error text for feed paging; a nonempty value exposes retry feedback. */
    error?: string;
    /** Recoverable availability notices use a polite live region. */
    errorRole?: 'alert' | 'status';
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
    /**
     * Callback: Requests the next page or retries the failed page; parent owns fetching and page
     * state.
     */
    onLoadMore?: () => void;
    /** Callback requesting the preceding page; returned records are prepended by the parent. */
    onLoadPrevious?: () => void;
    /** Callback retrying the exact page and direction that failed, if either page failed. */
    onRetry?: () => void;
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
  let head = $state<HTMLDivElement>();
  /**
   * Trimmed contribution type label used by the empty-feed invitation.
   */
  const singularType = $derived((emptyTypeName ?? '').trim());
  // Observe the tail only when another page can be requested; the manual button remains the fallback.
  $effect(() => {
    items.length;
    if (
      inline || typeof IntersectionObserver === 'undefined' ||
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
  // Page windows can start in the middle of the result set. Fetch the preceding page only when
  // the visitor approaches the top edge of the currently loaded window.
  $effect(() => {
    items.length;
    if (
      inline || typeof IntersectionObserver === 'undefined' ||
      !head ||
      loading ||
      error ||
      !hasPrevious ||
      !onLoadPrevious
    )
      return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) onLoadPrevious?.();
      },
      { rootMargin: '240px 0px 0px 0px' },
    );
    observer.observe(head);
    return () => observer.disconnect();
  });
  /** Reserve initial list space, including the first frame before the parent starts fetching. */
  const initialLoading = $derived(!items.length && !error && (loading || hasMore));
</script>

<!--
  Layout: A semantic list contains node cards; a separate tail region handles paging, retry,
  loading and the invitation to create the first contribution.
-->

<!-- Keep delay feedback outside aria-busy so assistive technology can announce it now. -->
<div class="node-feed" class:mini-notice={presentation === 'mini' && connected && inline && !!loadingNotice} role={loadingNotice ? 'status' : undefined} aria-live="polite">
  {#if loadingNotice}<p>{loadingNotice}</p>{/if}
</div>
<section class="node-feed" aria-label="Node feed" aria-busy={loading || initialLoading}>
  {#if initialLoading && presentation !== 'mini'}<LoadingPlaceholder />{/if}
  {#if hasPrevious}<div class="feed-head" bind:this={head}>
      <button type="button" disabled={loading || !!error || !onLoadPrevious} onclick={onLoadPrevious}
        >Load previous nodes</button
      >
    </div>{/if}
  <ul aria-label="Nodes" class:connected class:mini-feed={presentation === 'mini'} class:attached-mini-feed={presentation === 'mini' && inline && connected}>
    {#if initialLoading && presentation === 'mini'}
      <li><LoadingPlaceholder kind="mini" label="Loading subnodes…" /></li>
    {/if}
    {#each items as item (item.node.id)}
      <!--
        Per-item closures adapt the parent’s renderers into the zero-argument snippets accepted
        by NodeCard.
      -->
      {#snippet voting(compact?: boolean)}{#if renderVoting}{@render renderVoting(item, compact)}{/if}{/snippet}
      {#snippet reactions()}{#if renderReactions}{@render renderReactions(item)}{/if}{/snippet}
      <li
        class:moderated-card={item.node.isHidden === true}
        data-node-id={inline ? undefined : item.node.id}
        data-feed-page={inline ? undefined : item.page}
      >
        {#if item.node.isHidden}<p class="moderation-indicator">Hidden by moderator · Private contribution</p>{/if}
        <NodeCard
          reactionControls={renderReactions ? reactions : undefined}
          voting={renderVoting ? voting : undefined}
          node={item.node}
          {presentation}
          {apiBase}
          {inlineRelationships}
          relationshipContext={connected}
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
  <div class="feed-tail" class:mini-tail={presentation === 'mini'} bind:this={tail}>
    {#if error}<p role={errorRole}>{error}</p>{/if}
    {#if !loading && !error && !items.length && !hasMore && emptyTypeName && onCreateFirst}
      <div class="empty-invitation">
        <h3>Be the first to contribute</h3>
        <p>
          There aren't any {singularType.toLowerCase()} contributions here yet. Start the conversation
          by adding one.
        </p>
        <button type="button" onclick={onCreateFirst}
          >Add {singularType}</button
        >
      </div>
    {:else if !initialLoading}<p role="status">
        {loading
          ? 'Loading nodes…'
          : !items.length
            ? error
              ? 'No nodes loaded.'
              : 'No nodes found.'
            : `${items.length} ${items.length === 1 ? 'node' : 'nodes'} shown${hasMore ? '.' : ' · You’re all caught up.'}`}
      </p>{/if}
    {#if (hasMore || error) && !(initialLoading && presentation === 'mini')}<button
        type="button"
        disabled={loading || !onLoadMore}
        onclick={error ? onRetry ?? onLoadMore : onLoadMore}>{error ? 'Retry loading nodes' : 'Load more nodes'}</button
      >{/if}
  </div>
</section>

<style lang="scss">
  .moderation-indicator { font-weight: 700; margin: 0 0 .9rem; color: var(--atlas-text); }
  .moderated-card { padding: 1.2rem; border: 1px solid color-mix(in srgb, #bd8336 45%, var(--atlas-border)); border-radius: 1rem; background: color-mix(in srgb, #bd8336 7%, var(--atlas-surface)); }
  .moderated-card :global(.read-more) { margin-left: auto; }
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
  /* Only a parent-scoped list has this rail; normal Discover items remain independent. */
  ul.connected {
    --branch-gutter: var(--atlas-connected-branch-gutter, clamp(.85rem, 3vw, 1.6rem));
    --branch-color: var(--atlas-connected-branch-color, color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border)));
    padding-inline-start: var(--branch-gutter);
  }
  ul.connected > li { position: relative; }
  ul.connected > li::before, ul.connected > li::after {
    content: ''; position: absolute; pointer-events: none;
    inset-inline-start: calc(-1 * var(--branch-gutter) + 2px);
  }
  ul.connected > li::before {
    inset-block-start: 0; inset-block-end: -1.25rem;
    border-inline-start: 2px solid var(--branch-color);
  }
  ul.connected > li:first-child::before { inset-block-start: -.6rem; }
  ul.connected > li:last-child::before { inset-block-end: auto; block-size: 1.4rem; }
  ul.connected > li:only-child::before { block-size: 2rem; }
  ul.connected > li::after {
    inset-block-start: 1.4rem; inline-size: calc(var(--branch-gutter) - 2px);
    border-block-start: 2px solid var(--branch-color);
  }
  ul.connected.mini-feed {
    --mini-branch-gap: .75rem;
    --mini-branch-anchor: 1.1rem;
    gap: var(--mini-branch-gap);
  }
  ul.connected.mini-feed > li::before { inset-block-end: calc(-1 * var(--mini-branch-gap)); }
  ul.connected.mini-feed > li:last-child::before {
    inset-block-end: auto; block-size: var(--mini-branch-anchor);
  }
  ul.connected.mini-feed > li:only-child::before {
    block-size: calc(var(--mini-branch-anchor) + .6rem);
  }
  ul.connected.mini-feed > li::after { inset-block-start: var(--mini-branch-anchor); }
  /* In the compact card the section header supplies the first part of the rail.
     The list starts exactly where the header/slow-loading notice ends. */
  ul.connected.attached-mini-feed > li:first-child::before { inset-block-start: 0; }
  ul.connected.attached-mini-feed > li:only-child::before { block-size: var(--mini-branch-anchor); }
  .mini-notice {
    position: relative;
    padding-inline-start: var(--atlas-connected-branch-gutter, 1.6rem);
    padding-block-end: .5rem;
  }
  .mini-notice p { margin: 0; }
  .mini-notice::before {
    content: '';
    position: absolute;
    inset-block: 0;
    inset-inline-start: 2px;
    border-inline-start: 2px solid var(--atlas-connected-branch-color, var(--atlas-border));
    pointer-events: none;
  }
  .mini-tail { font-size: .8rem; }
  .feed-tail.mini-tail { padding: .65rem .25rem .15rem; }
  .mini-tail p { margin: .2rem 0; }
  .feed-tail {
    text-align: center;
    padding: 1.5rem 0.5rem;
    color: var(--atlas-muted);
  }
  .feed-head {
    display: flex;
    justify-content: center;
    min-height: 0.5rem;
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
