# Reviewing automated dependency updates

Dependabot proposes weekly NuGet minor/patch updates and GitHub Actions updates.
These PRs are proposals, not proof of compatibility or evidence that every package
has a vulnerability. CodeQL source findings and Dependabot dependency alerts are
separate forms of analysis.

## Review and validation

1. Confirm the author, package source, changed files and target versions. Read
   upstream release notes, including intermediate major versions for Actions.
2. Identify affected Atlas paths: runtime dependencies need application regression
   evidence; test-framework updates also need confirmation that tests are discovered
   and still execute; CI actions need a successful workflow and artifact checks.
3. Restore/build the rewrite and run its test projects. Run SQL Server integration
   tests for persistence/Identity changes. Skipped SQL tests are not passing SQL evidence.
4. Check advisories for direct and transitive packages with
   `dotnet list Atlas.Rewrite.Tests.slnf package --vulnerable --include-transitive`.
   A clean audit only covers the advisories known to the configured sources.
5. Resolve conflicts against current main without removing existing security fixes.
   Keep EF runtime, design package and local tool versions aligned. Do not generate
   migrations merely because a package version changed; check pending model changes.
6. Merge only with passing applicable checks. Close superseded proposals after the
   replacement identifies the updates it retains and the ones it intentionally omits.

The workflow runs once for pull requests to main and once after merge to main;
manual dispatch remains available. PR tests use read-only repository permissions
and an isolated SQL Server service, not production credentials. Dependency updates
are tested automatically but are not automatically approved or merged.

## Reasons to hold an update

- A breaking API or behavior change requires code/configuration changes and targeted
  tests. Passing compilation alone is insufficient.
- Dependency constraints, framework support, or a conflict with a security pin need
  resolution before restore/build can be trusted.
- Missing or failing evidence (including unavailable SQL integration tests) needs
  investigation; it does not establish an upstream regression by itself.

Regression tests demonstrate Atlas behaviors we exercise; they do not prove all
upstream behavior unchanged. Review and tests complement each other.

## Historical application

`legacy/` is retained as historical reference and is not built, executed, deployed
or maintained, as documented in Dependabot configuration. Legacy-only update PRs
can be closed on that basis; this does not mean their old packages are safe to run.
Reactivating that application requires a separate dependency/security review.
