# Atlas.Admin — private, non-interactive administration host

`Atlas.Admin` is a deliberately small .NET 10 executable for **protected Azure operations**. It is not the interactive `Atlas.Console`, not a public website and not a replica of the Atlas frontend.

The Azure `AtlasAdmin.yml` manual GitHub Actions workflow publishes this project, packages it into Linux triggered WebJobs and invokes exactly one selected operation:

| Command | Purpose | Safety |
| --- | --- | --- |
| `--bootstrap-cloud-identities` | Initial Azure SQL contained users and role grants | Requires temporary SQL admin credentials, correct live/demo target, explicit workflow approval |
| `--setup-database` | Apply checked-in EF migrations, SQL supplemental schema and Full-Text Search | Explicit operation only; no app-startup migration |
| `--seed-demo` | Seed initial demo accounts/content | Rejects live, requires a unique `ATLAS_DEMO_PASSWORD`, refuses existing data |

Only the production administration managed identity can connect to its matching private SQL database. The separate public API uses a more restricted identity. WebJobs run inside the App Service VNet integration and do **not** require direct SQL access from GitHub-hosted runners.

## Role administration — follow-up

The local `Atlas.Console` already supports trusted, interactive GlobalModerator grants; however, that is **not a deployed cloud administration feature** yet. Add tightly scoped `grant-global-moderator` and `revoke-global-moderator` commands here after establishing operator approvals, targeting, error reporting, durable security audit and session invalidation. Do not expose these operations as anonymous endpoints or assume a WebJob terminal can be used interactively.

An administrative **portal** (a private UI) would be a separate design choice; WebJobs are non-interactive jobs, not an interactive terminal or website.

## Local development

The interactive `src/hosts/Atlas.Console` remains a local development/demo host. The legacy `infrastructure/scripts/Initialize-AtlasDatabase.ps1` invokes that local Console against a SQL endpoint that your machine can already reach; it **cannot** connect to Azure SQL Private Link from an ordinary home machine. For Azure, use the manually triggered cloud workflow.

See [the operations guide](../../../infrastructure/PRIVATE-NETWORK-OPERATIONS.md) and [deployment runbook](../../../infrastructure/DEPLOYMENT.md). Azure wiring and end-to-end execution still require cloud verification.
