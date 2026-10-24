<!--
  LiveNodeForm: Session-aware creation, browser-private draft handling, custom types and backend submission.
  Public reads load types; session-aware writes save types/nodes, while private drafts stay in this browser.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { putDraft, deleteDraft, listDrafts } from '../api/privateDrafts';
  import NodeForm, { type NodeDraft } from './NodeForm.svelte';
  import SessionControl from './SessionControl.svelte';
  import type { BreadcrumbItem } from './Breadcrumb.svelte';
  import { readTypes, type NodeType } from '../api/nodePage';
  import { commentRequest, CommentRequestError } from '../api/comments';
  import { readDiscovery, discoveryCard } from '../api/discovery';
  let {
    draftId,
    parentId,
    parentTitle,
    initialTypeId,
    breadcrumb = [],
    focusOnMount = false,
    apiBase = '/atlas-public-api',
    sessionBase = '/atlas-session-api',
    onCreated,
    onDraftChange,
    onRegisterClose,
  }: {
    /** ID of an existing browser-private draft to restore for the signed-in participant. */
    draftId?: string;
    /** Parent node ID for child-node creation; omit for root creation. */
    parentId?: string;
    /** Display title for the parent breadcrumb; parentId is the actual association. */
    parentTitle?: string;
    /** Initial node type selection when creating a draft. */
    initialTypeId?: string;
    /** Parent ancestry used above a creation form, before the immediate parent is appended. */
    breadcrumb?: BreadcrumbItem[];
    /**
     * Requests initial focus/scroll placement when the relevant form is rendered in the browser.
     */
    focusOnMount?: boolean;
    /**
     * Public API base for reading the type catalog and resolving a parent node’s title/ancestry.
     * Creation writes use sessionBase instead.
     */
    apiBase?: string;
    /**
     * Session-aware API base for account controls and CSRF-protected custom-type/node creation.
     * Private draft saving uses browser storage rather than this endpoint.
     */
    sessionBase?: string;
    /** Callback: Receives the ID returned by the backend after successful node creation. */
    onCreated?: (id: string) => void;
    /**
     * Receives the current draft or null according to the callback contract; lets the parent track
     * child-form state.
     */
    onDraftChange?: (draft: NodeDraft | null) => void;
    /**
     * Callback: Registers an async draft-save function with the parent. Calling that function
     * returns whether saving succeeded; it does not itself close the form.
     */
    onRegisterClose?: (close: () => Promise<boolean>) => void;
  } = $props();
  let types = $state<NodeType[]>([]);
  let error = $state('');
  let loading = $state(true);
  let participantId = $state<string | null>(null);
  let hasDraft = $state(false);
  let submitting = $state(false);
  let saveError = $state('');
  let createdId = $state('');
  let createdType = $state<{ key: string; id: string } | null>(null);
  let controller: AbortController;
  let currentDraft = $state<NodeDraft | null>(null);
  let savedDraft = $state<NodeDraft | undefined>(undefined);
  let savedNotice = $state('');
  let ownerDraftId = $state('');
  let savedBreadcrumb = $state<BreadcrumbItem[] | undefined>(undefined);
  let savedParentTitle = $state<string | undefined>(undefined);
  async function resolveParentPath(id: string) {
    try {
      const page = await readDiscovery(apiBase, 1, '', AbortSignal.timeout(10000), id);
      const parent = page.items.map(discoveryCard).find((item) => item?.id === id);
      if (parent) return { breadcrumb: parent.breadcrumb, parentTitle: parent.title };
    } catch {
      /* Keep the draft usable if the ancestor is unavailable. */
    }
    return null;
  }
  // Restore private data only from the current participant’s browser draft collection.
  function sessionChanged(id: string | null) {
    participantId = id;
    hasDraft = !!id;
    savedDraft = undefined;
    savedBreadcrumb = undefined;
    savedParentTitle = undefined;
    ownerDraftId = '';
    if (id && draftId) {
      const found = listDrafts(id).find((item) => item.id === draftId);
      if (found) {
        savedDraft = found.data;
        ownerDraftId = found.id;
        savedBreadcrumb = found.breadcrumb;
        savedParentTitle = found.parentTitle;
        if (found.data.parentId && !found.parentTitle)
          void resolveParentPath(found.data.parentId).then((result) => {
            if (result && participantId === id) {
              savedBreadcrumb = result.breadcrumb;
              savedParentTitle = result.parentTitle;
            }
          });
      }
    }
  }
  // Save a titled draft locally; return success so a parent can decide whether it is safe to close.
  async function savePrivateDraft(): Promise<boolean> {
    if (!participantId || !currentDraft) return false;
    if (!currentDraft.title.trim()) {
      savedNotice = 'Enter a title before saving your draft.';
      return false;
    }
    try {
      const id = ownerDraftId || crypto.randomUUID();
      let ancestry = {
        breadcrumb: savedBreadcrumb ?? breadcrumb,
        parentTitle: savedParentTitle ?? parentTitle,
      };
      if (currentDraft.parentId && !ancestry.parentTitle)
        ancestry = (await resolveParentPath(currentDraft.parentId)) ?? ancestry;
      putDraft(participantId, {
        id,
        ownerId: participantId,
        updatedAt: new Date().toISOString(),
        data: currentDraft,
        breadcrumb: ancestry.breadcrumb,
        parentTitle: ancestry.parentTitle,
      });
      ownerDraftId = id;
      savedNotice = 'Saved privately in this browser. Open My drafts to find it.';
      return true;
    } catch {
      savedNotice = 'Draft could not be saved in this browser.';
      return false;
    }
  }
  // Expose the private-save function to the parent after mounting; registering it does not save or close yet.
  onMount(() => {
    onRegisterClose?.(savePrivateDraft);
  });
  const signal = () => AbortSignal.any([controller.signal, AbortSignal.timeout(15000)]);
  async function load() {
    loading = true;
    error = '';
    controller?.abort();
    controller = new AbortController();
    try {
      types = await readTypes(apiBase, signal());
    } catch {
      if (!controller.signal.aborted) error = 'Could not load node types.';
    } finally {
      loading = false;
    }
  }
  onMount(() => {
    void load();
    return () => controller?.abort();
  });
  const openCreated = () => {
    if (onCreated) onCreated(createdId);
    else window.location.assign(`/nodes?id=${encodeURIComponent(createdId)}`);
  };
  // Custom-type creation and node creation are separate writes. Reuse a saved type on an unchanged retry.
  async function save(draft: NodeDraft) {
    if (!participantId || submitting || createdId) return;
    submitting = true;
    saveError = '';
    try {
      let typeId = draft.typeId;
      if (draft.newType) {
        const key = JSON.stringify(draft.newType);
        if (createdType?.key === key) typeId = createdType.id;
        else {
          const type = await commentRequest<NodeType>(
            sessionBase,
            'catalog/node-types',
            'POST',
            { ...draft.newType, autoPluralize: draft.newType.autoPluralize ?? true },
            signal(),
          );
          if (!type.id) throw new Error('The type response did not contain an ID.');
          createdType = { key, id: type.id };
          typeId = type.id;
        }
      }
      if (!typeId) throw new Error('Select a node type.');
      const result = await commentRequest<{ id: string }>(
        sessionBase,
        'nodes',
        'POST',
        {
          title: draft.title,
          shortDescription: draft.shortDescription,
          markdown: draft.markdown,
          typeId,
          parentId: draft.parentId ?? null,
          requestedTypeIds: draft.requestedTypeIds,
        },
        signal(),
      );
      if (!result.id) throw new Error('The node response did not contain an ID.');
      if (!controller.signal.aborted) {
        createdId = result.id;
        if (participantId && ownerDraftId) deleteDraft(participantId, ownerDraftId);
      }
    } catch (cause) {
      if (!controller.signal.aborted) {
        saveError =
          cause instanceof Error
            ? cause.message
            : 'Could not create node. Your draft has been kept.';
        if (cause instanceof CommentRequestError && cause.status === 401) {
          participantId = null;
          window.dispatchEvent(new Event('atlas-session-changed'));
        }
      }
    } finally {
      submitting = false;
    }
    if (createdId) openCreated();
  }
