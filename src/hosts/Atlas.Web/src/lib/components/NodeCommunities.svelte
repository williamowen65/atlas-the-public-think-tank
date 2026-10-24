<!--
  NodeCommunities: Displays node-community associations and lets the node author change them.
  Shows association changes only to the node author; the backend remains authoritative for permission checks.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { commentRequest } from '../api/comments';
  import { publicCommunities, communityAction, type Community } from '../api/communities';
  let {
    nodeId,
    authorId,
    archived = false,
  }: { nodeId: string; authorId?: string; archived?: boolean } = $props();
  let linked = $state<Community[]>([]);
  let available = $state<Community[]>([]);
  let participantId = $state<string | null>(null);
  let selected = $state('');
  let busy = $state(false);
  let error = $state('');
  const isAuthor = $derived(!archived && !!authorId && participantId === authorId);
  async function refresh() {
    linked = await commentRequest<Community[]>(
      '/atlas-public-api',
      'communities/node/' + encodeURIComponent(nodeId),
    );
  }
  // Load associations, available communities and session identity independently; only association load failures are reported as load errors.
  onMount(() => {
    let active = true;
    void Promise.allSettled([
      refresh(),
      publicCommunities('?offset=0&size=50').then((c) => {
        if (active) available = c;
      }),
      commentRequest<{ participantId: string | null }>('/atlas-session-api', 'auth/session').then(
        (s) => {
          if (active) participantId = s.participantId;
        },
      ),
    ]).then((results) => {
      if (active && results[0].status === 'rejected')
        error = 'Could not load community associations.';
    });
    return () => {
      active = false;
    };
  });
  // Guard the local UI, send the association change, then reload the authoritative linked-community list.
  async function update(id: string, add: boolean) {
    if (!isAuthor || busy) return;
    busy = true;
    error = '';
    try {
      await communityAction(
        '/' + encodeURIComponent(id) + '/nodes/' + encodeURIComponent(nodeId),
        add ? 'PUT' : 'DELETE',
      );
      await refresh();
      selected = '';
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Could not change community association.';
    } finally {
      busy = false;
    }
  }
</script>

<section class="node-communities" aria-label="Node communities">
  <h3>Communities</h3>
  {#if linked.length}<ul>
      {#each linked as community (community.id)}
        <li>
          <a href={'/communities/' + community.id}>{community.name}</a>
          {#if isAuthor}<button disabled={busy} onclick={() => void update(community.id, false)}
              >Remove</button
            >{/if}
        </li>
      {/each}
    </ul>{:else}<p>This node isn't associated with a community yet.</p>{/if}
  {#if isAuthor}
    <label for={'community-for-' + nodeId}>Add this node to a community</label>
    <div class="associate">
      <select id={'community-for-' + nodeId} bind:value={selected}>
        <option value="">Choose a community</option>
        {#each available.filter((c) => c.status === 'Active' && !linked.some((l) => l.id === c.id)) as community (community.id)}
          <option value={community.id}>{community.name}</option>
        {/each}
      </select><button disabled={!selected || busy} onclick={() => void update(selected, true)}
        >Add</button
      >
    </div>
  {/if}
  {#if error}<p role="alert">{error}</p>{/if}
</section>

<style>
  .node-communities {
    max-width: 48rem;
    margin: 1rem auto;
    padding: 1rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  h3 {
    margin: 0 0 0.6rem;
  }
  ul {
    list-style: none;
    padding: 0;
    display: flex;
    flex-wrap: wrap;
    gap: 0.65rem;
  }
  li {
    display: flex;
    align-items: center;
    gap: 0.4rem;
  }
  button,
  select {
    font: inherit;
    padding: 0.4rem;
    border: 1px solid var(--atlas-border);
    border-radius: 0.35rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  button {
    cursor: pointer;
  }
  .associate {
    display: flex;
    gap: 0.5rem;
    margin-top: 0.4rem;
  }
  label {
    display: block;
  }
</style>
