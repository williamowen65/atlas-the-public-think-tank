# Local search ranking diagnostics

Run `npm run storybook:ranking` from `src/hosts/Atlas.Web`, then open **Compositions → Search ranking → Workbench**.

The separate loopback-only diagnostics host uses `ATLAS_SQL_CONNECTION_STRING` from the same shared user secrets or environment as Atlas.Api. The API and diagnostics host can run together. No SQL test connection is required. The host does not create, seed, reset or delete databases, apply migrations, or change votes/content.

Enter the same search terms and scope used in the application. Results use `SqlPublicSearch.Explain`, which runs the same ranking and pagination as public search. Scrolling to the end of the table automatically appends the next page while retaining earlier rows and indexed-text details. A new search clears the accumulated results; a failed continuation keeps them and offers retry. The table shows actual SQL relevance, node vote count and average or comment net score, support, multiplier and final score. Expand results for indexed text and tie dates; export inputs and diagnostics for investigation. Search again after changing data in the app. Text edits may wait for asynchronous full-text indexing.

The story no longer offers fixture scenarios or editors. Controlled fixtures remain in `tests/Fixtures/SearchRanking/scenarios.json` and continue to drive SQL regression assertions independently of Storybook. Browser tests cover the live-data diagnostic interface with transport stubs.
