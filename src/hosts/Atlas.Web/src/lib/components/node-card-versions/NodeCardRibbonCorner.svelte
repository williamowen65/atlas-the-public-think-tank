<!--
  @component
  NodeCardRibbonCorner: Common card contract using the ribbon-corner presentation.
  Accepts the shared NodeCardProps contract; parent callbacks handle navigation and optional snippets provide live controls.

  **Props**
  - `node` (`NodeCardData`; required): Presentation data for one node; follow the NodeCardData type
    definition for its fields.
  - `voting` (`Snippet`; optional): Renderable snippet that replaces the card’s built-in VotingDial.
    Supply LiveVotingDial here for API-backed voting; otherwise the built-in dial reports selections
    through onVote.
  - `reactionControls` (`Snippet`; optional): Renderable snippet replacing the static reaction
    summary with custom/live controls, such as LiveReactions.
  - `hasMoreAncestors` (`boolean`; optional; default `false`): Whether additional older ancestors
    can be loaded for the node’s breadcrumb.
  - `loadOlderAncestors` (`LoadOlderAncestors`; optional): Async callback receiving the oldest
    visible ancestor ID and returning BreadcrumbPage: older items and hasMore. Used by ribbon
    variants; context-below has no ancestor pager.
  - `apiBase` (`string`; optional): Public read base forwarded to breadcrumb lookups by ribbon
    variants. Context-below accepts but does not use it; this is not the vote-save endpoint.
  - `value` (`number | null`; optional; default `null`): Current controlled rating, 0–10. null means
    unrated; zero is a real rating.
  - `average` (`number | null`; optional; default `null`): Community average supplied by the parent;
    null means no average is available.
  - `totalVotes` (`number`; optional; default `0`): Community vote count supplied by the parent.
  - `onVote` (`(value: number) => void`; optional): Callback: Reports a selected rating to the
    parent. The presentation component does not save it to the API.
  - `onNavigate` (`(id: string) => void`; optional): Callback receiving a node ID from ancestor or
    related-node navigation. The parent decides how to open it in the application.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
  - `onReadMore` (`(id: string) => void`; optional): Callback: Receives the node ID when Read more
    is activated; absence disables that action.
  - `onChildType` (`(id: string, typeId: string) => void`; optional): Callback: Receives node ID and
    contribution type ID when a contribution-summary button is activated.
  - `onComments` (`(id: string) => void`; optional): Callback: Receives the node ID when its
    comment-count button is activated.
-->
<script lang="ts">
  import CommentButtonLabel from "../CommentButtonLabel.svelte";
  import { preloadWhenVisible } from "../../actions/preloadWhenVisible";
  import ActionButton from '../ActionButton.svelte';
  import CopyNodeLink from "../CopyNodeLink.svelte";
  import { relativeDate } from '../../format/relativeDate';
  import type { Snippet } from 'svelte';
  import type { ContentBlock } from '../../api/nodePage';
  import FeedBlockPreview from '../FeedBlockPreview.svelte';
  import LoadingPlaceholder from '../LoadingPlaceholder.svelte';
  import CommunityLinks from '../CommunityLinks.svelte';
  import VotingDial from '../VotingDial.svelte';

  import Breadcrumb from '../Breadcrumb.svelte';
  import type { NodeCardProps } from './types';
  let detailsOpen = $state(false);
  let subnodesOpen = $state(false);
  let subnodesVisited = $state(false);
  let reactionsOpen = $state(false);
  let reactionsVisited = $state(false);
  let visibleForPreload = $state(false);
  const preloadRelationships = import.meta.env.VITE_ATLAS_PRELOAD_RELATIONSHIPS !== 'false';
  let childFilter = $state('');
  let fallbackDialog = $state<HTMLDialogElement>();
  let {
    mobileLayout = false,
    inlineRelationships = false,
    relationshipContext = false,
    cornerControl,
    headerFields,
    authoringBody,
    authoringFooter,
    blocks,
    draft = false,
    creatingDraft = false,
    reactionControls,
    onOpenProfile,
    voting,
    node,
    value = null,
    average = null,
    totalVotes = 0,
    onVote,
    onNavigate,
    onReadMore,
    editOpensDetail = false,
    onChildType,
    onComments,
    hasMoreAncestors = false,
    loadOlderAncestors,
    apiBase,
  }: NodeCardProps & {
    /** Experimental mobile feed layout; desktop and full node detail remain unchanged. */
    mobileLayout?: boolean;
    /** Feed owner actions are supplied by NodeCard. */
    cornerControl?: Snippet;
    headerFields?: Snippet;
    authoringBody?: Snippet;
    authoringFooter?: Snippet;
    /** Draft content or freshly saved content, when already available. */
    blocks?: ContentBlock[];
    draft?: boolean;
    /** New node authoring only, not existing-node edit previews. */
    creatingDraft?: boolean;
  } = $props();
  const instance = $props.id();
  const childCount = $derived((node.childTypes ?? []).reduce((sum, child) => sum + child.count, 0));
  const reactionCount = $derived(node.reactionCount ?? (node.reactionSummary && !node.reactions.length && node.reactionSummary !== '—' ? undefined : node.reactions.reduce((sum, reaction) => sum + reaction.count, 0)));
  const graphControls = $derived(inlineRelationships && !draft && !node.isHidden && !authoringBody);
  function openRelated(id: string, type?: string) {
    if (type === 'comments' && onComments) onComments(id);
    else if (type && onChildType) onChildType(id, type);
    else if (onReadMore) onReadMore(id);
    else window.location.assign('/nodes?id=' + encodeURIComponent(id) + (type ? '&type=' + encodeURIComponent(type) + '#contributions' : ''));
  }
  // Prefer structured contribution types; use the text summary only for older projections.
  /**
   * Display-only connection summaries from legacy text and related-node titles, separate
   * from structured child-type buttons.
   */
  const connectionLabels = $derived([
    ...(!node.childTypes?.length && node.subNodeSummary && node.subNodeSummary !== 'None'
      ? node.subNodeSummary.split(' · ')
      : []),
    ...node.related.map((related) => `${related.typeName}: ${related.title}`),
  ]);
