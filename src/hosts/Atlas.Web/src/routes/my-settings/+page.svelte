<!--
  @component
  Session-aware public profile settings with save/discard actions and account-security
  availability notes.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import VotingWeightControl from '$lib/components/VotingWeightControl.svelte';
  import { watchSearchPreferences } from '$lib/state/searchPreferences.svelte';
  onMount(watchSearchPreferences);
  import SessionControl from '$lib/components/SessionControl.svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  /**
   * Signed-in participant ID used to scope account-specific data; null shows the sign-in
   * prompt.
   */
  let owner = $state<string | null>(null);
  /**
   * Editable public display name.
   */
  let name = $state('');
  /**
   * Editable public biography, separate from the fetched profile until saved.
   */
  let bio = $state('');
  /**
   * Last loaded/saved display name used by Discard changes.
   */
  let initialName = $state('');
  /**
   * Last loaded/saved biography used by Discard changes.
   */
  let initialBio = $state('');
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(false);
  /**
   * Whether a save is underway; locks the relevant form against duplicate submission.
   */
  let saving = $state(false);
  /**
   * Successful settings-save feedback.
   */
  let message = $state('');
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  let requestGeneration = 0;
  async function sessionChanged(id: string | null) {
    owner = id;
    name = '';
    bio = '';
    initialName = '';
    initialBio = '';
    message = '';
    error = '';
    const generation = ++requestGeneration;
    if (!id) return;
    loading = true;
    try {
      const result = await apiRequest<{ displayName: string; bio: string }>(
        '/atlas-public-api',
        'participants/' + encodeURIComponent(id),
      );
      if (generation !== requestGeneration) return;
      name = result.displayName;
      bio = result.bio ?? '';
      initialName = name;
      initialBio = bio;
    } catch {
      if (generation === requestGeneration) error = 'Unable to load your account profile.';
    } finally {
      if (generation === requestGeneration) loading = false;
    }
  }
  async function save() {
    if (!owner || saving || loading) return;
    const id = owner;
    saving = true;
    error = '';
    message = '';
    try {
      const result = await apiRequest<{ displayName: string; bio: string }>(
        '/atlas-session-api',
        'participants/me',
        'PUT',
        { displayName: name.trim(), bio },
      );
      if (owner !== id) return;
      name = result.displayName;
      bio = result.bio ?? '';
      initialName = name;
      initialBio = bio;
      message = 'Profile settings saved.';
    } catch (cause) {
      if (owner === id)
        error = cause instanceof Error ? cause.message : 'Could not save your settings.';
    } finally {
      saving = false;
    }
  }
  function reset() {
    name = initialName;
    bio = initialBio;
    message = '';
    error = '';
  }
</script>

<!--
  Layout: Session controls gate public-profile editing. Security availability and links follow,
  while success/error messages stay outside the signed-in branch.
-->

<svelte:head><title>Account settings · Atlas</title></svelte:head>
<section class="settings">
  <h1>Account settings</h1>
  <h2>Search preferences</h2>
  <VotingWeightControl />
  <SessionControl expanded onSession={sessionChanged} />
  {#if owner}
    <h2>Public profile</h2>
    <p>
      Your display name and biography are visible to other Atlas participants. Your sign-in email is
      not part of your public profile.
    </p>
    {#if loading}<p role="status">Loading settings…</p>
    {:else if initialName}
      <form
        onsubmit={(event) => {
          event.preventDefault();
          void save();
        }}
      >
        <label for="settings-name">Display name</label>
        <input id="settings-name" bind:value={name} required maxlength="100" />
        <label for="settings-bio">Public biography</label>
        <textarea id="settings-bio" bind:value={bio} maxlength="500" rows="5"></textarea>
        <div class="actions">
          <button disabled={saving || !name.trim()}>{saving ? 'Saving…' : 'Save settings'}</button
          ><button type="button" onclick={reset} disabled={saving}>Discard changes</button>
        </div>
      </form>
    {/if}
    <h2>Account security</h2>
    <p>
      Password and email changes aren't available in this preview yet. Account registration and
      email confirmation continue to use the existing API workflow.
    </p>
    <p><a href="/me">View my profile</a> · <a href="/my-drafts">My private drafts</a></p>
  {:else}
    <p>Sign in to manage your account settings.</p>
  {/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if message}<p role="status">{message}</p>{/if}
</section>

<style>
  .settings {
    max-width: 42rem;
    margin: 2rem auto;
    min-width: 0;
  }
  form {
    display: grid;
    gap: 0.6rem;
  }
  input,
  textarea {
    font: inherit;
    box-sizing: border-box;
    width: 100%;
    padding: 0.65rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  h2 {
    margin-top: 2rem;
  }
  .actions {
    display: flex;
    gap: 0.75rem;
    flex-wrap: wrap;
    margin-top: 0.5rem;
  }
  button {
    font: inherit;
    padding: 0.55rem 0.8rem;
    cursor: pointer;
  }
</style>
