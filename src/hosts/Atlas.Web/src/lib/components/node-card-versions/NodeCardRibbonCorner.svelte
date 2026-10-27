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
  import { bottomCornerAction } from '../../actions/bottomCornerAction';
  import NodeEditControl from '../NodeEditControl.svelte';
  import ActionButton from '../ActionButton.svelte';
  import CommunityLinks from '../CommunityLinks.svelte';
  import VotingDial from '../VotingDial.svelte';

  import Breadcrumb from '../Breadcrumb.svelte';
  import type { NodeCardProps } from './types';
  let {
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
    onChildType,
    onComments,
    hasMoreAncestors = false,
    loadOlderAncestors,
    apiBase,
  }: NodeCardProps = $props();
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

<article class="node-card" data-presentation="ribbon-corner" aria-labelledby={`${instance}-title`}>
  {#if node.breadcrumb.length}
    <div class="ribbon-context">
      <Breadcrumb
        items={node.breadcrumb}
        originId={node.id}
        {apiBase}
        {onNavigate}
        hasMore={hasMoreAncestors}
        loadOlder={loadOlderAncestors}
      />
    </div>
  {/if}
  <header>
    <NodeEditControl
      nodeId={node.id}
      authorId={node.authorId}
      {apiBase}
      onSaved={(update) => {
        node = { ...node, title: update.title, description: update.shortDescription };
      }}
    />
    <h2 id={`${instance}-title`}><span class="title-type">{node.typeName}:</span> {node.title}</h2>
    {#if node.status === 'Archived'}<p class="byline">Archived</p>{/if}
    <p class="byline">
      <span
        >By {#if node.authorId}<a
            href={`/profiles?id=${encodeURIComponent(node.authorId)}`}
            onclick={(event) => {
              if (onOpenProfile && node.authorId) {
                event.preventDefault();
                onOpenProfile(node.authorId);
              }
            }}>{node.authorName}</a
          >{:else}{node.authorName}{/if}</span
      ><span class="byline-separator" aria-hidden="true">·</span><time datetime={node.createdAt}
        >{new Date(node.createdAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric',
          timeZone: 'UTC',
        })}</time
      >{#if node.communities.length}
        <span class="byline-separator" aria-hidden="true">·</span>
        {node.communities.length === 1 ? 'Community' : 'Communities'}: <CommunityLinks
          nodeId={node.id}
          names={node.communities}
        />{/if}
    </p>
  </header>

  <div class="body">
    <aside class="dial" aria-label="Importance rating">
      {#if voting}{@render voting()}{:else}<VotingDial
          label="Node importance"
          {value}
          {average}
          {totalVotes}
          disabled={node.status === 'Archived'}
          onChange={onVote}
        />{/if}
    </aside>
    <p class="description" id={`${instance}-description`} use:bottomCornerAction>
      <span class="read-more"
        ><ActionButton
          label="Read more"
          disabled={!onReadMore}
          onActivate={() => onReadMore?.(node.id)}
        /></span
      >{node.description}
    </p>
    {#if node.requestedTypes.length}
      <p class="invitation">
        Help develop this node with: <strong>{node.requestedTypes.join(', ')}</strong>.
      </p>
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
</article>

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
  .node-card {
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
      align-items: start;
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
      align-items: baseline;
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
    padding: 0.5rem var(--card-padding) 1rem;
    display: flow-root;
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
    // Exclude only the button's bottom-corner footprint from the text flow.
    float: right;
    margin: var(--corner-top, 0px) 0 0 0.75rem;
    shape-outside: inset(var(--corner-top, 0px) 0 0 0);
    white-space: normal;
  }

  .invitation {
    clear: both;
    font-size: 0.9rem;
    color: var(--atlas-muted);
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
</style>
