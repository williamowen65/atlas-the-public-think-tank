# Atlas.Admin — private, non-interactive administration host

`Atlas.Admin` is a deliberately small .NET 10 command-line host for **local development database setup** and **protected Azure SSH and identity-bootstrap operations**. It is not the interactive `Atlas.Console`, a public website, or a replica of the Atlas frontend.

The **Atlas admin tools (publish or bootstrap)** Action (`AtlasAdmin.yml`) has two
operations (for the selected `main` or `rehearsal` installation): `publish` uploads the tools without running SQL; `bootstrap` publishes
the same package and runs confirmed one-time SQL identity setup. Schema and demo
seeding are explicit SSH commands, absent from the Action dropdown.

The package contains `/home/site/wwwroot/tools/atlas-admin` for SSH and a single
`atlas-bootstrap` triggered WebJob. Publishing cleans the dedicated admin host's
deployment directory, removing old diagnostic/schema/seed jobs. The temporary
managed-identity diagnostic command is retired.

| Execution context | Command |
| --- | --- |
| Operations App Service SSH (verified rehearsal route) | Tools directory, then `dotnet ./Atlas.Admin.dll --bootstrap-cloud-identities` |
| Optional bootstrap Action | Runs the same command; its trigger returned HTTP 404 in the rehearsal |
| Admin App Service SSH | `cd /home/site/wwwroot/tools/atlas-admin` then `dotnet ./Atlas.Admin.dll --setup-database` |
| Demo admin App Service SSH, after schema | Same directory, then `dotnet ./Atlas.Admin.dll --seed-demo` |

The commands retain these guards:

| Command | Purpose | Safety |
| --- | --- | --- |
| `--bootstrap-cloud-identities` | Initial Azure SQL contained users and role grants | Requires temporary SQL admin credentials, correct live/demo target, authorized operations host; workflow confirmation when using the Action |
| `--setup-database` | Apply checked-in EF migrations, SQL supplemental schema and Full-Text Search | Explicit operation only; no app-startup migration |
| `--seed-demo` | Seed initial demo accounts/content | Rejects live, requires a unique `ATLAS_DEMO_PASSWORD`, refuses existing data |

Only the production administration managed identity can connect to its matching private SQL database. The separate public API uses a more restricted identity. SSH commands and the bootstrap job run inside the App Service VNet integration and do **not** require direct SQL access from GitHub-hosted runners.

## Role administration — follow-up

The local `Atlas.Console` already supports trusted, interactive GlobalModerator grants; however, that is **not a deployed cloud administration feature** yet.

Add tightly scoped `grant-global-moderator` and `revoke-global-moderator` commands here after establishing operator approvals, targeting, error reporting, durable security audit and session invalidation. Do not expose these operations as anonymous endpoints or assume a WebJob terminal can be used interactively.

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

The protected `AtlasAdmin.yml` workflow publishes this same C# project, optionally invoking only identity bootstrap, with environment checks and **managed identity** authentication. Cloud operations do **not** load local User Secrets and require the shared `atlas-infra-apply` GitHub Environment and Azure OIDC access. Target names come from the selected tracked profile.

After provisioning, run `publish` for each selected variant. In the matching
operations App Service SSH terminal:

1. Run `--bootstrap-cloud-identities` while temporary credentials are enabled.
2. Confirm success, then apply Terraform with bootstrap disabled.
3. Run `--setup-database`.
4. For an empty demo database only, run `--seed-demo`.

A live-only installation has no demo steps. The optional bootstrap Action failed
at invocation (HTTP 404) during the rehearsal; SSH bootstrap succeeded. The Azure WebJob entry point is Bash because App Service Linux launches the compiled C# executable; PowerShell is not involved.

See [the operations guide](../../../infrastructure/PRIVATE-NETWORK-OPERATIONS.md) and [deployment runbook](../../../infrastructure/DEPLOYMENT.md). Schema and Full-Text setup succeeded through the admin App Service SSH terminal for live and demo. Use the [first-deployment walkthrough](../../../infrastructure/FIRST-DEPLOYMENT.md) for commands and target checks; schema and seed are no longer packaged as triggered WebJobs.
