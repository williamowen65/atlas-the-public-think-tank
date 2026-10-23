<script lang="ts">
  import SessionControl from '$lib/components/SessionControl.svelte';
  import { commentRequest } from '$lib/api/comments';
  let owner = $state<string | null>(null);
  let name = $state('');
  let bio = $state('');
  let initialName = $state('');
  let initialBio = $state('');
  let loading = $state(false);
  let saving = $state(false);
  let message = $state('');
  let error = $state('');
  let requestGeneration = 0;
  async function sessionChanged(id: string | null) {
    owner = id; name = ''; bio = ''; initialName = ''; initialBio = ''; message = ''; error = '';
    const generation = ++requestGeneration;
    if (!id) return;
    loading = true;
    try {
      const result = await commentRequest<{ displayName: string; bio: string }>('/atlas-public-api', 'participants/' + encodeURIComponent(id));
      if (generation !== requestGeneration) return;
      name = result.displayName; bio = result.bio ?? ''; initialName = name; initialBio = bio;
    } catch { if (generation === requestGeneration) error = 'Unable to load your account profile.'; }
    finally { if (generation === requestGeneration) loading = false; }
  }
  async function save() {
    if (!owner || saving || loading) return;
    const id = owner; saving = true; error = ''; message = '';
    try {
      const result = await commentRequest<{ displayName: string; bio: string }>('/atlas-session-api', 'participants/me', 'PUT', { displayName: name.trim(), bio });
      if (owner !== id) return;
      name = result.displayName; bio = result.bio ?? ''; initialName = name; initialBio = bio;
      message = 'Profile settings saved.';
    } catch (cause) { if (owner === id) error = cause instanceof Error ? cause.message : 'Could not save your settings.'; }
    finally { saving = false; }
  }
  function reset() { name = initialName; bio = initialBio; message = ''; error = ''; }
</script>
<svelte:head><title>Account settings · Atlas</title></svelte:head>
<section class="settings">
  <h1>Account settings</h1>
  <SessionControl expanded onSession={sessionChanged} />
  {#if owner}
    <h2>Public profile</h2>
    <p>Your display name and biography are visible to other Atlas participants. Your sign-in email is not part of your public profile.</p>
    {#if loading}<p role="status">Loading settings…</p>
    {:else if initialName}
      <form onsubmit={event => { event.preventDefault(); void save(); }}>
        <label for="settings-name">Display name</label>
        <input id="settings-name" bind:value={name} required maxlength="100" />
        <label for="settings-bio">Public biography</label>
        <textarea id="settings-bio" bind:value={bio} maxlength="500" rows="5"></textarea>
        <div class="actions"><button disabled={saving || !name.trim()}>{saving ? 'Saving…' : 'Save settings'}</button><button type="button" onclick={reset} disabled={saving}>Discard changes</button></div>
      </form>
    {/if}
    <h2>Account security</h2>
    <p>Password and email changes aren't available in this preview yet. Account registration and email confirmation continue to use the existing API workflow.</p>
    <p><a href="/me">View my profile</a> · <a href="/me/drafts">My private drafts</a></p>
  {:else}
    <p>Sign in to manage your account settings.</p>
  {/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if message}<p role="status">{message}</p>{/if}
</section>
<style>
  .settings{max-width:42rem;margin:2rem auto;min-width:0;}form{display:grid;gap:.6rem;}input,textarea{font:inherit;box-sizing:border-box;width:100%;padding:.65rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-text);}h2{margin-top:2rem;}.actions{display:flex;gap:.75rem;flex-wrap:wrap;margin-top:.5rem;}button{font:inherit;padding:.55rem .8rem;cursor:pointer;}
</style>
