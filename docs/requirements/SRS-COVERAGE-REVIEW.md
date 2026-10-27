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
