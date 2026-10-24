<!--
  @component
  Participant-scoped browser-local draft list with resume and delete actions; drafts are not
  synchronized to the API.
-->
<script lang="ts">
  import { goto } from '$app/navigation';
  import SessionControl from '$lib/components/SessionControl.svelte';
  import { listDrafts, deleteDraft, type SavedDraft } from '$lib/api/privateDrafts';
  import { readTypes, type NodeType } from '$lib/api/nodePage';
  import Breadcrumb, { type BreadcrumbItem } from '$lib/components/Breadcrumb.svelte';
  import { readDiscovery, discoveryCard, readOlderAncestors } from '$lib/api/discovery';
  /**
   * Signed-in participant ID used to scope account-specific data; null shows the sign-in
   * prompt.
   */
  let owner = $state<string | null>(null);
  /**
   * Draft records loaded from this participant’s browser-local storage.
   */
  let drafts = $state<SavedDraft[]>([]);
  /**
   * Draft ID awaiting deletion confirmation; empty text means no confirmation is open.
   */
  let confirmId = $state('');
  /**
   * Available non-archived node types used by labels, filters or creation choices.
   */
  let types = $state<NodeType[]>([]);
  /**
   * Resolved parent ancestry cached by draft ID.
   */
  let resolvedPaths = $state<Record<string, BreadcrumbItem[]>>({});
  /**
   * Whether each draft’s breadcrumb has older ancestors, keyed by draft ID.
   */
  let moreAncestors = $state<Record<string, boolean>>({});
  function loadOlder(draft: SavedDraft, oldestId?: string) {
    return readOlderAncestors(
      oldestId ?? draft.data.parentId!,
      AbortSignal.timeout(10000),
      '/atlas-public-api',
    );
  }
  $effect(() => {
    const entries = drafts.filter((draft) => draft.data.parentId && !draft.parentTitle);
    for (const entry of entries) {
      const parentId = entry.data.parentId!;
      if (resolvedPaths[entry.id]) continue;
      resolvedPaths[entry.id] = entry.breadcrumb ?? [];
      void readDiscovery('/atlas-public-api', 1, '', AbortSignal.timeout(10000), parentId)
        .then((page) => {
          const parent = page.items.map(discoveryCard).find((item) => item?.id === parentId);
          if (parent) {
            resolvedPaths[entry.id] = [...parent.breadcrumb, { id: parentId, title: parent.title }];
            moreAncestors[entry.id] = !!page.items.find((item) => item.nodeId === parentId)?.card
              ?.breadcrumbSegment?.continuationNodeId;
          }
        })
        .catch(() => {
          /* The parent may be unavailable or deleted. */
        });
    }
  });
  function draftPath(draft: SavedDraft): BreadcrumbItem[] {
    if (!draft.data.parentId) return [];
    return (
      resolvedPaths[draft.id] ?? [
        ...(draft.breadcrumb ?? []),
        { id: draft.data.parentId, title: draft.parentTitle ?? 'Parent node' },
      ]
    );
  }
  $effect(() => {
    if (owner) {
      void readTypes('/atlas-public-api', AbortSignal.timeout(10000))
        .then((result) => (types = result))
        .catch(() => {
          types = [];
        });
    } else types = [];
  });
  function typeName(draft: SavedDraft): string {
    if (draft.data.newType) return draft.data.newType.name || 'New node type';
    return (
      types.find((type) => type.id === draft.data.typeId)?.name ??
      (draft.data.typeId ? 'Node type unavailable' : 'No node type selected')
    );
  }
  function sessionChanged(id: string | null) {
    owner = id;
    drafts = id ? listDrafts(id) : [];
    resolvedPaths = {};
    moreAncestors = {};
    confirmId = '';
  }
  function remove(id: string) {
    if (!owner) return;
    try {
      deleteDraft(owner, id);
      drafts = listDrafts(owner);
      confirmId = '';
    } catch {
      message = 'Could not delete this draft.';
    }
  }
  /**
   * Draft load/save/delete feedback shown above the list.
   */
  let message = $state('');
</script>

<!--
  Layout: Storage explanation and session controls precede this participant’s local draft list,
  with resume and delete-confirmation actions per draft.
-->

<svelte:head><title>My drafts · Atlas</title></svelte:head>
<section class="drafts">
  <h1>My drafts</h1>
  <p>
    These private, unpublished drafts are saved only in this browser on this device, under your
    Atlas account. They are not synchronized with the server or other devices. Anyone with access to
    this browser's local data may be able to read them.
  </p>
  <SessionControl expanded onSession={sessionChanged} />
  {#if owner}
    <p><a href="/new">Create a new node</a></p>
    {#if message}<p role="alert">{message}</p>{/if}
    {#if drafts.length === 0}<p>You have no saved drafts in this browser.</p>{/if}
    <ul>
      {#each drafts as draft (draft.id)}
        <li>
          {#if draft.data.parentId}<div class="draft-breadcrumb">
              <Breadcrumb
                items={draftPath(draft)}
                hasMore={moreAncestors[draft.id] ?? false}
                loadOlder={(oldestId) => loadOlder(draft, oldestId)}
                onNavigate={(id) => void goto('/nodes?id=' + encodeURIComponent(id))}
              />
            </div>{/if}
          <h2>
            <span class="draft-type">{typeName(draft)}</span>
            <span class="draft-title">{draft.data.title || 'Untitled draft'}</span>
          </h2>
          <p>{draft.data.shortDescription || 'No description yet.'}</p>
          <small>Saved {new Date(draft.updatedAt).toLocaleString()}</small>
          <div class="actions">
            <button onclick={() => void goto('/new?draft=' + encodeURIComponent(draft.id))}
              >Continue editing</button
            >
            {#if confirmId === draft.id}
              <span>Delete this draft permanently from this browser?</span>
              <button onclick={() => remove(draft.id)}>Confirm delete</button>
              <button onclick={() => (confirmId = '')}>Cancel</button>
            {:else}
              <button onclick={() => (confirmId = draft.id)}>Delete</button>
            {/if}
          </div>
        </li>
      {/each}
    </ul>
  {:else}
    <p>Sign in to access drafts saved for your account on this device.</p>
  {/if}
</section>

<style>
  .drafts {
    width: 100%;
    max-width: 52rem;
    margin: 2rem auto;
    min-width: 0;
  }
  ul {
    padding: 0;
    list-style: none;
    display: grid;
    gap: 1rem;
    min-width: 0;
  }
  li {
    width: 100%;
    max-width: 48rem;
    min-width: 0;
    padding: 0 1rem 1rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    overflow: hidden;
  }
  .draft-breadcrumb {
    width: calc(100% + 2rem);
    max-width: calc(100% + 2rem);
    min-width: 0;
    margin: 0 -1rem 0.75rem;
    padding: 0.75rem 0 0;
    --atlas-breadcrumb-bottom-space: 0px;
  }
  h2 {
    margin: 0.25rem 0;
    display: flex;
    flex-wrap: wrap;
    align-items: baseline;
    gap: 0.5rem;
  }
  .draft-type {
    font-size: 0.8em;
    color: var(--atlas-muted);
    font-weight: 600;
  }
  .draft-title {
    font-weight: 700;
  }
  small {
    color: var(--atlas-muted);
  }
  .actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.6rem;
    margin-top: 1rem;
  }
  button {
    font: inherit;
    cursor: pointer;
    padding: 0.5rem 0.75rem;
  }
</style>
