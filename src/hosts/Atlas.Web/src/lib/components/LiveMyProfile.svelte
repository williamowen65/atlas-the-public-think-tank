<script lang="ts">
  import { onMount } from 'svelte';
  import LiveProfilePage from './LiveProfilePage.svelte';
  import SessionControl from './SessionControl.svelte';
  import { commentRequest } from '../api/comments';
  let { apiBase = '/atlas-preview-api', sessionBase = '/atlas-comments-api', onOpenNode, onOpenProfile }: { apiBase?: string; sessionBase?: string; onOpenNode?: (id: string) => void; onOpenProfile?: (id: string) => void } = $props();
  let participantId = $state<string | null>(null);
  let profile = $state<{ displayName: string; bio: string } | null>(null);
  let displayName = $state('');
  let bio = $state('');
  let editing = $state(false);
  let saving = $state(false);
  let notice = $state('');
  async function load(id: string) {
    try {
      const result = await commentRequest<{ displayName: string; bio: string }>(apiBase, 'participants/' + encodeURIComponent(id));
      profile = result; displayName = result.displayName; bio = result.bio ?? '';
    } catch { notice = 'Your public profile could not be loaded.'; }
  }
  function sessionChanged(id: string | null) {
    participantId = id; profile = null; editing = false; notice = '';
    if (id) void load(id);
  }
  async function save() {
    if (!participantId || saving) return;
    saving = true; notice = '';
    try {
      await commentRequest(sessionBase, 'participants/me', 'PUT', { displayName, bio });
      editing = false; notice = 'Profile saved.';
      await load(participantId);
    } catch { notice = 'Profile changes could not be saved. Please try again.'; }
    finally { saving = false; }
  }
</script>
<section class="my-profile">
  <h1>My profile</h1>
  <SessionControl expanded onSession={sessionChanged} {sessionBase} />
  {#if participantId}
    {#if profile}
      <button type="button" onclick={() => { editing = !editing; notice = ''; }}>{editing ? 'Cancel editing' : 'Edit my profile'}</button>
      {#if editing}
        <form onsubmit={event => { event.preventDefault(); void save(); }}>
          <label for="my-display-name">Display name</label>
          <input id="my-display-name" bind:value={displayName} maxlength="100" required />
          <label for="my-bio">Public biography</label>
          <textarea id="my-bio" bind:value={bio} maxlength="500" rows="5"></textarea>
          <button disabled={saving}>{saving ? 'Saving…' : 'Save changes'}</button>
        </form>
      {/if}
    {/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    <LiveProfilePage {participantId} {apiBase} {onOpenNode} {onOpenProfile} />
  {:else}
    <p>Sign in to view or edit your profile.</p>
  {/if}
</section>
<style lang="scss">
  .my-profile{max-width:70rem;margin:auto;}form{display:grid;gap:.65rem;max-width:36rem;margin:1rem 0;}input,textarea{font:inherit;padding:.65rem;background:var(--atlas-surface);color:var(--atlas-text);border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);}button{font:inherit;padding:.55rem .8rem;cursor:pointer;}
</style>