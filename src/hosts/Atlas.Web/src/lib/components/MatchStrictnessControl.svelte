<script lang="ts">
  import { searchPreferences, saveMatchStrictness } from '../state/searchPreferences.svelte';
  const id = $props.id();
</script>

<div class="strictness">
  <label for={id}>Match strictness: {searchPreferences.matchStrictness}%</label>
  <input
    {id}
    aria-label="Match strictness"
    type="range"
    min="0"
    max="100"
    step="5"
    value={searchPreferences.matchStrictness}
    onchange={(event) => saveMatchStrictness(Number(event.currentTarget.value))}
  />
  <small
    >0 keeps all matches. Higher values hide weaker text matches before voting is considered.</small
  >
  <button type="button" onclick={() => saveMatchStrictness(15)}>Reset match strictness</button>
</div>

<style>
  .strictness {
    display: grid;
    gap: 0.3rem;
    font-size: 0.85rem;
  }
  input {
    width: 100%;
    margin: 0.3rem 0;
  }
  button {
    justify-self: start;
    font: inherit;
    color: var(--atlas-accent);
    background: transparent;
    border: 0;
    padding: 0;
    cursor: pointer;
  }
  small {
    color: var(--atlas-muted);
  }
</style>
