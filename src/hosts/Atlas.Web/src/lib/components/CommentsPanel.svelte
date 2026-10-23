<script lang="ts">
  import LiveComments from './LiveComments.svelte';
  import SessionControl from './SessionControl.svelte';
  let {
    nodeId,
    apiBase = '/atlas-public-api',
    sessionBase = '/atlas-session-api',
    onAdded,
  }: { nodeId: string; apiBase?: string; sessionBase?: string; onAdded?: () => void } = $props();
  let participantId = $state<string | null>(null);
</script>

<section class="discussion" aria-label="Comment discussion">
  <header>
    <h2>Comments</h2>
    <p>Join the discussion or build on someone’s contribution.</p>
  </header>
  <SessionControl
    {sessionBase}
    hideWhenSignedIn
    prompt="Sign in to comment"
    onSession={(id) => (participantId = id)}
  />
  <LiveComments {nodeId} {apiBase} {sessionBase} {participantId} {onAdded} />
</section>

<style>
  h2 {
    margin: 0;
  }
  header p {
    color: var(--atlas-muted);
    margin: 0.4rem 0 1rem;
  }
</style>
