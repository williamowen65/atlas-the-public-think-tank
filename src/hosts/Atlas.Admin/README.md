# Atlas.Admin — private, non-interactive administration host

`Atlas.Admin` is a deliberately small .NET 10 command-line host for **local development database setup** and **protected Azure operations**. It is not the interactive `Atlas.Console`, a public website, or a replica of the Atlas frontend.

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

## Local development — C# only

`Atlas.Admin` uses the **same .NET User Secrets ID** as `Atlas.Api` and `Atlas.Console`. The existing local `ATLAS_SQL_CONNECTION_STRING` is read automatically (environment variables can override it). No PowerShell script or separately copied secrets are needed.

From the repository root:

```shell
dotnet run --project src/hosts/Atlas.Admin -- --initialize-local-database
```

For a **fresh, disposable local database** where you also want the existing demo fixture data:

```shell
dotnet run --project src/hosts/Atlas.Admin -- --initialize-local-database --seed-demo
```

The first command applies EF migrations and SQL Full-Text setup. The optional seed operation runs only after successful schema initialization and refuses to overwrite existing users or content. The seed can use `ATLAS_DEMO_PASSWORD` from User Secrets; without it, the established local-only fixture password is used. The local command **refuses Azure SQL** and Production environments. It does **not** drop the database or erase data.

`Atlas.Console` is still a separate, local interactive demo application; it is not the database-initialization tool or deployed Azure administration executable.

## Azure administration — publishing and SSH

The protected `AtlasAdmin.yml` workflow publishes this same C# project and invokes one cloud command at a time, with environment checks and **managed identity** authentication. Cloud operations do **not** load local User Secrets and require authorized GitHub Environments and Azure OIDC access.

After Terraform provisioning and GitHub/Azure authorization, run `bootstrap` for live and demo (temporary SQL provisioning credentials must be enabled), then disable/re-apply those credentials, run `schema` for both, and `seed-demo` for demo only. The Azure WebJob entry point is Bash because App Service Linux launches the compiled C# executable; PowerShell is not involved.

See [the operations guide](../../../infrastructure/PRIVATE-NETWORK-OPERATIONS.md) and [deployment runbook](../../../infrastructure/DEPLOYMENT.md). Schema and Full-Text setup succeeded through the admin App Service SSH terminal for live and demo. Use the [first-deployment walkthrough](../../../infrastructure/FIRST-DEPLOYMENT.md) for commands and target checks; schema/seed WebJob execution remains separately unverified.
