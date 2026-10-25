<!--
  @component
  LiveNodePage: Fetches a node, its content and types, then composes voting, tabs, child forms and discussion.
  Owns data and tab state; NodePage receives presentation props and renderable content snippets.

  **Props**
  - `nodeId` (`string`; optional; default `''`): ID of the node this component reads, displays, or
    modifies.
  - `initialTypeId` (`string`; optional; default `''`): Initial discussion tab ID (a contribution
    type ID or comments), not a node-creation type.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Public API base for discovery,
    node details, content blocks, type catalog and ancestor lookups. Voting and comments receive
    their session-aware behavior from nested components.
  - `onOpenNode` (`(id: string, typeId?: string) => void`; optional): Callback: Receives a node ID
    to open. Where the signature includes typeId, it selects the destination contribution tab.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
-->
<script lang="ts">
  import LoadingPlaceholder from './LoadingPlaceholder.svelte';
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import NodePage from './NodePage.svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import CommentsPanel from './CommentsPanel.svelte';
  import LiveNodeForm from './LiveNodeForm.svelte';
  import type { NodeDraft } from './NodeForm.svelte';
  import {
    readDiscovery,
    discoveryCard,
    readReactions,
    readOlderAncestors,
  } from '../api/discovery';
  import {
    readJson,
    readTypes,
    type NodeType,
    type ContentBlock,
    type NodeDetail,
  } from '../api/nodePage';
  import type { NodeCardData } from './NodeCard.svelte';
  let {
    nodeId = '',
    initialTypeId = '',
    focusCommentId = '',
    apiBase = '/atlas-public-api',
    onOpenNode,
    onOpenProfile,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId?: string;
    /** Initial discussion tab ID (a contribution type ID or comments), not a node-creation type. */
    initialTypeId?: string;
    /** Deep-linked comment whose ancestor thread should be revealed. */
    focusCommentId?: string;
    /**
     * Public API base for discovery, node details, content blocks, type catalog and ancestor
     * lookups. Voting and comments receive their session-aware behavior from nested components.
     */
    apiBase?: string;
    /**
     * Callback: Receives a node ID to open. Where the signature includes typeId, it selects the
     * destination contribution tab.
     */
    onOpenNode?: (id: string, typeId?: string) => void;
    /** Callback: Receives the author/participant ID to open in a profile page. */
    onOpenProfile?: (id: string) => void;
  } = $props();
  /**
   * Loaded public node-card model, or null before usable node data is available.
   */
  let node = $state<NodeCardData | null>(null);
  /**
   * Ordered full-content blocks loaded for this node.
   */
  let blocks = $state<ContentBlock[]>([]);
  /**
   * Available non-archived node types used by labels, filters or creation choices.
   */
  let types = $state<NodeType[]>([]);
  /**
   * Selected discussion-tab ID: comments, a contribution type ID or a temporary draft tab.
   */
  let active = $state('comments');
  $effect(() => {
    if (focusCommentId) active = 'comments';
  });
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(false);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Counter changed by Retry to rerun the reactive node-loading effect.
   */
  let retry = $state(0);
  /**
   * Type ID of the open child form; null means closed, while empty text permits initial
   * selection.
   */
  let createType = $state<string | null>(null);
  /**
   * Whether the node’s breadcrumb can load older ancestors.
   */
  let hasMoreAncestors = $state(false);
  /**
   * Latest child draft reported by the open form; determines its temporary tab label and
   * save behavior.
   */
  let editingDraft = $state<NodeDraft | null>(null);
  let saveChildDraft: (() => Promise<boolean>) | null = null;
  /**
   * Whether tab switching is awaiting draft handling; prevents overlapping transitions.
   */
  let switching = $state(false);
  const draftChanged = (draft: NodeDraft) =>
    !!(
      draft.title ||
      draft.shortDescription ||
      draft.markdown ||
      draft.newType?.name ||
      draft.newType?.description ||
      draft.requestedTypeIds.length
    );
  // Before switching tabs, save a titled draft or ask before discarding untitled changes.
  async function closeChildForm(): Promise<boolean> {
    if (createType === null) return true;
    if (editingDraft && draftChanged(editingDraft)) {
      if (editingDraft.title.trim()) {
        if (!saveChildDraft || !(await saveChildDraft())) {
          window.alert('Your draft could not be saved. The form will remain open.');
          return false;
        }
      } else if (
        !window.confirm(
          'This child node has unsaved changes but no title. A title is required to save a private draft. Discard these changes and close the form?',
        )
      )
        return false;
    }
    createType = null;
    editingDraft = null;
    saveChildDraft = null;
    return true;
  }
  async function changeTab(id: string) {
    if (switching) return;
    switching = true;
    try {
      if (await closeChildForm()) active = id;
    } finally {
      switching = false;
    }
  }
  // Track the editable child draft so its selected type can appear as a temporary discussion tab.
  function selectDraftType(draft: NodeDraft | null) {
    editingDraft = draft;
    if (createType === null || !draft) return;
    const selected = draft.newType ? 'draft-new-type' : draft.typeId;
    if (selected) active = selected;
  }
  /**
   * Backend-calculated average importance rating; null means no average is available.
   */
  let average = $state<number | null>(null);
  /**
   * Backend-reported importance vote count; the frontend does not calculate it.
   */
  let totalVotes = $state(0);
  const openNode = (id: string, typeId?: string) =>
    onOpenNode
      ? onOpenNode(id, typeId)
      : window.location.assign(
          `/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`,
        );
  // Reload when the node, initial tab or retry signal changes; abort stale requests on navigation.
  $effect(() => {
    const id = nodeId;
    const selectedType = initialTypeId;
    retry;
    const controller = new AbortController();
    const signal = AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
    loading = true;
    error = '';
    node = null;
    createType = null;
    editingDraft = null;
    saveChildDraft = null;
    active = 'comments';
    void (async () => {
      try {
        const page = await readDiscovery(apiBase, 1, '', signal, id || undefined);
        const entry = page.items.find((item) => discoveryCard(item));
        const display = entry && discoveryCard(entry);
        if (!display) throw new Error('Unavailable');
        const [detail, content, catalog, reactions] = await Promise.all([
          readJson<NodeDetail>(apiBase, `nodes/${encodeURIComponent(display.id)}`, signal),
          readJson<ContentBlock[]>(
            apiBase,
            `nodes/${encodeURIComponent(display.id)}/description`,
            signal,
          ),
          readTypes(apiBase, signal),
          readReactions(display.id, signal, apiBase).catch(() => []),
        ]);
        if (detail.hidden) throw new Error('Unavailable');
        if (controller.signal.aborted) return;
        node = { ...display, authorId: detail.authorId, reactions };
        hasMoreAncestors = !!entry!.card?.breadcrumbSegment?.continuationNodeId;
        blocks = content;
        average = entry!.averageVote;
        totalVotes = entry!.voteCount;
        active = display.childTypes?.some((type) => type.typeId === selectedType)
          ? selectedType
          : 'comments';
        types = [...catalog].sort(
          (a, b) =>
            Number(detail.requestedTypeIds.includes(b.id)) -
            Number(detail.requestedTypeIds.includes(a.id)),
        );
      } catch {
        if (!controller.signal.aborted) error = 'This node is unavailable or could not be loaded.';
      } finally {
        if (!controller.signal.aborted) loading = false;
      }
    })();
    return () => controller.abort();
  });
