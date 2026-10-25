<script lang="ts">
  import definitions from '../../../../../tests/Fixtures/SearchRanking/scenarios.json';
  import type { Scenario, Report, Entry } from './types';
  const fixtures = definitions as Scenario[];
  let selected = $state(fixtures[0].id);
  let scenario = $state<Scenario>(structuredClone(fixtures[0]));
  let votes = $state<Record<string, string>>({});
  let blocks = $state<Record<string, string>>({});
  let busy = $state(false);
  let error = $state('');
  let report = $state<Report | null>(null);
  let phase = $state(0);
  let submitted = $state('');
  let lastPayload = $state<Scenario | null>(null);
  let modified = $derived(
    JSON.stringify(scenario) !== JSON.stringify(fixtures.find((f) => f.id === selected)) ||
      scenario.entries.some(
        (entry) =>
          votes[entry.key] !== entry.votes.join(', ') ||
          blocks[entry.key] !== (entry.blocks ?? []).join('\n'),
      ),
  );
  let changedSinceRun = $derived(submitted !== JSON.stringify({ scenario, votes, blocks }));
  let result = $derived(report?.phases[phase]);

  function reset() {
    scenario = structuredClone(fixtures.find((f) => f.id === selected)!);
    votes = Object.fromEntries(scenario.entries.map((e) => [e.key, e.votes.join(', ')]));
    blocks = Object.fromEntries(scenario.entries.map((e) => [e.key, (e.blocks ?? []).join('\n')]));
    report = null;
    error = '';
    phase = 0;
    submitted = '';
  }
  reset();
  function parseVotes(entry: Entry) {
    const text = votes[entry.key].trim();
    if (!text) return [];
    const values = text.split(',').map((value) => value.trim());
    if (values.some((value) => !/^-?\d+$/.test(value)))
      throw new Error(`${entry.key}: use comma-separated whole-number votes.`);
    return values.map(Number);
  }
  async function run() {
    if (busy) return;
    error = '';
    let payload: Scenario;
    try {
      payload = {
        ...scenario,
        entries: scenario.entries.map((entry) => ({
          ...entry,
          votes: parseVotes(entry),
          blocks: blocks[entry.key] ? blocks[entry.key].split('\n') : [],
          sameTextAs: entry.sameTextAs || null,
        })),
      };
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Invalid votes.';
      return;
    }
    busy = true;
    report = null;
    const fingerprint = JSON.stringify({ scenario, votes, blocks });
    try {
      const response = await fetch('/atlas-ranking-lab/run', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        signal: AbortSignal.timeout(180000),
      });
      const body = await response.json();
      if (!response.ok) throw new Error(body.message ?? 'Ranking Lab is unavailable.');
      report = body as Report;
      lastPayload = payload;
      submitted = fingerprint;
      phase = 0;
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Could not run scenario.';
      if (error.includes('JSON') || error.includes('fetch'))
        error =
          'Ranking Lab is unavailable. Start npm run storybook:ranking with SQL test credentials configured.';
    } finally {
      busy = false;
    }
  }
  const number = (value: number) => value.toFixed(4);
</script>

