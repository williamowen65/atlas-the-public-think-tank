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
  <p><a href="https://github.com/williamowen65/atlas-the-public-think-tank/issues/new?template=bug_report.yml">Submit a bug report on GitHub</a>. GitHub requires an account, and reports there are public.</p>
  <p>You can also email <a href="mailto:atlas@thepublicthinktank.com?subject=Atlas%20bug%20report">atlas@thepublicthinktank.com</a>. Use email for account or sign-in problems.</p>
  <h3>What to include</h3>
  <ul>
    <li><strong>Short title</strong> that identifies the problem.</li>
    <li><strong>What happened?</strong> Describe the problem and its impact.</li>
    <li><strong>What should happen?</strong> Describe the expected result.</li>
    <li><strong>How can we reproduce it?</strong> List the steps and any inputs needed.</li>
    <li><strong>Affected areas</strong> such as Discovery, voting, or comments. Include the page URL if you have it.</li>
    <li><strong>Environment</strong> including your device, operating system, browser, and whether you were using live or demo.</li>
    <li><strong>Evidence</strong> such as screenshots or error messages, if available.</li>
    <li><strong>Suggested verification</strong> if you have an idea for how to tell that the fix worked.</li>
  </ul>
  <p>Remove passwords, sign-in codes, and personal information from your report and attachments.</p>
</section>

<section aria-labelledby="ideas">
  <h2 id="ideas">Help shape Atlas</h2>
  <p>Have an idea for Atlas or disagree with a design choice? Share it as a node so people can vote on it, discuss it, and build on it.</p>
  {#if feedbackNode}
    <p><a href={feedbackNode}>Open the Atlas feedback discussion</a>. Use <strong>Add a child node</strong> to suggest one improvement at a time, or build on an existing suggestion.</p>
  {:else}
    <p>We plan to link to the Atlas discussion and community here when they’re ready. In the meantime, you can <a href="/new">start a discussion</a> about your suggestion.</p>
  {/if}
</section>
