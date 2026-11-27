<script lang="ts">
  import type { NodeType } from '../api/nodePage';

  let { types, onChoose }: { types: NodeType[]; onChoose: (id: string) => void } = $props();

  const examples: ReadonlyMap<string, string> = new Map([
    ['issue', 'The waterfront is hard to reach without a car.'],
    ['question', 'How could we make the waterfront easier to visit?'],
    ['idea', 'Add benches along the waterfront path.'],
    ['solution', 'Run a weekend shuttle from downtown.'],
    ['evidence', 'Weekend bus service ends before the last ferry arrives.'],
    ['relationship', 'The shuttle and ferry proposals both depend on weekend service.'],
    ['location', 'Ferry terminal entrance'],
  ]);
</script>

<div class="chooser">
  <h2><label for="standalone-node-type">Choose a node type</label></h2>
  <p>Pick the type that fits what you want to add. Your choice opens the editor.</p>
  <select id="standalone-node-type" value="" required
    onchange={(event) => onChoose(event.currentTarget.value)}>
    <option value="" disabled>Select a node type…</option>
    {#each types as type (type.id)}<option value={type.id}>{type.name}</option>{/each}
    <option value="new">Define a new node type…</option>
  </select>

  {#if types.length}
    <details>
      <summary>See node type examples</summary>
      <dl>
        {#each types as type (type.id)}
          {@const example = examples.get(type.name.trim().toLowerCase())}
          {#if type.description || example}
            <dt>{type.name}</dt>
            <dd>
              {#if type.description}<span>{type.description}</span>{/if}
              {#if example}<span class="example">{example}</span>{/if}
            </dd>
          {/if}
        {/each}
      </dl>
    </details>
  {/if}
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
  summary { cursor: pointer; font-weight: 600; }
  dl { margin-block: .75rem 0; }
  dt { font-weight: 650; }
  dd { margin: .2rem 0 .85rem; }
  dd span { display: block; }
  .example { font-style: italic; }
</style>
