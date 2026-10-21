import { onDestroy } from 'svelte';

export type VoteOutcome = 'accepted' | 'anonymous' | 'failed';

/** Simulated submission feedback only; never calls the voting API. */
export function createVoteFeedback() {
  let timer: ReturnType<typeof setTimeout> | undefined;
  function cancel() { clearTimeout(timer); timer = undefined; }
  onDestroy(cancel);
  return {
    cancel,
    settle(outcome: VoteOutcome, reset: (message: string) => void) {
      cancel();
      if (outcome === 'accepted') return;
      // Restart while scrolling so rollback happens after the selection gesture.
      timer = setTimeout(() => {
        timer = undefined;
        reset(outcome === 'anonymous' ? 'Sign in to cast a vote. No vote was saved.' : 'Vote could not be saved. Try again.');
      }, 650);
    },
  };
}
