# Atlas deployment runbook

The deployment uses Terraform for Azure resources, GitHub Actions for publication
and identity bootstrap, and the dedicated admin App Service SSH terminal for schema
and demo initialization. Start with [Azure setup](AZURE-SETUP.md) for every shared
secret/variable and its source, then [the walkthrough](FIRST-DEPLOYMENT.md).

## Architecture

A full live/demo installation uses one Linux B1 plan for four apps: live, demo
and their operations hosts. A single-variant installation uses two apps and one
database, sharing the same plan/network pattern. Each public app serves the compiled Svelte static frontend and
ASP.NET Core API together from one HTTPS origin. `/atlas-public-api/*` and
`/atlas-session-api/*` are rewritten by the .NET host to `/api/*`.

The selected SQL databases share one logical server, private endpoint, VNet, network security
group and private DNS zone. SQL public network access stays disabled. App Service
VNet integration provides private SQL reachability; neither a normal laptop nor a
GitHub-hosted runner connects directly to SQL. Keep the normal SQL hostname in
connection strings rather than replacing it with a private IP.

Terraform creates four distinct application managed identities and passwordless SQL
connection strings. Bootstrap creates contained users and grants each runtime its
matching database access; each admin host uses its separate privileged identity.
The shared GitHub deployment identity is a different identity used to manage Azure
resources, not the runtime identity used for application SQL requests.

## Shared credentials and target selection

All four manual cloud Actions use the single existing GitHub Environment
`atlas-infra-apply`. It holds Azure credentials and common subscription, location,
SQL administrator and state-account configuration. The public
[deployment profiles](config/deployment-targets.json) supply installation prefixes
and state keys. Website, admin app and resource-group names are derived from them.
No extra GitHub Environment is required per target or variant.

| Run control | Meaning |
| --- | --- |
| Branch | Code/workflow version to run |
| Installation | `main` for the existing installation or `rehearsal` for the test installation |
| Applications to provision | `default` (main both, rehearsal live), `live`, `demo`, or `both` |
| Rehearsal name | Optional name/suffix; selects matching resource prefix and state in every Action |
| Variant | `live` or `demo` already provisioned inside that installation |
| Admin operation | `publish` tools only or `bootstrap` identities with confirmation |

Rehearsal is the default installation. Main targets require branch `main`. The
shared Environment must allow the trusted feature branch when testing before merge.
Both targets use the shared identity's Azure permissions; target checks prevent
mistakes, not malicious use of credentials by untrusted branch code.

