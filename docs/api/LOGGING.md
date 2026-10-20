# API logging and telemetry

## Terminal: host operation

`src/hosts/Atlas.Api/appsettings.json` configures the built-in simple console formatter:
one line per ordinary message, UTC time (`HH:mm:ss`), level, category, and message.
Multiline exceptions can still span lines. Framework and EF categories default to
Warning; Atlas categories remain Information. `Microsoft.Hosting.Lifetime` retains
Information so startup, listening addresses, environment, and shutdown remain visible.
Warnings and errors are not suppressed by these defaults.

Settings can be overridden through normal ASP.NET configuration, including user
secrets and environment variables. Program.cs retains two explicit Warning filters
for Hosting.Diagnostics and EF database commands; these prevent routine request URL
and SQL command information logs. Avoid enabling request body/header logging or EF
sensitive-data logging: account requests contain passwords, cookies and tokens.

The terminal is for lifecycle, infrastructure warnings and failures. Business
operation history belongs in diagnostics and SQL audit. Per-request start/end banners
would interleave under concurrent requests and add noise; operation IDs in telemetry
are the useful correlation mechanism.

## Files: business diagnostics and audit inspection

In Development, unless `Atlas:Telemetry:Enabled` is false, `ApiTelemetry` creates
`diagnostics`, `metrics`, and `traces` JSONL outputs. The audit worker also writes
`audit` inspection output after SQL outbox delivery. SQL remains the authoritative
audit record.

Default directory: `Path.Combine(AppContext.BaseDirectory, "logs", "observability")`.
For a normal Debug build this is under
`src/hosts/Atlas.Api/bin/Debug/net10.0/logs/observability/`; publish layouts differ.
`ATLAS_TELEMETRY_DIRECTORY` overrides the directory. Build output is ignored by Git.
This is separate from the confirmation payloads in `development-mail/`.

JSONL has one complete JSON object per line. To inspect local diagnostics in PowerShell:

```powershell
Get-Content ./src/hosts/Atlas.Api/bin/Debug/net10.0/logs/observability/diagnostics*.jsonl |
    ForEach-Object { $_ | ConvertFrom-Json } |
    Format-Table occurred_at_utc, event_name, outcome, reason_code, observability_id -AutoSize
```

Review a registration/confirmation/login, a successful mutation and a rejected
operation. Check bounded event/reason names, timestamps, correlation IDs, and the
absence of passwords, tokens, submitted text and raw request URLs. HTTP rejections
before `OperationContextMiddleware` (including rate limits and authorization) do
not pass through its business telemetry scope. Existing domain diagnostics are not
a complete HTTP access or security-rejection log.

## Known gaps to review next

- `EventJson` currently hardcodes `service = "atlas-console"` and environment
  `development`, including API-originated events and captured audit payloads.
  Correct host attribution requires coordinated diagnostic and audit capture work;
  the delivery worker must not relabel an event based on which host delivered it.
- API lifecycle messages currently come from framework logging; they are not
  automatically emitted as Atlas `host.started` / `host.stopped` events.
- Add bounded HTTP rejection observations (status, policy/reason, route template,
  correlation) without raw URLs, request bodies, identities or per-IP dimensions.
  Define aggregation/sampling before generating a warning for every rejected request.
- Review repeated audit-worker warnings during SQL outages and local output failure
  visibility; do not hide the underlying failure by globally raising log levels.
- PTT-136 covers production exporters, access, retention and alerting. Local file
  telemetry is Development-only in this host.

No runtime log files were available in the review environment. This documents the
configured output and code paths, not a verified live log sample.
