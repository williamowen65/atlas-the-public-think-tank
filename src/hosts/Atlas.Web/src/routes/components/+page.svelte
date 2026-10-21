<script lang="ts">
  import ActionButton from '$lib/components/ActionButton.svelte';

  // The app owns its composition; Storybook-only fixtures stay outside src.
  let message = $state('No action yet');
</script>

<svelte:head>
  <title>Atlas component integration</title>
</svelte:head>

<h1>Atlas component integration</h1>
<ActionButton
  label="Try action"
  onActivate={() => message = 'Action completed'}
/>
<p role="status">{message}</p>
