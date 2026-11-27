<script lang="ts">
  const feedbackNodeId = import.meta.env.VITE_ATLAS_FEEDBACK_NODE_ID?.trim();
  const feedbackNode = feedbackNodeId && /^[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i.test(feedbackNodeId)
    ? `/nodes?id=${encodeURIComponent(feedbackNodeId)}`
    : undefined;
</script>

<svelte:head><title>Give feedback · Atlas</title></svelte:head>

<h1>Give feedback</h1>
<p>Choose where to send your feedback based on what you want to tell us.</p>

<section aria-labelledby="bug">
  <h2 id="bug">Something broke</h2>
  <p>Tell us which page you were on, what you tried, and what happened. If it’s a sign-in or account problem, email us and leave out passwords and sign-in codes.</p>
  <p><a href="mailto:atlas@thepublicthinktank.com?subject=Atlas%20bug%20report">Email a bug report</a></p>
</section>

<section aria-labelledby="ideas">
  <h2 id="ideas">Help shape Atlas</h2>
  <p>Have an idea for Atlas or disagree with a design choice? Share it as a node so people can vote on it, discuss it, and build on it.</p>
  {#if feedbackNode}
    <p><a href={feedbackNode}>Open the Atlas feedback discussion</a>. Use <strong>Add a child node</strong> to suggest one improvement at a time, or build on an existing suggestion.</p>
  {:else}
    <p><a href="/">Browse Discovery</a> for a discussion about Atlas and add a child node where your idea fits. You can also <a href="/new">start a discussion</a> about your suggestion.</p>
  {/if}
  <p><a href="/how-to-write-a-node">How to Write a Node</a></p>
</section>
