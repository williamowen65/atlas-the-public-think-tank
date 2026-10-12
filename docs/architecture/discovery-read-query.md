# PTT-132: Discovery read-query baseline

Status: initial implementation slice. SQL execution and caching remain unimplemented.

## Implemented boundary

`DiscoveryService` validates requests and delegates to `IDiscoveryReadQuery`. Its existing candidate-source constructor wraps `InMemoryDiscoveryReadQuery`, preserving Console composition and current behavior. `FromReadQuery` accepts a storage implementation without requiring a candidate source. Page requests go directly to page methods; the facade does not fetch unpaged results or slice them again.

The extracted in-memory engine is the reference for SQL parity, not a performance improvement. Existing Discovery tests continue to exercise filtering, ranking, totals, archive status and authored moderation behavior through this adapter. New port tests guard against routing page requests through full-result methods and against invalid input reaching storage.

## SQL implementation requirements

Implement the port in Persistence and switch Console composition only after parity and SQL integration checks pass. Apply predicates, aggregate vote statistics, calculate scores, count matches, order and Skip/Take before materialization. Use existence predicates for community/reaction membership to avoid multiplying votes through joins. Batch-load display data for the selected IDs; inspect the downstream Console display path as well as Discovery itself for query fan-out.

Preserve the score: `(AverageVote ?? 0) + log10(VoteCount + 1) / 100`, plus 0.1 for a case/accent-insensitive title search match. Order by score descending, count descending, updated time descending, case/accent-insensitive title ascending, then GUID ascending. SQL collation and SQL Server GUID ordering do not automatically match these .NET semantics; demonstrate parity or define an explicit equivalent sort key before switching implementations. Content search must preserve the current block-specific searchable fields and case behavior.

Created-date bounds currently use the candidate timestamp's calendar date. Review stored offsets before assuming UTC date predicates are equivalent. Average filters exclude unrated candidates. Reaction filters require every selected definition. Public results exclude hidden nodes; authored scope can include hidden placeholders, but text searches never match hidden candidates. Scope IDs do not establish caller authorization.

Keep fixed page size three and global ranks. Retain a long offset calculation for extreme page numbers and return an empty page with the correct total when beyond the result set. Start with offset paging for API compatibility. Cursor paging requires a separate contract and an explicit policy for changing rankings between requests.

## Evidence required before claiming SQL optimization

Compare first, later and empty pages against the reference using representative SQL fixtures, including combined filters, ranking ties, authored/hidden/archive states and unvoted nodes. Capture generated SQL, query counts, rows materialized and execution plans at increasing data sizes. Measure count and page queries separately and record consistency behavior if a write occurs between them. Add indexes based on those plans rather than assuming database execution is sufficient.

## Caching work after the SQL baseline

No cache is added in this slice. Measure SQL costs first. Query-page keys must include normalized filters, page, page size, ordering version and visibility scope. Separate bounded immutable detail entries keyed by identity from page IDs/totals, so a filter change can reuse details without duplicating graphs. Define capacity, expiration, miss coordination, per-instance versus distributed behavior and personalized-data isolation.

Specify invalidation dependencies for votes, reactions, content, links, archive/restore and moderation. Publish invalidation only after successful commits; test rollback behavior. Revalidate current visibility when resolving cached IDs, so hidden content cannot survive in an old page or detail entry. Expiration alone is insufficient for moderation. Record tests proving detail reuse across filter changes and invalidation of ranking/totals separately from title/detail changes.

## Verification

Run `dotnet test tests/Atlas.Discovery.Tests/Atlas.Discovery.Tests.csproj` and the Console and Moderation suites for this extraction. The authoring environment has no .NET SDK; local execution was unavailable. The repository PR workflow supplies .NET 10 and runs these suites. SQL performance measurements are outstanding.

## Discovery text collation decision

Use `Latin1_General_100_CI_AI_SC` explicitly on NodeRows.Title and BlockRows Markdown, AltText, Caption, Title and Description. Existing nvarchar storage preserves Unicode, including supplementary characters under the SC collation. CI ignores case and AI ignores accents for SQL search/equality/sorting; original spelling is stored unchanged. No storage conversion to UTF-8 is needed. There is no single culturally correct ordering for every language; this is Atlas's shared default, not locale-specific sorting.

The developer-generated column migration must update existing databases, not just newly created ones. Review the generated Down operations: they should remove explicit overrides and restore inheritance from the database default. Identity/account identifiers, URLs and unique community/reaction keys retain their previous comparison rules. This slice intentionally configures Discovery content rather than changing the entire database's authentication and uniqueness semantics. Future SQL search over URL fields should apply the search collation at the expression boundary, without redefining URL identity.

The in-memory reference now uses invariant case/accent-insensitive matching and sorting, including the title-match score boost. This is an intentional product behavior change: cafe can find Café. .NET invariant comparisons and SQL Windows collation are not guaranteed equivalent for every Unicode sequence; parity fixtures are still required before SQL cutover. GUID and remaining ranking rules are unchanged.

Tests are intended to cover migrated SQL title/content matching, preservation of Unicode/original text, and case/accent-insensitive title scoring through the current service. Local .NET/SQL execution remains unavailable in the authoring environment; The AI-authored migration and designer have been removed, and the prior snapshot restored. Follow `src/Atlas.Persistence/AGENTS.md`: generate and review the migration locally, then commit its generated files and snapshot before running migration-dependent SQL tests and CI.

References: https://learn.microsoft.com/en-us/sql/relational-databases/collations/collation-and-unicode-support and https://learn.microsoft.com/en-us/sql/relational-databases/collations/set-or-change-the-column-collation
