<!--
  @component
  NodePage: Presentational full-node composition with discussion tabs and caller-supplied snippets.
  The parent owns fetching and tab state; snippets supply voting, reactions and the active tab content.

  **Props**
  - `voting` (`Snippet`; optional): Renderable snippet for the voting region, replacing the
    read-only vote summary. LiveNodePage supplies LiveVotingDial here to add
    API/session behavior.
  - `reportControl` (`Snippet`; optional): Report action aligned to the right of the footer controls.
  - `reactions` (`Snippet`; optional): Renderable snippet for custom/live reaction controls. If
    omitted, NodePage displays the static reaction summary from node.reactions.
  - `scrollToDiscussionTabs` (`boolean`; optional; default `false`): Requests initial focus and
    scrolling toward 20px below the viewport top, limited by the page’s natural scroll range. No extra panel height is added.
  - `node` (`NodeCardData`; required): Display model for the node header, breadcrumb, status,
    community and contribution summaries. NodeCardData describes its fields; fetching belongs to the
    parent.
  - `blocks` (`ContentBlock[]`; required): Ordered content blocks passed to ContentBlocks. The
    parent supplies these separately from the node’s short description.
  - `types` (`NodeType[]`; required): Available node types and their singular/plural display labels.
  - `active` (`string`; required): Controlled discussion-tab ID: comments, a contribution type ID,
    or a temporary draft-tab ID. The parent changes this value after onTabChange requests.
  - `draftTab` (`{ id: string; name: string }`; optional): Temporary contribution-tab ID and label
    for an in-progress child draft.
  - `onTabChange` (`(id: string) => void`; required): Callback: Receives the requested
    discussion-tab ID; the parent controls active.
  - `onAddChild` (`(id?: string) => void`; required): Callback: Requests a child form for the
    supplied type ID, or lets the parent choose when omitted.
  - `onNavigate` (`(id: string) => void`; optional): Callback receiving an ancestor node ID from
    Breadcrumb. The parent decides how to navigate in the application.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
  - `hasMoreAncestors` (`boolean`; optional; default `false`): Whether additional older ancestors
    can be loaded for the node’s breadcrumb.
  - `loadOlderAncestors` (`LoadOlderAncestors`; optional): Async callback forwarded to Breadcrumb.
    Receives the oldest displayed node ID (possibly undefined) and returns a page of older ancestor
    items plus hasMore.
  - `apiBase` (`string`; optional): Forwarded to Breadcrumb for alternate-parent lookups; the parent
    supplies node/content data.
  - `average` (`number | null`; optional; default `null`): Community average supplied by the parent;
    null means no average is available.
  - `totalVotes` (`number`; optional; default `0`): Community vote count supplied by the parent.
  - `children` (`Snippet`; required): Renderable content supplied between the NodePage tags:
    comments, a child-node feed and/or a creation form for the active tab. Rendered in the tab
    panel; unrelated to whether child nodes exist.