</script>

<section
  class="live-node-form"
  aria-label={parentId ? 'Child node creation' : 'Root node creation'}
>
  {#if createdId}<p role="status">Node created.</p>
    <button onclick={openCreated}>Open created node</button>
  {:else}
    <SessionControl
      {sessionBase}
      hideWhenSignedIn
      prompt={parentId ? 'Sign in to add a child node' : 'Sign in to create a node'}
      expanded
      {focusOnMount}
      onSession={sessionChanged}
    />
    {#if loading}<p role="status">Loading node types…</p>{:else if error}<p role="alert">{error}</p>
      <button onclick={() => void load()}>Retry node types</button>{:else if hasDraft}
      <fieldset class="draft-fields" disabled={submitting || !participantId}>
        <NodeForm
          {types}
          parentId={savedDraft?.parentId ?? parentId}
          parentTitle={savedParentTitle ?? parentTitle}
          {initialTypeId}
          breadcrumb={savedBreadcrumb ?? breadcrumb}
          initialDraft={savedDraft}
          {focusOnMount}
          {submitting}
          onPreview={(draft) => {
            currentDraft = draft;
            onDraftChange?.(draft);
          }}
          onSubmit={save}
        />
      </fieldset>
      {#if participantId}<button type="button" onclick={savePrivateDraft} disabled={submitting}
          >Save private draft</button
        > <a href="/my-drafts">My drafts</a>{/if}
      {#if savedNotice}<p role="status">{savedNotice}</p>{/if}
    {/if}
    {#if saveError}<p role="alert">{saveError} Your draft has been kept.</p>{/if}
    {#if createdType}<p class="note">
        Your custom type has been created. If node creation fails, retrying this unchanged type uses
        the same saved type.
      </p>{/if}
  {/if}
</section>

<style>
  .live-node-form {
    max-width: 48rem;
    margin-inline: auto;
    min-width: 0;
  }
  .draft-fields {
    margin: 1rem 0 0;
    padding: 0;
    border: 0;
    min-width: 0;
  }
  button {
    font: inherit;
    cursor: pointer;
  }
  .note {
    color: var(--atlas-muted);
    font-size: 0.85rem;
  }
</style>
