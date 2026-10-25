<script lang="ts">
  import type { Evidence } from './types';
  type Report = {
    policy: string;
    page: { totalCount: number; page: number; hasNextPage: boolean };
    rows: {
      position: number;
      searchText: string;
      evidence: Evidence;
      item: {
        voteCount: number;
        averageVote: number | null;
        card?: { title: string };
        comment?: { nodeTitle: string; excerpt: string; score: number };
      };
    }[];
  };
  let query = $state('');
  let scope = $state('all');
  let busy = $state(false);
  let error = $state('');
  let report = $state<Report | null>(null);
  let submitted = $state<{ query: { searchText: string; page: number }; scope: string } | null>(
    null,
  );
  async function run(page = 1) {
    if (busy) return;
    busy = true;
    error = '';
    report = null;
    const payload = { query: { searchText: query, page }, scope };
    try {
      const response = await fetch('/atlas-ranking-lab/run', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        signal: AbortSignal.timeout(60000),
      });
      const body = await response.json();
      if (!response.ok) throw new Error(body.message ?? 'Local ranking search is unavailable.');
      report = body;
      submitted = payload;
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Search failed.';
      if (error.includes('JSON') || error.includes('fetch'))
        error =
          'Ranking Lab is unavailable. Run npm run storybook:ranking using the same ATLAS_SQL_CONNECTION_STRING as your API.';
    } finally {
      busy = false;
    }
  }
  const number = (value: number) => value.toFixed(4);
</script>

<main>
  <h1>See why one result outranks another</h1>
  <p>
    Inspect actual search results from your local Atlas database. This tool reads your data without
    changing it.
  </p>
  <p>
    Start with <code>npm run storybook:ranking</code>, using the same
    <code>ATLAS_SQL_CONNECTION_STRING</code> as your API.
  </p>
  <form
    onsubmit={(event) => {
      event.preventDefault();
      void run();
    }}
  >
    <label>Search terms<input bind:value={query} maxlength="4000" /></label>
    <label
      >Scope<select bind:value={scope}
        ><option value="all">All</option><option value="nodes">Nodes</option><option
          value="comments">Comments</option
        ></select
      ></label
    >
    <button disabled={busy}>Search</button>
  </form>
  {#if busy}<p role="status">Searching local Atlas data…</p>{/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if report}
    <h2>Ranking diagnostics</h2>
    <p>{report.page.totalCount} results · Page {report.page.page} · Policy {report.policy}</p>
    {#if submitted?.query.searchText !== query || submitted?.scope !== scope}<p role="status">
        Inputs changed. Search again to update these results.
      </p>{/if}
    <div class="table-wrap">
      <table>
        <thead
          ><tr
            ><th>Rank</th><th>Result</th><th>Text relevance</th><th>Vote count</th><th
              >Average / net</th
            ><th>Support</th><th>Multiplier</th><th>Final score</th></tr
          ></thead
        >
        <tbody
          >{#each report.rows as row}<tr>
              <td>{row.position}</td><th
                >{row.item.card?.title ?? row.item.comment?.nodeTitle}<small
                  >{row.evidence.kind}</small
                >{#if row.item.comment}<p>{row.item.comment.excerpt}</p>{/if}</th
              >
              <td>{row.evidence.textRank}</td><td
                >{row.evidence.kind === 'node' ? row.item.voteCount : '—'}</td
              >
              <td
                >{row.evidence.kind === 'node'
                  ? (row.item.averageVote?.toFixed(2) ?? 'Unrated')
                  : row.item.comment?.score}</td
              >
              <td>{number(row.evidence.support)}</td><td>×{number(row.evidence.multiplier)}</td><td
                >{number(row.evidence.finalScore)}</td
              >
            </tr>{/each}</tbody
        >
      </table>
    </div>
    {#if report.page.totalCount === 0}<p>No matching results.</p>{/if}
    <button disabled={busy || report.page.page <= 1} onclick={() => void run(report!.page.page - 1)}
      >Previous page</button
    >
    <button
      disabled={busy || !report.page.hasNextPage}
      onclick={() => void run(report!.page.page + 1)}>Next page</button
    >
    <h3>Indexed text and tie-breaks</h3>
    <p>
      Equal final scores sort by node before comment, then newer ranking date, then SQL GUID order.
    </p>
    {#each report.rows as row}<details>
        <summary>{row.position}. {row.evidence.kind} · {row.evidence.id}</summary>
        <pre>{row.searchText}</pre>
        <p>Ranking date: {row.evidence.tieDate}</p>
      </details>{/each}
    <details>
      <summary>Exportable diagnostics</summary><textarea
        aria-label="Search inputs and ranking results"
        readonly
        value={JSON.stringify({ inputs: submitted, report }, null, 2)}></textarea>
    </details>
  {/if}
  <p>
    Full-text relevance depends on the entire local corpus. Changes in the app appear on the next
    search once text indexing catches up; votes do not require reindexing.
  </p>
</main>

<style>
  main {
    max-width: 1200px;
    margin: auto;
    padding: 2rem;
    color: var(--atlas-text);
  }
  form {
    display: flex;
    gap: 1rem;
    align-items: end;
    flex-wrap: wrap;
  }
  label {
    display: grid;
    gap: 0.5rem;
  }
  input,
  select,
  button,
  textarea {
    font: inherit;
    padding: 0.6rem;
  }
  .table-wrap {
    overflow-x: auto;
  }
  table {
    border-collapse: collapse;
    width: 100%;
    margin: 1rem 0;
  }
  th,
  td {
    text-align: left;
    vertical-align: top;
    padding: 0.7rem;
    border-bottom: 1px solid var(--atlas-border);
  }
  small {
    display: block;
    font-weight: normal;
  }
  pre {
    white-space: pre-wrap;
    overflow-wrap: anywhere;
  }
  textarea {
    box-sizing: border-box;
    width: 100%;
    min-height: 18rem;
  }
  details {
    padding: 0.8rem 0;
  }
  [role='alert'] {
    color: #b3261e;
  }
</style>
