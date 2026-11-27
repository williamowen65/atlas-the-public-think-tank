<script lang="ts">
  import type { NodeType } from '../api/nodePage';

  let { types, onChoose }: { types: NodeType[]; onChoose: (id: string) => void } = $props();

  const examples: ReadonlyMap<string, string> = new Map([
    ['issue', 'The waterfront is hard to reach without a car.'],
    ['question', 'How could we make the waterfront easier to visit?'],
    ['idea', 'Add benches along the waterfront path.'],
    ['solution', 'Run a weekend shuttle from downtown.'],
    ['evidence', 'Weekend bus service ends before the last ferry arrives.'],
    ['relationship', 'The shuttle and ferry proposals both depend on weekend service.'],
    ['location', 'Ferry terminal entrance'],
  ]);
</script>

<div class="workshop">
  <div class="chooser">
    <h2><label for="standalone-node-type">Choose a node type</label></h2>
    <p>Pick the type that fits what you want to add. Your choice opens the editor.</p>
    <select id="standalone-node-type" value="" required
      onchange={(event) => onChoose(event.currentTarget.value)}>
      <option value="" disabled>Select a node type…</option>
      {#each types as type (type.id)}<option value={type.id}>{type.name}</option>{/each}
      <option value="new">Define a new node type…</option>
    </select>

    {#if types.length}
      <details>
        <summary>See node type examples</summary>
        <dl>
          {#each types as type (type.id)}
            {@const example = examples.get(type.name.trim().toLowerCase())}
            {#if type.description || example}
              <dt>{type.name}</dt>
              <dd>
                {#if type.description}<span>{type.description}</span>{/if}
                {#if example}<span class="example">{example}</span>{/if}
              </dd>
            {/if}
          {/each}
        </dl>
      </details>
    {/if}

    <div class="placement">
      <h3>Where does your idea fit?</h3>
      <p>
        Search Atlas for a discussion your idea could build on. If you find one, open it
        and choose <strong>Add a child node</strong>. If you’re starting a new discussion,
        choose a type here.
      </p>
      <a href="/">Browse Discovery</a>
    </div>
  </div>

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
</div>

<style>
  .workshop {
    display: grid;
    gap: clamp(1rem, 3vw, 2rem);
    padding-block: .75rem 2rem;
  }
  h2, h3, p { margin: 0; }
  h2 { font-size: 1.25rem; }
  h3 { font-size: 1rem; }
  p { line-height: 1.5; }
  .chooser, .placement, .lesson, .comparison > div { display: grid; align-content: start; gap: .65rem; min-width: 0; }
  select {
    font: inherit;
    width: 100%;
    min-width: 0;
    padding: .55rem .75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  summary { cursor: pointer; font-weight: 600; }
  dl { margin-block: .75rem 0; }
  dt { font-weight: 650; }
  dd { margin: .2rem 0 .85rem; }
  dd span { display: block; }
  .example { font-style: italic; }
  .placement { border-top: 1px solid var(--atlas-border); margin-top: .5rem; padding-top: 1rem; }
  .lesson {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    padding: clamp(.9rem, 2.5vw, 1.5rem);
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
  @media (min-width: 50rem) {
    .workshop { grid-template-columns: minmax(13rem, 1fr) minmax(0, 2.3fr); }
  }
  @media (min-width: 66rem) {
    .comparison { grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); }
  }
</style>
