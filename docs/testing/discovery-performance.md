# Discovery performance audit

The audit uses a standalone Playwright Chromium browser to measure the feed and runs k6 inside Docker for repeatable anonymous HTTP traffic. It covers the unfiltered Discovery feed. Choose a seeded local database or demo with several pages of nodes.

## Local Windows run

1. Start Docker Desktop in Linux container mode. From `src/hosts/Atlas.Web`, run `npm ci` and `npx playwright install chromium --only-shell`.
2. Start your usual local SQL database and Redis. From the web directory, use this PowerShell setup to send the API's local telemetry to the directory shared with the cache audit.

   ```powershell
   $env:ATLAS_TELEMETRY_DIRECTORY = [IO.Path]::GetFullPath((Join-Path (Get-Location).Path '../../../logs/observability'))
   npm run dev:api
   ```

3. In a second terminal in the web directory, run `npm run audit:performance`. It targets `http://localhost:5173` by default. The container reaches Vite through `host.docker.internal` and the browser uses localhost. If you started Vite on another port, pass the local frontend origin after `--`.

Each stage prints its report and saves it under the repo's ignored `logs/observability/performance/` directory. Start with the browser-only baseline, one k6 visitor, and the browser probes under 10 and 25 k6 visitors. Browser timings include first card and time without new cards after reaching the preceding feed tail. Prefetch can produce a zero wait. A short feed can produce too few samples for comparison, so use the same seed data and compare the individual waits as well as the summary. The k6 visitor count represents independent request loops with pauses for reading.

If local cache metrics are empty, check that the API was started in Development with the telemetry directory above. If using a custom output directory, set `ATLAS_TELEMETRY_DIRECTORY` in both terminals. `npm run audit:discovery-cache` still provides the individual cache request rows. Both commands read data while the API is running.

## Redis 03 and Azure Monitor

Use the demo **frontend** origin with `npm run audit:performance -- https://<demo-frontend-host> demo`. For GitHub Actions, set the repository variable `ATLAS_PERFORMANCE_DEMO_ORIGIN` to that demo origin and run **Atlas Discovery performance** manually once the workflow is available on the default branch. The GitHub run identifies the workflow commit. Record the deployed commit separately if the demo is running another build. Keep the JSON artifact so you have each stage's UTC timestamps and measurements.

1. Open the demo Application Insights resource and select **Metrics**. Pick the absolute time window for one stage. Look for `atlas.discovery.phase.duration` and split by phase where available. Compare `RevisionCheck`, `ExternalRead`, and `PageRebuild` between stages. For cache lookups, use `atlas.discovery.cache.lookups` and split by layer and outcome where available. If a custom metric is absent in the selected namespace, check a larger time range and confirm the demo is exporting Atlas telemetry.
2. In `atlas-rehearsal-redis03-logs` → **Logs**, use the same UTC range. Start with `AppRequests` and filter to the Discovery request operation. Compare request count, duration, and failures. Inspect a slow operation in Application Insights transaction diagnostics or look up its `OperationId` in `AppDependencies` to see associated dependency calls that were captured.
3. Compare Azure's server timings with the k6 HTTP report and the Playwright wait samples. Local JSONL percentiles come from individual observations. Azure Metrics may show aggregated observations, so compare the same supported aggregation where possible. Stage windows can include unrelated demo traffic. For cleaner comparison, run against an idle demo and let telemetry ingestion catch up before reviewing.

The first pass is read-only. For a mixed-use follow-up, schedule a demo-only edit or publish manually during a loaded stage and note its time. Playwright results go in the audit report. Capturing the same visible wait from real visitors in Azure will require frontend telemetry.
