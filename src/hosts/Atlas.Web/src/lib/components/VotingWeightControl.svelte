<script lang="ts">
  import { searchPreferences, saveVotingWeight } from '../state/searchPreferences.svelte';
  const id = $props.id();
</script>

<div class="voting-weight">
  <label for={id}>Voting weight: {searchPreferences.votingWeight}%</label>
  <input
    {id}
    aria-label="Voting weight"
    type="range"
    min="0"
    max="100"
    step="5"
    value={searchPreferences.votingWeight}
    onchange={(event) => saveVotingWeight(Number(event.currentTarget.value))}
  />
  <small
    >Text {100 - searchPreferences.votingWeight}% · Votes {searchPreferences.votingWeight}%</small
  >
  <button type="button" onclick={() => saveVotingWeight(40)}>Reset voting weight</button>
  <small>Affects text search. Remembered in this browser for your current account.</small>
  {#if searchPreferences.message}<small role="status">{searchPreferences.message}</small>{/if}
</div>

<style>
  .voting-weight {
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
