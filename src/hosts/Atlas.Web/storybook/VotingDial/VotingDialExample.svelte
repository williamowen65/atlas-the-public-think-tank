<script lang="ts">
  import VotingDial from '../../src/lib/components/VotingDial.svelte';

  let { initialValue = null, average = 7.4, totalVotes = 21, compact = false, disabled = false, label = 'Node importance' }: {
    initialValue?: number | null; average?: number | null; totalVotes?: number;
    compact?: boolean; disabled?: boolean; label?: string;
  } = $props();
  let value = $state<number | null>(null);

  // Reset local fixture state when Controls change the initial selection.
  $effect(() => { value = initialValue; });
</script>

<VotingDial {label} {value} {average} {totalVotes} {compact} {disabled} onChange={(next) => value = next} />
<p role="status">{value === null ? 'No rating selected' : `Preview selection: ${value} out of 10`}</p>
<p class="fixture-note">Local preview only. Average and total are fixtures; nothing is saved.</p>
