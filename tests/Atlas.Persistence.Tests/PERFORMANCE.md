# Shared persistence test database trial

Ordinary SqlTestDatabase.Create calls share one migration-backed SQL database and
its supplemental schema/full-text indexes for this testhost run. Current-model and
no-full-text requests on reusable fixtures now use that common schema. Explicit
reuse:false calls retain their original setup mode and disposable databases.

The database is exclusively leased until Dispose resets committed application and
search-projection rows and returns it. Whole-table Count/Single assertions remain
valid because ordinary SQL fixtures do not overlap. Methods are still scheduled
in parallel; shared fixtures queue at acquisition, while independent tests can
overlap. Concurrency tests can still run multiple requests inside their own lease.
This is NOT parallel independent datasets inside one database.

Parity helpers release the previous dataset before acquiring a replacement.
Database-identity cache coverage explicitly requests another database. Sequential
lease coverage asserts that the connection identity is reused. Shared schema is
deleted at assembly cleanup. A failed reset discards it and allows a replacement;
a broken database must never contaminate the next test.

Exceptions:
- DiscoveryDatabaseSetupTests prove fresh model/migration setup and reject outer
  transactions. They deliberately retain fresh disposable databases.
- The two unavailable-index cases in DiscoveryFullTextTests disable/drop an index
  to exercise the real failure contract, using disposable databases. Healthy
  full-text behavior tests now use the shared database.
- One operation-validation regression installs a rejecting CHECK constraint;
  that method alone uses an independent database.
- Cache cross-database and simultaneous independent-fixture checks intentionally
  need distinct database identities.
Audit and demo-seed tests also use the shared database. Their assertions remain;
dedicated setup tests retain responsibility for fresh-schema validation. Exceptions
above mean the entire project is not literally one database.

Readiness waits remain before actual text-search assertions. Relational ancestor,
date-ordering and child-feed checks no longer wait for indexing. Newly committed
searchable data must still be populated by SQL Server; database reuse does not
guarantee that local full-text timeouts disappear.

The project sets RunSettingsFilePath to ../persistence-parallel.runsettings, also
listed in Atlas.sln. Test Explorer project defaults and explicit CLI runs point at
the same file. A manually selected solution-wide settings file may override it;
use this file when comparing runtimes. The general tests/parallel.runsettings is
unchanged for other projects.

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --settings tests/persistence-parallel.runsettings --logger "console;verbosity=detailed"
```

Fixture totals report creation/reuse/reset counts and cumulative readiness/reset
time. Counts include independent SqlTestDatabase fixtures, audit and demo-seed leases. Timings overlap when independent fixtures run concurrently.
Measure wall-clock runtime and unchanged test results separately. Physical database
allocation reflects reuse/high-water marks rather than a fresh file per scenario.

Validation: this change needs compilation and SQL execution in CI plus repeated
local full-suite runs. No measured speedup or resolution of the local indexing
failures is claimed.
