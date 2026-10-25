<script lang="ts">
  import type { Evidence } from './types';
  import { words, matches, parts, passage, experiment } from './evidence';
  type Report = {
    policy: string;
    page: { totalCount: number; page: number; hasNextPage: boolean };
    rows: {
      sources?: { field: string; text: string; blockId?: string | null }[];
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
  let votingPercent = $state(40);
  let query = $state('');
  let scope = $state('all');
  let busy = $state(false);
  let error = $state('');
  let report = $state<Report | null>(null);
  const experimental = $derived(
    (report?.rows ?? [])
      .map((row) => ({ row, ...experiment(row.evidence, votingPercent) }))
      .sort((a, b) => b.score - a.score || a.row.position - b.row.position),
  );
  let submitted = $state<{ query: { searchText: string; page: number }; scope: string } | null>(
    null,
  );
  let sentinel = $state<HTMLDivElement>();
  $effect(() => {
    if (
      !sentinel ||
      busy ||
      error ||
      !report?.page.hasNextPage ||
      submitted?.query.searchText !== query ||
      submitted?.scope !== scope
    )
      return;
    const observer = new IntersectionObserver((entries) => {
      if (entries.some((entry) => entry.isIntersecting)) void run(report!.page.page + 1);
    });
    observer.observe(sentinel);
    return () => observer.disconnect();
  });
  async function run(page = 1) {
    if (busy) return;
    busy = true;
    error = '';
    if (page === 1) report = null;
    const payload = {
      query: { searchText: page === 1 ? query : submitted!.query.searchText, page },
      scope: page === 1 ? scope : submitted!.scope,
    };
    try {
      const response = await fetch('/atlas-ranking-lab/run', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        signal: AbortSignal.timeout(60000),
      });
      const body = await response.json();
      if (!response.ok) throw new Error(body.message ?? 'Local ranking search is unavailable.');
      const next = body as Report;
      if (page > 1 && report) {
        const seen = new Set(report.rows.map((row) => row.evidence.id));
        next.rows = [...report.rows, ...next.rows.filter((row) => !seen.has(row.evidence.id))];
      }
      report = next;
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
    <p>
      {report.page.totalCount} results · Pages loaded: {report.page.page} · Policy {report.policy}
    </p>
    {#if submitted?.query.searchText !== query || submitted?.scope !== scope}<p role="status">
        Inputs changed. Search again to update these results.
      </p>{/if}
    <div class="table-wrap">
      <table>
        <thead
          ><tr
            ><th>Rank</th><th>Result</th><th>Text relevance</th><th>Indexed words / literal hits</th
            ><th>Vote count</th><th>Average / net</th><th>Support</th><th>Multiplier</th><th
              >Final score</th
            ></tr
          ></thead
        >
        <tbody
          >{#each report.rows as row}<tr>
              <td>{row.position}</td><th
                >{row.item.card?.title ?? row.item.comment?.nodeTitle}<small
                  >{row.evidence.kind}</small
                >
                <p>
                  {#each parts(passage(row.searchText, submitted?.query.searchText ?? ''), submitted?.query.searchText ?? '') as part}{#if part.match}<mark
                        >{part.text}</mark
                      >{:else}{part.text}{/if}{/each}
                </p></th
              >
              <td>{row.evidence.textRank}</td><td
                >{words(row.searchText).length} / {matches(
                  row.searchText,
                  submitted?.query.searchText ?? '',
                )}</td
              ><td>{row.evidence.kind === 'node' ? row.item.voteCount : '—'}</td>
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
    <div bind:this={sentinel} aria-hidden="true" style="height: 1px"></div>
    {#if error && report.page.hasNextPage}
      <button disabled={busy} onclick={() => void run(report!.page.page + 1)}
        >Retry loading results</button
      >
    {/if}
    <section aria-label="Experimental ranking comparison">
      <h3>Experimental order among loaded results</h3>
      <p>
        Preview only: public search still uses {report.policy}. Comparing {report.rows.length} of {report
          .page.totalCount} results. Unloaded candidates are not included; this is not a global experimental
        rank.
      </p>
      <label
        >Voting weight (%)<input
          type="range"
          min="0"
          max="100"
          step="5"
          bind:value={votingPercent}
        /></label
      >
      <p>
        {100 - votingPercent}% text + {votingPercent}% voting. Text = ln(1 + SQL relevance) /
        ln(1001). Node voting = support / 10; comment voting = net / (5 + |net|).
      </p>
      <button type="button" onclick={() => (votingPercent = 40)}>Reset experiment</button>
      <p>
        Logarithmic compression narrows relevance gaps. Node voting retains the average ×
        count/(count+1) confidence adjustment. Comment voting approaches +1 or −1 as net votes grow.
        These scales are a hypothesis to evaluate, not equivalent measures of quality. At 0% voting,
        text ordering is preserved. Ties retain the current loaded order.
      </p>
      <div class="table-wrap">
        <table aria-label="Experimental ranking">
          <thead
            ><tr
              ><th>Preview rank</th><th>Current rank</th><th>Result</th><th>Compressed text</th><th
                >Voting signal</th
              ><th>Text contribution</th><th>Voting contribution</th><th>Experimental score</th></tr
            ></thead
          >
          <tbody
            >{#each experimental as entry, i}<tr>
                <td>{i + 1}</td><td>{entry.row.position}</td><th
                  >{entry.row.item.card?.title ?? entry.row.item.comment?.nodeTitle}<small
                    >{entry.row.evidence.kind}</small
                  ></th
                >
                <td>{number(entry.text)}</td><td>{number(entry.vote)}</td><td
                  >{number(entry.textContribution)}</td
                ><td>{number(entry.voteContribution)}</td><td>{number(entry.score)}</td>
              </tr>{/each}</tbody
          >
        </table>
      </div>
    </section>
    <h3>Indexed text and tie-breaks</h3>
    <p>
      Support: node average × vote count / (vote count + 1); comment net upvotes minus downvotes.
      Current multiplier = 1 + 0.25 × support / (10 + |support|). Current final score = SQL
      relevance × multiplier.
    </p>
    <p>
      Equal final scores sort by node before comment, then newer ranking date, then SQL GUID order.
      Excerpts are display-only and never added to relevance.
    </p>
    <p>
      Word counts and highlighted hits below use literal, case-insensitive Unicode words, not SQL's
      tokenizer. SQL also matches inflections and can match “test” when you search “testing”. Zero
      literal hits does not mean zero SQL matches. Source fields are read after ranking; edits or
      asynchronous indexing can temporarily make them differ from the indexed text.
    </p>
    {#each report.rows as row}<details>
        <summary
          >{row.position}. {row.item.card?.title ?? row.item.comment?.nodeTitle} · {row.evidence
            .kind} · {words(row.searchText).length} indexed words · {matches(
            row.searchText,
            submitted?.query.searchText ?? '',
          )} literal hits</summary
        >
        <h4>Source fields and matching passages</h4>
        {#each row.sources ?? [] as source}
          <section class="source">
            <h5>
              {source.field} · {words(source.text).length} words · {matches(
                source.text,
                submitted?.query.searchText ?? '',
              )} literal hits
            </h5>
            <p>
              {#each parts(passage(source.text, submitted?.query.searchText ?? ''), submitted?.query.searchText ?? '') as part}{#if part.match}<mark
                    >{part.text}</mark
                  >{:else}{part.text}{/if}{/each}
            </p>
            <details>
              <summary>Full field text</summary>
              <pre>{#each parts(source.text, submitted?.query.searchText ?? '') as part}{#if part.match}<mark
                      >{part.text}</mark
                    >{:else}{part.text}{/if}{/each}</pre>
            </details>
          </section>
        {:else}<p>Field evidence unavailable. Restart the ranking backend after pulling.</p>{/each}
        <h4>Exact search projection text</h4>
        <pre>{#each parts(row.searchText, submitted?.query.searchText ?? '') as part}{#if part.match}<mark
                >{part.text}</mark
              >{:else}{part.text}{/if}{/each}</pre>
        <p>Ranking date: {row.evidence.tieDate} · Result ID: {row.evidence.id}</p>
      </details>{/each}
    <details>
      <summary>Exportable diagnostics</summary><textarea
        aria-label="Search inputs and ranking results"
        readonly
        value={JSON.stringify(
          {
            inputs: submitted,
            report,
            experiment: {
              votingPercent,
              scope: 'loaded results only',
              rows: experimental.map(({ row, ...score }) => ({
                id: row.evidence.id,
                currentRank: row.position,
                ...score,
              })),
            },
          },
          null,
          2,
        )}></textarea>
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
  .source {
    border-left: 3px solid var(--atlas-border);
    padding-left: 1rem;
  }
  mark {
    background: #ffe48a;
    color: #202020;
  }
  [role='alert'] {
    color: #b3261e;
  }
</style>
