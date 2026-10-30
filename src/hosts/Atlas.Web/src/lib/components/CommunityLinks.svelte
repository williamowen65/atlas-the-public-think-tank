<!--
  @component
  CommunityLinks: Reads node-community associations and displays links, with names as fallback.
  Uses supplied names until public community records are available.

  **Props**
  - `nodeId` (`string`; required): ID of the node this component reads, displays, or modifies.
  - `names` (`string[]`; required): Fallback community display names while linked community records
    are being fetched.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { communityAction, myCommunities } from '../api/communities';
  import { ApiRequestError } from '../api/apiRequest';
  import { apiRequest } from '../api/apiRequest';
  import type { Community } from '../api/communities';
  let {
    nodeId,
    names,
    allowJoin = false,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /** Fallback community display names while linked community records are being fetched. */
    names: string[];
    /** Show real join actions beside linked communities in experimental feed cards. */
    allowJoin?: boolean;
  } = $props();
  /**
   * Public community associations loaded for this node.
   */
  let linked = $state<Community[]>([]);
  /** Session membership is kept separate from anonymous public community records. */
  let memberships = $state<string[]>([]);
  let membershipReady = $state(false);
  let busy = $state('');
  let error = $state('');
  /** Re-read membership after sign-in/out or another card successfully joins. */
  async function refreshMembership() {
    membershipReady = false;
    try { memberships = (await myCommunities()).filter(c => c.isMember).map(c => c.id); membershipReady = true; }
    catch (cause) {
      memberships = [];
      membershipReady = cause instanceof ApiRequestError && cause.status === 401;
    }
  }
  /** Join once through the existing CSRF-protected membership endpoint. */
  async function join(id: string) {
    if (busy) return;
    busy = id; error = '';
    try {
      await communityAction('/' + encodeURIComponent(id) + '/membership', 'PUT');
      memberships = [...memberships, id];
      window.dispatchEvent(new Event('atlas-membership-changed'));
    } catch (cause) {
      if (cause instanceof ApiRequestError && cause.status === 401)
        window.location.assign('/login?returnTo=' + encodeURIComponent(location.pathname + location.search));
      else error = 'Could not join this community. Please retry.';
    } finally { busy = ''; }
  }
  // Fetch linkable records once mounted; abort on removal and keep fallback names on failure.
  onMount(() => {
    const controller = new AbortController();
    if (allowJoin) {
      void refreshMembership();
      window.addEventListener('atlas-session-changed', refreshMembership);
      window.addEventListener('atlas-membership-changed', refreshMembership);
    }
    void apiRequest<Community[]>(
      '/atlas-public-api',
      'communities/node/' + encodeURIComponent(nodeId),
      'GET',
      undefined,
      controller.signal,
    )
      .then((items) => {
        if (!controller.signal.aborted) linked = items;
      })
      .catch(() => {});
    return () => {
      controller.abort();
      window.removeEventListener('atlas-session-changed', refreshMembership);
      window.removeEventListener('atlas-membership-changed', refreshMembership);
    };
  });
  /**
   * Linked community records when available, otherwise supplied display-only names.
   */
  const labels = $derived(
    linked.length
      ? linked.map((c) => ({ id: c.id, name: c.name }))
      : names.map((name) => ({ id: '', name })),
  );
</script>

<!--
  Layout: An inline comma-separated list: known community IDs become links, while fallback names
  remain plain text. No block wrapper is added.
-->

{#each labels as community, i (community.id || i)}
  {#if i > 0},
  {/if}
  {#if community.id}<a href={'/communities/' + encodeURIComponent(community.id)}>{community.name}</a
    >{:else}{community.name}{/if}
  {#if allowJoin && community.id && linked.find(c => c.id === community.id)?.status === 'Active'}
    {#if memberships.includes(community.id)}<span class="joined">Joined</span>
    {:else}<button type="button" class="join-community" disabled={!!busy || !membershipReady}
      aria-label={'Join ' + community.name} onclick={() => void join(community.id)}>{busy === community.id ? 'Joining…' : 'Join'}</button>{/if}
  {/if}
{/each}
{#if error}<span role="alert">{error}</span>{/if}

<style>
  .join-community { font: inherit; color: var(--atlas-accent); background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: 999px; padding: 0.25rem 0.65rem; margin-inline-start: 0.4rem; min-height: 2.75rem; cursor: pointer; }
  .joined { color: var(--atlas-muted); font-size: 0.75rem; margin-inline-start: 0.4rem; }

  a {
    color: inherit;
    text-decoration: none;
  }
  a:hover,
  a:focus-visible {
    text-decoration: underline;
  }
</style>
