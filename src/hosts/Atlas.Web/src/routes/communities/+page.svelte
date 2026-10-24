<!--
  @component
  Community directory with local search, creation allowance feedback and the community creation
  form.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import {
    publicCommunities,
    creationAllowance,
    communityAction,
    type Community,
    type CreationAllowance,
  } from '$lib/api/communities';
  /**
   * Loaded community records displayed in this view.
   */
  let communities = $state<Community[]>([]);
  /**
   * Current participant’s community creation quotas, or null when unavailable.
   */
  let allowance = $state<CreationAllowance | null>(null);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(true);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Working name for the community creation form.
   */
  let name = $state('');
  /**
   * Working description for the community creation form.
   */
  let description = $state('');
  /**
   * Local text filter for the loaded community list.
   */
  let search = $state('');
  /**
   * Whether community creation is underway; locks the submit button.
   */
  let creating = $state(false);
  /**
   * Loaded communities matching the local search text.
   */
  const visible = $derived(
    communities.filter((c) => c.name.toLowerCase().includes(search.trim().toLowerCase())),
  );
  async function refresh() {
    try {
      communities = await publicCommunities('?offset=0&size=50');
      try {
        allowance = await creationAllowance();
      } catch {
        allowance = null;
      }
      error = '';
    } catch {
      error = 'Communities could not be loaded.';
    } finally {
      loading = false;
    }
  }
  onMount(() => {
    void refresh();
  });
  async function create() {
    if (creating) return;
    creating = true;
    error = '';
    try {
      await communityAction('', 'POST', { name: name.trim(), description: description.trim() });
      name = '';
      description = '';
      await refresh();
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Could not create community.';
    } finally {
      creating = false;
    }
  }
</script>

<!--
  Layout: Community browsing and creation share the page; creation allowance information sits
  with the form rather than the public list.
-->

<svelte:head><title>Communities · Atlas</title></svelte:head>
<div class="communities">
  <h1>Communities</h1>
  <p>Find people exploring similar questions, ideas, and goals.</p>
  <label for="community-search">Find a community</label>
  <input
    id="community-search"
    type="search"
    bind:value={search}
    placeholder="Search by community name"
  />
  {#if loading}<p role="status">Loading communities…</p>{:else if error}<p role="alert">
      {error} <button onclick={() => void refresh()}>Retry</button>
    </p>{/if}
  <div class="directory">
    {#each visible as community (community.id)}
      <article>
        <h2><a href={'/communities/' + community.id}>{community.name}</a></h2>
        <p>{community.description}</p>
        <small>{community.memberCount} members · {community.nodeCount} nodes</small>
      </article>
    {:else}<p>{loading ? 'Loading communities…' : 'No communities match your search.'}</p>{/each}
  </div>
  <section class="create">
    <h2>Create a community</h2>
    {#if allowance}
      <p>
        You own {allowance.owned} of {allowance.maximumOwned} permitted communities.
        {#if allowance.rolling?.remaining === 0}
          Your next creation slot opens {allowance.rolling.nextSlotAt
            ? new Date(allowance.rolling.nextSlotAt).toLocaleString()
            : 'later'}.{/if}
      </p>
    {:else}<p>
        Sign in to create a community. You can own up to five communities and create one per 24
        hours.
      </p>{/if}
    <form
      onsubmit={(event) => {
        event.preventDefault();
        void create();
      }}
    >
      <label for="community-name">Name</label><input
        id="community-name"
        maxlength="100"
        required
        bind:value={name}
      />
      <label for="community-description">Description</label><textarea
        id="community-description"
        maxlength="1000"
        rows="4"
        bind:value={description}></textarea>
      <button
        disabled={creating ||
          (allowance !== null &&
            (allowance.owned >= allowance.maximumOwned || allowance.rolling?.remaining === 0))}
        >{creating ? 'Creating…' : 'Create community'}</button
      >
    </form>
  </section>
</div>

<style>
  .communities {
    max-width: 66rem;
    margin: 1.5rem auto;
    padding: 0 1rem;
  }
  input,
  textarea {
    font: inherit;
    box-sizing: border-box;
    width: 100%;
    padding: 0.6rem;
    border: 1px solid var(--atlas-border);
    border-radius: 0.4rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  label {
    display: block;
    margin: 0.65rem 0 0.3rem;
  }
  .directory {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 17rem), 1fr));
    gap: 1rem;
    margin: 1.5rem 0;
  }
  .directory article,
  .create {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: 1rem;
  }
  .directory h2 {
    font-size: 1.2rem;
    margin: 0;
  }
  .directory p {
    overflow-wrap: anywhere;
  }
  .directory small {
    color: var(--atlas-muted);
  }
  .create form {
    max-width: 35rem;
  }
  .create button {
    margin-top: 1rem;
  }
  button {
    font: inherit;
    cursor: pointer;
    padding: 0.5rem 0.8rem;
  }
</style>
