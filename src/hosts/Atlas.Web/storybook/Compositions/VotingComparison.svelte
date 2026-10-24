<script lang="ts">
  import VotingDial from '../../src/lib/components/VotingDial.svelte';

  let first = $state<number | null>(null);
  let second = $state<number | null>(0);
</script>

<div class="comparison">
  <VotingDial
    label="First node importance"
    value={first}
    average={7.4}
    totalVotes={21}
    onChange={(next) => (first = next)}
  />
  <VotingDial
    label="Second node importance"
    value={second}
    average={6.2}
    totalVotes={8}
    compact
    onChange={(next) => (second = next)}
  />
</div>
<p class="fixture-note">Independent local selections; no votes are submitted.</p>

<style lang="scss">
  .comparison {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-start;
    gap: var(--atlas-space);
  }
</style>
