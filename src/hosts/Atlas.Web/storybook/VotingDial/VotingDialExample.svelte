<!--
  @component
  Presentational voting-dial example with simulated local outcomes and status feedback; no API
  vote is saved.
-->
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
    /** Initial simulated dial value in the Storybook example; null starts unrated. */
    initialValue?: number | null;
    /** Community average supplied by the parent; null means no average is available. */
    average?: number | null;
    /** Community vote count supplied by the parent. */
    totalVotes?: number;
    /** Selects the compact dial layout. */
    compact?: boolean;
    /** Prevents interaction with this control. */
    disabled?: boolean;
    /** Accessible name for the simulated rating spinbutton. */
    label?: string;
    /**
     * Simulated acceptance outcome for the standalone dial story: accepted, anonymous, or failed.
     * No backend vote is saved.
     */
    voteOutcome?: VoteOutcome;
  } = $props();
  /**
   * Local preview rating; simulated outcomes may restore it and no API vote is saved.
   */
  let value = $state<number | null>(null);

  /**
   * Non-error feedback shown after an action completes.
   */
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

<!--
  Layout: The presentational dial is followed by local selection/result feedback and a note
  distinguishing the example from persisted voting.
-->

<VotingDial {label} {value} {average} {totalVotes} {compact} {disabled} onChange={choose} />
<p role="status">
  {value === null ? 'No rating selected' : `Preview selection: ${value} out of 10`}
</p>
{#if notice}<p role="note">{notice}</p>{/if}
<p class="fixture-note">
  Local preview only. voteOutcome simulates a cast result; accepted also does not save a vote.
  Average and total are fixtures; nothing is saved.
</p>
