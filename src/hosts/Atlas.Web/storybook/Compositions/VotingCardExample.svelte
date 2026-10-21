<script lang="ts">
  import { createVoteFeedback, type VoteOutcome } from '../shared/voteFeedback';
  import LiveNodePreview from '../NodeCard/LiveNodePreview.svelte';
  import type { NodeCardPresentation } from '../../src/lib/components/NodeCard.svelte';
  import NodeCard from '../../src/lib/components/NodeCard.svelte';
  import { nodeFixtures } from '../NodeCard/nodeFixtures';

  let { presentation = 'ribbon', source = 'fixtures', voteOutcome = 'accepted', selectedNode = 'question', onSelect }: { presentation?: NodeCardPresentation; source?: 'fixtures' | 'local-api'; voteOutcome?: VoteOutcome; selectedNode?: string; onSelect?: (key: string) => void } = $props();
  let ratings = $state<Record<string, number>>({});
  let discussion = $state<string | null>(null);
  const fixture = $derived(nodeFixtures[selectedNode] ?? nodeFixtures.question);
  const value = $derived(ratings[fixture.node.id] ?? null);
  let notice = $state('');
  const feedback = createVoteFeedback();
  $effect(() => { selectedNode; source; voteOutcome; feedback.cancel(); notice = ''; if (voteOutcome !== 'accepted') ratings = {}; });
  function choose(next: number) {
    const id = fixture.node.id;
    ratings[id] = next;
    notice = '';
    feedback.settle(voteOutcome, message => { delete ratings[id]; notice = message; });
  }
  function navigate(id: string) {
    const match = Object.entries(nodeFixtures).find(([, item]) => item.node.id === id);
    if (match) onSelect?.(match[0]);
  }
</script>

{#if source === 'local-api'}
  <LiveNodePreview {presentation} />
{:else}
<div class="preview">
  <p class="fixture-note">Example nodes · local interaction preview. Ratings, discussion and totals are fixtures; nothing is saved.</p>
  <NodeCard {presentation} node={fixture.node} average={fixture.average} totalVotes={fixture.totalVotes} {value}
    onVote={choose} onNavigate={navigate} onComments={(id) => discussion = discussion === id ? null : id} />
  <p role="status">{value === null ? 'No rating selected' : `Preview selection: ${value} out of 10`}</p>
  {#if notice}<p role="note">{notice}</p>{/if}
  {#if discussion === fixture.node.id}
    <section class="discussion" aria-label="Discussion preview">
      <h3>Discussion</h3>
      {#each fixture.comments as comment}<p>{comment}</p>{:else}<p>No comments yet.</p>{/each}
    </section>
  {/if}
</div>

{/if}

<style lang="scss">
  .preview {
    max-width: 48rem;
  }

  .discussion {
    padding: var(--atlas-space);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    p {
      line-height: 1.6;
    }
  }
</style>
