<!--
  @component
  NodeCard: Selects a card presentation and forwards the common card contract.
  Only selects a visual variant; shared props and optional voting/reaction snippets pass through unchanged.

  **Props**
  - `node` (`NodeCardData`; required): Presentation data for one node; follow the NodeCardData type
    definition for its fields.
  - `voting` (`Snippet`; optional): Renderable snippet that replaces the card’s built-in VotingDial.
    Supply LiveVotingDial here for API-backed voting; otherwise the built-in dial reports selections
    through onVote.
  - `reactionControls` (`Snippet`; optional): Renderable snippet replacing the static reaction
    summary with custom/live controls, such as LiveReactions.
  - `hasMoreAncestors` (`boolean`; optional): Whether additional older ancestors can be loaded for
    the node’s breadcrumb.
  - `loadOlderAncestors` (`LoadOlderAncestors`; optional): Async callback receiving the oldest
    visible ancestor ID and returning BreadcrumbPage: older items and hasMore. Used by ribbon
    variants; context-below has no ancestor pager.
  - `apiBase` (`string`; optional): Public read base forwarded to breadcrumb lookups by ribbon
    variants. Context-below accepts but does not use it; this is not the vote-save endpoint.
  - `value` (`number | null`; optional): Current controlled rating, 0–10. null means unrated; zero
    is a real rating.
  - `average` (`number | null`; optional): Community average supplied by the parent; null means no
    average is available.
  - `totalVotes` (`number`; optional): Community vote count supplied by the parent.
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
  - `presentation` (`NodeCardPresentation`; optional; default `'ribbon'`): Selects a
    NodeCardPresentation layout; it does not change the underlying node data.
-->
<script lang="ts" module>
  export type { NodeCardData, NodeCardPresentation } from './node-card-versions/types';
</script>

<script lang="ts">
  import NodeCardContextBelow from './node-card-versions/NodeCardContextBelow.svelte';
  import NodeCardRibbon from './node-card-versions/NodeCardRibbon.svelte';
  import NodeCardRibbonCorner from './node-card-versions/NodeCardRibbonCorner.svelte';
  import NodeCardRibbonSplit from './node-card-versions/NodeCardRibbonSplit.svelte';
  import NodeCardRibbonResponsive from './node-card-versions/NodeCardRibbonResponsive.svelte';
  import type { NodeCardProps, NodeCardPresentation } from './node-card-versions/types';
  // A supplied presentation overrides the default; the remaining props are forwarded to the selected variant.
  let {
    presentation = 'ribbon',
    ...props
  }: NodeCardProps & {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
  } = $props();
</script>

<!--
  Layout: Exactly one presentation variant is rendered; this component adds no layout wrapper
  and forwards the common card props.
-->

{#if presentation === 'context-below'}
  <NodeCardContextBelow {...props} />
{:else if presentation === 'mobile-experiment'}
  <NodeCardRibbonCorner {...props} mobileLayout />
{:else if presentation === 'ribbon-corner'}
  <NodeCardRibbonCorner {...props} />
{:else if presentation === 'ribbon-split'}
  <NodeCardRibbonSplit {...props} />
{:else if presentation === 'ribbon-responsive'}
  <NodeCardRibbonResponsive {...props} />
{:else}
  <NodeCardRibbon {...props} />
{/if}
