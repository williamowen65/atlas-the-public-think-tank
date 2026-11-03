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
  import HiddenNodePublicNotice from './HiddenNodePublicNotice.svelte';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import NodePage from './NodePage.svelte';
  import NodeReportControl from './NodeReportControl.svelte';
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
  type HiddenDetail = NodeDetail & { moderationNotice?: string | null; reviewRequested?: boolean; notice?: string; publicReason?: string | null };
  let hiddenDetail = $state<HiddenDetail | null>(null);
  let hiddenTitle = $state('');
  let hiddenIntro = $state('');
  let hiddenMarkdown = $state('');
  let hiddenBlocks = $state<ContentBlock[]>([]);
  let hiddenBusy = $state(false);
  let hiddenMessage = $state('');
  let hiddenEdited = $state(false);
  let hiddenAuthor = $state(false);
  type ModerationFeedback = { reasons: { publicReason: string; rationale: string; decidedAt: string | null; reviewRequested: boolean }[] };
  let hiddenFeedback = $state<ModerationFeedback | null>(null);
  let hiddenFeedbackError = $state(false);
  type ReportHistory = { id: string; reason: string; explanation: string | null; createdAt: string; status: string; reporter: string; reviewer: string | null; decisionReason: string | null; decidedAt: string | null; publicReason: string; reviewRequestedAt: string | null; visibilityRestoredAt: string | null; restoredBy: string | null; restorationReason: string | null };
  let hiddenModerator = $state(false);
  let hiddenHistory = $state<ReportHistory[]>([]);
  let hiddenPreview = $state<NodeCardData | null>(null);
  let hiddenPreviewError = $state(false);
  type TextRevision = { title: string; shortDescription: string; markdown: string };
  let hiddenRevisions = $state<{ reported: TextRevision | null; revised: TextRevision } | null>(null);
  let revisionTab = $state<'reported' | 'revised'>('reported');
  let revisionError = $state('');
  let moderatorRationale = $state('');
  let moderatorBusy = $state(false);
  let moderatorConfirm = $state(false);
  let moderatorMessage = $state('');
  async function restoreHiddenNode() {
    if (!hiddenDetail || !hiddenModerator || !hiddenDetail.reviewRequested || !moderatorRationale.trim() || moderatorBusy) return;
    moderatorBusy = true;
    moderatorMessage = '';
    try {
      await apiRequest('/atlas-session-api',
        `moderation/nodes/${encodeURIComponent(hiddenDetail.id)}/restore`,
        'POST', { text: moderatorRationale.trim() });
      moderatorMessage = 'Node restored to public view.';
      moderatorRationale = '';
      moderatorConfirm = false;
      retry++;
    } catch (cause) {
      moderatorMessage = cause instanceof Error ? cause.message : 'Could not restore this node.';
    } finally {
      moderatorBusy = false;
    }
  }
  async function saveHiddenRevision() {
    if (!hiddenDetail || !hiddenAuthor || hiddenBusy) return;
    hiddenBusy = true; hiddenMessage = '';
    try {
      const id = encodeURIComponent(hiddenDetail.id);
      if (hiddenTitle.trim() !== hiddenDetail.title) await apiRequest('/atlas-session-api', `nodes/${id}/title`, 'PUT', { text: hiddenTitle.trim() });
      if (hiddenIntro.trim() !== hiddenDetail.shortDescription) await apiRequest('/atlas-session-api', `nodes/${id}/short-description`, 'PUT', { text: hiddenIntro.trim() });
      const original = hiddenBlocks.find(block => block.kind.toLowerCase() === 'markdowntext' || block.kind.toLowerCase() === 'markdown');
      if (original && hiddenMarkdown !== (original.content.markdown ?? '')) await apiRequest('/atlas-session-api', `nodes/${id}/description/blocks/${encodeURIComponent(original.id)}`, 'PUT', { kind: 'markdown', text: hiddenMarkdown });
      if (!original && hiddenMarkdown.trim()) await apiRequest('/atlas-session-api', `nodes/${id}/description/blocks`, 'POST', { kind: 'markdown', text: hiddenMarkdown });
      hiddenEdited = true;
      hiddenMessage = 'Changes saved. You can now request moderator review.';
      retry++;
    } catch (cause) { hiddenMessage = cause instanceof Error ? cause.message : 'Could not save revision.'; }
    finally { hiddenBusy = false; }
  }
  async function requestHiddenReview() {
    if (!hiddenDetail || !hiddenAuthor || hiddenBusy) return;
    hiddenBusy = true; hiddenMessage = '';
    try {
      await apiRequest('/atlas-session-api', `moderation/nodes/${encodeURIComponent(hiddenDetail.id)}/review`, 'POST');
      hiddenMessage = 'Review requested. The node stays hidden until a moderator restores it.';
      hiddenEdited = false;
      retry++;
    } catch (cause) { hiddenMessage = cause instanceof Error ? cause.message : 'Could not request review.'; }
    finally { hiddenBusy = false; }
  }
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
    hiddenDetail = null;
    hiddenAuthor = false;
    hiddenFeedback = null;
    hiddenFeedbackError = false;
    hiddenModerator = false;
    hiddenHistory = [];
    hiddenPreview = null;
    hiddenPreviewError = false;
    hiddenRevisions = null;
    revisionTab = 'reported';
    revisionError = '';
    moderatorRationale = '';
    moderatorConfirm = false;
    moderatorMessage = '';
    createType = null;
    editingDraft = null;
    saveChildDraft = null;
    active = 'comments';
    void (async () => {
      try {
        if (id) {
          // The authenticated node-detail endpoint, not public discovery, determines access
          // to hidden originals. A public placeholder never contains editable content.
          const detail = await apiRequest<HiddenDetail>('/atlas-session-api', `nodes/${encodeURIComponent(id)}`, 'GET', undefined, signal);
          if (detail.hidden) {
            hiddenDetail = detail;
            if (detail.authorId && detail.title) {
              const session = await apiRequest<{ participantId: string | null }>('/atlas-session-api', 'auth/session', 'GET', undefined, signal);
              hiddenAuthor = session.participantId === detail.authorId;
              if (hiddenAuthor) {
                try {
                  hiddenFeedback = await apiRequest<ModerationFeedback>(
                    '/atlas-session-api', `moderation/nodes/${encodeURIComponent(id)}/notice`, 'GET', undefined, signal,
                  );
                } catch {
                  if (!signal.aborted) hiddenFeedbackError = true;
                }
              }
              // The protected history endpoint verifies global-moderator authorization.
              // A signed-in author must not receive moderator controls merely for owning a node.
              try {
                hiddenHistory = await apiRequest<ReportHistory[]>(
                  '/atlas-session-api', `moderation/nodes/${encodeURIComponent(id)}/history`, 'GET', undefined, signal);
                hiddenModerator = true;
                try {
                  hiddenRevisions = await apiRequest<{ reported: TextRevision | null; revised: TextRevision }>(
                    '/atlas-session-api', `moderation/nodes/${encodeURIComponent(id)}/revisions`, 'GET', undefined, signal);
                } catch (cause) {
                  if (!signal.aborted) revisionError = cause instanceof Error
                    ? `Revision history could not be loaded: ${cause.message}`
                    : 'Revision history could not be loaded.';
                }
                try {
                  const [previewPage, originalBlocks] = await Promise.all([
                    readDiscovery(apiBase, 1, '', signal, id, undefined, false, true),
                    apiRequest<ContentBlock[]>('/atlas-session-api', `nodes/${encodeURIComponent(id)}/description`, 'GET', undefined, signal),
                  ]);
                  const previewEntry = previewPage.items.find(item => item.nodeId === id);
                  hiddenPreview = previewEntry ? discoveryCard(previewEntry, true) : null;
                  hiddenBlocks = originalBlocks;
                  if (!hiddenPreview) hiddenPreviewError = true;
                } catch {
                  if (!signal.aborted) hiddenPreviewError = true;
                }
              } catch (cause) {
                if (!(cause instanceof ApiRequestError && [401, 403].includes(cause.status)) && !signal.aborted)
                  moderatorMessage = 'Moderation tools could not be loaded.';
              }
              hiddenTitle = detail.title;
              hiddenIntro = detail.shortDescription ?? '';
              hiddenBlocks = await apiRequest<ContentBlock[]>('/atlas-session-api', `nodes/${encodeURIComponent(id)}/description`, 'GET', undefined, signal);
              hiddenMarkdown = hiddenBlocks.filter(b => ['markdown', 'markdowntext'].includes(b.kind.toLowerCase())).map(b => b.content.markdown ?? '').join('\n\n');
            }
            return;
          }
        }
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

{#if loading}<LoadingPlaceholder kind="node" label="Loading node…" />{:else if hiddenDetail}
  <HiddenNodePublicNotice reason={hiddenDetail.publicReason} author={hiddenAuthor} />
  {#if hiddenDetail.title}
    <section class="hidden-node" aria-label="Private hidden contribution">
      {#if hiddenAuthor}
        <h2>Your hidden contribution</h2>
        <p>Only you and authorized moderators can see the original content below. The public sees the explanation above, not this content.</p>
        <h3>Moderator feedback</h3>
        {#if hiddenFeedback?.reasons?.length}
          {#each hiddenFeedback.reasons as decision, index (index)}
            <div class="feedback">
              <p><strong>Public category:</strong> {decision.publicReason}</p>
              <p><strong>Moderator explanation:</strong> {decision.rationale}</p>
            </div>
          {/each}
        {:else if hiddenFeedbackError}
          <p role="alert">Moderator feedback could not be loaded. You can still revise your contribution.</p>
        {:else}
          <p>No additional written moderator explanation is available.</p>
        {/if}
        <h3>Revise your contribution</h3>
        {#if hiddenDetail.reviewRequested}
          <p role="status">Your review request is pending. The contribution remains hidden until a moderator restores it.</p>
        {/if}
        <form onsubmit={(event) => { event.preventDefault(); void saveHiddenRevision(); }}>
          <label for="hidden-title">Title</label><input id="hidden-title" bind:value={hiddenTitle} maxlength="500" required />
          <label for="hidden-intro">Short description</label><textarea id="hidden-intro" bind:value={hiddenIntro} rows="3" required></textarea>
          <label for="hidden-description">Description (Markdown)</label><textarea id="hidden-description" bind:value={hiddenMarkdown} rows="8"></textarea>
          <button type="submit" disabled={hiddenBusy}>Save revision</button>
        </form>
        <button type="button" disabled={hiddenBusy || !!hiddenDetail.reviewRequested || !hiddenEdited} onclick={() => void requestHiddenReview()}>Request moderator review</button>
        <p>Save a revision that addresses the feedback before requesting review. Saving does not automatically make the contribution public.</p>
      {:else}
        <h2>Moderator inspection</h2>
        <p>This is a read-only rendering of the original contribution using the standard node-page layout. Only the author can revise it and request review.</p>
        {#if hiddenPreview}
          <div class="revision-tabs" role="tablist" aria-label="Compare node revisions">
            <button type="button" role="tab" id="reported-revision-tab" aria-controls="moderator-node-revision" aria-selected={revisionTab === 'reported'} class:chosen={revisionTab === 'reported'} disabled={!hiddenRevisions?.reported} onclick={() => revisionTab = 'reported'}>Reported version</button>
            <button type="button" role="tab" id="revised-revision-tab" aria-controls="moderator-node-revision" aria-selected={revisionTab === 'revised'} class:chosen={revisionTab === 'revised'} onclick={() => revisionTab = 'revised'}>Revised version</button>
          </div>
          {#if revisionError}<p role="alert">{revisionError}</p>{/if}
          {#if !revisionError && !hiddenRevisions?.reported}
            <p role="status">No earlier revision was found. Confirm that the temporal migration was applied and the demo database was reseeded after that migration.</p>
          {/if}
          <div id="moderator-node-revision" role="tabpanel" aria-labelledby={revisionTab === 'reported' ? 'reported-revision-tab' : 'revised-revision-tab'}>

          <NodePage
            node={hiddenRevisions?.reported && revisionTab === 'reported'
              ? { ...hiddenPreview, title: hiddenRevisions.reported.title, description: hiddenRevisions.reported.shortDescription }
              : hiddenPreview}
            blocks={hiddenRevisions?.reported && revisionTab === 'reported'
              ? hiddenBlocks.map(block => ['markdown', 'markdowntext'].includes(block.kind.toLowerCase())
                ? { ...block, content: { ...block.content, markdown: hiddenRevisions!.reported!.markdown } }
                : block)
              : hiddenBlocks}
            types={[]}
            active="comments"
            inspectionOnly
            {apiBase}
            {onOpenProfile}
            onNavigate={openNode}
            onTabChange={() => {}}
            onAddChild={() => {}}
          ></NodePage>
          </div>
        {:else if hiddenPreviewError}
          <p role="alert">The full node preview could not be loaded. Try reloading this page.</p>
        {:else}
          <p role="status">Loading original node preview…</p>
        {/if}
      {/if}
      {#if hiddenModerator}
        <section class="moderator-tools" aria-label="Moderator actions">
          <h2>Global moderator controls</h2>
          <p>This contribution is hidden. Moderation actions are checked by the server and recorded with the moderator's identity.</p>
          <h3>Report history</h3>
          {#if hiddenHistory.length === 0}
            <p>No report history is available.</p>
          {:else}
            <ul>
              {#each hiddenHistory as report (report.id)}
                <li>
                  <strong>Report submitted</strong> · {new Date(report.createdAt).toLocaleString()}
                  <p>Reported by {report.reporter} · {report.reason}</p>
                  {#if report.explanation}<p>{report.explanation}</p>{/if}
                  {#if report.decidedAt}
                    <p><strong>Moderator decision: {report.status === 'Actioned' ? 'Hidden' : report.status}</strong>
                      · {new Date(report.decidedAt).toLocaleString()}</p>
                    <p>Decided by {report.reviewer ?? 'Unknown moderator'} · {report.publicReason}</p>
                    {#if report.decisionReason}<p>Rationale: {report.decisionReason}</p>{/if}
                  {:else}
                    <p>Awaiting moderator decision</p>
                  {/if}
                  {#if report.reviewRequestedAt}
                    <p><strong>Author requested review</strong> · {new Date(report.reviewRequestedAt).toLocaleString()}</p>
                  {/if}
                  {#if report.visibilityRestoredAt}
                    <p><strong>Visibility restored</strong> · {new Date(report.visibilityRestoredAt).toLocaleString()}</p>
                    <p>Restored by {report.restoredBy ?? 'Unknown moderator'}</p>
                    {#if report.restorationReason}<p>Rationale: {report.restorationReason}</p>{/if}
                  {/if}
                </li>
              {/each>
            </ul>
          {/if}
          {#if hiddenDetail.reviewRequested}
            <p>The author has requested review. Review their revised content before restoring visibility.</p>
            <label for="moderator-restore-rationale">Restoration rationale (required)</label>
            <textarea id="moderator-restore-rationale" bind:value={moderatorRationale} maxlength="2000" rows="3"></textarea>
            {#if moderatorConfirm}
              <p role="alert">Restore this contribution to public Discovery and search?</p>
              <button type="button" disabled={moderatorBusy} onclick={() => moderatorConfirm = false}>Cancel</button>
              <button type="button" disabled={moderatorBusy || !moderatorRationale.trim()} onclick={() => void restoreHiddenNode()}>Confirm restoration</button>
            {:else}
              <button type="button" disabled={moderatorBusy || !moderatorRationale.trim()} onclick={() => moderatorConfirm = true}>Restore visibility</button>
            {/if}
          {:else}
            <p>Restoration is available after the author revises the contribution and requests review.</p>
          {/if}
          <p><a href="/moderation">Open moderation dashboard</a></p>
          {#if moderatorMessage}<p role="status">{moderatorMessage}</p>{/if}
        </section>
      {/if}
      {#if hiddenMessage}<p role="status">{hiddenMessage}</p>{/if}
    </section>
  {/if}
{:else if error}<p
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
            mobileCompact
            nodeId={node.id}
            initialAverage={average}
            initialTotalVotes={totalVotes}
            archived={node.status === 'Archived'}
          />{/if}{/snippet}
      {#snippet reactions()}{#if node}<LiveReactions
            nodeId={node.id}
            archived={node.status === 'Archived'}
          />{/if}{/snippet}
      <NodeReportControl nodeId={node.id} />
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
  .hidden-node { max-width: 48rem; margin: 1rem auto; padding: 1rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); }
  .revision-tabs { display: flex; gap: .5rem; margin: 1rem 0; }
  .revision-tabs button { border: 1px solid var(--atlas-border); border-radius: .5rem; padding: .6rem 1rem; }
  .revision-tabs button:disabled { opacity: .5; cursor: not-allowed; }
  .revision-tabs button.chosen { font-weight: 700; border-color: currentColor; }
  .moderator-tools { margin-top: 2rem; padding-top: 1rem; border-top: 1px solid var(--atlas-border); }
  .moderator-tools label { display: block; margin: .75rem 0 .35rem; }
  .moderator-tools ul { padding-left: 1.5rem; }
  .hidden-node form { display: grid; gap: .6rem; margin: 1rem 0; }
  .hidden-node input, .hidden-node textarea { width: 100%; padding: .65rem; background: var(--atlas-surface); color: var(--atlas-text); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); }
  .hidden-node button { padding: .6rem 1rem; margin: .5rem .5rem .5rem 0; }
  .child-form {
    max-width: 48rem;
    margin: 1rem auto;
  }
  .child-form > button {
    margin-bottom: 1rem;
  }
</style>
