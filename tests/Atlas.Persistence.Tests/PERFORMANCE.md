# Persistence test runtime

A source file is not a database boundary. Each SqlTestDatabase.Create call exclusively
leases a database. A lease is returned after resetting committed data. Pools are
separated by connection template, migration/current-model setup, and full-text flag.
Multiple fixture instances held at the same time require different databases.
Setup/schema-mutation tests opt out. SqlAuditTests and SqlPersistenceTests also
manage databases independently.

Assembly cleanup prints fixture creation/reuse/reset counts and cumulative full-text
wait/reset time. These timings overlap across workers and must not be interpreted
as wall-clock totals. Counts exclude independently managed databases.

Cache tests wait explicitly before text assertions, including after changing block
text. Pure relational cache assertions do not wait for indexing. Parity tests opt
into one post-seed barrier for immutable datasets used in text searches. Dedicated
full-text mutation/readiness tests retain their existing waits and assertions.

Run from the repository root:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --settings tests/persistence-parallel.runsettings --logger "console;verbosity=detailed"
```

Workers=0 uses available logical processors, while class scope keeps each class's
methods sequential. tests/parallel.runsettings still caps workers at two if selected;
the explicit persistence settings above select automatic worker count instead.
Compare repeated runs on the same SQL Server. If automatic workers cause contention,
set Workers in the persistence settings to 2 or 4 and compare elapsed time and waits.
Preserve test count and failures when comparing results. No speedup is assumed until
measured. Full-text indexing of newly committed text still takes time even when
database/schema setup is reused.
