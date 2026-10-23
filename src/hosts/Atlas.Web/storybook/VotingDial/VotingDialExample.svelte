<script lang="ts">
  import { createVoteFeedback, type VoteOutcome } from '../shared/voteFeedback';
  import VotingDial from '../../src/lib/components/VotingDial.svelte';

  let {
    initialValue = null,
    average = 7.4,
    totalVotes = 21,
    compact = false,
    disabled = false,
    label = 'Node importance',
    voteOutcome = 'accepted',
  }: {
    initialValue?: number | null;
    average?: number | null;
    totalVotes?: number;
    compact?: boolean;
    disabled?: boolean;
    label?: string;
    voteOutcome?: VoteOutcome;
  } = $props();
  let value = $state<number | null>(null);

  let notice = $state('');
  const feedback = createVoteFeedback();
  function choose(next: number) {
    value = next;
    notice = '';
    feedback.settle(voteOutcome, (message) => {
      value = null;
      notice = message;
    });
  }

  // Reset local fixture state when Controls change the initial selection.
  $effect(() => {
    initialValue;
    voteOutcome;
    feedback.cancel();
    notice = '';
    value = initialValue;
  });
</script>

<VotingDial {label} {value} {average} {totalVotes} {compact} {disabled} onChange={choose} />
<p role="status">
  {value === null ? 'No rating selected' : `Preview selection: ${value} out of 10`}
</p>
{#if notice}<p role="note">{notice}</p>{/if}
<p class="fixture-note">
  Local preview only. voteOutcome simulates a cast result; accepted also does not save a vote.
  Average and total are fixtures; nothing is saved.
</p>
