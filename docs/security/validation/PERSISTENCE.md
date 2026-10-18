# Persistence validation and SQL injection

Persistence means storing and retrieving Atlas data. This page separates three questions:

- **SQL safety:** can user input change the meaning of a database command?
- **Data integrity:** do the stored records and relationships follow Atlas's rules?
- **Other boundaries:** what still needs protection when stored data is displayed in a browser or used to make a network request?

## What EF Core protects

[SqlRepository](../../../src/Atlas.Application/Storage/SqlRepository.cs) uses EF LINQ queries, `Find`, tracked row changes and `SaveChanges`. Identity also uses EF-backed stores.

- **Values become parameters.** EF sends the SQL command and its data values separately, so user text is treated as data rather than executable SQL.
- **Valid text stays intact.** Apostrophes, semicolons, SQL-looking phrases and Unicode do not need to be stripped as an SQL-injection defense.
- **Parameterization is not domain validation.** A safely stored value can still be an invalid title, missing reference, unacceptable URL or unauthorized change.

For example, a name containing `O'Brien` should be stored normally. Parameterization protects the SQL command; domain rules decide whether the name itself is acceptable.

### Raw SQL

The initial review found no raw application SQL. The follow-up introduced a [parameterized lock command](../../../src/Atlas.Application/Storage/SqlOperationBoundary.cs) using `ExecuteSqlInterpolated`.

- Its lock resource is passed as a parameter.
- Its SQL structure is owned by the backend, rather than built from request text.
- Generated migration SQL and historical `legacy/` SQL are outside the user-input query paths reviewed here.

For future raw SQL:

- Pass **values** through EF's parameterized APIs or explicit provider parameters.
- Do not build command text by concatenating or inserting untrusted strings.
- Choose **column names, sort expressions and other query structure** from backend-owned allowlists. Value parameters cannot stand in for SQL identifiers or syntax.

See Microsoft's [EF Core SQL query guidance](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries).

## What data integrity means

**Data integrity** means that stored data stays consistent with the rules it is supposed to follow. For Atlas, examples include a Node referencing an existing document, one vote per participant/target pair, and parent relationships containing no cycles.

**Integrity protection** is the collection of checks and storage guarantees that preserve those rules.

**Layered** means different parts of the system handle different responsibilities. A constructor, an application service and a database constraint each know different facts. Passing one check does not prove that every other rule has been satisfied.

### Which layer protects which rules?

- **Domain objects protect local rules.**
  - Constructors, mutations and reconstitution check text, numeric ranges, enums and lifecycle consistency.
  - They can check the values they receive, but cannot independently know whether a referenced record exists in SQL.

