# Discovery cache across API restarts (PTT-184)

Redis stores ranked pages and public display cards under separate keys.
A ranked page key includes the normalized filters, page number and committed
page revision. A card key includes the node ID and committed detail revision.
Both include the SQL database namespace and the payload format version. A vote
can invalidate rankings while still allowing card reuse across filters. Atlas
reads the committed revisions in the same SQL snapshot used to rebuild results.
Even a Redis hit requires that revision read.

The external cache covers paged public Discovery, including non-search filters
and all page numbers. Full-text result rankings bypass caching because SQL's
index can update independently. Public cards from those results can still be
reused. Authored and moderator views retain their existing in-process behavior.
Personalized votes stay outside Redis.

A running API checks its bounded, 30-second in-process cache first. On a local
miss, it fetches the ranked page and batches missing cards in Redis. Redis keys
expire after ten minutes. Entries are limited to 128 KiB, and connection and
command timeouts are bounded. If Redis is unavailable, the API falls back to SQL
and briefly backs off Redis attempts. If restoring or replacing SQL at the same
server and database name, stop the API and clear its Redis cache before restarting
against the restored database because revision numbers can recur.

## Test an API restart locally

From the repository root in PowerShell, use your existing seeded local SQL
database. Start Redis in Docker Desktop and verify that it responds.

```powershell
docker run -d --name atlas-discovery-redis -p 127.0.0.1:6379:6379 redis:7.4-alpine
docker exec atlas-discovery-redis redis-cli ping
```

In the API terminal, set the connection and start the API. Keep the same
terminal and settings when restarting.

```powershell
$env:ATLAS_DISCOVERY_REDIS_CONNECTION = 'localhost:6379'
$env:ATLAS_TELEMETRY_DIRECTORY = Join-Path $env:TEMP 'atlas-redis-evidence'
dotnet run --project src/hosts/Atlas.Api --launch-profile https
```

1. In a second terminal request a populated page. Use the exact same URL
   before and after the restart.

   ```powershell
   Invoke-RestMethod 'https://localhost:7240/api/nodes?page=1'
   docker exec atlas-discovery-redis redis-cli --scan --pattern 'atlas:discovery:*'
   ```

   The scan should show a page key and card keys if the response has items.
2. Press **Ctrl+C in the API terminal**, leave Redis and SQL running, and
   rerun the same `dotnet run` command. Request the same URL again before
   the ten-minute Redis expiration.
3. Examine the newest phase records from the API.

   ```powershell
   Get-ChildItem (Join-Path $env:TEMP 'atlas-redis-evidence') -Filter 'metrics*.jsonl' |
     Sort-Object LastWriteTime | Select-Object -Last 1 |
     Get-Content | Select-String 'atlas.discovery.phase.duration' | Select-Object -Last 12
   ```

   The restart request should record one `RevisionCheck` and Redis
   `ExternalRead` hits, with no `PageRebuild` or `DetailRebuild` for that request.
   For a comparison, stop Redis with `docker stop atlas-discovery-redis`,
   restart the API again, and repeat the request. The same page should load
   with SQL rebuilds.
4. To run the automated equivalent with a disposable SQL test database, set
   `ATLAS_DISCOVERY_REDIS_TEST_CONNECTION=localhost:6379` and your existing
   `ATLAS_SQL_TEST_CONNECTION_STRING`, then run
   `dotnet test tests/Atlas.Persistence.Tests --filter FullyQualifiedName~RedisDiscoveryCacheTests`.

[Local API setup](../api/README.md) covers SQL credentials and development HTTPS.

## Watch cache sources while browsing

Start the API with `ATLAS_TELEMETRY_DIRECTORY` set as above. In a second
PowerShell window, run the following, then scroll the feed in your browser.

```powershell
$log = Get-ChildItem (Join-Path $env:TEMP 'atlas-redis-evidence') -Filter 'metrics-*.jsonl' |
  Sort-Object LastWriteTime | Select-Object -Last 1
Get-Content -LiteralPath $log.FullName -Tail 0 -Wait |
  Select-String '"name":"atlas.discovery.cache.request"' |
  ForEach-Object { $_.Line | ConvertFrom-Json } |
  Format-Table page, ranking_source, memory_cards, redis_cards, sql_cards, missing_cards
```

Each row is one completed Discovery read. A page ranking comes from
`ProcessMemory`, `Redis`, or `Sql`. Card counts can be mixed because each card is
looked up separately. A cold fetch may show SQL for the ranking and three
cards. Repeat the same request promptly to see process memory. Restart only
the API and repeat it within the Redis lifetime to see Redis. If scrolling
only reuses results in the browser, there is no new API row. Use the
browser's Network tab or repeat the exact feed URL to force a request.

After an API restart, rerun the watcher to follow the new metrics file.
Match `operation_id` with the phase records in that file to inspect the SQL
revision check or Redis failure/backoff. The summary records counts and cache
sources locally. Cloud telemetry continues to report aggregate phase metrics.

## Configure Demo in Azure

Once demo SQL is available:

1. Create a Free Redis database in the [Upstash Console](https://console.upstash.com/)
   in the closest available region. Copy its Redis protocol host, port and
   password from the connection details. The HTTP REST endpoint and token are
   for HTTP clients.
2. Set the GitHub Actions secret `ATLAS_DISCOVERY_REDIS_CONNECTIONS` on the
   repository, or in the environment used by Azure Plan and Apply. Store a JSON
   object with the demo TLS connection (substitute your actual host, port,
   and password).

   ```json
   {"demo":"YOUR_HOST:6379,ssl=true,user=default,password=YOUR_PASSWORD"}
   ```

   The same secret format accepts a `live` key with a separate database and
   credential when live is ready. Keep this value in secrets because Terraform
   configures it as an App Service setting and persists it in the protected state.
3. Run **Atlas Azure Terraform plan** on `ptt-184-discovery-read-cache`
   with `installation=main` and `deployment_shape=demo`. Verify the demo API
   app settings change. Run the matching **Atlas Azure Terraform apply** with
   the same inputs, then deploy the API from that branch to demo. Terraform wires the
   service into App Service. Upstash database creation is handled in the
   console so the Free plan can be selected directly.
4. Warm a populated page, restart only the demo API, and repeat the exact
   request within ten minutes. In Application Insights, compare
   `atlas.discovery.phase.duration` by phase and outcome for revision checks,
   page rebuilds, detail rebuilds and Redis reads. Compare Redis command usage
   and request latency in the same window.

[Upstash pricing](https://upstash.com/pricing/redis) lists the Free plan limits
and Pay-as-you-go pricing. Azure Managed Redis uses a separately priced cache
tier. Both the demo and live API use the same Redis protocol and App Service
settings, with deployment-specific credentials.
