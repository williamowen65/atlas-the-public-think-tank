# Check Azure telemetry

**Status:** telemetry is configured in code and Terraform. Cloud delivery has not
yet been confirmed by the operator.

Use this guide to check a deployed installation. A working website or a successful
deployment Action does not prove telemetry is arriving.

## What each tool shows

| Azure tool | Use it for |
| --- | --- |
| App Service **Log stream** | Startup messages and application console output |
| **Application Insights** | Application performance and request investigation |
| **Log Analytics → Logs** | Query stored requests, Atlas diagnostic logs and metrics |

**Live Metrics is disabled in the current application configuration.**
Use stored logs for this walkthrough.

## 1. Find the correct resources

Open **Azure Portal → Resource groups → your installation’s resource group**.

Find:

- Public App Service: `<prefix>-live` or `<prefix>-demo`.
- Application Insights: `<prefix>-insights`.
- Log Analytics workspace: `<prefix>-logs`.

Use the installation prefix selected during deployment. Live and demo share one
monitoring resource pair within that installation; a rehearsal has its own pair.

Your Azure account needs permission to read those monitoring resources and query
their logs.

## 2. Generate a little activity

1. Note the current time, site URL and variant.
2. Open the deployed site and load its feed.
3. Open a node or search for content.
4. If you have a test account, sign in and out.

Use ordinary test activity. You do not need to create production data or deliberately
cause an error to confirm basic reporting.

An empty live feed can still produce API requests. Opening only a cached static
page may not produce the activity you expect.

## 3. Check App Service Log stream

Open the **public App Service → Monitoring → Log stream**.

Look for:

- Application startup and listening messages.
- Startup errors, if the site is unavailable.
- Messages beginning `Atlas event` after relevant application operations.

Log streaming may require App Service application logging to be enabled. If prompted,
review the logging settings before enabling it; do not change production verbosity
to Debug just for this check.

Console output confirms the app is logging. It does **not** confirm export to
Application Insights.

## 4. Check stored requests

Open **Log Analytics workspace → Logs** and switch to **KQL mode** if needed.
Select a recent time range, such as the last 30 minutes.

Run:

```kusto
AppRequests
| where TimeGenerated > ago(30m)
| project TimeGenerated, Name, ResultCode, Success, DurationMs, OperationId
| order by TimeGenerated desc
| take 20
```

Look for requests near your test time and their response status.
Keep an `OperationId` if you want to investigate related records.

Allow a few minutes for export and ingestion, then refresh. Traces are sampled,
so do not expect every browser request to have a stored request row.

### If you opened Logs from Application Insights

These queries deliberately use **Log Analytics** table and field names.
Application Insights Logs uses names such as `requests`, `traces` and
`customMetrics` instead of `AppRequests`, `AppTraces` and `AppMetrics`.

Return to the workspace’s Logs view to run the examples unchanged.
See Microsoft’s [table-name mapping](https://learn.microsoft.com/azure/azure-monitor/app/data-model-complete#types-of-telemetry).

## 5. Check Atlas diagnostic logs

In the same workspace, run:

```kusto
AppTraces
| where TimeGenerated > ago(30m)
| where Message startswith "Atlas event "
| project TimeGenerated, Message, Properties, OperationId
| order by TimeGenerated desc
| take 20
```

Atlas messages contain an event name, outcome, reason and deployment variant.
Expand a row’s `Properties` to inspect the structured fields.

Check that the variant matches your test site. Atlas’s custom diagnostic message
is designed to omit body text, SQL, credentials, actor IDs and target IDs.

If requests appear but these messages do not, check the activity generated and
the effective log-level filters before concluding all export has failed.

## 6. Check custom metrics

Run:

```kusto
AppMetrics
| where TimeGenerated > ago(30m)
| where Name startswith "atlas."
| project TimeGenerated, Name, Sum, ItemCount, Properties
| order by TimeGenerated desc
| take 30
```

The application defines these instruments:

| Instrument | Meaning |
| --- | --- |
| `atlas.events.count` | Counts observed Atlas operations |
| `atlas.operation.duration` | Measures operation duration in milliseconds |
| `atlas.discovery.cache.lookups` | Counts discovery cache hits and misses |

Expand `Properties` to inspect dimensions such as `variant`, `operation` and
`outcome`. Dimensions depend on the instrument.

Metrics are exported in batches and aggregates. A metric row is not necessarily
one operation, and not every action produces every instrument.

## If nothing appears

Check these in order:

1. **Target and time:** correct installation, workspace and time range.
2. **Activity:** a real API request reached the running site.
3. **Startup:** inspect the public app’s Log stream for errors.
4. **Configuration:** `APPLICATIONINSIGHTS_CONNECTION_STRING` exists and points to
   the intended resource; `Atlas:Telemetry:Enabled` is not false.
5. **Delivery:** review Azure ingestion/network errors, access permissions and
   resource ingestion limits.

Inspect configuration privately; do not paste connection strings into logs or
screenshots. A “table not found” error can mean the wrong query scope or that the
table has not received data yet.

For exporter-specific diagnostics, use Microsoft’s
[.NET OpenTelemetry troubleshooting guide](https://learn.microsoft.com/troubleshoot/azure/azure-monitor/app-insights/telemetry/opentelemetry-troubleshooting-dotnet).

## Record what you confirmed

Keep a small verification note:

- Installation, variant and test time.
- Activity performed.
- Whether requests, Atlas logs and metrics appeared.
- Any missing signal or export error.

Record partial success explicitly. Seeing requests alone does not confirm custom
metrics, and seeing logs does not confirm durable audit delivery.

## Diagnostic telemetry is not the audit trail

SQL audit records and their outbox are the durable application evidence.
Cloud diagnostic export is best-effort; buffering a log is not an acknowledgement
that Azure stored an audit record.

The current `ApiAuditWorker` delivers to local files only. A cloud
`IAuditDestination`, acknowledgement/retry handling and
`RequireCloudAuditDelivery` still need separate implementation and testing.

## Configuration reference

- [Program.cs](../src/hosts/Atlas.Api/Program.cs): non-development Azure Monitor
  export, trace sampling target of two per second, Live Metrics disabled, SQL client
  auto-instrumentation disabled.
- [CloudTelemetry.cs](../src/hosts/Atlas.Api/CloudTelemetry.cs): bounded Atlas logs
  and custom metrics, including variant dimensions.
- [ApiTelemetry.cs](../src/hosts/Atlas.Api/ApiTelemetry.cs): local versus cloud
  behavior and local-only audit delivery.
- [Observability module](terraform/modules/observability/main.tf): shared monitoring
  resources and configured workspace retention of 30 days.

The trace sampling setting does not cap log or metric ingestion. Review monitoring
usage and retention before assuming telemetry is free.
