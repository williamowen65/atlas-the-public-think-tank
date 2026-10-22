<script lang="ts">
  import { onMount } from 'svelte';
  import { putDraft, deleteDraft, listDrafts } from '../api/privateDrafts';
  import NodeForm, { type NodeDraft } from './NodeForm.svelte';
  import SessionControl from './SessionControl.svelte';
  import type { BreadcrumbItem } from './Breadcrumb.svelte';
  import { readTypes, type NodeType } from '../api/nodePage';
  import { commentRequest, CommentRequestError } from '../api/comments';
  import { readDiscovery, discoveryCard } from '../api/discovery';
  let { draftId, parentId, parentTitle, initialTypeId, breadcrumb = [], focusOnMount = false, apiBase = '/atlas-preview-api', sessionBase = '/atlas-comments-api', onCreated }: { draftId?: string; parentId?: string; parentTitle?: string; initialTypeId?: string; breadcrumb?: BreadcrumbItem[]; focusOnMount?: boolean; apiBase?: string; sessionBase?: string; onCreated?: (id: string) => void } = $props();
  let types = $state<NodeType[]>([]); let error = $state(''); let loading = $state(true);
  let participantId = $state<string | null>(null); let hasDraft = $state(false); let submitting = $state(false); let saveError = $state(''); let createdId = $state(''); let createdType = $state<{ key: string; id: string } | null>(null);
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
      const parent = page.items.map(discoveryCard).find(item => item?.id === id);
      if (parent) return { breadcrumb: parent.breadcrumb, parentTitle: parent.title };
    } catch { /* Keep the draft usable if the ancestor is unavailable. */ }
    return null;
  }
  function sessionChanged(id: string | null) {
    participantId = id; hasDraft = !!id; savedDraft = undefined; savedBreadcrumb = undefined; savedParentTitle = undefined; ownerDraftId = '';
    if (id && draftId) { const found = listDrafts(id).find(item => item.id === draftId); if (found) { savedDraft = found.data; ownerDraftId = found.id; savedBreadcrumb = found.breadcrumb; savedParentTitle = found.parentTitle;
      if (found.data.parentId && !found.parentTitle) void resolveParentPath(found.data.parentId).then(result => { if (result && participantId === id) { savedBreadcrumb = result.breadcrumb; savedParentTitle = result.parentTitle; } }); } }
  }
  async function savePrivateDraft() {
    if (!participantId || !currentDraft) return;
    if (!currentDraft.title.trim()) { savedNotice = 'Enter a title before saving your draft.'; return; }
    try {
      const id = ownerDraftId || crypto.randomUUID();
      let ancestry = { breadcrumb: savedBreadcrumb ?? breadcrumb, parentTitle: savedParentTitle ?? parentTitle };
      if (currentDraft.parentId && !ancestry.parentTitle) ancestry = await resolveParentPath(currentDraft.parentId) ?? ancestry;
      putDraft(participantId, { id, ownerId: participantId, updatedAt: new Date().toISOString(), data: currentDraft, breadcrumb: ancestry.breadcrumb, parentTitle: ancestry.parentTitle });
      ownerDraftId = id; savedNotice = 'Saved privately in this browser. Open My drafts to find it.';
    }
    catch { savedNotice = 'Draft could not be saved in this browser.'; }
  }
  const signal = () => AbortSignal.any([controller.signal, AbortSignal.timeout(15000)]);
  async function load() { loading = true; error = ''; controller?.abort(); controller = new AbortController(); try { types = await readTypes(apiBase, signal()); } catch { if (!controller.signal.aborted) error = 'Could not load node types.'; } finally { loading = false; } }
  onMount(() => { void load(); return () => controller?.abort(); });
  const openCreated = () => { if (onCreated) onCreated(createdId); else window.location.assign(`/nodes?id=${encodeURIComponent(createdId)}`); };
  async function save(draft: NodeDraft) {
    if (!participantId || submitting || createdId) return;
    submitting = true; saveError = '';
    try {
      let typeId = draft.typeId;
      if (draft.newType) {
        const key = JSON.stringify(draft.newType);
        if (createdType?.key === key) typeId = createdType.id;
        else {
          const type = await commentRequest<NodeType>(sessionBase, 'catalog/node-types', 'POST', { ...draft.newType, autoPluralize: draft.newType.autoPluralize ?? true }, signal());
          if (!type.id) throw new Error('The type response did not contain an ID.');
          createdType = { key, id: type.id }; typeId = type.id;
        }
      }
      if (!typeId) throw new Error('Select a node type.');
      const result = await commentRequest<{ id: string }>(sessionBase, 'nodes', 'POST', { title: draft.title, shortDescription: draft.shortDescription, markdown: draft.markdown, typeId, parentId: draft.parentId ?? null, requestedTypeIds: draft.requestedTypeIds }, signal());
      if (!result.id) throw new Error('The node response did not contain an ID.');
      if (!controller.signal.aborted) { createdId = result.id; if (participantId && ownerDraftId) deleteDraft(participantId, ownerDraftId); }
    } catch (cause) {
      if (!controller.signal.aborted) {
        saveError = cause instanceof Error ? cause.message : 'Could not create node. Your draft has been kept.';
        if (cause instanceof CommentRequestError && cause.status === 401) { participantId = null; window.dispatchEvent(new Event('atlas-session-changed')); }
      }
    } finally { submitting = false; }
    if (createdId) openCreated();
  }
</script>
<section class="live-node-form" aria-label={parentId ? 'Child node creation' : 'Root node creation'}>
  {#if createdId}<p role="status">Node created.</p><button onclick={openCreated}>Open created node</button>
  {:else}
    <SessionControl {sessionBase} hideWhenSignedIn prompt={parentId ? 'Sign in to add a child node' : 'Sign in to create a node'} expanded {focusOnMount} onSession={sessionChanged} />
    {#if loading}<p role="status">Loading node types…</p>{:else if error}<p role="alert">{error}</p><button onclick={() => void load()}>Retry node types</button>{:else if hasDraft}
      <fieldset class="draft-fields" disabled={submitting || !participantId}><NodeForm {types} parentId={savedDraft?.parentId ?? parentId} parentTitle={savedParentTitle ?? parentTitle} {initialTypeId} breadcrumb={savedBreadcrumb ?? breadcrumb} initialDraft={savedDraft} focusOnMount {submitting} onPreview={draft => { currentDraft = draft; }} onSubmit={save} /></fieldset>
      {#if participantId}<button type="button" onclick={savePrivateDraft} disabled={submitting}>Save private draft</button> <a href="/me/drafts">My drafts</a>{/if}
      {#if savedNotice}<p role="status">{savedNotice}</p>{/if}
    {/if}
    {#if saveError}<p role="alert">{saveError} Your draft has been kept.</p>{/if}
    {#if createdType}<p class="note">Your custom type has been created. If node creation fails, retrying this unchanged type uses the same saved type.</p>{/if}
  {/if}
</section>
<style>.live-node-form{max-width:48rem;margin-inline:auto;min-width:0;}.draft-fields{margin:1rem 0 0;padding:0;border:0;min-width:0;}button{font:inherit;cursor:pointer;}.note{color:var(--atlas-muted);font-size:.85rem;}</style>