-->
<script lang="ts">
  import { preloadWhenVisible } from "../actions/preloadWhenVisible";
  import { nodeTypeSelect } from '../actions/nodeTypeSelect';
  import CopyNodeLink from "./CopyNodeLink.svelte";
  import { relativeDate } from '../format/relativeDate';
  import NodeEditControl from './NodeEditControl.svelte';
  import type { Snippet } from 'svelte';
  import type { NodeCardData } from './NodeCard.svelte';
  import type { NodeType, ContentBlock } from '../api/nodePage';
  import ContentBlocks from './ContentBlocks.svelte';
  import CommunityLinks from './CommunityLinks.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  import Breadcrumb from './Breadcrumb.svelte';
  let {
    onOpenProfile,
    voting,
    reactions,
    commentsPanel,
    commentsInitiallyOpen = false,
    reportControl,
    scrollToDiscussionTabs = false,
    inspectionOnly = false,
    node,
    blocks,
    types,
    active,
    draftTab,
    onTabChange,
    onAddChild,
    childFormOpen = false,
    createTypeSelection = '',
    onCreateTypeChange,
    onCloseChild,
    childFormBusy = false,
    onNavigate,
    hasMoreAncestors = false,
    loadOlderAncestors,
    apiBase,
    average = null,
    totalVotes = 0,
    children,
  }: {
    /**
     * Renderable snippet for the voting region, replacing the read-only vote summary
     * fallback. LiveNodePage supplies LiveVotingDial here to add API/session behavior.
     */
    voting?: Snippet;
    /**
     * Renderable snippet for custom/live reaction controls. If omitted, NodePage displays the
     * static reaction summary from node.reactions.
     */
    reactions?: Snippet;
    /** Independent inline discussion; child feed remains mounted outside the card. */
    commentsPanel?: Snippet;
    commentsInitiallyOpen?: boolean;
    reportControl?: Snippet;
    /**
     * Requests initial focus and scrolling of the selected discussion tab to 20px below the
     * viewport top where the natural page height allows; never adds space just to reach the target.
     */
    scrollToDiscussionTabs?: boolean;
    /** Render the normal node presentation without editing, voting, or discussion controls. */
    inspectionOnly?: boolean;
    /**
     * Display model for the node header, breadcrumb, status, community and contribution summaries.
     * NodeCardData describes its fields; fetching belongs to the parent.
     */
    node: NodeCardData;
    /**
     * Ordered content blocks passed to ContentBlocks. The parent supplies these separately from
     * the node’s short description.
     */
    blocks: ContentBlock[];
    /** Available node types and their singular/plural display labels. */
    types: NodeType[];
    /**
     * Controlled discussion-tab ID: comments, a contribution type ID, or a temporary draft-tab ID.
     * The parent changes this value after onTabChange requests.
     */
    active: string;
    /** Temporary contribution-tab ID and label for an in-progress child draft. */
    draftTab?: { id: string; name: string };
    /** Callback: Receives the requested discussion-tab ID; the parent controls active. */
    onTabChange: (id: string) => void;
    /**
     * Callback: Requests a child form for the supplied type ID, or lets the parent choose when
     * omitted.
     */
    onAddChild: (id?: string) => void;
    childFormOpen?: boolean;
    /** Empty until the user explicitly selects a type, independent of the current feed filter. */
    createTypeSelection?: string;
    onCreateTypeChange?: (id: string) => void;
    onCloseChild?: () => void;
    childFormBusy?: boolean;
    /**
     * Callback receiving an ancestor node ID from Breadcrumb. The parent decides how to navigate
     * in the application.
     */
    onNavigate?: (id: string) => void;
    /** Callback: Receives the author/participant ID to open in a profile page. */
    onOpenProfile?: (id: string) => void;
    /** Whether additional older ancestors can be loaded for the node’s breadcrumb. */
    hasMoreAncestors?: boolean;
    /**
     * Async callback forwarded to Breadcrumb. Receives the oldest displayed node ID (possibly
     * undefined) and returns a page of older ancestor items plus hasMore.
     */
    loadOlderAncestors?: LoadOlderAncestors;
    /**
     * Forwarded to Breadcrumb for alternate-parent lookups; the parent supplies node/content data.
     */
    apiBase?: string;
    /** Community average supplied by the parent; null means no average is available. */
    average?: number | null;
    /** Community vote count supplied by the parent. */
    totalVotes?: number;
    /**
     * Renderable content supplied between the NodePage tags: comments, a child-node feed and/or a
     * creation form for the active tab. Rendered in the tab panel; unrelated to whether child
     * nodes exist.
     */
    children: Snippet;
  } = $props();
  /**
   * Stable unique ID for this component instance, used to link tab/panel DOM IDs
   * through aria-controls and aria-labelledby. Unlike an each-block key, it does
   * not control component reuse; it prevents collisions between NodePage instances.
   */
  const instance = $props.id();
  let reactionsOpen = $state(false);
  let reactionsVisited = $state(false);
  let visibleForPreload = $state(false);
  let commentsOpen = $state(false);
  $effect(() => { commentsOpen = commentsInitiallyOpen; });
  /**
   * Two primary views; the controlled active ID also carries the child-type filter
   * or a temporary draft ID so existing navigation and draft handling remain intact.
   */
  const reactionCount = $derived(node.reactionCount ?? (node.reactionSummary && !node.reactions.length && node.reactionSummary !== '—' ? undefined : node.reactions.reduce((sum, reaction) => sum + reaction.count, 0)));
  const childCount = $derived((node.childTypes ?? []).reduce((sum, type) => sum + type.count, 0));
  const childFilters = $derived([
    ...(node.childTypes ?? []).map((type) => ({ id: type.typeId, name: type.label })),
    ...(draftTab && !(node.childTypes ?? []).some((type) => type.typeId === draftTab.id)
      ? [draftTab]
      : []),
  ]);
  /**
   * Svelte action attached to the tab strip with use:alignDiscussionTabs.
   * Supports direct navigation from a contribution/comment link: focus the selected
   * tab and move the strip toward 20px below the viewport top, within natural scroll limits.
   * Correct initial image/feed/font layout shifts for up to ten seconds, but stop
   * as soon as the visitor interacts so automatic scrolling does not fight them.
   * @param tabs Mounted tab-strip element; this shadows the outer tab-data array.
   * @returns Action cleanup that releases observers, events and scheduled work.
   */
  function alignCommentControls(element: HTMLDivElement) {
    if (commentsInitiallyOpen) return alignDiscussionTabs(element, true);
  }
  function alignChildControls(element: HTMLDivElement) {
    if (!commentsInitiallyOpen) return alignDiscussionTabs(element);
  }
  function alignDiscussionTabs(tabs: HTMLDivElement, focusComments = false) {
    if (!scrollToDiscussionTabs) return;
    const card = tabs.closest<HTMLElement>('.node-page-layout') ?? tabs;
    // Browser animation/resize APIs are globals: no import is needed.
    // frame stores the latest requestAnimationFrame handle, not elapsed time or a clock.
    // Zero is its initial placeholder before this action schedules any work.
    let frame = 0;
    /** Whether alignment has been stopped by interaction, timeout or destruction. */
    let stopped = false;
    /** Focus only once; later layout corrections should only adjust scroll position. */
    let focused = false;
    /** Measure the current layout, focus once, and correct the tab strip’s vertical offset. */
    const align = () => {
      if (stopped || !tabs.isConnected) return;
      // ResizeObserver may replace the first scheduled frame. Keep focus in the
      // alignment callback so that rescheduling cannot silently cancel it.
      if (!focused) {
        const target = focusComments ? tabs.querySelector<HTMLElement>('[data-comments-toggle]') : tabs.querySelector<HTMLElement>('select, button');
        target?.focus({ preventScroll: true });
        focused = true;
      }
      // The browser clamps scrolling at the natural page bottom. Short/empty feeds
      // may leave the tabs below the target; do not add blank space to force alignment.
      const delta = tabs.getBoundingClientRect().top - 20;
      if (Math.abs(delta) > 1) window.scrollBy({ top: delta, behavior: 'instant' });
    };
    /** Combine repeated layout notifications into one callback before a browser repaint. */
    const schedule = () => {
      // Cancel the previous queued callback; requestAnimationFrame returns its replacement ID.
      // It schedules align for a future frame rather than running a repeating timer.
      cancelAnimationFrame(frame);
      frame = requestAnimationFrame(align);
    };
    // ResizeObserver reports element-size changes, not just window resize events.
    // The card and tab strip may change size as content arrives or labels wrap.
    const observer = new ResizeObserver(schedule);
    // const prevents reassigning events. `as const` additionally makes its TypeScript
    // type a readonly tuple of these exact event names instead of a mutable string[].
    const events = ['wheel', 'touchstart', 'pointerdown', 'keydown'] as const;
    /** Stop automatic correction and release its resources; safe to call more than once. */
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
    // Capture input early; passive listeners observe intent without preventing the action.
    for (const event of events)
      window.addEventListener(event, stop, { capture: true, passive: true });
    // Bound initial correction even if the visitor does not interact.
    const timeout = setTimeout(stop, 10000);
    schedule();
    // Font loading can change wrapping after the first measurement. Queue another
    // measurement when fonts settle. void discards the Promise; it does not await it.
    // If already stopped, align’s guard prevents further focus or scrolling.
    void document.fonts.ready.then(schedule);
    // Svelte calls an action’s destroy hook when its element is removed, such as
    // when navigating away. The same stop function also handles input and timeout.
    return { destroy: stop };
  }
