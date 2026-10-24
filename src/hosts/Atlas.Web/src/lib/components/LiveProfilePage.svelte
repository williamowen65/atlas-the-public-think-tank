<!--
  LiveProfilePage: Loads a public participant profile and their contribution feed.
  Public data is readable anonymously; edit controls depend on the current session and profile ownership.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import { commentRequest } from '../api/comments';
  import type { Community } from '../api/communities';
  let {
    participantId = '',
    apiBase = '/atlas-public-api',
    onOpenNode,
    onOpenProfile,
  }: {
    participantId?: string;
    apiBase?: string;
    onOpenNode?: (id: string) => void;
    onOpenProfile?: (id: string) => void;
  } = $props();
  type Profile = {
    id: string;
    displayName: string;
    bio: string;
    createdAt?: string;
    deleted?: boolean;
  };
  let profile = $state<Profile | null>(null);
  let loading = $state(true);
  let error = $state('');
  let sessionId = $state<string | null>(null);
  let sessionReady = $state(false);
  let editing = $state(false);
  let saving = $state(false);
  let notice = $state('');
  let displayName = $state('');
  let bio = $state('');
  let communities = $state<Community[]>([]);
  const isOwner = $derived(!!participantId && sessionId === participantId);
  const ownedCommunities = $derived(
    communities.filter((c) => c.ownerParticipantId === profile?.id),
  );
  const joinedCommunities = $derived(
    communities.filter((c) => c.isMember && c.ownerParticipantId !== profile?.id),
  );
  // Load the public profile first; unavailable community data should not hide that profile.
  async function loadProfile(id: string, signal: AbortSignal) {
    loading = true;
    error = '';
    profile = null;
    communities = [];
    try {
      const result = await commentRequest<Profile>(
        apiBase,
        'participants/' + encodeURIComponent(id),
        'GET',
        undefined,
        signal,
      );
      if (signal.aborted) return;
      profile = result;
      displayName = result.displayName;
      bio = result.bio ?? '';
      try {
        const associated = await commentRequest<Community[]>(
          apiBase,
          'communities/participant/' + encodeURIComponent(id),
          'GET',
          undefined,
          signal,
        );
        if (!signal.aborted) communities = associated;
      } catch {
        /* A missing community service should not hide the profile. */
      }
    } catch {
      if (!signal.aborted) error = 'Could not load this public profile.';
    } finally {
      if (!signal.aborted) loading = false;
    }
  }
  // Session identity controls owner-edit affordances; it is independent of the public profile being viewed.
  async function checkSession() {
    try {
      const session = await commentRequest<{ participantId: string | null }>(
        '/atlas-session-api',
        'auth/session',
      );
      sessionId = session.participantId;
    } catch {
      sessionId = null;
    } finally {
      sessionReady = true;
    }
  }
  onMount(() => {
    void checkSession();
    const changed = () => void checkSession();
    window.addEventListener('atlas-session-changed', changed);
    return () => window.removeEventListener('atlas-session-changed', changed);
  });
  // Changing the participant cancels the previous lookup and clears stale editing state.
  $effect(() => {
    const id = participantId;
    editing = false;
    notice = '';
    if (!id) {
      profile = null;
      loading = false;
      return;
    }
    const controller = new AbortController();
    void loadProfile(id, controller.signal);
    return () => controller.abort();
  });
  async function save() {
    if (!isOwner || saving || !profile) return;
    saving = true;
    notice = '';
    try {
      await commentRequest('/atlas-session-api', 'participants/me', 'PUT', { displayName, bio });
      profile = { ...profile, displayName, bio };
      editing = false;
      notice = 'Profile saved.';
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      notice = 'Profile changes could not be saved. Please try again.';
    } finally {
      saving = false;
    }
  }
</script>

