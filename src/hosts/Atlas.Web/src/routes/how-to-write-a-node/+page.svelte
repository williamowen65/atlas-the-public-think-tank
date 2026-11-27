<script lang="ts">
  import { onDestroy } from 'svelte';
  import { readTypes, type NodeType } from '$lib/api/nodePage';

  const controller = new AbortController();
  let types = $state<NodeType[]>();
  let loadingTypes = $state(false);
  let typesError = $state(false);

  async function loadTypes() {
    if (types || loadingTypes) return;
    loadingTypes = true;
    typesError = false;
    try {
      types = await readTypes('/atlas-public-api', controller.signal);
    } catch {
      if (!controller.signal.aborted) typesError = true;
    } finally {
      loadingTypes = false;
    }
  }

  onDestroy(() => controller.abort());
</script>

<svelte:head><title>How to Write a Node · Atlas</title></svelte:head>

<div class="guide">
  <header>
    <h1>How to Write a Node</h1>
    <p>On Atlas, a node is a contribution to a discussion. It can be anything you want to share, from a question or an idea to an observation or a photo. Pick a type that fits, or define your own.</p>
    <details class="type-catalog" ontoggle={(event) => {
      if (event.currentTarget.open) void loadTypes();
    }}>
      <summary>Explore Node Types</summary>
      {#if types?.length}
        <dl>
          {#each types as type (type.id)}
            <div>
              <dt>{type.name}</dt>
              {#if type.description}<dd>{type.description}</dd>{/if}
            </div>
          {/each}
        </dl>
      {:else if types}
        <p>No node types are available.</p>
      {:else if typesError}
        <p role="alert">Could not load node types.</p>
        <button type="button" onclick={loadTypes}>Retry</button>
      {:else if loadingTypes}
        <p role="status">Loading node types…</p>
      {/if}
    </details>
  </header>

  <figure class="tree-diagram" aria-label="A root node has two child nodes, and one child has a child of its own">
    <div class="tree-root">
      <strong>Root node</strong>
      <span>Starts a discussion</span>
    </div>
    <div class="tree-children">
      <div>Child node</div>
      <div>Child node</div>
    </div>
    <div class="tree-third-level">
      <div>Child node</div>
    </div>
    <figcaption>Child nodes, also called subnodes, can have children of their own.</figcaption>
  </figure>

  <section class="basics" aria-labelledby="basics-heading">
    <h2 id="basics-heading">The Basics</h2>
    <div class="creation-options">
      <div>
        <h3>Start a discussion</h3>
        <p>Choose <a href="/new">Create a root node</a>, pick a type, then write and publish your contribution.</p>
      </div>
      <div>
        <h3>Add a subnode</h3>
        <ol>
          <li><a href="/">Browse Discovery</a> or use Search to find an existing discussion.</li>
          <li>Open the specific node you want to build on and choose <strong>Add a child node</strong>.</li>
          <li>Pick a type, then write and publish your contribution.</li>
        </ol>
      </div>
    </div>
  </section>

  <section class="lesson" aria-labelledby="split-node-heading">
    <h2 id="split-node-heading">Give each idea its own node</h2>
    <p>As you write, ask yourself whether someone might vote yes on one idea and no on another. If so, give each idea its own node.</p>
    <div class="comparison">
      <div class="bundled">
        <h3><span aria-hidden="true">✕</span> Avoid bundling ideas</h3>
        <div class="example-node">
          <strong>Make the playground better</strong>
          <p>Let’s add swings and plant a shade tree.</p>
        </div>
        <p class="reason"><strong>One vote for both.</strong> What if you want swings but have a different idea for shade?</p>
      </div>
      <div class="separate">
        <h3><span aria-hidden="true">✓</span> Give each idea a branch</h3>
        <ul class="branch" role="list">
          <li>
            <div class="example-node"><span>Root node</span><strong>How could we make the playground better?</strong></div>
            <ul role="list">
              <li><div class="example-node"><span>Child node</span><strong>Add swings</strong></div></li>
              <li><div class="example-node"><span>Child node</span><strong>Plant a shade tree</strong></div></li>
            </ul>
          </li>
        </ul>
        <p class="reason"><strong>Each idea gets its own vote.</strong> You can also add “Put the swings next to the slide” directly under “Add swings.”</p>
      </div>
    </div>
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
  header, section, .creation-options > div, .comparison > div { display: grid; align-content: start; gap: .75rem; min-width: 0; }
  h1, h2, h3, p, ol, figure { margin: 0; }
  .type-catalog {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: .65rem .8rem;
    background: var(--atlas-surface);
  }
  .type-catalog summary { cursor: pointer; font-weight: 600; }
  .type-catalog dl { margin: .65rem 0 0; max-height: 18rem; overflow: auto; }
  .type-catalog dl > div { padding: .45rem .25rem; border-top: 1px solid var(--atlas-border); }
  .type-catalog dt { font-weight: 600; }
  .type-catalog dd { margin: .15rem 0 0; color: var(--atlas-muted); }
  .tree-diagram {
    max-width: 25rem;
    width: 100%;
    margin-inline: auto;
    text-align: center;
  }
  .tree-root, .tree-children > div, .tree-third-level > div {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    padding: .5rem;
  }
  .tree-root {
    display: grid;
    width: fit-content;
    margin-inline: auto;
    padding-inline: 1rem;
  }
  .tree-root span, figcaption { color: var(--atlas-muted); font-size: .9rem; }
  .tree-children {
    position: relative;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: .65rem;
    padding-top: 1.75rem;
  }
  .tree-children::before, .tree-children::after, .tree-children > div::before {
    content: '';
    position: absolute;
    border-color: var(--atlas-muted);
    border-style: solid;
  }
  .tree-children::before { top: 0; left: 50%; height: .85rem; border-width: 0 0 0 1px; }
  .tree-children::after {
    top: .85rem;
    inset-inline: calc((100% - .65rem) / 4);
    height: .9rem;
    border-width: 1px 1px 0;
  }
  .tree-children > div { position: relative; }
  .tree-children > div::before {
    top: -.43rem;
    left: calc(50% - .2rem);
    width: .4rem;
    height: .4rem;
    border-width: 0 1px 1px 0;
    transform: rotate(45deg);
  }
  .tree-third-level {
    width: calc((100% - .65rem) / 2);
    position: relative;
    padding-top: 1.45rem;
  }
  .tree-third-level::before, .tree-third-level::after {
    content: '';
    position: absolute;
    border-color: var(--atlas-muted);
    border-style: solid;
  }
  .tree-third-level::before {
    top: 0;
    left: 50%;
    height: 1.45rem;
    border-width: 0 0 0 1px;
  }
  .tree-third-level::after {
    top: 1rem;
    left: calc(50% - .2rem);
    width: .4rem;
    height: .4rem;
    border-width: 0 1px 1px 0;
    transform: rotate(45deg);
  }
  figcaption { margin-top: .65rem; }
  .creation-options { display: grid; gap: 1rem; }
  .basics {
    border-top: 1px solid var(--atlas-border);
    padding-top: 1rem;
  }
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
  .comparison > div {
    padding: .8rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  .comparison h3, .reason {
    padding: .5rem .65rem;
    border-radius: var(--atlas-radius);
    color: var(--example-ink);
    background: var(--example-tint);
  }
  .bundled { --example-ink: #791c23; --example-tint: #fce4e5; }
  .separate { --example-ink: #175433; --example-tint: #e0f4e7; }
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
    .creation-options, .comparison { grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); }
  }
</style>
