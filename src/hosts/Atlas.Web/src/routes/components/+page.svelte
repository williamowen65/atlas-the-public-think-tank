<!--
  @component
  Small application integration example demonstrating ActionButton callbacks and status
  feedback.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import ActionButton from '$lib/components/ActionButton.svelte';

  // The app owns its composition; Storybook-only fixtures stay outside src.
  /**
   * Status text showing the integration example’s most recent button result.
   */
  let message = $state('No action yet');
</script>

<!--
  Layout: An integration example pairs an action button with a status message that shows its
  callback result.
-->

<svelte:head>
  <title>Atlas component integration</title>
</svelte:head>

<h1>Atlas component integration</h1>
<ActionButton label="Try action" onActivate={() => (message = 'Action completed')} />
<p role="status">{message}</p>
