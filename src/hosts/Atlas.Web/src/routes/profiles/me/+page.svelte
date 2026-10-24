<!--
  @component
  Current-participant profile route with session checking and a sign-in fallback.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { apiRequest } from '$lib/api/apiRequest';
  import LiveProfilePage from '$lib/components/LiveProfilePage.svelte';
  import SessionControl from '$lib/components/SessionControl.svelte';
  /**
   * Current participant ID reported by the session API, or null when no usable identity is
   * known.
   */
  let participantId = $state<string | null>(null);
  /**
   * Whether the initial session check is pending; controls the sign-in loading state.
   */
  let checking = $state(true);
  onMount(() => {
    let active = true;
    async function refresh() {
      try {
        const session = await apiRequest<{ participantId: string | null }>(
          '/atlas-session-api',
          'auth/session',
        );
        if (active) participantId = session.participantId;
      } catch {
        if (active) participantId = null;
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
  Layout: A known current participant renders the shared profile page; session checking and
  sign-in are alternate states.
-->

<svelte:head><title>My profile · Atlas</title></svelte:head>
{#if participantId}
  <LiveProfilePage
    {participantId}
    onOpenNode={(id) => void goto('/nodes?id=' + encodeURIComponent(id))}
    onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
  />
{:else if checking}
  <p role="status">Checking your profile…</p>
{:else}
  <h1>My profile</h1>
  <SessionControl prompt="Sign in to view your profile" onSession={(id) => (participantId = id)} />
{/if}