- **Application workflows and reference lookups protect relationships and availability.**
  - They resolve references and check whether an actor or target is available for the proposed operation.
  - A record can exist but be inactive, archived or hidden. Existence alone also does not establish the caller's permission.
  - See the [reference policies](IMPLEMENTATION.md#references-and-relationships--ptt-129).

- **Database constraints protect mapped relationships and uniqueness.**
  - Foreign keys require referenced rows to exist where those relationships are mapped.
  - Unique indexes enforce rules such as one vote per participant/target, one notification per occurrence/recipient, and unique community/reaction names.
  - These mappings and their tests are in [AtlasDataContext](../../../src/Atlas.Persistence/AtlasDataContext.cs) and [SqlReferentialIntegrityTests](../../../tests/Atlas.Persistence.Tests/SqlReferentialIntegrityTests.cs).
  - A foreign key does not establish authorization, active status or acyclic ancestry. Ordinary foreign keys also cannot express every polymorphic target, such as a target whose kind selects which entity its ID refers to.

- **Transactions and coordination protect complete operations.**
  - [SqlOperationBoundary](../../../src/Atlas.Application/Storage/SqlOperationBoundary.cs) groups related writes so they commit together or roll back together.
  - Serializable transactions and constraints protect competing database changes. Expected SQL deadlock/constraint conflicts become `OperationConflictException`.
  - Node creation and parent changes also share a graph application lock to protect cycle checks. Metadata saves preserve parents and do not request that lock.
  - See the [workflow tests](IMPLEMENTATION.md#workflows-and-evidence) and [transaction design and limits](IMPLEMENTATION.md#complete-operations--ptt-130).

### Limits of storage protection

- **Column settings are not a complete validation system.** `HasMaxLength` configures storage size; domain limits may be stricter.
- **Some invalid values can fit in a row.** Nullable columns, strings and foreign keys do not reject every missing optional reference, default ID, invalid enum string or out-of-range rating.
- **Direct writes can bypass application checks.** Public row types and direct `SaveChanges` calls must not become an unchecked API write path.
- **Atomicity has a scope.** The shared transaction covers cooperating operations in one database, with sequential calls inside the operation.
- **Rollback does not reset caller objects.** An object changed in memory can still hold an attempted change after SQL rolls it back. Reload before retrying.
- **Deferred references remain deferred.** Poll/chart existence checks await those domains; storing an ID is not proof that its target exists.

## Automated SQL regression

[SqlLookingTextIsStoredAndQueriedAsParameterData](../../../tests/Atlas.Persistence.Tests/SqlValidationTests.cs#L13) checks the current EF path:

1. Save text containing an apostrophe, SQL-looking content and Unicode.
2. Query that text through the repository's EF query helper.
3. Inspect the command to confirm the query value is a parameter, rather than part of the SQL command text.
4. Confirm the text is preserved and a normal subsequent write still succeeds.

The test requires a real SQL Server and `ATLAS_SQL_TEST_CONNECTION_STRING`; CI supplies both. It covers that path alongside domain tests and production-source review. It does not prove that every future SQL query will be safe.

## Resetting the local application database

See the [Persistence README PowerShell reset script](../../../src/Atlas.Persistence/README.md#reset-the-local-database-to-demo-seed-data-powershell) to delete the local application database, apply committed migrations and restore demo seed data. This is separate from the disposable databases used by SQL tests below.

## Local SQL test configuration

Persistence, Identity and Console SQL tests share one configuration:

- **Local secrets:** all three use the Console application's `UserSecretsId`, `02103f3e-daff-4644-b045-6eb5f00889eb`. Manage User Secrets opens the same file.
- **Environment overrides:** environment variables are loaded after secrets and take precedence. This lets CI use its disposable SQL Server.
- **Connection key:** `ATLAS_SQL_TEST_CONNECTION_STRING`.

For local SQL Express with Windows authentication:

```json
{
  "ATLAS_SQL_TEST_CONNECTION_STRING": "Server=localhost\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True"
}
```

When a SQL test runs:

1. Replace any supplied database name with a unique test database name.
2. Apply migrations.
3. Run the test and delete its database afterward.

The account needs permission to create and delete databases on the dedicated test server. Missing configuration marks SQL tests inconclusive; a configured but unreachable server makes them fail. Keep actual credentials in local secrets, outside the repository.

## Browser and network concerns

Safe SQL storage is one boundary. What happens to that stored value afterward needs its own checks.

### Displaying stored content: XSS

**Cross-site scripting (XSS)** happens when content supplied by a user is interpreted as executable browser content.

- A Markdown or HTML value can be safely stored as a SQL parameter and still be unsafe to render later.
- The future browser boundary must control Markdown/HTML rendering and apply output encoding appropriate to where a value is displayed.
- For example, displaying a title as text and inserting it into an HTML attribute are different output contexts.
- These browser protections belong to [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125); parameterization does not provide them.

### Fetching a user URL: SSRF

**Server-side request forgery (SSRF)** happens when a user can make the backend request a destination it should not access, such as an internal service.

- [LinkPreviewBlock](../../../src/Atlas.Content/Blocks/Types/LinkPreviewBlock.cs) restricts schemes to HTTP(S). That checks the URL's form, but does not establish that its destination is safe.
- Storing a link and having the server fetch it are separate actions. These Content/Console paths currently store references; they do not implement a link-preview network fetch.
- If fetching is added, that boundary must control destinations, redirects and DNS resolution, plus response-size and time limits.
- For example, a public URL could redirect to an internal address. Validating only the initial URL would miss the destination actually contacted.

### Image and video references

[ImageBlock](../../../src/Atlas.Content/Blocks/Types/ImageBlock.cs) and [VideoBlock](../../../src/Atlas.Content/Blocks/Types/VideoBlock.cs) currently allow media reference strings. Their future asset policy must define which references are accepted and how they are resolved or displayed. A value being stored successfully does not prove that it identifies an approved asset.
