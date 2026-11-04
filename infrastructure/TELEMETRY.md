# Azure observability for Atlas

The deployment provisions an Application Insights resource connected to a Log Analytics workspace through the Terraform [observability module](terraform/modules/observability/main.tf). This is resource configuration; the **application itself** selects the production exporter.

## What is wired in application code

- `Atlas.Api` reads `APPLICATIONINSIGHTS_CONNECTION_STRING`, injected by the [Web App Terraform module](terraform/modules/webapp/main.tf). Production refuses to start without it.
- The [API entry point](../src/hosts/Atlas.Api/Program.cs) initializes Microsoft's OpenTelemetry distro in non-development environments, with Azure Monitor export. Development retains local JSONL instead.
- [`CloudTelemetry`](../src/hosts/Atlas.Api/CloudTelemetry.cs) adapts existing `Atlas.Contracts.Observability` events into bounded, category-only logs, operation counters, duration histograms and discovery-cache metrics.
- Existing `Atlas.Contracts` ActivitySource spans and `Atlas.Observability` Meter instruments are registered for export. SQL query auto-instrumentation is deliberately disabled to reduce private-data leakage. The rate limit caps trace collection at two spans per second; sampling and ingestion can be tuned further.
- The Terraform code no longer enables a separate App Service auto-instrumentation agent, avoiding duplicate telemetry pipelines.

## Distinguish diagnostic telemetry from authoritative audit

The **SQL audit records and outbox** are durable evidence committed with important operations. Exported traces and diagnostic logs are best-effort. OpenTelemetry accepting an event into a local buffer **is not proof of durable cloud acceptance**, and it must not cause an audit outbox row to be marked delivered.

The current `ApiAuditWorker` still handles **local-file delivery only**. The code now exports Atlas semantic diagnostics and custom metrics in Azure, but **a cloud `IAuditDestination` with provider acknowledgement/retry and enabling `RequireCloudAuditDelivery` still need a separate implementation and test** before claiming full durable cloud audit delivery. Do not equate the two.

## Deployment acceptance checklist

1. From the deployed HTTPS site, produce an anonymous request and an authenticated allowed action without exposing private content.
2. Open Application Insights > Logs / Transaction search: verify request traces, safe Atlas semantic log categories and the `atlas.events.count` / `atlas.operation.duration` instruments. Allow time for ingestion.
3. Confirm the `ATLAS_DEPLOYMENT_VARIANT` tag distinguishes `live` and `demo` (no user identity used as a metric dimension).
4. Cause a safe rejected request and confirm only a bounded status/reason appears in diagnostics, with no URI query string, request body, token or secret.
5. Check ingestion costs, privacy/redaction, role-limited access, and retention. The Log Analytics workspace is configured for 30 days.
6. Confirm pending audit evidence remains queryable in SQL during exporter failure; cloud audit retry/ack is an independent follow-up, not silently claimed complete.

See [Azure Monitor's OpenTelemetry .NET instructions](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable) for exporter verification and troubleshooting. The workspace has a limited monthly free ingestion allowance on eligible billing models, but extra ingestion or services can be charged.
