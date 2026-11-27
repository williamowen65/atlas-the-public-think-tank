<svelte:head><title>How to Write a Node · Atlas</title></svelte:head>

<div class="guide">
  <header>
    <h1>How to Write a Node</h1>
    <p>A node gives one question, idea, or piece of evidence a place in a discussion. Start with what you want others to respond to or build on.</p>
    <a href="/new">Create a node</a>
  </header>

  <section class="lesson" aria-labelledby="split-node-heading">
    <h2 id="split-node-heading">Give each idea its own node</h2>
    <p>Could someone vote differently on two ideas in your draft? Give each idea a node of its own.</p>
    <div class="comparison">
      <div>
        <h3>One node with several ideas</h3>
        <div class="example-node">
          <strong>Make the waterfront easier to visit</strong>
          <p>We should add benches along the path, run a weekend shuttle, and put the first benches near the ferry terminal.</p>
        </div>
        <p>One vote covers the whole bundle. Someone could want benches and have reservations about the shuttle.</p>
      </div>
      <div>
        <h3>Separate nodes in one discussion</h3>
        <ul class="branch" role="list">
          <li>
            <div class="example-node"><span>Question</span><strong>How could we make the waterfront easier to visit?</strong></div>
            <ul role="list">
              <li>
                <div class="example-node"><span>Idea</span><strong>Add benches along the path</strong></div>
                <ul role="list">
                  <li><div class="example-node"><span>Location</span><strong>Start near the ferry terminal</strong></div></li>
                </ul>
              </li>
              <li><div class="example-node"><span>Solution</span><strong>Run a weekend shuttle</strong></div></li>
            </ul>
          </li>
        </ul>
        <p>People can find and vote on each proposal, and add a location suggestion directly under the bench idea.</p>
      </div>
    </div>
  </section>

  <section aria-labelledby="placement-heading">
    <h2 id="placement-heading">Find a place for your contribution</h2>
    <p>If your contribution answers a question, offers evidence, or develops an idea, attach it to the node it directly builds on. In the waterfront example, the ferry terminal suggestion belongs under the bench idea.</p>
    <ol>
      <li><a href="/">Browse Discovery</a> or use Search to find an existing discussion.</li>
      <li>Open the node your contribution builds on and choose <strong>Add a child node</strong>.</li>
      <li>Pick a node type, write your contribution, and publish it.</li>
    </ol>
    <p>Starting a new discussion? <a href="/new">Create a node</a> and give it a title that tells readers what they can respond to.</p>
  </section>
</div>

<style>
  .guide {
    max-width: 58rem;
    margin-inline: auto;
    display: grid;
    gap: 1.5rem;
    padding-block: 1rem 2rem;
  }
  header, section, .comparison > div { display: grid; align-content: start; gap: .75rem; min-width: 0; }
  h1, h2, h3, p, ol { margin: 0; }
  p, li { line-height: 1.5; }
  ol { padding-inline-start: 1.4rem; }
  ol li + li { margin-top: .35rem; }
  .lesson {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    padding: clamp(.9rem, 3vw, 1.5rem);
  }
  .comparison { display: grid; gap: 1rem; margin-top: .3rem; }
  .example-node {
    display: grid;
    gap: .25rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: .55rem .7rem;
    overflow-wrap: anywhere;
  }
  .example-node span { color: var(--atlas-muted); font-size: .85rem; }
  .branch, .branch ul { list-style: none; margin: 0; padding: 0; }
  .branch ul { margin-left: .45rem; border-left: 2px solid var(--atlas-border); }
  .branch ul > li { position: relative; padding: .4rem 0 0 .65rem; }
  .branch ul > li::before {
    content: '';
    position: absolute;
    top: 1.4rem;
    left: 0;
    width: .65rem;
    border-top: 2px solid var(--atlas-border);
  }
  @media (min-width: 45rem) {
    .comparison { grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); }
  }
</style>
