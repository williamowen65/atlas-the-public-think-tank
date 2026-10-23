<script lang="ts">
  import { goto } from '$app/navigation';
  import SessionControl from '$lib/components/SessionControl.svelte';

  // Only the dedicated sign-in route redirects after authentication.
  // Embedded sign-in controls (comments, node creation, etc.) stay in context.
  function signedIn(participantId: string | null) {
    if (participantId) void goto('/');
  }
</script>

<svelte:head><title>Sign in · Atlas</title></svelte:head>
<h1>Sign in</h1>
<SessionControl expanded onSession={signedIn} />
