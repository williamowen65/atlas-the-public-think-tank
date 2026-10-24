<script lang="ts">
  import ActionButton from '../../src/lib/components/ActionButton.svelte';

  interface Props {
    /** Visible label and accessible name for the demonstration action button. */
    label?: string;
    /** Prevents interaction with this control. */
    disabled?: boolean;
  }

  let { label = 'Try action', disabled = false }: Props = $props();
  let message = $state('No action yet');
</script>

<!-- Demonstration wrapper: the status message observes the button callback. -->
<section aria-label="Action button example">
  <ActionButton {label} {disabled} onActivate={() => (message = 'Action completed')} />
  <p role="status">{message}</p>
</section>
