<script lang="ts">
  import { onMount } from 'svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import { commentRequest } from '../api/comments';
  let { participantId = '', apiBase = '/atlas-preview-api', onOpenNode }: { participantId?: string; apiBase?: string; onOpenNode?: (id: string) => void } = $props();
  type Profile = { id: string; displayName: string; bio: string; createdAt?: string; deleted?: boolean };
  let profile = $state<Profile | null>(null);
  let profiles = $state<Profile[]>([]);
  let selected = $state(participantId);
  let loading = $state(true);
  let error = $state('');
  let page = $state(0);
  let hasMore = $state(false);
  const currentId = $derived(selected);
  async function loadProfile(id: string) {
    if (!id) return;
    loading = true; error = ''; profile = null;
    try { profile = await commentRequest<Profile>(apiBase, 'participants/' + encodeURIComponent(id)); }
    catch { error = 'Could not load this public profile.'; }
    finally { loading = false; }
  }
  async function loadDirectory(reset = false) {
    try {
      const offset = reset ? 0 : page * 20;
      const batch = await commentRequest<Profile[]>(apiBase, 'participants?offset=' + offset + '&size=20');
      profiles = reset ? batch : [...profiles, ...batch];
      page = reset ? 1 : page + 1;
      hasMore = batch.length === 20;
      if (!currentId && profiles.length) selected = profiles[0].id;
    } catch { error = 'Could not load public participants.'; }
    finally { loading = false; }
  }
  onMount(() => { void loadDirectory(true); });
  $effect(() => { if (currentId) void loadProfile(currentId); });
</script>
<section class="profile-page" aria-label="Public participant profile">
  <nav class="profile-navigation" aria-label="Profile navigation">
    <label for="participant-picker">View a participant</label>
    <select id="participant-picker" value={currentId} onchange={event => { selected = event.currentTarget.value; }}>
      {#if currentId && !profiles.some(p => p.id === currentId)}<option value={currentId}>Current profile</option>{/if}
      {#each profiles as participant}<option value={participant.id}>{participant.displayName}</option>{/each}
    </select>
    {#if hasMore}<button type="button" onclick={() => void loadDirectory()}>Load more participants</button>{/if}
  </nav>
  {#if loading && !profile}<p role="status">Loading profile…</p>{/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if profile}
    <header class="profile-header">
      <div class="avatar" aria-hidden="true">{profile.displayName.charAt(0).toUpperCase()}</div>
      <div><h1>{profile.displayName}</h1>
        {#if profile.createdAt}<p class="muted">Member since {new Date(profile.createdAt).toLocaleDateString()}</p>{/if}
        <p>{profile.bio || (profile.deleted ? 'This account has been deleted.' : 'No public bio yet.')}</p>
      </div>
    </header>
    {#if !profile.deleted}
      <h2>Published contributions</h2>
      {#key profile.id}<LiveNodeFeed authorId={profile.id} {apiBase} {onOpenNode} />{/key}
    {/if}
  {/if}
</section>
<style lang="scss">
  .profile-page{max-width:70rem;margin-inline:auto;min-width:0;}
  .profile-navigation{display:flex;flex-wrap:wrap;align-items:center;gap:.75rem;margin:0 0 1.25rem;}
  select,button{font:inherit;padding:.45rem;max-width:100%;}
  .profile-header{display:flex;align-items:flex-start;gap:1.25rem;padding:1.5rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);}
  .avatar{display:grid;place-items:center;flex:0 0 4rem;height:4rem;border-radius:50%;background:var(--atlas-accent);color:var(--atlas-on-accent);font-size:2rem;font-weight:700;}
  h1{margin:0;}h2{margin:2rem 0 1rem;}.muted{color:var(--atlas-muted);font-size:.9rem;}
  @media(max-width:450px){.profile-header{padding:.8rem;gap:.75rem;}.avatar{flex-basis:3rem;height:3rem;font-size:1.4rem;}}
</style>