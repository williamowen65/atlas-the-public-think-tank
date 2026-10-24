<!--
  @component
  NodePage: Presentational full-node composition with discussion tabs and caller-supplied snippets.
  The parent owns fetching and tab state; snippets supply voting, reactions and the active tab content.

  **Props**
  - `voting` (`Snippet`; optional): Renderable snippet for the voting region, replacing the
    always-disabled plain VotingDial fallback. LiveNodePage supplies LiveVotingDial here to add
    API/session behavior.
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
  import type { Snippet } from 'svelte';
  import type { NodeCardData } from './NodeCard.svelte';
  import type { NodeType, ContentBlock } from '../api/nodePage';
  import ContentBlocks from './ContentBlocks.svelte';
  import CommunityLinks from './CommunityLinks.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  import Breadcrumb from './Breadcrumb.svelte';
  import VotingDial from './VotingDial.svelte';
  let {
    onOpenProfile,
    voting,
    reactions,
    scrollToDiscussionTabs = false,
    node,
    blocks,
    types,
    active,
    draftTab,
    onTabChange,
    onAddChild,
    onNavigate,
    hasMoreAncestors = false,
    loadOlderAncestors,
    apiBase,
    average = null,
    totalVotes = 0,
    children,
  }: {
    /**
     * Renderable snippet for the voting region, replacing the always-disabled plain VotingDial
     * fallback. LiveNodePage supplies LiveVotingDial here to add API/session behavior.
     */
    voting?: Snippet;
    /**
     * Renderable snippet for custom/live reaction controls. If omitted, NodePage displays the
     * static reaction summary from node.reactions.
     */
    reactions?: Snippet;
    /**
     * Requests initial focus and scrolling of the selected discussion tab to 20px below the
     * viewport top where the natural page height allows; never adds space just to reach the target.
     */
    scrollToDiscussionTabs?: boolean;
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
  /**
   * Rebuild discussion choices when counts or draft state change: comments first,
   * backend contribution categories next, and a temporary draft tab if not already
   * represented. The parent owns active and supplies the selected panel’s content.
   */
  const tabs = $derived([
    {
      id: 'comments',
      name: `${node.commentCount} ${node.commentCount === 1 ? 'Comment' : 'Comments'}`,
    },
    ...(node.childTypes ?? []).map((type) => ({
      id: type.typeId,
      name: type.label,
    })),
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
  function alignDiscussionTabs(tabs: HTMLDivElement) {
    if (!scrollToDiscussionTabs) return;
    const card = tabs.closest<HTMLElement>('.node-page')!;
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
        tabs.querySelector<HTMLElement>('[aria-selected="true"]')?.focus({ preventScroll: true });
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
  /**
   * Provide keyboard access to the tab list: Left/Right wrap between tabs, while
   * Home/End select the first/last tab. Only the selected tab enters the normal
   * Tab-key order. Ask the parent to change active, then focus the matching button.
   * @param event Keyboard event from a tab button; unrelated keys keep normal behavior.
   * @param index Position of that button in the derived tabs array.
   */
  function key(event: KeyboardEvent, index: number) {
    let next = index;
    if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
    else if (event.key === 'ArrowLeft') next = (index - 1 + tabs.length) % tabs.length;
    else if (event.key === 'Home') next = 0;
    else if (event.key === 'End') next = tabs.length - 1;
    else return;
    // These keys now navigate tabs, so prevent their usual page-scrolling behavior.
    event.preventDefault();
    onTabChange(tabs[next].id);
    document.getElementById(`${instance}-${tabs[next].id}`)?.focus();
  }
</script>

<!--
  Layout: Breadcrumb and heading/byline lead into node content, reactions and discussion. The
  dial floats beside content; the selected panel sits below the tab toolbar.
-->

<article class="node-page atlas-panel">
  {#if node.breadcrumb.length}<div class="ribbon-context">
      <Breadcrumb
        items={node.breadcrumb}
        originId={node.id}
        {apiBase}
        {onNavigate}
        hasMore={hasMoreAncestors}
        loadOlder={loadOlderAncestors}
      />
    </div>{/if}
  <header>
    <h1>{node.typeName}: {node.title}</h1>
    <p class="byline">
      By {#if node.authorId}<a
          href={`/profiles?id=${encodeURIComponent(node.authorId)}`}
          onclick={(event) => {
            if (onOpenProfile && node.authorId) {
              event.preventDefault();
              onOpenProfile(node.authorId);
            }
          }}>{node.authorName}</a
        >{:else}{node.authorName}{/if} <span class="byline-separator" aria-hidden="true">·</span>
      <time datetime={node.createdAt}
        >{new Date(node.createdAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric',
          timeZone: 'UTC',
        })}</time
      >{#if node.communities.length}
        <span class="byline-separator" aria-hidden="true">·</span> Community: <CommunityLinks
          nodeId={node.id}
          names={node.communities}
        />{/if}
    </p>
    {#if node.status === 'Archived'}<p>Archived</p>{/if}
  </header>
  <!--
    Contains the floated voting dial so the introduction and content can flow beside it without
    affecting later sections.
  -->
  <div class="post-body">
    <aside class="dial" aria-label="Importance rating">
      <!-- A supplied voting snippet owns its behavior (LiveNodePage supplies LiveVotingDial).
           Without one, show totals with an always-disabled VotingDial: there is no save handler.
           The bare disabled attribute below means disabled={true}, regardless of archive status. -->
      {#if voting}{@render voting()}{:else}<VotingDial
          label="Node importance"
          {average}
          {totalVotes}
          disabled
        />{/if}
    </aside>
    <p class="description">{node.description}</p>
    <ContentBlocks {blocks} />
  </div>
  {#if reactions}{@render reactions()}{:else if node.reactions.length}<div
      class="reactions"
      role="group"
      aria-label="Community reactions"
    >
      {#each node.reactions as reaction}<span
          >{reaction.label} <strong>{reaction.count}</strong></span
        >{/each}
    </div>{/if}
  <!-- Keeps the Add action with the discussion tab strip, above the selected panel. -->
  <div class="discussion-toolbar">
    <div class="add">
      <button
        onclick={() => onAddChild(active === 'comments' ? undefined : active)}
        disabled={node.status === 'Archived'}
        >Add {active === 'comments'
          ? 'a child node'
          : (types.find((type) => type.id === active)?.name ?? 'a child node')}</button
      >
    </div>
    <div
      class="tabs"
      use:alignDiscussionTabs
      role="tablist"
      aria-label="Node discussions and child types"
    >
      {#each tabs as tab, index}<button
          role="tab"
          id={`${instance}-${tab.id}`}
          aria-selected={active === tab.id}
          aria-controls={`${instance}-panel`}
          tabindex={active === tab.id ? 0 : -1}
          onclick={() => onTabChange(tab.id)}
          onkeydown={(event) => key(event, index)}>{tab.name}</button
        >{/each}
    </div>
  </div>
  <!--
    One shared panel renders the parent-supplied comments, child feed or draft content; ARIA
    links it to the selected tab.
  -->
  <div
    role="tabpanel"
    id={`${instance}-panel`}
    aria-labelledby={`${instance}-${active}`}
    tabindex="0"
  >
    {@render children()}
  </div>
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
  .ribbon-context {
    min-width: 0;
    padding: 0.75rem 0 0;
    margin-bottom: var(--card-padding);
  }
  header {
    padding: var(--card-padding) var(--card-padding) 0.5rem;
  }
  .ribbon-context + header {
    padding-top: 0;
  }
  h1 {
    font-size: clamp(1.4rem, 4vw, 2rem);
    margin: 0 0 0.5rem;
  }
  .byline {
    font-size: 0.85rem;
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
    padding: 0.5rem var(--card-padding) 0;
    line-height: var(--atlas-line-height, 1.65);
  }
  .dial {
    float: left;
    margin: 0 var(--dial-text-gap) 0.6rem -0.4rem;
  }
  .description {
    margin: 0 0 var(--atlas-paragraph-gap, 0.65rem);
    white-space: pre-line;
  }
  .reactions {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    padding: 0.5rem var(--card-padding) 0;
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }
  .reactions span {
    border: 1px solid var(--atlas-border);
    border-radius: 999px;
    padding: 0.2rem 0.6rem;
  }
  .discussion-toolbar {
    display: flow-root;
    border-bottom: 1px solid var(--atlas-border);
    padding: 0.5rem var(--card-padding) 0.75rem;
  }
  .add {
    float: right;
    margin: 0 0 0.75rem 1.25rem;
  }
  .tabs {
    scroll-margin-top: 20px;
    display: block;
  }
  .tabs button {
    margin: 0 0.4rem 0.4rem 0;
    vertical-align: top;
  }
  button {
    font: inherit;
    padding: 0.5rem 0.75rem;
    cursor: pointer;
    max-width: 100%;
    overflow-wrap: anywhere;
  }
  [aria-selected='true'] {
    font-weight: bold;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
  }
  [role='tabpanel'] {
    padding: 1rem var(--card-padding) var(--card-padding);
    min-width: 0;
  }
  @media (max-width: 380px) {
    .node-page {
      --card-padding: var(--atlas-card-padding-narrow, 0.75rem);
      --dial-text-gap: var(--atlas-dial-text-gap, 0.6rem);
    }
  }
</style>
