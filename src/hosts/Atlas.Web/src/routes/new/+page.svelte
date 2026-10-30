<!--
  @component
  Node creation route; resumes an optional local draft and opens the node after successful
  creation.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { apiRequest, ApiRequestError } from '$lib/api/apiRequest';
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodeForm from '$lib/components/LiveNodeForm.svelte';
  let ready = $state(false);
  let error = $state('');
  async function checkSession() {
    error = '';
    try {
      const session = await apiRequest<{ participantId: string | null }>('/atlas-session-api', 'auth/session');
      if (session.participantId) ready = true;
      else await goto('/login?returnTo=' + encodeURIComponent(page.url.pathname + page.url.search), { replaceState: true });
    } catch (cause) {
      if (cause instanceof ApiRequestError && cause.status === 401)
        await goto('/login?returnTo=' + encodeURIComponent(page.url.pathname + page.url.search), { replaceState: true });
      else error = 'Could not check sign-in. Please retry.';
    }
  }
  onMount(() => { void checkSession(); });
</script>

<!--
  Layout: Page heading followed by the live creation form; a URL draft ID can resume local edits
  and successful creation navigates to the node.
-->

<svelte:head><title>Create a node · Atlas</title></svelte:head>
{#if ready}
<h1>Create a node</h1>
<LiveNodeForm
  draftId={page.url.searchParams.get('draft') ?? undefined}
  onCreated={(id) => void goto(`/nodes?id=${encodeURIComponent(id)}`)}
/>
{/if}
{#if error}<p role="alert">{error}</p><button onclick={() => void checkSession()}>Retry</button>{:else if !ready}<p role="status">Checking sign-in…</p>{/if}
