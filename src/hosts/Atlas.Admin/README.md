# Atlas.Admin

Atlas.Admin is the C# command-line tool for database setup. It runs locally for
development and in the Azure operations App Service for cloud administration.

It is separate from the interactive local `Atlas.Console`.

## Azure commands

| Command | Purpose |
| --- | --- |
| `--bootstrap-cloud-identities` | Establish contained users and SQL grants using temporary administrator credentials |
| `--setup-database` | Apply EF migrations, supplemental SQL and Full-Text setup |
| `--seed-demo` | Initialize demo accounts/content; refuses live and populated databases |

Use [First deployment](../../../infrastructure/FIRST-DEPLOYMENT.md) for publication,
SSH commands, execution order and troubleshooting. That is the deployment procedure;
this page does not maintain a second copy.

The package includes SSH tools and one optional bootstrap WebJob. The
[Bash entry point](../../../infrastructure/webjobs/run-webjob.sh) launches the
compiled C# program. Schema and seeding do not run automatically at app startup.

## Local database setup

Atlas.Admin uses the same .NET User Secrets ID as Atlas.Api and Atlas.Console.
It reads `ATLAS_SQL_CONNECTION_STRING`; environment variables can override it.

From the repository root:

```shell
dotnet run --project src/hosts/Atlas.Admin -- --initialize-local-database
```

This applies migrations and Full-Text setup without dropping the database.
The local command refuses Azure SQL and Production environments.

### Optional local demo data

For a fresh, disposable local database:

```shell
dotnet run --project src/hosts/Atlas.Admin -- --initialize-local-database --seed-demo
```

Seeding runs after schema setup and refuses existing users/content.

It uses `ATLAS_DEMO_PASSWORD` from User Secrets when present. Otherwise, the existing
local-only fixture password applies. Cloud demo seeding requires a unique password;
it does not use that local fallback.

## Cloud role administration is separate work

Interactive GlobalModerator grants in the local console are not a deployed cloud
feature. Cloud grant/revoke commands need operator authorization, audit and session
invalidation before being added here.

A private administration portal would be a separate feature, not a WebJob.
