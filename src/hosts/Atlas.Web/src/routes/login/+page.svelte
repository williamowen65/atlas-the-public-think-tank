<!--
  @component
  Sign-in route using SessionControl; handles navigation after successful authentication.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { goto } from '$app/navigation';
  import SessionControl from '$lib/components/SessionControl.svelte';

  // Only the dedicated sign-in route redirects after authentication.
  // Embedded sign-in controls (comments, node creation, etc.) stay in context.
  function signedIn(participantId: string | null) {
    if (participantId) void goto('/');
  }
</script>

<!--
  Layout: Page heading followed by the expanded shared sign-in control; successful
  authentication is handled by the route callback.
-->

<svelte:head><title>Sign in · Atlas</title></svelte:head>
<h1>Sign in</h1>
<SessionControl expanded onSession={signedIn} />
