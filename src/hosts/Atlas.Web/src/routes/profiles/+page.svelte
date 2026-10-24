<!--
  @component
  Public profile route selecting an explicit participant or the current session, with a sign-in
  fallback.
-->
<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  import LiveProfilePage from '$lib/components/LiveProfilePage.svelte';
  import SessionControl from '$lib/components/SessionControl.svelte';
  /**
   * Current session participant ID used when no explicit profile ID is requested.
   */
  let mine = $state<string | null>(null);
  /**
   * Whether the initial session check is pending; controls the sign-in loading state.
   */
  let checking = $state(true);
  /**
   * Profile ID from the URL query, or null when viewing the current participant.
   */
  const requestedId = $derived(page.url.searchParams.get('id'));
  /**
   * Explicit requested profile ID, falling back to the session participant or empty text.
   */
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
