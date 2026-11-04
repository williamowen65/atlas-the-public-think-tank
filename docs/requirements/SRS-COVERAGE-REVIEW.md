# SRS coverage review

Task: [PTT-153](https://thepublicthinktank.atlassian.net/browse/PTT-153)

## Baseline and review state

- Source baseline: main at `09df7b6235f9da83042423149ae54dd8ad9ce3c0`, captured 2026-10-27 UTC.
- Requirements catalog: [REQUIREMENTS.md](REQUIREMENTS.md).
- Existing requirements-to-implementation/test matrix: [TRACEABILITY.md](TRACEABILITY.md).
- Coverage collection: [backend coverage quickstart](../testing/backend-coverage-quickstart.md).
- State: review opened; requirement-by-requirement assertion audit and updated coverage totals are pending.

This document starts a fresh review following PTT-152. Existing Verified labels are historical catalog claims, not independently revalidated findings of this review. No new SRS coverage percentage is established by this initial commit.

## Initial findings

The requirements README describes an initial domain/Console baseline. The matrix still calls the REST API/web host future work and describes production session authentication as deferred. These passages need comparison with current API and web implementation and tests before they are used in a current report. Supplemental moderation and notification tables also use Prototype and Console prototype labels outside the main status vocabulary. Reconcile these explicitly rather than silently treating them as Verified or missing.

## Evidence to collect

For each requirement, record its stable ID, acceptance criteria, implementation paths, exact test methods or Playwright titles, assertions supporting each criterion, verification layer, and gaps. Check that linked files exist and that their assertions establish the claimed behavior. A class-level link alone is a starting point for review.

Keep three separate measures:

1. **Requirement evidence:** whether tests support all, some, or none of a requirement's acceptance criteria, or the mapping is still unreviewed.
2. **Execution evidence:** results from identified runs, including commit, environment, command, timestamp, failures and skips. A mapped test without a matching result is not evidence of a successful run.
3. **Source coverage:** measured line/branch coverage for the included assemblies, with unmeasured assemblies and exclusions disclosed. This is not the percentage of SRS requirements satisfied.

## Review classifications and reporting rules

Use all-criteria supported, partially supported, no supporting test found, and unreviewed for the evidence review. Keep these separate from the catalog's implementation status. Absence of evidence is not proof that behavior is absent.

Before publishing totals, enumerate unique catalog IDs, reconcile missing or duplicate matrix rows, declare the in-scope denominator, and list any Deferred/Rejected exclusions with rationale. Report fully supported and partially supported requirements separately. Do not count unreviewed records as verified. Preserve permanent IDs; identify capabilities missing from the catalog as scope gaps instead of inventing retrospective coverage claims.

## Work remaining

- [ ] Reconcile catalog IDs, matrix rows, status vocabulary, and scope.
- [ ] Audit acceptance criteria against concrete backend test assertions.
- [ ] Audit API and Playwright assertions, distinguishing mocked browser behavior from integration with the real API.
- [ ] Refresh outdated matrix links and descriptions based on evidence.
- [ ] Attach available run provenance and coverage summaries; clearly label user-reported results if raw evidence is unavailable.
- [ ] Publish requirement totals, evidence classifications, gaps, and a report-ready narrative.

The user reports both previously timing-out SQL tests now pass in the full local backend run without coverage. The earlier timeout remains unexplained. This observation is context, not a new execution artifact or proof of requirement completeness. No tests were executed for this documentation-only opening commit.

## Preliminary reporting snapshot — 2026-10-27

This is a documentation inventory at baseline `09df7b6235f9da83042423149ae54dd8ad9ce3c0`, not a completed acceptance-criteria audit. Denominator: all 81 unique requirement headings in REQUIREMENTS.md, including one Deferred record (NFR-002). No IDs were excluded or duplicated. The matrix has 75 unique catalog IDs and no extra IDs.

| Measure | Count | Percentage of 81 | Interpretation |
|---|---:|---:|---|
| Catalog requirements | 81 | 100% | Current written scope, not every app capability |
| Requirements represented by an RTM table row | 75 | 92.6% | Structural traceability only |
| Requirements whose RTM verification cell links to an existing test file or test directory | 56 | 69.1% | Link presence, not assertion completeness or a passing run |
| Catalog records labeled exactly Verified | 39 | 48.1% | Historical recorded status; not revalidated in this review |
| Missing RTM rows | 6 | 7.4% | CON-003 through CON-008 |

The exact catalog status distribution is 39 Verified, 14 Implemented, 15 Partial, 3 Approved, 1 Deferred, and 9 records with qualified implementation/prototype descriptions. The nine qualified labels are retained as a separate group rather than equated with complete implementation. Five substantive catalog/matrix status mismatches are REA-008 (Approved/Verified), REA-011 (Implemented/Verified), VOT-004 (Partial/Implemented), VOT-007 (Approved/Implemented), and VOT-011 (Partial/Verified). Other qualified labels differ in wording. The catalog supplies the status counts above; no attempt was made to choose the more optimistic status.

| Area prefix | Catalog requirements | Catalog Verified | RTM rows |
|---|---:|---:|---:|
| GRA | 5 | 5 | 5 |
| TYP | 5 | 3 | 5 |
| REL | 4 | 3 | 4 |
| CON | 8 | 5 | 2 |
| PAR | 2 | 1 | 2 |
| SEC | 3 | 0 | 3 |
| AUT | 2 | 2 | 2 |
| EVT | 2 | 1 | 2 |
| PER | 2 | 0 | 2 |
| REA | 11 | 4 | 11 |
| VOT | 11 | 3 | 11 |
| NFR | 2 | 0 | 2 |
| COM | 8 | 5 | 8 |
| DIS | 7 | 7 | 7 |
| MOD | 4 | 0 | 4 |
| NOT | 3 | 0 | 3 |
| IDN | 2 | 0 | 2 |

### Concrete traceability gaps

- CON-003 through CON-008 have catalog entries but no matrix table rows. Five of those six are labeled Verified in the catalog, making their missing traceability particularly important.
- DIS-001 through DIS-007 each link to nonexistent `tests/Atlas.Discovery.Tests/DiscoveryServiceTests.cs`. Existing DiscoveryReadQueryTests and the Persistence discovery suites are candidates for assertion review; they are not assumed to be interchangeable replacements.
- 18 catalog requirements have no Markdown link in their RTM verification cell, including the six missing rows. Another seven have only the broken Discovery link. Together these account for the 25 requirements outside the existing-test-link count. A directory link counts for link presence but still requires individual test mapping.
- The catalog covers 17 area prefixes, but has no dedicated threaded-comment requirement area. COM is Communities. Comment-related behavior appears in some other requirements; completeness of threaded-comment and newer web/API product scope needs separate review.

### Confirmed current API evidence (source inspection)

[BoundaryTests.cs](../../tests/Atlas.Api.Tests/BoundaryTests.cs) contains `Anonymous_mutation_is_401_without_redirect_or_database_access`, asserting POST /api/nodes returns 401 with no redirect using the real API middleware in an in-memory test host. This supports the unauthenticated-action criterion of SEC-001. `All_write_actions_have_authorization_except_explicit_account_entry_points` inspects write action authorization metadata and the explicit anonymous account exceptions. These are specific API evidence beyond the matrix's future-API wording. They do not establish all SEC-001 acceptance criteria: authenticated actor binding and forged actor data need the SQL workflow tests and implementation reviewed. SEC-001 therefore is not upgraded here. These tests were inspected, not executed during this review.

### Text suitable for the current report

> A preliminary inventory identified 81 documented SRS requirements. The existing traceability matrix represents 75 (92.6%), and 56 (69.1%) have verification links resolving to existing test files or directories. The requirements catalog currently labels 39 (48.1%) as Verified. These figures describe documented traceability and recorded status, rather than independently audited requirement satisfaction. Six missing Content mappings, seven stale Discovery test links, and catalog/matrix status inconsistencies remain to be reconciled. Current API tests provide evidence beyond some outdated matrix descriptions. Acceptance-criterion completeness and execution evidence are still under review; source-code coverage is reported separately.

## Updated snapshot after focused mapping repairs

This section supersedes the initial structural counts above for the PTT-153 branch. The original main-baseline inventory is retained for comparison. No catalog status has been upgraded.

| Measure | Original main baseline | After mapping repairs |
|---|---:|---:|
| Unique catalog requirements | 81 | 81 |
| Requirements with an RTM row | 75 (92.6%) | 81 (100%) |
| Requirements linking to existing test files/directories in the verification cell | 56 (69.1%) | 68 (84.0%) |
| Catalog records labeled Verified | 39 (48.1%) | 39 (48.1%) |

Changes: restored six Content rows, linked five to inspected relevant tests, replaced seven missing DiscoveryServiceTests references with specific real-SQL DiscoverySqlParityTests methods, and added current API rejection evidence to SEC-001. All 81 IDs have exactly one RTM row. Links added by this repair were checked against the baseline repository tree. No test execution was performed for these documentation edits.

The six restored rows retain the catalog statuses and explicitly flag limits of the evidence. CON-008 still needs a direct assertion that unused concrete payload columns remain NULL; a successful mixed-block round trip does not prove that condition. Other Content criteria, such as Console prompts, duplicate references, required video URL validation, and stable seeded IDs, are not all established by the inspected tests. The Discovery mappings identify real SQL-backed assertions rather than assuming the two current domain-level port tests replace the removed suite.

> After focused traceability maintenance, all 81 documented SRS requirements have matrix entries, and 68 (84.0%) link to existing test files or directories. The catalog records 39 (48.1%) as Verified; these historical status labels have not yet undergone a complete acceptance-criteria audit. The improvements restore missing Content mappings and identify current SQL-backed Discovery and API evidence. Structural traceability is complete for the written catalog, while assertion completeness, status reconciliation, and newer product-scope gaps remain under review.
