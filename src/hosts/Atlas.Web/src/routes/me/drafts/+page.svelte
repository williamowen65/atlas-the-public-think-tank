<script lang="ts">
  import { goto } from '$app/navigation';
  import SessionControl from '$lib/components/SessionControl.svelte';
  import { listDrafts, deleteDraft, type SavedDraft } from '$lib/api/privateDrafts';
  let owner = $state<string | null>(null);
  let drafts = $state<SavedDraft[]>([]);
  let confirmId = $state('');
  function sessionChanged(id: string | null) { owner = id; drafts = id ? listDrafts(id) : []; confirmId = ''; }
  function remove(id: string) {
    if (!owner) return;
    try { deleteDraft(owner, id); drafts = listDrafts(owner); confirmId = ''; }
    catch { message = 'Could not delete this draft.'; }
  }
  let message = $state('');
</script>
<svelte:head><title>My drafts · Atlas</title></svelte:head>
<section class="drafts">
  <h1>My drafts</h1>
  <p>These private, unpublished drafts are saved only in this browser on this device, under your Atlas account. They are not synchronized with the server or other devices. Anyone with access to this browser's local data may be able to read them.</p>
  <SessionControl expanded onSession={sessionChanged} />
  {#if owner}
    <p><a href="/new">Create a new node</a></p>
    {#if message}<p role="alert">{message}</p>{/if}
    {#if drafts.length === 0}<p>You have no saved drafts in this browser.</p>{/if}
    <ul>
      {#each drafts as draft (draft.id)}
        <li>
          <h2>{draft.data.title || 'Untitled draft'}</h2>
          <p>{draft.data.shortDescription || 'No description yet.'}</p>
          <small>Saved {new Date(draft.updatedAt).toLocaleString()}</small>
          <div class="actions">
            <button onclick={() => void goto('/new?draft=' + encodeURIComponent(draft.id))}>Continue editing</button>
            {#if confirmId === draft.id}
              <span>Delete this draft permanently from this browser?</span>
              <button onclick={() => remove(draft.id)}>Confirm delete</button>
              <button onclick={() => confirmId = ''}>Cancel</button>
            {:else}
              <button onclick={() => confirmId = draft.id}>Delete</button>
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
  .drafts{max-width:52rem;margin:2rem auto;min-width:0;}ul{padding:0;list-style:none;display:grid;gap:1rem;}li{padding:1rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);}h2{margin:.25rem 0;}small{color:var(--atlas-muted);}.actions{display:flex;align-items:center;flex-wrap:wrap;gap:.6rem;margin-top:1rem;}button{font:inherit;cursor:pointer;padding:.5rem .75rem;}
</style>
