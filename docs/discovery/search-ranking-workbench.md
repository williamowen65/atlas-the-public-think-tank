# Local search ranking diagnostics

Run `npm run storybook:ranking` from `src/hosts/Atlas.Web`, then open **Compositions → Search ranking → Workbench**.

The separate loopback-only diagnostics host uses `ATLAS_SQL_CONNECTION_STRING` from the same shared user secrets or environment as Atlas.Api. The API and diagnostics host can run together. No SQL test connection is required. The host does not create, seed, reset or delete databases, apply migrations, or change votes/content.

Enter the same search terms and scope used in the application. Results use `SqlPublicSearch.Explain`, which runs the same ranking and pagination as public search. Scrolling to the end of the table automatically appends the next page while retaining earlier rows and indexed-text details. A new search clears the accumulated results; a failed continuation keeps them and offers retry. The table shows actual SQL relevance, node vote count and average or comment net score, support, multiplier and final score. Expand results for indexed text and tie dates; export inputs and diagnostics for investigation. Search again after changing data in the app. Text edits may wait for asynchronous full-text indexing.

The story no longer offers fixture scenarios or editors. Controlled fixtures remain in `tests/Fixtures/SearchRanking/scenarios.json` and continue to drive SQL regression assertions independently of Storybook. Browser tests cover the live-data diagnostic interface with transport stubs.


## Text evidence and an experimental balance

The main table shows backend `weighted-v1` order and scores for the submitted voting weight. It now includes approximate Unicode word counts, case-insensitive whole-word literal hit counts, and highlighted passages for both nodes and comments. Expand a result to inspect title, short description and ordered block search text separately, or the complete comment body, plus the exact search projection. Counts and highlights are explanatory browser calculations, not SQL Server tokenization or BM25 internals. SQL linguistic matches such as test/testing can occur with zero literal hits. Excerpts never contribute to SQL relevance. Source fields are collected after ranking and may differ from indexed data during concurrent edits or index population.

The comparison table reranks **loaded results only** when adjusting its slider. Press Search to apply that weight on the backend across all candidates before pagination. It defaults to 60% text plus 40% voting. Text is `ln(1 + rank) / ln(1001)`; node voting is `support / 10` (clamped to 0–1), while comment voting is `net / (5 + abs(net))`. Negative comment votes reduce the score. Voting weight can be varied from 0% to 100% and reset. At 0% the text ranking is preserved. Equal experimental scores retain the current loaded order. Scores are comparisons, not quality percentages.

This deliberately compresses SQL relevance gaps and increases the influence of voting. Node importance and comment approval are different signals; the scales are a testable hypothesis. In the reported example, the rated 269-relevance node outranks the unvoted 608-relevance comment in the default experiment. That is an authored experiment expectation, not a SQL regression requirement. An unvoted result can still outrank a voted one depending on the text and vote signals. Scroll to load more candidates before judging the experimental order; it cannot account for unseen results. Export includes raw evidence, source fields, weight and component scores for the loaded comparison.

The application sends VotingWeight with global text searches and uses the same additive formula. Clients omitting VotingWeight retain the legacy support-v2 calculation for compatibility. No migration or new SQL files are required. Restart the ranking backend after pulling to return field evidence.


## User voting weight

The application's search Filters and Settings → Search preferences expose a compact 0–100% voting slider, default 40%. Its complement is the text weight. Changes apply immediately to the preference; opening results issues a new page-one query. Subsequent pages retain that selection. Clearing query/filter tokens does not erase this preference; Reset voting weight restores 40%.

Preferences are stored in localStorage per participant (or anonymous visitor) on this browser origin, not synchronized to a server or across devices. Storage failure keeps the in-session value and shows feedback. Settings uses the same control and state as Filters.

`VotingWeight` is an optional integer API query parameter validated from 0 to 100. When supplied, SQL orders the entire matching set by the additive logarithmic-text/voting formula before counting/paging. It changes order, not eligibility. At 0% only text contributes, at 100% only voting contributes; existing deterministic kind/date/ID tie-breaks remain. No-text node browsing keeps its established ordering. The lab sends the selected weight too; its main table is authoritative for that backend run, while slider previews remain limited to loaded candidates until Search is pressed again. No schema migration is required.
