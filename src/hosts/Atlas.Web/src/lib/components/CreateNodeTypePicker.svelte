<script lang="ts">
  import { nodeTypeSelect } from '../actions/nodeTypeSelect';
  import type { NodeType } from '../api/nodePage';

  let { types, onChoose }: { types: NodeType[]; onChoose: (id: string) => void } = $props();

</script>

<div class="chooser">
  <h2><label for="standalone-node-type">Choose a node type</label></h2>
  <p>Pick the type that fits what you want to add. Your choice opens the editor.</p>
  <select id="standalone-node-type" data-node-type-select use:nodeTypeSelect={{ label: 'Choose a node type', value: '' }} value="" required
    onchange={(event) => onChoose(event.currentTarget.value)}>
    <option value="" disabled>Select a node type…</option>
    {#each types as type (type.id)}<option value={type.id} data-description={type.description}>{type.name}</option>{/each}
    <option value="new">Define a new node type…</option>
  </select>


</div>

<style>
  .chooser { display: grid; gap: .65rem; padding-block: 1rem; min-width: 0; }
  h2, p { margin: 0; }
  h2 { font-size: 1.25rem; }
  select {
    font: inherit;
    width: 100%;
    min-width: 0;
    padding: .55rem .75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
</style>
