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
  import { relativeDate } from '../../format/relativeDate';
  import { bottomCornerAction } from '../../actions/bottomCornerAction';
  import type { Snippet } from 'svelte';
  import type { ContentBlock } from '../../api/nodePage';
  import FeedBlockPreview from '../FeedBlockPreview.svelte';
  import ActionButton from '../ActionButton.svelte';
  import CommunityLinks from '../CommunityLinks.svelte';
  import VotingDial from '../VotingDial.svelte';

  import Breadcrumb from '../Breadcrumb.svelte';
  import type { NodeCardProps } from './types';
  let {
    mobileLayout = false,
    cornerControl,
    headerFields,
    authoringBody,
    authoringFooter,
    blocks,
    draft = false,
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
  } = $props();
  const instance = $props.id();
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

<article class="node-card" class:mobile-layout={mobileLayout} data-presentation="ribbon-corner" aria-labelledby={`${instance}-title`}>
  {#if editOpensDetail}
    <button class="hidden-edit-link" type="button" aria-label="Open hidden node to edit and request review" title="Open node to revise" onclick={() => onReadMore?.(node.id)}>✎ Edit</button>
  {:else}
    {@render cornerControl?.()}
  {/if}
  {#if node.breadcrumb.length}
    <div class="ribbon-context">
      <Breadcrumb
        items={draft ? node.breadcrumb.map(({ id, title }) => ({ id, title })) : node.breadcrumb}
        originId={draft ? undefined : node.id}
        {apiBase}
        {onNavigate}
        hasMore={hasMoreAncestors}
        loadOlder={loadOlderAncestors}
      />
    </div>
  {/if}
  <header>
    {#if headerFields}
      <div id={`${instance}-title`}>{@render headerFields()}</div>
    {:else}
      <div class="node-type-row"><p class="mobile-type">{node.typeName}</p></div>
      <h2 id={`${instance}-title`}><span class="title-type">{node.typeName}:</span> {node.title}</h2>
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
      ><span class="byline-separator" aria-hidden="true">·</span><time datetime={node.createdAt} title={new Date(node.createdAt).toLocaleString()}
        >{mobileLayout ? relativeDate(node.createdAt) : new Date(node.createdAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric',
          timeZone: 'UTC',
        })}</time
      >{#if node.communities.length}
        <span class="byline-separator" aria-hidden="true">·</span>
        <span class="community-prefix">{node.communities.length === 1 ? 'Community' : 'Communities'}: </span>
        {#if draft}{node.communities.join(', ')}{:else}<CommunityLinks
          nodeId={node.id}
          allowJoin={mobileLayout}
          names={node.communities}
        />{/if}{/if}
    </p>
  </header>

  {#if authoringBody}
    <div class="body authoring-body">{@render authoringBody()}</div>
    {@render authoringFooter?.()}
  {:else}
    <div class="body">
      <aside class="dial" aria-label="Importance rating">
        {#if voting}{@render voting()}{:else}<VotingDial
            label="Node importance"
            {value}
            {average}
            {totalVotes}
            disabled={draft || node.status === 'Archived'}
            onChange={onVote}
          />{/if}
      </aside>
      {#if node.isHidden}
        {@render legacyDescription(node.description)}
      {:else}
        <FeedBlockPreview nodeId={node.id} fallback={node.description} renderFallback={legacyDescription} {apiBase} {blocks}
          onReadMore={onReadMore ? () => onReadMore?.(node.id) : undefined} />
      {/if}
    </div>
    <div class="card-details">
      {@render reactions()}
      <div class="navigation-details">
        {@render comments()}
        {#each node.childTypes ?? [] as contribution}<span
            class="navigation-separator"
            aria-hidden="true">·</span
          ><button
            class="text-action connection-summary"
            type="button"
            disabled={!onChildType}
            onclick={() => onChildType?.(node.id, contribution.typeId)}>{contribution.label}</button
          >{/each}
        {#each connectionLabels as label}<span class="navigation-separator" aria-hidden="true">·</span
          ><span class="text-action connection-summary">{label}</span>{/each}
      </div>
    </div>
  {/if}
</article>

{#snippet legacyDescription(text: string)}
  <div class="description" id={`${instance}-description`} use:bottomCornerAction>
    <span class="read-more"
      ><ActionButton
        label="Read more"
        disabled={!onReadMore}
        onActivate={() => onReadMore?.(node.id)}
      /></span
    >{text}
  </div>
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

<style lang="scss">
  .hidden-edit-link { position: absolute; z-index: 2; top: 0; right: 0; border: 1px solid var(--atlas-border); border-top: 0; border-right: 0; border-radius: 0 var(--atlas-radius) 0 var(--atlas-radius); padding: .55rem .8rem; background: var(--atlas-surface); color: var(--atlas-text); font: inherit; cursor: pointer; }
  .hidden-edit-link:hover { color: var(--atlas-accent); }
  .mobile-type { display: none; }


  // Only owner-visible corner controls reserve space; breadcrumbs keep their scroll behavior.
  .node-card:has(> :global(.edit-node.corner)) > .ribbon-context {
    padding-right: 3.25rem;
    min-height: 2.75rem;
  }

  .node-card {
    position: relative;
    --card-padding: var(--atlas-card-padding, 1.25rem);
    --dial-text-gap: var(--atlas-dial-text-gap, 1rem);
    --atlas-breadcrumb-bottom-space: 0px;
    max-width: 48rem;
    width: 100%;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    overflow-wrap: anywhere;
    header {
      padding: var(--card-padding) var(--card-padding) 0.5rem;
    }
    h2 {
      margin: 0 0 var(--atlas-heading-gap, 0.5rem);
      font-size: calc(clamp(1.35rem, 4vw, 1.9rem) * var(--atlas-heading-scale, 1.5) / 1.5);
      line-height: 1.2;
    }
    p {
      line-height: var(--atlas-line-height, 1.65);
    }
  }

  .ribbon-context {
    min-width: 0;
    padding: 0.75rem 0 0;
    margin-bottom: var(--card-padding);
  }

  .ribbon-context + header {
    padding-top: 0;
    h2 {
      margin-top: 0;
    }
  }

  .title-type {
    color: var(--atlas-accent);
  }

  [data-presentation] {
    .card-details {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      align-items: center;
      gap: 0.35rem 1rem;
      padding: 0 var(--card-padding) 1rem;
      font-size: 0.9rem;
      color: var(--atlas-muted);
    }
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
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }

  .body {
    padding: 0.5rem var(--card-padding) 0;
    display: flow-root;
  }

  .authoring-body { padding-bottom: var(--card-padding); }

  .body:has(> :global(.has-slides)) {
    display: grid;
    grid-template-columns: auto minmax(0, 1fr);
    align-items: start;
    gap: 0 var(--dial-text-gap);
    margin-bottom: var(--atlas-paragraph-gap, 0.65rem);
    .dial { float: none; margin-right: 0; }
  }

  .dial {
    float: left;
    margin: 0 var(--dial-text-gap) 0.6rem -0.4rem;
  }

  .description {
    margin: 0 0 var(--atlas-paragraph-gap, 0.65rem);
    white-space: pre-line;
  }

  .read-more {
    --atlas-action-padding: 0.4rem 0.7rem;
    // Exclude only the button's bottom-corner footprint from the text flow.
    float: right;
    margin: var(--corner-top, 0px) 0 0 0.75rem;
    shape-outside: inset(var(--corner-top, 0px) 0 0 0);
    white-space: normal;
  }

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
    .mobile-layout {
      --card-padding: 0.75rem;
      .mobile-type { display: block; margin: 0 0 0.25rem; color: var(--atlas-accent); font-size: 2.1rem; line-height: 0; font-weight: 700; }
      h2 { font-size: 1.2rem; }
      .title-type, .by-prefix, .community-prefix { display: none; }
      .byline { font-size: 0.75rem; line-height: 1.5; }
      // Keep the description and Read more in one text flow, as on desktop.
      // The button's existing right float lets text occupy the remaining width.
      .body { display: flow-root; }
      .description { display: block; }
      .body:has(> :global(.has-slides)) { display: block; }
      .dial { display: contents; }
      .navigation-details { justify-content: flex-start; text-align: left; margin-left: 0; min-width: 0; flex-basis: 100%; }
    }
  }

  .node-type-row { display: none; }
  @media (max-width: 560px) {
    .mobile-layout .node-type-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: nowrap;
      min-width: 0;
      container-type: inline-size;
      gap: 0.4rem 0.75rem;
      margin-bottom: 0.5rem;
    }
    // Scale with the available header width, not just the viewport. CSS cannot
    // measure text and select an exact fitting font size, so truncate as a
    // last resort rather than allowing the label to overlap the vote summary.
    .node-type-row .mobile-type {
      margin: 0;
      min-width: 0;
      flex: 0 1 auto;
      max-width: 100%;
      font-size: clamp(0.95rem, 8.1cqi, 2.1rem);
      line-height: 1.15;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .node-type-row :global(.header-summary) {
      flex-shrink: 0;
    }
    
  }
</style>
