<!--
  @component
  Community detail route with membership, owner editing and a community-filtered contribution
  feed.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { page } from '$app/state';
  import { untrack } from 'svelte';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
  import SessionControl from '$lib/components/SessionControl.svelte';
  import {
    communityDetail,
    communityAction,
    myCommunities,
    type Community,
  } from '$lib/api/communities';
  /**
   * Loaded details for the URL-selected community, or null until available.
   */
  let community = $state<Community | null>(null);
  /**
   * Current participant’s community records, used to establish ownership.
   */
  let mine = $state<Community[]>([]);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(true);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Whether a mutation is underway; prevents overlapping actions.
   */
  let busy = $state(false);
  /**
   * Whether the session lookup found a signed-in participant.
   */
  let signedIn = $state(false);
  /**
   * Whether the profile or community edit form is open.
   */
  let editing = $state(false);
  /**
   * Working community name while editing.
   */
  let name = $state('');
  /**
   * Working community description while editing.
   */
  let description = $state('');
  /**
   * Community ID derived from the route parameter.
   */
  const id = $derived(page.params.id);
  /**
   * Whether the current participant owns the displayed community.
   */
  const owned = $derived(
    community !== null &&
      mine.some((c) => c.id === community?.id && c.ownerParticipantId === currentParticipant),
  );
  /**
   * Current session’s participant ID for ownership comparison.
   */
  let currentParticipant = $state<string | null>(null);
  let routeVersion = 0;
  let refreshVersion = 0;

  async function refresh() {
    const communityId = id;
    const request = ++refreshVersion;
    const isCurrent = () => id === communityId && request === refreshVersion;
    try {
      const result = await communityDetail(communityId);
      if (!isCurrent()) return;
      let participant: string | null = null;
      let memberships: Community[] = [];
      let isMember = result.isMember;
      try {
        const session = await import('$lib/api/apiRequest').then((m) =>
          m.apiRequest<{ participantId: string | null }>('/atlas-session-api', 'auth/session'),
        );
        if (!isCurrent()) return;
        participant = session.participantId;
        if (participant) {
          try {
            memberships = await myCommunities();
            isMember = memberships.some((c) => c.id === communityId && c.isMember);
          } catch {
            /* Membership lookup failure does not invalidate authentication. */
          }
        }
      } catch {
        /* Public community details remain available without a session. */
      }
      if (!isCurrent()) return;
      currentParticipant = participant;
      signedIn = !!participant;
      mine = memberships;
      community = { ...result, isMember };
      name = result.name;
      description = result.description;
      error = '';
    } catch {
      if (isCurrent()) error = 'This community could not be loaded.';
    } finally {
      if (isCurrent()) loading = false;
    }
  }
  $effect(() => {
    if (!id) return;
    routeVersion++;
    community = null;
    mine = [];
    loading = true;
    editing = false;
    busy = false;
    error = '';
    untrack(() => void refresh());

    return () => {
      routeVersion++;
      refreshVersion++;
    };
  });
  async function action(path: string, method: 'POST' | 'PUT' | 'DELETE', body?: unknown) {
    if (busy) return;
    const communityId = id;
    const version = routeVersion;
    const isCurrent = () => id === communityId && routeVersion === version;
    busy = true;
    error = '';
    try {
      await communityAction('/' + encodeURIComponent(communityId) + path, method, body);
      if (isCurrent()) await refresh();
    } catch (cause) {
      if (isCurrent()) error = cause instanceof Error ? cause.message : 'Could not update community.';
    } finally {
      if (isCurrent()) busy = false;
    }
  }
  async function save() {
    if (!community || busy) return;
    const version = routeVersion;
    const communityId = id;
    const nextDescription = description.trim();
    const descriptionChanged = nextDescription !== community.description;
    if (name.trim() !== community.name) await action('/name', 'PUT', { text: name.trim() });
    if (id !== communityId || routeVersion !== version) return;
    if (descriptionChanged) await action('/description', 'PUT', { text: nextDescription });
    if (id === communityId && routeVersion === version) editing = false;
  }
</script>

<!--
  Layout: Back navigation and community identity precede membership/owner controls and the
  community-scoped contribution feed.
-->

<svelte:head><title>{community?.name ?? 'Community'} · Atlas</title></svelte:head>
<div class="community-page">
  <div class="community-opening">
    <a href="/communities">← All communities</a>
    {#if !loading && signedIn && community?.status === 'Active' && !owned}
      <button
        type="button"
        class="membership-action"
        aria-label={community.isMember ? 'Leave community' : 'Join community'}
        class:joined={community.isMember}
        disabled={busy}
        onclick={() => void action('/membership', community?.isMember ? 'DELETE' : 'PUT')}
      >{community.isMember ? 'Joined' : 'Join'}</button>
    {/if}
  </div>
  {#if loading}<p role="status">Loading community…</p>{:else if community}
    <header>
      <h1>{community.name}</h1>
      <p>{community.description}</p>
      <p class="stats">{community.memberCount} active members · {community.nodeCount} nodes</p>
      {#if community.status !== 'Active'}<p>This community is archived.</p>{/if}
      {#if signedIn && community.status === 'Active' && owned}
        <button onclick={() => (editing = !editing)}
          >{editing ? 'Cancel editing' : 'Edit community'}</button
        >
      {:else if !signedIn && community.status === 'Active'}<SessionControl
          prompt="Sign in to join this community"
          onSession={(participant) => {
            if (participant) void refresh();
          }}
        />{/if}
      {#if editing && owned}
        <form
          onsubmit={(event) => {
            event.preventDefault();
            void save();
          }}
        >
          <label>Name<input required maxlength="100" bind:value={name} /></label>
          <label
            >Description<textarea maxlength="1000" rows="4" bind:value={description}
            ></textarea></label
          >
          <button disabled={busy}>Save changes</button>
        </form>
      {/if}
    </header>
    <h2>Community contributions</h2>
    <LiveNodeFeed filters={{ communityId: id }} />
  {/if}
  {#if error}<p role="alert">{error} <button onclick={() => void refresh()}>Retry</button></p>{/if}
</div>

<style>
  .community-page {
    max-width: 66rem;
    margin: 1.5rem auto;
    padding: 0 1rem;
  }
  .community-page header {
    border-bottom: 1px solid var(--atlas-border);
    padding-bottom: 1.25rem;
    margin-bottom: 1.5rem;
  }
  .stats {
    color: var(--atlas-muted);
  }
  .community-page button {
    font: inherit;
    padding: 0.5rem 0.8rem;
    cursor: pointer;
  }
  .community-opening {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
  }
  .community-page .membership-action {
    min-height: 2.25rem;
    padding: 0.25rem 0.85rem;
    border: 1px solid var(--atlas-accent);
    border-radius: 999px;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    font-size: 0.875rem;
    font-weight: 600;
  }
  .community-page .membership-action.joined {
    background: var(--atlas-surface);
    color: var(--atlas-accent);
  }
  .membership-action:disabled {
    opacity: 0.6;
    cursor: default;
  }
  form {
    display: grid;
    gap: 0.8rem;
    max-width: 36rem;
    margin-top: 1rem;
  }
  label {
    display: grid;
    gap: 0.3rem;
  }
  input,
  textarea {
    font: inherit;
    padding: 0.5rem;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
  }
  @media (max-width: 560px) {
    .community-page {
      padding-inline: 0;
    }
  }
</style>