<main>
  <header>
    <p class="eyebrow">Atlas · Search ranking workbench</p>
    <h1>See why one result outranks another</h1>
    <p>
      Run the real SQL search against a small, disposable corpus. These scenario definitions also
      power backend assertions.
    </p>
    <p class="setup">
      Start with <code>npm run storybook:ranking</code>. Configure
      <code>ATLAS_SQL_TEST_CONNECTION_STRING</code> in shared user secrets or the environment. The SQL
      account needs permission to create and delete disposable databases and install full-text indexes.
    </p>
  </header>

  <section aria-labelledby="scenario-heading">
    <h2 id="scenario-heading">Scenario inputs</h2>
    <fieldset disabled={busy}>
      <div class="toolbar">
        <label
          >Scenario<select aria-label="Scenario" bind:value={selected} onchange={reset}
            >{#each fixtures as fixture}<option value={fixture.id}>{fixture.name}</option
              >{/each}</select
          ></label
        >
        <button type="button" onclick={reset}>Reset scenario</button>
        <button type="button" class="primary" onclick={() => void run()}>Run scenario</button>
      </div>
      <p>{scenario.description}</p>
      <label>Search terms<input bind:value={scenario.query} maxlength="200" /></label>
      <p class="muted">
        {modified
          ? 'Modified inputs: saved expectations remain visible as comparisons.'
          : 'Saved fixture: expectations below are enforced by SQL integration tests.'}
      </p>
      <div class="entries">
        {#each scenario.entries as entry (entry.key)}
          <details>
            <summary
              >{entry.key} <span class="kind">{entry.kind}</span> · {entry.votes.length} saved votes</summary
            >
            <div class="editor">
              {#if entry.kind === 'node'}
                <label
                  >Title for {entry.key}<input bind:value={entry.title} maxlength="200" /></label
                >
                <label
                  >Short description for {entry.key}<textarea
                    bind:value={entry.shortDescription}
                    maxlength="280"></textarea></label
                >
                <label
                  >Markdown blocks for {entry.key} (one per line)<textarea
                    bind:value={blocks[entry.key]}></textarea></label
                >
              {:else}
                <label
                  >Match complete node text for {entry.key}<select bind:value={entry.sameTextAs}
                    ><option value="">Use comment body below</option
                    >{#each scenario.entries.filter((e) => e.kind === 'node') as node}<option
                        value={node.key}>{node.key}</option
                      >{/each}</select
                  ></label
                >
                <label
                  >Body for {entry.key}<textarea
                    bind:value={entry.body}
                    disabled={!!entry.sameTextAs}
                    maxlength="10000"></textarea></label
                >
              {/if}
              <label
                >Votes for {entry.key} ({entry.kind === 'node' ? '0–10' : '-1 or +1'},
                comma-separated)<input bind:value={votes[entry.key]} /></label
              >
              <label
                >Ranking date for {entry.key} (ISO timestamp)<input bind:value={entry.at} /></label
              >
            </div>
          </details>
        {/each}
      </div>
    </fieldset>
    <details>
      <summary>Saved phases and expectations</summary>{#each scenario.phases as item}<p>
          <strong>{item.name}:</strong>
          {item.expectedOrder.length
            ? item.expectedOrder.join(' → ')
            : 'Exploration: no cross-kind ordering assumption.'}
        </p>
        {#if Object.keys(item.votes).length}<p>
            Vote overrides: <code>{JSON.stringify(item.votes)}</code>. Overrides carry forward until
            replaced.
          </p>{/if}{#if item.sameScoresAs}<p>
            Scores and order must match {item.sameScoresAs}.
          </p>{/if}{/each}
    </details>
  </section>
  {#if busy}<p role="status">
      Creating an isolated corpus, waiting for its initial text index, and running every phase…
    </p>{/if}
  {#if error}<p role="alert" class="error">{error}</p>{/if}

  {#if report && result}
    <section aria-labelledby="results-heading">
      <h2 id="results-heading">Ranking diagnostics</h2>
      <p>
        {report.corpus} Policy: <strong>{report.policy}</strong>. Votes change between phases
        without reindexing.
      </p>
      {#if changedSinceRun}<p role="status">
          Inputs changed. These are results from the last run; run again to update them.
        </p>{/if}
      <label
        >Result phase<select aria-label="Result phase" bind:value={phase}
          >{#each report.phases as item, i}<option value={i}>{item.name}</option>{/each}</select
        ></label
      >
      <ul class="checks">
        {#each result.checks as check}<li class:failed={!check.passed}>
            <strong>{check.passed ? 'PASS' : 'FAIL'}</strong> — {check.description}
          </li>{/each}
      </ul>
      <!-- svelte-ignore a11y_no_noninteractive_tabindex (Keyboard users need focus here to scroll the wide diagnostic table.) -->
      <div class="table-wrap" role="region" aria-label="Ranking diagnostic table" tabindex="0">
        <table>
          <caption>Actual SQL order · {result.name}</caption>
          <thead
            ><tr
              ><th scope="col">Rank / page</th><th scope="col">Result</th><th scope="col"
                >Text relevance</th
              ><th scope="col">Votes</th><th scope="col">Average / net</th><th scope="col"
                >Support</th
              ><th scope="col">Multiplier</th><th scope="col">Final score</th></tr
            ></thead
          >
          <tbody
            >{#each result.rows as row}<tr>
                <td>{row.position} / {row.page}</td><th scope="row"
                  >{row.key}<small>{row.evidence.kind}</small></th
                ><td>{row.evidence.textRank}</td><td
                  >{row.votes.length ? row.votes.join(', ') : 'No votes'}</td
                ><td
                  >{row.evidence.kind === 'node'
                    ? row.average === null
                      ? 'Unrated'
                      : row.average.toFixed(2)
                    : row.netScore}</td
                ><td>{number(row.evidence.support)}</td><td>×{number(row.evidence.multiplier)}</td
                ><td><strong>{number(row.evidence.finalScore)}</strong></td>
              </tr>{/each}</tbody
          >
        </table>
      </div>
      <h3>Actual searchable text and tie-breaks</h3>
      {#each result.rows as row}<details>
          <summary>{row.position}. {row.key} · {row.evidence.kind}</summary>
          <pre>{row.searchText}</pre>
          <p>
            After equal final scores: node before comment, then newer ranking date, then SQL GUID
            order.
          </p>
          <p>
            Date: <time datetime={row.evidence.tieDate}>{row.evidence.tieDate}</time><br />ID:
            <code>{row.evidence.id}</code>
          </p>
        </details>{/each}
      <details>
        <summary>Exportable diagnostics</summary>
        <p>Copy these inputs and results into a bug report to reproduce the scenario.</p>
        <textarea
          class="export"
          aria-label="Scenario inputs and ranking results"
          readonly
          value={JSON.stringify({ inputs: lastPayload, report }, null, 2)}></textarea>
      </details>
    </section>
  {/if}
  <footer>
    Raw full-text relevance depends on the entire corpus. A shared index makes the ranks comparable;
    it does not guarantee equal relevance when only one word matches.
  </footer>
</main>

<style lang="scss">
  main {
    max-width: 1200px;
    margin: 0 auto;
    padding: clamp(1rem, 3vw, 2.5rem);
    color: var(--atlas-text);
  }
  h1 {
    font-size: clamp(1.6rem, 3vw, 2.3rem);
    margin: 0.35rem 0 0.8rem;
  }
  h2 {
    font-size: 1.25rem;
  }
  p {
    line-height: 1.6;
  }
  .eyebrow,
  .muted,
  footer,
  small {
    color: var(--atlas-muted);
  }
  section {
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: 1.25rem;
    margin: 1.4rem 0;
  }
  .setup {
    font-size: 0.9rem;
  }
  fieldset {
    border: 0;
    padding: 0;
    margin: 0;
    min-width: 0;
  }
  .toolbar {
    display: flex;
    align-items: end;
    gap: 0.75rem;
    flex-wrap: wrap;
  }
  .toolbar label {
    flex: 1;
    min-width: min(100%, 18rem);
  }
  label {
    display: grid;
    gap: 0.35rem;
    margin: 0.65rem 0;
    font-size: 0.9rem;
  }
  input,
  select,
  textarea,
  button {
    font: inherit;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    padding: 0.55rem 0.7rem;
  }
  input,
  select,
  textarea {
    box-sizing: border-box;
    max-width: 100%;
    min-width: 0;
    width: 100%;
  }
  textarea {
    min-height: 5rem;
    resize: vertical;
  }
  button,
  summary {
    cursor: pointer;
  }
  button:disabled {
    opacity: 0.6;
  }
  .primary {
    background: var(--atlas-accent);
    color: var(--atlas-surface);
  }
  details {
    border-top: 1px solid var(--atlas-border);
    padding: 0.8rem 0;
  }
  summary {
    font-weight: 600;
  }
  .editor {
    padding: 0.5rem 0.2rem;
  }
  .kind {
    font-size: 0.8rem;
    color: var(--atlas-muted);
  }
  .table-wrap {
    overflow-x: auto;
    margin: 1rem 0;
  }
  table {
    border-collapse: collapse;
    width: 100%;
    font-size: 0.9rem;
  }
  caption {
    text-align: left;
    font-weight: 600;
    margin-bottom: 0.75rem;
  }
  th,
  td {
    text-align: left;
    padding: 0.7rem;
    border-bottom: 1px solid var(--atlas-border);
    vertical-align: top;
  }
  small {
    display: block;
    font-weight: normal;
  }
  .checks {
    padding-left: 1.2rem;
  }
  .checks li {
    padding: 0.3rem 0;
  }
  .error,
  .failed {
    color: #b3261e;
  }
  pre {
    white-space: pre-wrap;
    overflow-wrap: anywhere;
    background: var(--atlas-bg);
    padding: 1rem;
  }
  .export {
    min-height: 16rem;
    font-family: monospace;
  }
  footer {
    font-size: 0.85rem;
    line-height: 1.6;
  }
</style>
