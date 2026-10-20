<script lang="ts">
  interface Props {
    label: string;
    disabled?: boolean;
    onActivate?: () => void;
  }

  // The component owns its actual HTML. Pages and stories supply only inputs.
  let { label, disabled = false, onActivate }: Props = $props();
</script>

<button
  type="button"
  class="atlas-action"
  {disabled}
  onclick={() => onActivate?.()}
>
  {label}
</button>
