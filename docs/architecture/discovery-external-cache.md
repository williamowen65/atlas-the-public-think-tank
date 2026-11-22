# Discovery cache across API restarts (PTT-184)

Discovery reads the singleton `DiscoveryRevisions` row in the same SQL snapshot
as its ranked results and display cards. A vote can advance the page revision
alone. Content or moderation changes can advance both. A rollback advances
neither. Even an external cache hit still needs a SQL connection, snapshot
transaction and revision read. Reducing refills and preventing SQL wake-ups are
different goals.

## Backend choice

- **Azure Blob Storage (pilot)** uses a small Hot LRS account per enabled variant.
  Cost depends on stored bytes and operations. The API uses managed identity
  and can test the same data protocol locally with Azurite. One blob per page
  avoids remote calls for each card.
- **Azure Managed Redis** offers lower-latency cache primitives and native TTL,
  with a separately priced compute tier. Revisit it if Blob latency or more
  general cross-instance caching warrants the additional infrastructure.
- **Upstash Redis** has a command-limited free tier and usage-based plans.
  It would add a separate service account, region selection and credentials.
  Its free allocation covers one database, which limits a two-instance trial.

Pricing references are [Azure Blob Storage](https://azure.microsoft.com/en-us/pricing/details/storage/blobs/),
[Azure Managed Redis](https://azure.microsoft.com/en-us/pricing/details/managed-redis/)
and [Upstash](https://upstash.com/pricing/redis). The separate share-image cache
serves a different workload.

## Scope and invalidation

The pilot stores nonempty pages 1–10 of the unfiltered public Discovery feed.
The existing in-process page and card caches retain their separate revisions,
visibility keys, 30-second lifetime and budgets. The external bundle is
limited to 512 KiB and has a ten-minute absolute lifetime. Azure lifecycle
cleanup deletes old objects asynchronously after one day. A warm local hit
avoids Blob I/O. A remote hit can repopulate local entries for up to their
normal lifetime. Search, authored and moderator pages use the existing local
path. Participant-specific votes remain outside the shared cache.

The external key includes a payload version, a hash of the SQL server/database
pair, the ranked page key, and the committed detail revision. Both revisions
must match before a bundle can be used. A failed Blob operation falls back to
SQL and briefly backs off Blob requests. The SDK uses a 400 ms per-operation
deadline and zero retries. API instances can independently fill a cold key.
If replacing or restoring a database at the same server and database name,
stop the API and clear that variant's cache before restarting against the
restored database. Revision counters can repeat after a restore.

## Local verification

Start Azurite with a volume so it stays available during app restarts.

```sh
docker run --rm --name atlas-azurite -p 127.0.0.1:10000:10000 \
  -v atlas-discovery-azurite:/data mcr.microsoft.com/azure-storage/azurite:3.35.0 \
  azurite-blob --blobHost 0.0.0.0 --skipApiVersionCheck
```

Run the integration tests with your existing disposable SQL Server test
connection configured. The tests create their own Azurite containers.

```sh
export ATLAS_DISCOVERY_CACHE_TEST_EMULATOR='UseDevelopmentStorage=true'
dotnet test tests/Atlas.Persistence.Tests --filter FullyQualifiedName~BlobDiscoveryCacheTests
```

For a manual API restart, create a container named `discovery` in Azurite
and start the local API with
`ATLAS_DISCOVERY_CACHE_EMULATOR='UseDevelopmentStorage=true'`. As an anonymous
visitor load page 1 twice, stop only the API, start it again and reload within
ten minutes. Then edit or hide a result and reload. Repeat with Azurite stopped
to check the SQL fallback. Keep the same SQL server and database across the
API restart.

## Demo trial

Leave Azure deployments on hold until demo SQL is available. Terraform defaults
to no external caches. For the demo trial, set the repository variable
`ATLAS_DISCOVERY_CACHE_VARIANTS` to `["demo"]` and review the demo plan before
applying. The Terraform identity needs permission to create the storage
resources and write the container-scoped role assignment. The runtime identity
needs only the Blob data role on its own variant container. Use the API's
managed identity in Azure and keep live disabled during the demo trial.

Measure anonymous requests to the same page cold, warm, and immediately after
an API restart. Keep the restart within the external lifetime and run the
sequence with the cache disabled and enabled. Break down
`atlas.discovery.phase.duration` by `variant`, `phase` and `outcome`. Histogram
counts give completed revision checks, page rebuilds, detail rebuilds and
external read results. Compare durations for each phase independently.
Revision timing starts after transaction setup. Rebuild timings exclude Blob
operations. Each rebuild can issue several SQL commands. Compare the phase
counts with SQL reads, connection/startup latency and storage operations over
the same window before attributing Azure SQL usage to cold cache refills.
