<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/state';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
  import SessionControl from '$lib/components/SessionControl.svelte';
  import {
    communityDetail,
    communityAction,
    myCommunities,
    type Community,
  } from '$lib/api/communities';
  let community = $state<Community | null>(null);
  let mine = $state<Community[]>([]);
  let loading = $state(true);
  let error = $state('');
  let busy = $state(false);
  let signedIn = $state(false);
  let editing = $state(false);
  let name = $state('');
  let description = $state('');
  const id = $derived(page.params.id);
  const owned = $derived(
    community !== null &&
      mine.some((c) => c.id === community?.id && c.ownerParticipantId === currentParticipant),
  );
  let currentParticipant = $state<string | null>(null);
  async function refresh() {
    try {
      const result = await communityDetail(id);
      community = result;
      name = result.name;
      description = result.description;
      try {
        const session = await import('$lib/api/comments').then((m) =>
          m.commentRequest<{ participantId: string | null }>('/atlas-session-api', 'auth/session'),
        );
        currentParticipant = session.participantId;
        signedIn = !!session.participantId;
        if (signedIn) {
          try {
            mine = await myCommunities();
            community = { ...result, isMember: mine.some((c) => c.id === id && c.isMember) };
          } catch {
            /* Membership lookup failure does not invalidate authentication. */
          }
        }
      } catch {
        signedIn = false;
        currentParticipant = null;
      }
      error = '';
    } catch {
      error = 'This community could not be loaded.';
    } finally {
      loading = false;
    }
  }
  onMount(() => {
    void refresh();
  });
  async function action(path: string, method: 'POST' | 'PUT' | 'DELETE', body?: unknown) {
    if (busy) return;
    busy = true;
    error = '';
    try {
      await communityAction('/' + encodeURIComponent(id) + path, method, body);
      await refresh();
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Could not update community.';
    } finally {
      busy = false;
    }
  }
  async function save() {
    if (!community || busy) return;
    if (name.trim() !== community.name) await action('/name', 'PUT', { text: name.trim() });
    if (description.trim() !== community.description)
      await action('/description', 'PUT', { text: description.trim() });
    editing = false;
  }
</script>

<svelte:head><title>{community?.name ?? 'Community'} · Atlas</title></svelte:head>
<div class="community-page">
  <p><a href="/communities">← All communities</a></p>
  {#if loading}<p role="status">Loading community…</p>{:else if community}
    <header>
      <h1>{community.name}</h1>
      <p>{community.description}</p>
      <p class="stats">{community.memberCount} active members · {community.nodeCount} nodes</p>
      {#if community.status !== 'Active'}<p>This community is archived.</p>{/if}
      {#if signedIn && community.status === 'Active'}
        {#if owned}
          <button onclick={() => (editing = !editing)}
            >{editing ? 'Cancel editing' : 'Edit community'}</button
          >
        {:else}<button
            disabled={busy}
            onclick={() => void action('/membership', community?.isMember ? 'DELETE' : 'PUT')}
            >{community.isMember ? 'Leave community' : 'Join community'}</button
          >{/if}
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
</style>
