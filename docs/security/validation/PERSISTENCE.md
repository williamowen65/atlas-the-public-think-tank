# Persistence validation and SQL injection

## What EF Core protects

Current production persistence in [SqlRepository](../../../src/Atlas.Console/Storage/SqlRepository.cs) uses EF LINQ queries, `Find`, tracked row changes and `SaveChanges`. Identity also uses EF-backed stores. User values go through provider-managed query/write parameters rather than being concatenated into SQL commands. This is **parameterization**, not removal or escaping of supposedly dangerous characters.

An apostrophe, semicolon, SQL-looking phrase or Unicode text should remain ordinary data when it is otherwise valid for the domain field. Do not strip these characters as a substitute for using parameters. SQL safety does not mean the value is valid as a title, reference, URL, Markdown document or authorized change.

The initial review found no raw application SQL. The follow-up adds one [parameterized `ExecuteSqlInterpolated` call](../../../src/Atlas.Console/Storage/SqlOperationBoundary.cs) for the backend-owned transaction lock. Its resource name is bound as a parameter; it contains no request-derived query structure. Generated migration SQL and historical `legacy/` SQL are not user-input query paths reviewed here.

If raw SQL is introduced, use EF's parameterized APIs or explicit provider parameters for values. Never interpolate untrusted text into command strings. Database identifiers, sort expressions and query structure cannot be made safe by value parameters; choose them from backend-owned allowlists. Refer to Microsoft's [EF Core SQL query guidance](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries).

## Integrity protection is layered

| Expectation | Enforcement / evidence | Limit |
|---|---|---|
| References to mapped principals exist | [AtlasDataContext](../../../src/Atlas.Persistence/AtlasDataContext.cs) FK mappings; [SqlReferentialIntegrityTests](../../../tests/Atlas.Persistence.Tests/SqlReferentialIntegrityTests.cs) | Does not establish actor permission, active status or acyclic ancestry |
| One vote per participant/target; one notification per occurrence/recipient; unique community/reaction names | Unique indexes in the EF model; repository/persistence tests | Shared write transaction/lock prevents cooperating check-and-write races; expected SQL constraint conflicts become OperationConflictException |
| Domain text, ranges, enums and lifecycle are valid | Domain constructors/mutations and reconstitution | Row mappings and nullable/string columns do not encode every domain invariant |
| A coordinated workflow is atomic | [SqlOperationBoundary](../../../src/Atlas.Console/Storage/SqlOperationBoundary.cs); [rollback/concurrency tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs) | Same database and synchronous cooperating operations; caller objects must be reloaded after persistence failure |
| Polymorphic targets and deferred content references are valid | Voting/Comments availability ports and host coordination; [reference policies](IMPLEMENTATION.md#references-and-relationships--ptt-129); poll/chart existence deferred | Ordinary FKs cannot express all target kinds |

`HasMaxLength` configures storage; it is not a general EF input-validation engine. SQL column sizes may differ from stricter domain limits. Public row types or direct `SaveChanges` calls must not become an unvalidated API write path. Do not assume FK mappings reject all null optional references, default IDs, enum strings or out-of-range vote values.

## Automated SQL regression

[SqlValidationTests](../../../tests/Atlas.Persistence.Tests/SqlValidationTests.cs) saves and queries SQL-looking text through the current EF repository and then confirms a normal write still works. Its command interceptor also asserts that the supplied query value is a command parameter rather than SQL text. It requires `ATLAS_SQL_TEST_CONNECTION_STRING` and a real SQL Server; the existing SQL CI job supplies both. It complements domain validation tests and does not replace the production-source review.

See [transaction design, evidence and limits](IMPLEMENTATION.md#complete-operations--ptt-130) for the complete workflow boundary.

## Local SQL test configuration

Persistence, Identity and Console SQL tests load `ATLAS_SQL_TEST_CONNECTION_STRING` from user secrets, then environment variables. All three test projects use the Console application's shared `UserSecretsId` (`02103f3e-daff-4644-b045-6eb5f00889eb`), so Manage User Secrets opens the same local file. Environment variables override secrets, preserving CI's disposable SQL Server configuration.

For local SQL Express with Windows authentication, add this to the shared secrets file:

```json
{
  "ATLAS_SQL_TEST_CONNECTION_STRING": "Server=localhost\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True"
}
```

The test fixtures replace any supplied database name with a unique test database, apply migrations and delete it afterward. The local account must have permission to create and delete databases on the dedicated test server. Missing configuration marks SQL tests inconclusive; a configured but unreachable server produces a test failure. Keep actual credentials in local secrets, outside the repository.

## Browser and network concerns

Parameterized SQL does not prevent stored XSS. Markdown/HTML rendering and context-appropriate output encoding belong to the future browser boundary (PTT-125). `LinkPreviewBlock` limits schemes to HTTP(S), but that alone is not an SSRF defense. If a backend later retrieves a user URL, define destination/redirect/DNS and size/time restrictions at that fetch boundary. Image/video values currently allow media reference strings, so their policy must fit the future asset system.
