# Testing and coverage reporting

This guide separates **test pass rate**, **code coverage**, and **SRS verification coverage**. These are different denominators; do not present one as another.

## Automated test results

Run the rewrite's .NET test projects from the repository root. **Do not run `dotnet test Atlas.sln`** for this purpose: the solution also contains legacy projects, including the legacy website, whose build prerequisites can fail independently of Atlas tests. The CI workflow tests each rewrite project separately.

PowerShell:

```powershell
$projects = Get-ChildItem tests -Directory -Filter 'Atlas.*.Tests' |
    ForEach-Object { Join-Path $_.FullName ($_.Name + '.csproj') } |
    Where-Object { Test-Path $_ }

foreach ($project in $projects) {
    dotnet test $project --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Test project failed: $project" }
}
```

This discovers the rewrite test projects under `tests/` and stops if any project fails. Capture each project's pass/fail/skip counts and sum them for the report. SQL-dependent tests may skip without a configured database.

Some SQL integration tests require `ATLAS_SQL_TEST_CONNECTION_STRING` and SQL Server Full-Text Search; without a configured test database, skipped tests must **not** be counted as passes. CI also runs SQL-backed test jobs against its own SQL Server container. See [GitHub Actions](../../.github/workflows/AtlasRewriteTests.yml).

Run frontend checks from `src/hosts/Atlas.Web`:

```powershell
npm ci
npx playwright install
npm run check
npm test
```

Report the counts of **passed, failed, skipped**, and the date/commit of the run. Pass rate = passed / (passed + failed); report skipped separately. Never assume all tests pass based only on a successful build or an earlier run.

## .NET code coverage

The .NET SDK test command does not automatically provide a percentage. Use the cross-platform **coverlet collector** and a report generator. First check whether the test projects already reference `coverlet.collector`; if not, add it to the relevant test projects in a separate reviewed tooling change.

Once installed, run:

```powershell
foreach ($project in $projects) {
    dotnet test $project --collect:"XPlat Code Coverage" --results-directory TestResults
    if ($LASTEXITCODE -ne 0) { throw "Coverage test run failed: $project" }
}
```

The collector writes Cobertura XML files under `TestResults`. Generate an HTML summary with ReportGenerator (installed as a .NET tool):

```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:HtmlSummary;TextSummary"
```

Exclude generated code, migrations and test assemblies according to an explicit documented policy. Report **line coverage** and **branch coverage** separately; the result is not a measure of requirements coverage. This workflow is a proposed measurement procedure, **not a coverage result already measured for Atlas**.

For Svelte/TypeScript coverage, use Playwright's V8 coverage or Istanbul instrumentation with appropriate source maps and a reviewed exclusion policy; do not add frontend and backend percentages together without a defined weighted denominator.

## Requirements (SRS) verification coverage

Source of truth: [traceability matrix](TRACEABILITY.md). Each requirement must have implementation evidence and tests that actually exercise its acceptance criteria before marking it **Verified**.

Use:

- **Strict SRS verification:** Verified applicable requirements / all applicable requirements.
- **Traceability linkage:** requirements with linked automated tests / all applicable requirements. This is **not** proof of full verification.
- **Implementation status:** report Implemented and Partial separately; do not treat them as Verified.

The current RTM contains some legacy and prototype rows with inconsistent status columns. Normalize these before using an aggregate percentage as a formal SRS claim. A test reference can cover only part of a requirement. Do not promote Partial entries when security, concurrency, cross-host, or lifecycle criteria remain open.

## Reporting template

| Metric | Numerator / denominator | Value | Evidence |
|---|---|---|---|
| Backend tests passed | Passed / executed | Not measured in this documentation pass | Dated CI/test output |
| Frontend tests passed | Passed / executed | Not measured in this documentation pass | Dated CI/test output |
| .NET line coverage | Covered / instrumented lines | Not measured | Cobertura/ReportGenerator |
| .NET branch coverage | Covered / instrumented branches | Not measured | Cobertura/ReportGenerator |
| SRS verification | Verified / applicable requirements | Recalculate after RTM review | RTM and linked tests |
| Requirements linked to tests | Linked / applicable requirements | Recalculate after RTM review | RTM |

For each published figure, capture the **commit SHA, date, test environment, exclusions, skipped tests and report location**. Avoid reporting a pass rate or code coverage percentage without an actual run.
