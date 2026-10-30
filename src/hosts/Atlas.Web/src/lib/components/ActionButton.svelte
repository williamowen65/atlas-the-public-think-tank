<!--
  @component
  ActionButton: Reusable action button with accessibility attributes and an activation callback.
  The parent supplies the label and callback; this component performs no API requests.

  **Props**
  - `label` (`string`; required): Visible button text and its accessible name. The parent supplies
    the action-specific wording.
  - `type` (`'button' | 'submit'`; optional; default `'button'`): Native button behavior.
  - `disabled` (`boolean`; optional; default `false`): Prevents interaction with this control.
  - `expanded` (`boolean`; optional): Sets aria-expanded to describe whether the controlled region
    is open.
  - `controls` (`string`; optional): HTML ID of the region controlled by the button; supplies
    aria-controls.
  - `onActivate` (`() => void`; optional): Callback: Called when the enabled button is activated by
    pointer or keyboard.
-->
<script lang="ts">
  interface Props {
    /**
     * Visible button text and its accessible name. The parent supplies the action-specific
     * wording.
     */
    label: string;
    /** Submit participates in the enclosing form, including Enter-key submission. */
    type?: 'button' | 'submit';
    /** Prevents interaction with this control. */
    disabled?: boolean;
    /** Sets aria-expanded to describe whether the controlled region is open. */
    expanded?: boolean;
    /** HTML ID of the region controlled by the button; supplies aria-controls. */
    controls?: string;
    /** Callback: Called when the enabled button is activated by pointer or keyboard. */
    onActivate?: () => void;
  }

  // The component owns its actual HTML. Pages and stories supply only inputs.
  let { label, type = 'button', disabled = false, expanded, controls, onActivate }: Props = $props();
</script>

<!--
  Layout: One native button: the supplied label is its visible text; the parent handles
  activation.
-->

<button
  {type}
  class="atlas-action"
  {disabled}
  aria-expanded={expanded}
  aria-controls={controls}
  onclick={() => onActivate?.()}
>
  {label}
</button>

<style lang="scss">
  .atlas-action {
    font: inherit;
    padding: var(--atlas-action-padding, 0.65rem 1rem);
    border: 2px solid var(--atlas-accent);
    border-radius: var(--atlas-radius);
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    cursor: pointer;
    width: var(--atlas-action-width, auto);
    max-width: 100%;
    overflow-wrap: anywhere;

    &:disabled {
      background: var(--atlas-bg);
      border-color: var(--atlas-border);
      color: var(--atlas-muted);
      cursor: default;
    }
  }
</style>