The Action files already exist on main, so their PR-branch versions can be manually
dispatched before merging. See [rehearsal instructions](FIRST-DEPLOYMENT.md#6-rehearse-alongside-the-existing-installation).

## Workflow responsibilities

| Workflow | Responsibility |
| --- | --- |
| [AtlasInfrastructure.yml](../.github/workflows/AtlasInfrastructure.yml) | Offline Terraform validation, admin compilation, policy/workflow tests and security checks |
| [AtlasAzurePlan.yml](../.github/workflows/AtlasAzurePlan.yml) | Manual Azure-connected preview; never applies or uploads a saved plan |
| [AtlasAzureCleanup.yml](../.github/workflows/AtlasAzureCleanup.yml) | Rehearsal-only cleanup: preview deletion or explicitly delete after confirmation and approval |
| [AtlasAzureApply.yml](../.github/workflows/AtlasAzureApply.yml) | Plan, encrypt saved plan, obtain approval and apply that exact plan |
| [AtlasAdmin.yml](../.github/workflows/AtlasAdmin.yml) | Publish SSH maintenance tools; optionally run confirmed identity bootstrap and verify completion |
| [AtlasDeploy.yml](../.github/workflows/AtlasDeploy.yml) | Build and publish the frontend/backend package for the selected installation/variant |

The shared Environment requires reviewer approval; apply explicitly checks it.
A fresh apply run is required rather than rerunning a previously saved plan.
State and decrypted plans contain secrets and must not be committed or exposed.
The preview uses a private temporary plan for deletion checks, removes it on exit,
and uploads no plan artifact. JSON state/plan values are piped directly into checks
without being printed.

The normal deployment path never deletes or replaces resources. A preflight checks
tracked variants before planning; a plan inspection rejects every action containing
`delete`, including replacements, before encryption/apply. The decrypted saved plan
is checked again immediately before apply. A stopped run explains whether to keep
the existing application set, choose a new name, or perform deliberate cleanup.
If a deployment genuinely needs replacement, review it through a separate maintenance
procedure; this Action has no destructive override. Rehearsal installation deletion
uses the separate [cleanup workflow](REHEARSAL-CLEANUP.md).

## First deployment order

1. Configure state storage, shared Environment, OIDC and permissions once using
   [Azure setup](AZURE-SETUP.md). Review the tracked profile before selecting it.
2. Preview the selected installation. For initial grants, apply with
   `enable_sql_bootstrap=true`.
3. Run admin `bootstrap` for each selected variant with exact `bootstrap-live` /
   `bootstrap-demo` confirmation. A package upload alone is not successful bootstrap.
4. Apply again with bootstrap disabled to remove temporary provisioning credentials.
5. In each admin App Service SSH terminal, open
   `/home/site/wwwroot/tools/atlas-admin` and run
   `dotnet ./Atlas.Admin.dll --setup-database`.
6. For an empty demo only, run `dotnet ./Atlas.Admin.dll --seed-demo` after schema
   succeeds. It requires `ATLAS_DEMO_PASSWORD` and refuses live or populated data.
7. Deploy the selected installation's provisioned public apps separately. Verify
   a real database-backed request, authentication and Full-Text search.

Later app releases normally need only the website deployment Action. Run schema
explicitly when required. Admin `publish` refreshes the tools without executing SQL.
Its clean deployment removes retired diagnostic/schema/seed job directories only
on the dedicated admin host. It does not publish the public website.

## Local Terraform alternative

Install Azure CLI, Terraform, .NET 10 and Node 24. From `infrastructure/terraform`,
copy the example to an untracked tfvars file and supply provisioning/demo passwords
privately. Match the selected installation's prefix and state key:

```powershell
az login
az account set --subscription "<your-subscription-id>"
Copy-Item terraform.tfvars.example terraform.tfvars
$env:TF_VAR_sql_admin_password = "<strong-generated-password>"
terraform init -backend-config="resource_group_name=<state-resource-group>" -backend-config="storage_account_name=<state-account>" -backend-config="container_name=tfstate" -backend-config="key=<installation-state-key>" -backend-config="use_azuread_auth=true"
terraform fmt -check -recursive
terraform validate
terraform plan -out=atlas.tfplan
terraform apply atlas.tfplan
terraform output
```

Review before applying. Never substitute a new state key for an existing deployment
or use `-target` as the normal workflow. SQL `prevent_destroy` protects databases;
cleanup requires a separate deliberate recovery/deletion plan.

Local database setup remains C#: `dotnet run --project src/hosts/Atlas.Admin --
--initialize-local-database` (one line), optionally with `--seed-demo` for a fresh
local database. It reads the existing .NET User Secrets store and refuses Azure SQL.

## Deployment preference and SQL connection diagnostics

The operator confirmed live/demo deployment and successful SSH schema/Full-Text
initialization. Earlier admin WebJob attempts hit Kudu deployment HTTP 502s, SQL
login resets and managed-identity token acquisition cancellation at different
phases. Their root causes were not established. Diagnostic/schema/seed WebJob
shortcuts and temporary probe code are retired; the successful SSH path is kept.

Bootstrap and schema retry recognized transient failures while opening a connection,
at most three attempts ten seconds apart. They do not replay migrations or grants.
A later failure can leave completed migrations; inspect its phase before rerunning.
Logs include redacted provider details and structured SQL/socket codes. Demo seed
does not have that connection-opening retry policy.

Website publication and readiness checks have distinct results. The deployment
Action now offers `deploy` and `verify`. Deploy publishes the package and performs
a short HTTP check in a separate job: one request to `/health/live` expecting
Atlas's `{"status":"ok"}` response, and one to `/`, each bounded to eight seconds.
It does not wait/retry for minutes. A successful publication with an unavailable
site is explicitly **verification pending**, with a warning and summary, rather
than a failed publication or a claimed readiness pass.

Later, run the same Action with operation `verify` and the same target/name/variant.
That operation skips build, artifacts, Azure login and deployment. It performs only
the two public HTTP checks, failing if readiness is still unconfirmed. A successful
check does not establish SQL readiness, authentication, or the exact running release.
Persistent failure requires startup-log inspection rather than unlimited waiting.

Database wake-up UI is tracked in PTT-172. See [telemetry](TELEMETRY.md) for cloud
monitoring and the separate durable-audit follow-up.

## Costs and verification

The deployment shares a billable B1 plan and private endpoint while using explicitly
enabled SQL serverless free allowances. Private DNS, state storage, data transfer
and monitoring beyond included usage may also incur charges. Verify current
region/subscription pricing and SQL eligibility; this architecture is not entirely
free. The SQL setting pauses at the free limit rather than opting into paid overage.
A second installation provisions another plan/endpoint and has its own costs.

Record the reviewed plan, selected profile/state key, source commit, workflow results,
URLs and real persistence, search, auth and monitoring checks. Never include secrets
in that evidence. Offline tests validate configuration and scripts but do not
prove a complete Azure rehearsal succeeded.