</script>

<!--
  Layout: An ancestor ribbon leads into the heading and dial/body; Read more occupies the
  description’s corner, with reactions and discussion links below.
-->

<article use:preloadWhenVisible={() => visibleForPreload = true} class="node-card" class:mobile-layout={mobileLayout} data-presentation="ribbon-corner" aria-labelledby={`${instance}-title`}>
  {#if editOpensDetail}
    <button class="hidden-edit-link" type="button" aria-label="Open hidden node to edit and request review" title="Open node to revise" onclick={() => onReadMore?.(node.id)}>✎ Edit</button>
  {:else}
    {@render cornerControl?.()}
  {/if}
  <header>
    {#if headerFields}
      {#if node.breadcrumb.length}
        <Breadcrumb items={node.breadcrumb} originId={draft ? undefined : node.id}
          {apiBase} {onNavigate} hasMore={hasMoreAncestors} loadOlder={loadOlderAncestors} />
      {/if}
      <div id={`${instance}-title`}>{@render headerFields()}</div>
    {:else}
      {#if draft || node.isHidden}<Breadcrumb
        expandable
        items={draft ? node.breadcrumb.map(({ id, title }) => ({ id, title })) : node.breadcrumb}
        originId={draft ? undefined : node.id}
        {apiBase}
        {onNavigate}
        hasMore={hasMoreAncestors}
        loadOlder={loadOlderAncestors}
      />
        {@render typeRow(undefined)}
      {:else if !relationshipContext}
        <Breadcrumb expandable connectedCards preloadConnected={preloadRelationships && visibleForPreload && graphControls} connectedHeader={typeRow} items={node.breadcrumb}
          originId={node.id} {apiBase} {onNavigate} hasMore={hasMoreAncestors}
          loadOlder={loadOlderAncestors} />
      {:else}
        {@render typeRow(undefined)}
      {/if}
      <h2 id={`${instance}-title`}>{node.title}</h2>
    {/if}
    {#if node.status === 'Archived'}<p class="byline">Archived</p>{/if}
    <p class="byline">
      <span
        ><span class="by-prefix">{'By '}</span>{#if node.authorId && !draft}<a
            href={`/profiles?id=${encodeURIComponent(node.authorId)}`}
            onclick={(event) => {
              if (onOpenProfile && node.authorId) {
                event.preventDefault();
                onOpenProfile(node.authorId);
              }
            }}>{node.authorName}</a
          >{:else}{node.authorName}{/if}</span
      ><span class="byline-separator" aria-hidden="true">·</span>{#if creatingDraft}<span>Currently creating a draft</span>{:else}<time datetime={node.createdAt} title={new Date(node.createdAt).toLocaleString()}
        >{mobileLayout ? relativeDate(node.createdAt) : new Date(node.createdAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric',
          timeZone: 'UTC',
        })}</time>{/if}{#if node.communities.length}
        <span class="byline-separator" aria-hidden="true">·</span>
        <span class="community-prefix">{node.communities.length === 1 ? 'Community' : 'Communities'}: </span>
        {#if draft}{node.communities.join(', ')}{:else}<CommunityLinks
          nodeId={node.id}
          names={node.communities}
        />{/if}{/if}
    {#if !draft && !node.isHidden && node.status !== 'Archived'}<CopyNodeLink nodeId={node.id} />{/if}
    </p>
  </header>

  {#if authoringBody}
    <div class="body authoring-body">{@render authoringBody()}</div>
    {@render authoringFooter?.()}
  {:else}
    <div class="body">
      {#if node.isHidden}
        {@render legacyDescription(node.description)}
      {:else}
        <FeedBlockPreview nodeId={node.id} fallback={node.description} renderFallback={legacyDescription} leadingAction={detailsToggle} readMoreAction={graphControls ? readMore : undefined} {apiBase} {blocks}
          onReadMore={onReadMore ? () => onReadMore?.(node.id) : undefined} />
      {/if}
    </div>
    {#if graphControls}
      <div class="graph-reactions" id={`${instance}-reactions`} hidden={!reactionsOpen}>
        {#if reactionsVisited || (visibleForPreload && graphControls)}{@render reactions()}{/if}
      </div>
      <section class="subnode-section" id={`${instance}-subnodes`} aria-label="Subnodes" hidden={!subnodesOpen}>
        {#if subnodesVisited || (preloadRelationships && visibleForPreload && childCount > 0)}
          <div class="subnode-header">
            <strong>{childCount} {childCount === 1 ? 'Subnode' : 'Subnodes'}</strong>
            <label class="inline-filter">Node type
              <select bind:value={childFilter}>
                <option value="">All types</option>
                {#each node.childTypes ?? [] as type (type.typeId)}
                  <option value={type.typeId}>{type.label}</option>
                {/each}
              </select>
            </label>
          </div>
          {#key childFilter}
            {#await import('../LiveNodeFeed.svelte')}
              <div class="subnode-import-placeholder"><LoadingPlaceholder kind="mini" label="Loading subnodes…" /></div>
            {:then { default: ChildFeed }}
              <ChildFeed parentId={node.id} typeId={childFilter || undefined} {apiBase}
                initialSubnodes={childFilter ? undefined : node.subnodePreview ?? undefined}
                {onOpenProfile} onOpenNode={openRelated} connected inline presentation="mini" />
            {:catch}
              <button type="button" onclick={() => openRelated(node.id, 'subnodes')}>Open subnodes on node page</button>
            {/await}
          {/key}
        {/if}
      </section>
    {:else}
      <div class="card-details" class:details-open={detailsOpen} id={`${instance}-details`}>
        {@render reactions()}
        <div class="navigation-details">
          {@render comments()}
          {#each node.childTypes ?? [] as contribution}<span class="navigation-separator" aria-hidden="true">·</span>
            <button class="text-action connection-summary" type="button" disabled={!onChildType}
              onclick={() => onChildType?.(node.id, contribution.typeId)}>{contribution.label}</button>
          {/each}
          {#each connectionLabels as label}<span class="navigation-separator" aria-hidden="true">·</span>
            <span class="text-action connection-summary">{label}</span>
          {/each}
        </div>
      </div>
    {/if}
  {/if}
</article>

{#snippet detailsToggle()}
  {#if graphControls}
    <div class="graph-actions">
      {#if childCount > 0}
        <button class="details-toggle" type="button" aria-expanded={subnodesOpen} aria-controls={`${instance}-subnodes`}
          aria-label={subnodesOpen ? 'Hide subnodes' : 'Show subnodes'}
          title={subnodesOpen ? 'Hide subnodes' : 'Show subnodes'}
          onclick={() => { subnodesOpen = !subnodesOpen; subnodesVisited = true; }}>
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2"
            stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
        </button>
      {/if}
      <button class="reaction-toggle" type="button" aria-expanded={reactionsOpen} aria-controls={`${instance}-reactions`}
        onclick={() => { reactionsOpen = !reactionsOpen; reactionsVisited = true; }}>Reactions{reactionCount === undefined ? '' : ` (${reactionCount})`}</button>
      <button class="comments-button" type="button"
        aria-label={`Comments (${node.commentCount})`} title={`Comments (${node.commentCount})`}
        onclick={() => openRelated(node.id, 'comments')}>
        <CommentButtonLabel count={node.commentCount} />
      </button>
    </div>
  {:else}
    <button class="details-toggle" type="button" aria-expanded={detailsOpen} aria-controls={`${instance}-details`}
      aria-label={detailsOpen ? 'Hide reactions, comments, and subnodes' : 'Show reactions, comments, and subnodes'}
      onclick={() => detailsOpen = !detailsOpen}>
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2"
        stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
    </button>
  {/if}
{/snippet}

{#snippet legacyDescription(text: string)}
  <div class="description" id={`${instance}-description`}>
    {#if text.trim()}<span class="fallback-text">{text}</span>{/if}
    {@render detailsToggle()}
    {#if graphControls}
      {@render readMore()}
    {:else}
      <span class="read-more"><ActionButton label="Read more" disabled={!onReadMore} onActivate={() => onReadMore?.(node.id)} /></span>
    {/if}
  </div>
{/snippet}

{#snippet readMore()}
  <button class="compact-read-more" type="button" aria-label="Read more" title="Read more"
    disabled={!onReadMore} onclick={() => onReadMore?.(node.id)}>
    <svg class="compact-icon" viewBox="0 0 24 24" width="20" height="20" fill="none"
      stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      <path d="M12 6C9 4 6 4 3 5v14c3-1 6-1 9 1 3-2 6-2 9-1V5c-3-1-6-1-9 1Zm0 0v14" />
    </svg>
    <span class="action-text">Read More</span>
  </button>
{/snippet}

{#snippet reactions()}
  {#if reactionControls}{@render reactionControls()}{:else}
    {#if node.reactions.length}
      <div class="reactions" role="group" aria-label="Community reactions">
        {#each node.reactions as reaction}<span class="reaction-pill"
            >{reaction.label} <strong>{reaction.count}</strong></span
          >{/each}
      </div>
    {/if}
  {/if}
{/snippet}

{#snippet comments()}
  <button
    class="text-action"
    type="button"
    disabled={!onComments}
    onclick={() => onComments?.(node.id)}
    >{node.commentCount} {node.commentCount === 1 ? 'Comment' : 'Comments'}</button
  >
{/snippet}

{#snippet typeRow(ancestryControl: Snippet | undefined)}
  <div class="node-type-row">
    {@render ancestryControl?.()}
    <p class="mobile-type">{node.typeName}</p>
    {#if !authoringBody}
      {#if voting}
        {@render voting(true)}
      {:else}
        <button class="fallback-vote" type="button" aria-haspopup="dialog"
          disabled={draft || node.status === 'Archived' || node.isHidden}
          onclick={() => fallbackDialog?.showModal()}>
          {average == null ? '—' : average.toFixed(1)} · {totalVotes ?? 0} votes · Vote
        </button>
        <dialog bind:this={fallbackDialog} class="fallback-vote-dialog" aria-label="Rate node importance">
          <button type="button" onclick={() => fallbackDialog?.close()}>Close</button>
          <VotingDial label="Node importance" {value} {average} {totalVotes} onChange={onVote} />
        </dialog>
      {/if}
    {/if}
  </div>
{/snippet}

<style lang="scss">
  .graph-actions {
    display: flex; align-items: center; gap: .4rem;
    min-width: 0; margin-inline-end: auto;
  }
  .graph-actions .details-toggle { flex: 0 0 auto; margin-inline-end: 0; }
  .reaction-toggle, .comments-button, .inline-filter select {
    font: inherit; max-width: 100%; min-height: 2.25rem;
    color: var(--atlas-accent); background: var(--atlas-surface);
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius);
    padding: .25rem .6rem; cursor: pointer;
  }
  .reaction-toggle[aria-expanded="true"] { border-color: var(--atlas-accent); }
  .reaction-toggle:focus-visible, .comments-button:focus-visible, .compact-read-more:focus-visible, .details-toggle:focus-visible {
    outline: 2px solid var(--atlas-accent); outline-offset: 2px;
  }
  .graph-reactions {
    padding: .6rem var(--card-padding) .8rem;
    min-width: 0;
  }
  .graph-reactions[hidden], .subnode-section[hidden] { display: none; }
  .subnode-section {
    min-width: 0;
    border-top: 1px solid var(--atlas-border);
    background: var(--atlas-tinted-surface);
    border-radius: 0 0 var(--atlas-radius) var(--atlas-radius);
    --atlas-connected-branch-gutter: clamp(.85rem, 3vw, 1.6rem);
    --atlas-connected-branch-color: color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border));
    --subnode-top-padding: .75rem;
    --subnode-header-gap: .65rem;
    padding: var(--subnode-top-padding) var(--card-padding) .45rem;
  }
  .subnode-header {
    position: relative;
    display: flex; align-items: center; justify-content: space-between;
    flex-wrap: wrap; gap: .35rem .75rem; margin-block-end: var(--subnode-header-gap);
    padding-inline-start: var(--atlas-connected-branch-gutter);
    color: var(--atlas-text); font-size: .9rem;
  }
  /* Keep the count/filter in the content column, with a continuous straight rail
     in the left gutter from the parent divider to the first mini child. */
  .subnode-section:has(:global(ul.attached-mini-feed > li)) .subnode-header::before,
  .subnode-section:has(.subnode-import-placeholder) .subnode-header::before {
    content: '';
    position: absolute;
    inset-inline-start: 2px;
    inset-block-start: calc(-1 * var(--subnode-top-padding));
    inset-block-end: calc(-1 * var(--subnode-header-gap));
    border-inline-start: 2px solid var(--atlas-connected-branch-color);
    pointer-events: none;
  }
  .subnode-import-placeholder {
    position: relative;
    padding-inline-start: var(--atlas-connected-branch-gutter);
  }
  .subnode-import-placeholder::before, .subnode-import-placeholder::after {
    content: ''; position: absolute; inset-inline-start: 2px; pointer-events: none;
  }
  .subnode-import-placeholder::before {
    inset-block-start: 0; block-size: 1.1rem;
    border-inline-start: 2px solid var(--atlas-connected-branch-color);
  }
  .subnode-import-placeholder::after {
    inset-block-start: 1.1rem;
    inline-size: calc(var(--atlas-connected-branch-gutter) - 2px);
    border-block-start: 2px solid var(--atlas-connected-branch-color);
  }
  .inline-filter {
    display: flex; align-items: center; gap: .4rem; flex-wrap: wrap;
    min-width: 0; color: var(--atlas-muted); font-size: .8rem;
  }
  .inline-filter select { color: var(--atlas-text); min-height: 2rem; }
  .hidden-edit-link { position: absolute; z-index: 2; top: 0; right: 0; border: 1px solid var(--atlas-border); border-top: 0; border-right: 0; border-radius: 0 var(--atlas-radius) 0 var(--atlas-radius); padding: .55rem .8rem; background: var(--atlas-surface); color: var(--atlas-text); font: inherit; cursor: pointer; }
  .hidden-edit-link:hover { color: var(--atlas-accent); }



  .node-card {
    position: relative;
    --card-padding: var(--atlas-card-padding, 1.25rem);
    --dial-text-gap: var(--atlas-dial-text-gap, 1rem);
    max-width: 48rem;
    width: 100%;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    overflow-wrap: anywhere;
    header {
      padding: var(--atlas-node-header-inset) var(--card-padding) 0;
    }
    h2 {
      margin: 0 0 var(--atlas-heading-gap, var(--atlas-node-title-gap));
      font-size: calc(clamp(1.35rem, 4vw, 1.9rem) * var(--atlas-heading-scale, 1.5) / 1.5);
      line-height: var(--atlas-node-line-height);
    }
  }

  .node-card:has(> :is(:global(.edit-node.corner), .hidden-edit-link)) {
    --ancestry-end-padding: 5rem;
  }




  .details-toggle {
    display: inline-grid; place-items: center; margin-inline-end: auto;
    width: 2.25rem; min-height: 2.25rem; padding: 0;
    color: var(--atlas-accent); background: var(--atlas-surface);
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); cursor: pointer;
  }
  .details-toggle[aria-expanded="true"] svg { transform: rotate(180deg); }

  [data-presentation] {
    .card-details {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      align-items: center;
      gap: 0.35rem 1rem;
      padding: .65rem var(--card-padding) 1rem;
      &:not(.details-open) { display: none; }
      font-size: 0.9rem;
      color: var(--atlas-muted);
    }
    &:has(.card-details:not(.details-open)) .body, &:has(> .subnode-section) .body { padding-block-end: .85rem; }
    .reactions {
      display: flex;
      flex-wrap: wrap;
      align-items: start;
      gap: 0.3rem;
      flex: 1 1 0;
      min-width: 0;
      span {
        display: inline-block;
        margin: 0;
        padding: 0.1rem 0.65rem;
        border: 1px solid var(--atlas-border);
        border-radius: 999px;
        background: var(--atlas-surface);
        line-height: 1.7;
      }
    }
    .navigation-details {
      min-width: min(300px, 100%);
      max-width: 100%;
      display: flex;
      flex-wrap: wrap;
      justify-content: flex-end;
      align-items: center;
      align-self: center;
      column-gap: 0.35em;
      font-size: 0.9rem;
      text-align: right;
      margin-left: auto;
      flex: 1 1 0;
      .text-action {
        padding-block: 0.15rem;
      }
      .navigation-separator {
        font-size: 0.9rem;
      }
      .connection-summary {
        text-decoration: none;
        color: var(--atlas-muted);
        cursor: default;
        &:is(button):not(:disabled) {
          cursor: pointer;
        }
        &:is(button):not(:disabled):hover {
          color: var(--atlas-accent);
          text-decoration: underline;
        }
      }
    }
  }

  .byline-separator {
    margin-inline: 0.35em;
  }

  .byline {
    a {
      color: inherit;
      text-decoration: none;
      &:hover,
      &:focus-visible {
        text-decoration: underline;
      }
    }
    margin: 0;
    font-size: var(--atlas-node-meta-size);
    line-height: var(--atlas-node-meta-line-height);
    color: var(--atlas-muted);
  }

  .body {
    --atlas-markdown-line-height: var(--atlas-node-line-height);
    line-height: var(--atlas-node-line-height);
    padding: var(--atlas-node-content-gap) var(--card-padding) 0;
    display: flow-root;
  }

  header:has(+ .authoring-body) { padding-block: var(--card-padding) .5rem; }
  header:has(> :global(.ancestry:not([hidden]))) { padding-block-start: 0; }
  .authoring-body { padding-block: .5rem var(--card-padding); }

  .fallback-vote, .fallback-vote-dialog {
    color: var(--atlas-text); background: var(--atlas-surface);
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius);
    padding: .5rem; font: inherit;
  }
  .fallback-vote-dialog::backdrop { background: #0006; }

  .description {
    display: grid;
    grid-template-columns: 1fr auto;
    align-items: center;
    gap: .5rem;
  }

  .fallback-text {
    grid-column: 1 / -1;
    white-space: pre-line;
  }

  .read-more { --atlas-action-padding: 0.25rem 0.65rem; }
  .compact-read-more {
    margin-inline-start: auto;
    font: inherit;
    white-space: nowrap;
    padding: .25rem .65rem;
    min-height: 2.25rem;
    border: 1px solid var(--atlas-accent);
    border-radius: var(--atlas-radius);
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    cursor: pointer;
  }
  .compact-read-more:disabled { background: var(--atlas-bg); border-color: var(--atlas-border); }
  .compact-icon { display: none; }


  .text-action {
    border: 0;
    padding: 0.5rem 0;
    background: transparent;
    color: var(--atlas-accent);
    font: inherit;
    font-size: 0.9rem;
    cursor: pointer;
    text-decoration: none;
    &:not(:disabled):hover {
      text-decoration: underline;
    }
  }

  .reactions {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }

  button:disabled {
    cursor: default;
    color: var(--atlas-muted);
    text-decoration: none;
  }

  @media (max-width: 380px) {
    .node-card {
      --card-padding: var(--atlas-card-padding-narrow, 0.75rem);
      --dial-text-gap: var(--atlas-dial-text-gap, 0.6rem);
    }
    .node-card header,
    .body {
      padding-inline: var(--card-padding);
    }
  }
  @media (max-width: 560px) {
    .graph-actions { gap: .3rem; }
    .reaction-toggle { white-space: nowrap; }
    .comments-button, .compact-read-more {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: .25rem;
      min-width: 2.25rem;
      padding-inline: .35rem;
    }
    .compact-icon { display: block; }
    .action-text { display: none; }
    .mobile-layout {
      --card-padding: 0.75rem;
      h2 { font-size: 1.2rem; }
      .by-prefix, .community-prefix { display: none; }
      .body { display: flow-root; }
      .navigation-details { justify-content: flex-start; text-align: left; margin-left: 0; min-width: 0; flex-basis: 100%; }
    }
  }

  .node-type-row {
    display: flex;
    align-items: center;
    gap: .4rem .6rem;
    min-width: 0;
    container-type: inline-size;
    margin-block-end: var(--atlas-node-title-gap);
  }
  .node-type-row .mobile-type {
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
  .node-type-row :global(.live-voting), .fallback-vote { flex-shrink: 0; }
</style>
