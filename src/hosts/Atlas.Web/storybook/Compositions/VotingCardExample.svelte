<script lang="ts">
  import VotingDial from '../../src/lib/components/VotingDial.svelte';

  let { compact = false }: { compact?: boolean } = $props();
  let value = $state<number | null>(null);
</script>

<article class="atlas-panel card-preview">
  <div>
    <p class="fixture-note">Example node card composition</p>
    <h2>Community-owned internet networks</h2>
    <p>How might communities improve access to reliable information?</p>
    <p role="status">{value === null ? 'No rating selected' : `Preview selection: ${value} out of 10`}</p>
    <p class="fixture-note">Fixture content. Selection does not submit a vote.</p>
  </div>
  <VotingDial label="Node importance" {value} average={7.4} totalVotes={21} {compact} onChange={(next) => value = next} />
</article>

<style>
  .card-preview { display: flex; flex-wrap: wrap; align-items: flex-start; gap: var(--atlas-space); max-width: 55rem; }
  .card-preview > div { flex: 1 1 16rem; min-width: 0; }
</style>