</script>

<!--
  Layout: Breadcrumb and heading/byline lead into node content, reactions and discussion. The
  dial floats beside content; the selected panel sits below the tab toolbar.
-->

<div class="node-page-layout">
<article use:preloadWhenVisible={() => visibleForPreload = true} class="node-page atlas-panel">
    {#if !inspectionOnly}<NodeEditControl
      corner
      {node}
      {average}
      {totalVotes}
      {apiBase}
      onSaved={(update) => {
        node = { ...node, title: update.title };
        blocks = update.blocks;
      }}
    />{/if}
  <header>
    {#if inspectionOnly}<Breadcrumb
      expandable
      items={node.breadcrumb}
      originId={node.id}
      {apiBase}
      {onNavigate}
      hasMore={hasMoreAncestors}
      loadOlder={loadOlderAncestors}
    />
      {@render typeRow(undefined)}
    {:else}
      <Breadcrumb expandable connectedCards preloadConnected={visibleForPreload && !inspectionOnly} connectedHeader={typeRow} items={node.breadcrumb}
        originId={node.id} {apiBase} {onNavigate} hasMore={hasMoreAncestors}
        loadOlder={loadOlderAncestors} />
    {/if}
    <h1>{node.title}</h1>
    <p class="byline">
      <span class="by-prefix">{'By '}</span>{#if node.authorId}<a
          href={`/profiles?id=${encodeURIComponent(node.authorId)}`}
          onclick={(event) => {
            if (onOpenProfile && node.authorId) {
              event.preventDefault();
              onOpenProfile(node.authorId);
            }
          }}>{node.authorName}</a
        >{:else}{node.authorName}{/if} <span class="byline-separator" aria-hidden="true">·</span>
      <time datetime={node.createdAt} title={new Date(node.createdAt).toLocaleString()}
        ><span class="relative-date">{relativeDate(node.createdAt)}</span><span class="full-date">{new Date(node.createdAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric',
          timeZone: 'UTC',
        })}</span></time
      >{#if node.communities.length}
        <span class="byline-separator" aria-hidden="true">·</span> <span class="community-prefix">Community: </span> <CommunityLinks
          nodeId={node.id}
          names={node.communities}
        />{/if}
    {#if !inspectionOnly && !node.isHidden && node.status !== 'Archived'}<CopyNodeLink nodeId={node.id} />{/if}
    </p>
    {#if node.status === 'Archived'}<p>Archived</p>{/if}
  </header>
  <div class="post-body">
    <div class="description">
      {#if blocks.some((block) => block.kind !== 'markdown' || !!block.content.markdown?.trim())}
        <ContentBlocks {blocks} />
      {:else if node.description}
        <!-- Compatibility for older nodes whose Content document is blank. -->
        <p class="plain-description">{node.description}</p>
      {:else}
        <ContentBlocks {blocks} />
      {/if}
    </div>
  </div>
  {#if !inspectionOnly}
    <div class="node-actions" use:alignCommentControls>
      <div class="discussion-actions">
        <button type="button" aria-expanded={reactionsOpen} aria-controls={`${instance}-reactions`}
          onclick={() => { reactionsOpen = !reactionsOpen; reactionsVisited = true; }}>Reactions{reactionCount === undefined ? '' : ` (${reactionCount})`}</button>
        {#if commentsPanel}
          <button type="button" data-comments-toggle aria-expanded={commentsOpen} aria-controls={`${instance}-comments`}
            onclick={() => commentsOpen = !commentsOpen}>Comments ({node.commentCount})</button>
        {/if}
      </div>
      {@render reportControl?.()}
    </div>
    <div class="reaction-controls" id={`${instance}-reactions`} hidden={!reactionsOpen}>
      {#if reactionsVisited || (visibleForPreload && !inspectionOnly)}
        {#if reactions}{@render reactions()}{:else}
          <div class="reactions" role="group" aria-label="Community reactions">
            {#each node.reactions as reaction}<span>{reaction.label} <strong>{reaction.count}</strong></span>{/each}
          </div>
        {/if}
      {/if}
    </div>
    {#if commentsPanel}
      <div class="comments-controls" id={`${instance}-comments`} hidden={!commentsOpen}>
        {#if commentsOpen}{@render commentsPanel()}{/if}
      </div>
    {/if}
  {/if}
</article>
{#if !inspectionOnly}
  <section class="external-subnodes" aria-label="Subnodes">
    {#if !childFormOpen || !createTypeSelection}
    <div class="subnode-toolbar" use:alignChildControls>
      {#if childFormOpen}
        <div class="child-filter create-type">
          <strong>Choose a node type:</strong>
          <select aria-label="Choose a node type" data-node-type-select
            use:nodeTypeSelect={{ label: 'Choose a node type', value: createTypeSelection }} value={createTypeSelection} required
            disabled={childFormBusy} onchange={(event) => onCreateTypeChange?.(event.currentTarget.value)}>
            <option value="" disabled>Select a node type…</option>
            {#each types as type (type.id)}<option value={type.id} data-description={type.description}>{type.name}</option>{/each}
            <option value="new">Define a new node type…</option>
          </select>
        </div>
      {:else}
      <strong>{childCount} {childCount === 1 ? 'Subnode' : 'Subnodes'}</strong>
      <label class="child-filter">
        <span>Node type</span>
        <select value={active === 'comments' ? 'subnodes' : active} onchange={(event) => onTabChange(event.currentTarget.value)}>
          <option value="subnodes">All types</option>
          {#each childFilters as filter (filter.id)}<option value={filter.id}>{filter.name}</option>{/each}
          {#if active !== 'comments' && active !== 'subnodes' && !childFilters.some((filter) => filter.id === active)}
            <option value={active}>{types.find((type) => type.id === active)?.name ?? 'New node type'}</option>
          {/if}
        </select>
      </label>
      {/if}
      <div class="add">
        {#if childFormOpen}
          <button class="close-child" type="button" onclick={onCloseChild} disabled={childFormBusy}>Close subnode form</button>
        {:else}
          <button type="button" onclick={() => onAddChild(active === 'comments' || active === 'subnodes' ? undefined : active)}
            disabled={node.status === 'Archived' || childFormBusy}>Add {active === 'comments' || active === 'subnodes' ? 'a child node' : (types.find(type => type.id === active)?.name ?? 'a child node')}</button>
        {/if}
      </div>
    </div>
    {/if}
    <div class="external-child-content">
      {@render children()}
    </div>
  </section>
{/if}
</div>

{#snippet typeRow(ancestryControl: Snippet | undefined)}
  <div class="node-type-row">
    {@render ancestryControl?.()}
    <p class="node-type">{node.typeName}</p>
    {#if !inspectionOnly}
      {#if voting}{@render voting()}
      {:else}<button class="vote-summary" type="button" disabled>
        {average == null ? '—' : average.toFixed(1)} · {totalVotes ?? 0} votes · Vote
      </button>{/if}
    {/if}
  </div>
{/snippet}

<style lang="scss">
  .relative-date { display: none; }
  .introduction { display: contents; }


  .node-page-layout {
    --card-padding: var(--atlas-card-padding, 1.25rem);
    --atlas-connected-branch-gutter: clamp(.85rem, 3vw, 1.6rem);
    --atlas-connected-branch-color: color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border));
    max-width: 48rem;
    margin-inline: auto;
    min-width: 0;
  }
  .node-page {
    position: relative;
    margin: 0;
    padding: 0;
  }
  header {
    padding: var(--atlas-node-header-inset) var(--card-padding) 0;
  }

  header:has(> :global(.ancestry:not([hidden]))) { padding-top: 0; }

  .node-page:has(> :global(.edit-node.corner)) {
    --ancestry-end-padding: 5rem;
  }

  h1 {
    line-height: var(--atlas-node-line-height);
    font-size: clamp(1.4rem, 4vw, 2rem);
    margin: 0 0 var(--atlas-node-title-gap);
  }
  .byline {
    font-size: var(--atlas-node-meta-size);
    line-height: var(--atlas-node-meta-line-height);
    color: var(--atlas-muted);
    margin: 0;
    a {
      color: inherit;
      text-decoration: none;
      &:hover,
      &:focus-visible {
        text-decoration: underline;
      }
    }
  }
  .byline-separator {
    margin-inline: 0.35em;
  }
  .post-body {
    display: flow-root;
    padding: var(--atlas-node-content-gap) var(--card-padding) 0;
    --atlas-markdown-line-height: var(--atlas-node-line-height);
    line-height: var(--atlas-node-line-height);
  }
  .description {
    margin: 0 0 var(--atlas-paragraph-gap, 0.65rem);
  }
  .description :global(.content-blocks > :first-child),
  .description :global(.content-blocks > .markdown:first-child > :first-child),
  .plain-description {
    margin-block-start: 0;
  }
  .plain-description {
    white-space: pre-line;
  }
  .node-actions {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: center;
    gap: .4rem .75rem;
    padding: .65rem var(--card-padding) .85rem;
  }
  .discussion-actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: .4rem;
    min-width: 0;
  }
  /* Keep the trigger in the footer row, with its optional form on a separate row. */
  .node-actions :global(.report-control) { display: contents; }
  .node-actions :global(.report-control > .report-trigger),
  .node-actions :global(.report-control > [role='status']) {
    grid-column: 2;
    justify-self: end;
    text-align: end;
    margin: 0;
  }
  .node-actions :global(.report-control > form),
  .node-actions :global(.report-control > [role='alert']) {
    grid-column: 1 / -1;
    justify-self: end;
    box-sizing: border-box;
  }
  @media (max-width: 380px) {
    .node-actions { grid-template-columns: minmax(0, 1fr); }
    .node-actions :global(.report-control > .report-trigger),
    .node-actions :global(.report-control > [role='status']) { grid-column: 1; }
  }
  .discussion-actions button {
    font: inherit;
    min-height: 2.25rem;
    padding: .25rem .6rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    color: var(--atlas-accent);
    background: var(--atlas-surface);
  }
  .discussion-actions button[aria-expanded='true'] { border-color: var(--atlas-accent); }
  .discussion-actions button:focus-visible { outline: 2px solid var(--atlas-accent); outline-offset: 2px; }
  .reaction-controls, .comments-controls { padding: 0 var(--card-padding) .85rem; min-width: 0; }
  .reaction-controls[hidden], .comments-controls[hidden] { display: none; }
  .comments-controls { border-block-start: 1px solid var(--atlas-border); padding-block-start: .85rem; }
  .external-subnodes {
    padding-inline: var(--card-padding);
    min-width: 0;
  }
  .subnode-toolbar {
    position: relative;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: .5rem .75rem;
    padding: .75rem 0 .75rem var(--atlas-connected-branch-gutter);
    color: var(--atlas-text);
    font-size: .9rem;
  }
  .external-subnodes:has(:global(.child-form[data-branch-ready])) .subnode-toolbar::before,
  .external-subnodes:has(:global(ul.connected > li)) .subnode-toolbar::before,
  .external-subnodes:has(:global(ul.connected > li)) :global(.child-form)::before,
  .external-subnodes:has(:global(ul.connected > li)) :global(.node-feed:has(> p))::before,
  .external-subnodes:has(:global(ul.connected > li)) :global(.feed-head)::before {
    content: '';
    position: absolute;
    inset-inline-start: 2px;
    inset-block: 0;
    border-inline-start: 2px solid var(--atlas-connected-branch-color);
    pointer-events: none;
  }
  .external-subnodes :global(.child-form) {
    position: relative; margin: 0; padding: .75rem 0 .75rem var(--atlas-connected-branch-gutter);
  }
  .external-subnodes:not(:has(:global(ul.connected > li))) :global(.child-form[data-branch-ready])::before {
    content: '';
    position: absolute;
    inset-inline-start: 2px;
    inset-block-start: 0;
    block-size: var(--draft-branch-y);
    border-inline-start: 2px solid var(--atlas-connected-branch-color);
    pointer-events: none;
  }
  .external-subnodes :global(.child-form[data-branch-ready])::after {
    content: '';
    position: absolute;
    inset-inline-start: 2px;
    inset-block-start: var(--draft-branch-y);
    inline-size: calc(var(--draft-branch-x) - 2px);
    border-block-start: 2px solid var(--atlas-connected-branch-color);
    pointer-events: none;
  }
  .external-subnodes :global(.child-form .draft-fields) { margin-block-start: 0; }
  .external-subnodes :global(.node-feed:has(> p)), .external-subnodes :global(.feed-head) { position: relative; padding-inline-start: var(--atlas-connected-branch-gutter); }
  .external-subnodes :global(.node-feed > p) { margin: 0; padding-block-end: .5rem; }
  .external-subnodes :global(ul.connected > li:first-child::before) { inset-block-start: 0; }
  .external-subnodes :global(ul.connected > li:only-child::before) { block-size: 1.4rem; }
  .external-child-content { min-width: 0; }
  .reactions {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }
  .reactions span {
    border: 1px solid var(--atlas-border);
    border-radius: 999px;
    padding: 0.2rem 0.6rem;
  }
  .add { margin-inline-start: auto; max-inline-size: 100%; }
  .add button {
    border: 1px solid var(--atlas-accent);
    border-radius: var(--atlas-radius);
    min-block-size: 2.25rem;
    padding: 0.25rem 0.65rem;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    line-height: 1.25;
    font-weight: 600;

    &:not(:disabled):hover {
      background: color-mix(in srgb, var(--atlas-accent) 85%, var(--atlas-text));
    }

    &:focus-visible {
      outline: 3px solid var(--atlas-focus);
      outline-offset: 3px;
    }

    &:disabled {
      border-color: var(--atlas-border);
      background: var(--atlas-bg);
      color: var(--atlas-muted);
      cursor: default;
    }
  }
  .add button.close-child:not(:disabled) {
    background: var(--atlas-surface);
    color: var(--atlas-accent);
  }
  .add button.close-child:not(:disabled):hover {
    background: color-mix(in srgb, var(--atlas-accent) 8%, var(--atlas-surface));
  }
  .child-filter {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.5rem;
    min-width: 0;
    color: var(--atlas-muted);
    font-size: 0.9rem;
  }
  .create-type strong { color: var(--atlas-text); font-size: 1.1rem; }
  .create-type select { font-weight: 600; }
  .child-filter select {
    font: inherit;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    min-block-size: 2.25rem;
    max-inline-size: 100%;
    padding: 0.3rem 0.6rem;
  }
  button {
    font: inherit;
    padding: 0.5rem 0.75rem;
    cursor: pointer;
    max-width: 100%;
    overflow-wrap: anywhere;
  }
  @media (max-width: 380px) {
    .node-page-layout {
      --card-padding: var(--atlas-card-padding-narrow, 0.75rem);
    }
  }
  @media (max-width: 560px) {
    .node-page-layout { --card-padding: 0.75rem; }
    .by-prefix, .community-prefix, .full-date { display: none; }
    .relative-date { display: inline; }
  }
  .node-type-row {
    display: flex;
    align-items: center;
    gap: .4rem .6rem;
    min-width: 0;
    container-type: inline-size;
    margin-block-end: var(--atlas-node-title-gap);
  }
  .node-type {
    margin: 0 auto 0 0;
    min-width: 0;
    color: var(--atlas-accent);
    font-weight: 700;
    font-size: clamp(.95rem, 8.1cqi, 2.1rem);
    line-height: 1.15;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
  .node-type-row :global(.live-voting), .vote-summary { flex-shrink: 0; }
  .vote-summary {
    font: inherit;
    padding: .5rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-muted);
  }
</style>
