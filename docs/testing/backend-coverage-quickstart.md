# Combined backend coverage

From repository root after pulling PTT-152-test-consistency:

```powershell
powershell -NoProfile -File tests/Invoke-BackendCoverage.ps1
```

The existing Invoke-GraphCoverage.ps1 still runs the smaller Graph pilot.

## Outputs

Open the printed html/index.html. The top-level total combines measured Atlas
application assemblies; expand assembly/project rows for classes, methods and
source lines. It merges execution hits rather than averaging percentages.

Each of the 13 rewrite test projects keeps its own TRX under results/.
test-summary.csv records project outcomes, failures and not-executed counts.
assembly-inventory.csv identifies backend assemblies absent from the merged
report. These are unmeasured, not implicitly covered; they do not contribute to
the displayed percentage. Review that inventory before calling a report a
complete backend baseline. In particular, RankingLab has no dedicated .NET test
project, and source-less contracts or generated code may not have coverable lines.

Coverage includes Atlas.* assemblies, excludes *.Tests and obj-generated files.
It does not include TypeScript/Svelte, legacy applications or requirements
acceptance coverage. Keep requirements traceability separate.

## SQL and validity

Tests run sequentially through the existing rewrite test-project selection,
so legacy build dependencies are not selected. SQL tests use their existing
environment/user-secret configuration. Do not use a production database.
Without SQL prerequisites some tests can be skipped; inspect not-executed counts,
TRX reasons and assembly inventory. The report may then be a partial baseline.

CI starts the same isolated SQL Server/Full-Text Search image used by the backend
tests before running this script. SQL schemas/query correctness remain test
assertions, not C# line-coverage measurements.

Failures do not stop collection for subsequent projects. Available reports are
generated, but missing TRX/coverage or any test failure causes a failing script.
Partial evidence must not be treated as a passing complete report.

## Storage

Local artifacts/test-reports is ignored by Git. Local reports remain on your
disk until you delete their individual run directories; they are not uploaded
automatically when running locally.

CI uploads one atlas-backend-test-reports artifact per run, containing results,
merged HTML/data and metadata, not bin/obj or database files. retention-days: 14
automatically expires uploaded artifacts. Download review/release evidence you
want to retain longer. Account storage usage still depends on artifact size and
run frequency; this is not a promise of unlimited storage. No deletion script is
required for the configured expiration.

## Verification status

The Graph pilot was reported working locally before this expansion. The expanded
13-project collector, SQL CI job and merged report still need first-run
verification. This environment cannot execute dotnet; no backend-wide percentage
is asserted yet. Check CSV counts, source navigation and absent assemblies after
your first run.
