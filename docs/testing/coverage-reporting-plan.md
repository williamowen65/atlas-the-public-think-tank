# Test reporting and requirements evidence plan

Status: proposed reporting scope; no collectors or coverage gates implemented by
this initial document. Tracking: [PTT-152](https://thepublicthinktank.atlassian.net/browse/PTT-152).
Documentation follow-up: [PTT-77](https://thepublicthinktank.atlassian.net/browse/PTT-77).

## Three distinct reports

| Report | Question answered | Expected output |
| --- | --- | --- |
| Test execution | Which tests actually ran and what happened? | .NET TRX; Playwright HTML/JUnit; passed, failed, skipped and retried counts |
| Code coverage | Which application lines/branches were executed by those tests? | Separate .NET and frontend HTML reports plus machine-readable coverage |
| Requirements traceability | Which acceptance criteria have relevant verification evidence? | Reviewed requirement-to-test mapping joined to actual run results and explicit gaps |

Do not combine C# and TypeScript into a single percentage. A passing test is not
proof that its requirement is fully verified. Executed code is not proof of
correct assertions or complete acceptance criteria. “Absolute coverage” should
mean clearly defined measured counts, not a claim of complete correctness.

## Backend first implementation slice

- Inspect all rewrite test project runners and package versions before choosing
  Coverlet collector versus runner-specific integration. An inspected API test
  project currently references MSTest 4.5.1; it does not reference Coverlet.
  Earlier work on another branch is not evidence of current main configuration.
- Collect application line and branch coverage plus per-project test results.
  Exclude test assemblies, generated code and legacy code with documented rules.
  Ensure eligible application assemblies with zero hits are visible; otherwise
  the denominator can hide entirely untested code.
- Retain unit/non-SQL and SQL integration run evidence separately. Missing SQL
  prerequisites and skipped tests must remain visible.
- Merge coverage hits for an explicitly identified commit/run; do not average
  project percentages or sum overlapping matrix results.
- Generate downloadable HTML and Cobertura-compatible evidence suitable for PR
  review, with commit SHA, commands, environment and inclusion/exclusion scope.
- Use the existing rewrite test selection rather than rebuilding legacy projects.
  Add package/tool pins and reproducible local commands during implementation.

## Frontend first implementation slice

Publish Playwright HTML/JUnit test results and failure artifacts for both the
main browser suite and dedicated Ranking Lab suite. Distinguish development
Storybook, static Storybook and SPA execution.

Browser execution reports do not automatically provide frontend code coverage.
Frontend coverage requires additional collection and source-map handling for
Svelte/TypeScript and served bundles. Evaluate instrumentation versus Chromium
coverage, including navigation/reload collection, before claiming source coverage.
Coverlet does not collect Svelte/TypeScript coverage.

Verify source mappings on a representative Svelte component and TypeScript file
before broad rollout. A shared component exercised in Storybook and SPA should
merge hits rather than be counted twice. Document unmeasured code.

## SRS and traceability

Start with [requirements](../requirements/REQUIREMENTS.md), the
[RTM](../requirements/TRACEABILITY.md), and its [status vocabulary](../requirements/README.md).
Preserve permanent requirement IDs. Audit stale statements first: the inspected
RTM still describes portions of REST API verification as future work.

Map requirement ID and individual acceptance criteria to named tests, layer,
asserted behavior, mocks/live dependencies, and any manual verification.
Join that mapping to actual results at the reviewed SHA. Record missing mappings,
partial evidence, failed tests and skipped/not-run evidence explicitly.
Do not label criteria Verified from test names, code coverage, or file links alone.
Do not invent a requirements coverage percentage without an agreed denominator.

Suggested review columns:
requirement/criterion; test identity; verification layer; evidence limit;
latest result; commit/run link; remaining gap.

## Rollout and review

1. Agree scope and evidence formats.
2. Implement backend execution/coverage reporting and demonstrate a real report.
3. Publish frontend execution reports; validate frontend source coverage as a
   separate step.
4. Pilot acceptance-criteria traceability for a small reviewed set of existing IDs,
   then extend it.
5. Establish measured baselines before setting coverage thresholds. Prefer
   reviewed critical-path gaps and changed-code evidence over an arbitrary target.

Reports should be available on failures as well as successes where artifacts
exist. Missing/empty collection must be distinguished from genuine zero coverage.
Document local commands, CI artifact retention and how reviewers open reports.

## Initial validation

Repository requirements/RTM, test strategy and one representative backend test
project were inspected. No report has been generated and no baseline percentage
or threshold is asserted. Runner compatibility, full project inventory and
frontend instrumentation remain implementation work.