</script>

<!--
  Layout: Loading/error states precede NodePage. Named snippets fill voting/reactions; remaining
  child markup supplies the selected discussion panel.
-->

{#if loading}<LoadingPlaceholder kind="node" label="Loading node…" />{:else if error}<p
    role="alert"
  >
    {error}
  </p>
  <button onclick={() => retry++}>Retry node</button>{:else if node}
  {#key node.id}
    <NodePage
      {apiBase}
      {onOpenProfile}
      scrollToDiscussionTabs={!!initialTypeId && !focusCommentId && active === initialTypeId}
      {hasMoreAncestors}
      loadOlderAncestors={(id) =>
        id
          ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase)
          : Promise.resolve({ items: [], hasMore: false })}
      {node}
      {blocks}
      {types}
      {active}
      draftTab={createType !== null && editingDraft
        ? {
            id: editingDraft.newType ? 'draft-new-type' : (editingDraft.typeId ?? createType),
            name: editingDraft.newType
              ? (editingDraft.newType.name || 'New node type') + ' (draft)'
              : (types.find((type) => type.id === editingDraft?.typeId)?.name ?? 'Child') +
                ' (draft)',
          }
        : undefined}
      {average}
      {totalVotes}
      onNavigate={openNode}
      onTabChange={(id) => void changeTab(id)}
      onAddChild={(id) => {
        if (createType !== null) return;
        createType = id ?? types[0]?.id ?? '';
        active = createType;
      }}
    >
      <!-- This named snippet becomes NodePage's voting prop and replaces its disabled fallback.
           Archive state reaches LiveVotingDial, which also disables itself while loading/saving. -->
      {#snippet voting()}{#if node}<LiveVotingDial
            nodeId={node.id}
            initialAverage={average}
            initialTotalVotes={totalVotes}
            archived={node.status === 'Archived'}
          />{/if}{/snippet}
      {#snippet reactions()}{#if node}<LiveReactions
            nodeId={node.id}
            archived={node.status === 'Archived'}
          />{/if}{/snippet}
      <!-- This child form is regular child markup for NodePage’s panel, not a named snippet. -->
      {#if createType !== null}<div class="child-form">
          <button onclick={() => void closeChildForm()}>Close child form</button
          >{#key createType}<LiveNodeForm
              parentId={node.id}
              parentTitle={node.title}
              initialTypeId={createType}
              onCreated={openNode}
              onDraftChange={selectDraftType}
              onRegisterClose={(close) => (saveChildDraft = close)}
              breadcrumb={node.breadcrumb}
              focusOnMount
              {apiBase}
            />{/key}
        </div>{/if}
      <!--
        Changing tabs recreates the selected discussion subtree so its local paging/form state
        does not leak across tabs.
      -->
      {#key active}{#if active === 'comments'}<CommentsPanel
            nodeId={node.id}
            {focusCommentId}
            {apiBase}
            onAdded={() => {
              if (node) node = { ...node, commentCount: node.commentCount + 1 };
            }}
          />{:else if active === 'draft-new-type' || (createType !== null && active === editingDraft?.typeId && !node.childTypes?.some((type) => type.typeId === active))}<p
          >
            The child node draft is open above.
          </p>{:else}<LiveNodeFeed
            {onOpenProfile}
            parentId={node.id}
            typeId={active}
            {apiBase}
            onOpenNode={openNode}
            emptyTypeName={types.find((type) => type.id === active)?.name ??
              node.childTypes
                ?.find((type) => type.typeId === active)
                ?.label.replace(/^\s*\d+\s+/, '')
                .replace(/\s*\(\d+\)\s*$/, '')
                .replace(/ies$/i, 'y')
                .replace(/s$/i, '')}
            onCreateFirst={() => {
              if (createType === null) {
                createType = active;
              }
            }}
          />{/if}{/key}
    </NodePage>
  {/key}
{/if}

<style>
  .child-form {
    max-width: 48rem;
    margin: 1rem auto;
  }
  .child-form > button {
    margin-bottom: 1rem;
  }
</style>
