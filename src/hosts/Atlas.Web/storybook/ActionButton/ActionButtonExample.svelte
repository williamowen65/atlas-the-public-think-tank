<!--
  @component
  Interactive ActionButton example reporting activation through a local status message.
-->
<script lang="ts">
  import ActionButton from '../../src/lib/components/ActionButton.svelte';

  interface Props {
    /** Visible label and accessible name for the demonstration action button. */
    label?: string;
    /** Prevents interaction with this control. */
    disabled?: boolean;
  }

  let { label = 'Try action', disabled = false }: Props = $props();
  /**
   * Local status text demonstrating the button activation callback.
   */
  let message = $state('No action yet');
</script>

<!--
  Layout: The action button and a status message demonstrate activation without adding
  application behavior.
-->

<!-- Demonstration wrapper: the status message observes the button callback. -->
<section aria-label="Action button example">
  <ActionButton {label} {disabled} onActivate={() => (message = 'Action completed')} />
  <p role="status">{message}</p>
</section>
