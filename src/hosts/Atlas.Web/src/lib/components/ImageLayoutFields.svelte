<script lang="ts">
  import { imagePlacements, imageWidths, type ImagePlacement, type ImageWidth } from '../markdown/imageLayout';
  let { width = $bindable(), placement = $bindable(), inline = false }: {
    width: ImageWidth;
    placement: ImagePlacement;
    inline?: boolean;
  } = $props();
</script>

<div class="image-layout">
  <label>Image width
    <select bind:value={width}>
      {#each imageWidths as option (option.value)}
        <option value={option.value}>{option.label}</option>
      {/each}
    </select>
  </label>
  <label>Image placement
    <select bind:value={placement}>
      {#each imagePlacements as option (option)}
        <option value={option}>{option === 'center' ? 'Centered' : option === 'left' ? (inline ? 'Left (wrap text)' : 'Left') : (inline ? 'Right (wrap text)' : 'Right')}</option>
      {/each}
    </select>
  </label>
</div>

<style>
  .image-layout { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(8rem, 100%), 1fr)); gap: .5rem; }
  label { display: grid; gap: .2rem; }
  select { width: 100%; min-width: 0; font: inherit; color: inherit; background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); padding: .4rem; }
</style>