<section class="profile-page" aria-label="Public participant profile">
  {#if !participantId}<p>Select a participant to view their public profile.</p>{/if}
  {#if loading && !profile}<p role="status">Loading profile…</p>{/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if profile}
    <header class="profile-header">
      <div class="avatar" aria-hidden="true">{profile.displayName.charAt(0).toUpperCase()}</div>
      <div class="profile-details">
        <div class="profile-title">
          <h1>{profile.displayName}</h1>
          {#if isOwner && !profile.deleted}<button
              type="button"
              class="edit-profile"
              aria-label={editing ? 'Cancel editing' : 'Edit my profile'}
              title={editing ? 'Cancel editing' : 'Edit my profile'}
              onclick={() => {
                editing = !editing;
                notice = '';
              }}>✎</button
            >{/if}
        </div>
        {#if profile.createdAt}<p class="muted">
            Member since {new Date(profile.createdAt).toLocaleDateString()}
          </p>{/if}
        <p>
          {profile.bio ||
            (profile.deleted ? 'This account has been deleted.' : 'No public bio yet.')}
        </p>
        {#if ownedCommunities.length}<p class="community-line">
            <strong>Community owner of:</strong>
            {#each ownedCommunities as community, i (community.id)}{#if i > 0},
              {/if}<a href={'/communities/' + community.id}>{community.name}</a>{/each}
          </p>{/if}
        {#if joinedCommunities.length}<p class="community-line">
            <strong>Member of:</strong>
            {#each joinedCommunities as community, i (community.id)}{#if i > 0},
              {/if}<a href={'/communities/' + community.id}>{community.name}</a>{/each}
          </p>{/if}
      </div>
    </header>
    {#if editing && isOwner}<form
        onsubmit={(event) => {
          event.preventDefault();
          void save();
        }}
      >
        <label for="profile-display-name">Display name</label><input
          id="profile-display-name"
          bind:value={displayName}
          maxlength="100"
          required
        /><label for="profile-bio">Public biography</label><textarea
          id="profile-bio"
          bind:value={bio}
          maxlength="500"
          rows="5"></textarea><button disabled={saving}
          >{saving ? 'Saving…' : 'Save changes'}</button
        >
      </form>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if !profile.deleted}
      <h2>Published contributions</h2>
      {#key profile.id}<LiveNodeFeed
          authorId={profile.id}
          {apiBase}
          {onOpenNode}
          {onOpenProfile}
        />{/key}
    {/if}
  {/if}
</section>

<style lang="scss">
  .profile-page {
    max-width: 70rem;
    margin-inline: auto;
    min-width: 0;
  }
  button {
    font: inherit;
    padding: 0.45rem;
    max-width: 100%;
    cursor: pointer;
  }
  .profile-title {
    display: flex;
    align-items: center;
    gap: 0.7rem;
  }
  .edit-profile {
    border: 1px solid var(--atlas-border);
    border-radius: 0.4rem;
    background: transparent;
    color: var(--atlas-text);
  }
  .profile-details {
    min-width: 0;
    flex: 1;
  }
  form {
    display: grid;
    gap: 0.65rem;
    max-width: 36rem;
    margin: 1rem 0;
  }
  input,
  textarea {
    font: inherit;
    padding: 0.65rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  .community-line {
    margin: 0.35rem 0;
    font-size: 0.9rem;
    line-height: 1.5;
    overflow-wrap: anywhere;
  }
  .community-line a {
    color: inherit;
    text-decoration: none;
  }
  .community-line a:hover,
  .community-line a:focus-visible {
    text-decoration: underline;
  }
  .profile-header {
    display: flex;
    align-items: flex-start;
    gap: 1.25rem;
    padding: 1.5rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
  }
  .avatar {
    display: grid;
    place-items: center;
    flex: 0 0 4rem;
    height: 4rem;
    border-radius: 50%;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    font-size: 2rem;
    font-weight: 700;
  }
  h1 {
    margin: 0;
  }
  h2 {
    margin: 2rem 0 1rem;
  }
  .muted {
    color: var(--atlas-muted);
    font-size: 0.9rem;
  }
  @media (max-width: 450px) {
    .profile-header {
      padding: 0.8rem;
      gap: 0.75rem;
    }
    .avatar {
      flex-basis: 3rem;
      height: 3rem;
      font-size: 1.4rem;
    }
  }
</style>
