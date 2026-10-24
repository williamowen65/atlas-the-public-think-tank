<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  import LiveProfilePage from '$lib/components/LiveProfilePage.svelte';
  import SessionControl from '$lib/components/SessionControl.svelte';
  let mine = $state<string | null>(null);
  let checking = $state(true);
  const requestedId = $derived(page.url.searchParams.get('id'));
  const participantId = $derived(requestedId ?? mine ?? '');
  onMount(() => {
    let active = true;
    async function refresh() {
      try {
        const session = await apiRequest<{ participantId: string | null }>(
          '/atlas-session-api',
          'auth/session',
        );
        if (active) mine = session.participantId;
      } catch {
        if (active) mine = null;
      } finally {
        if (active) checking = false;
      }
    }
    void refresh();
    window.addEventListener('atlas-session-changed', refresh);
    return () => {
      active = false;
      window.removeEventListener('atlas-session-changed', refresh);
    };
  });
</script>

<!--
  Layout: An explicit or current participant renders the shared profile page; otherwise show a
  session check or sign-in prompt.
-->

<svelte:head><title>Profiles · Atlas</title></svelte:head>
{#if requestedId || participantId}
  <LiveProfilePage
    {participantId}
    onOpenNode={(id) => void goto('/nodes?id=' + encodeURIComponent(id))}
    onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
  />
{:else if checking}
  <p role="status">Checking your profile…</p>
{:else}
  <h1>My profile</h1>
  <SessionControl
    prompt="Sign in to view your profile"
    onSession={(id) => {
      mine = id;
    }}
  />
{/if}
