# First deployment and isolated rehearsal

This is the operator walkthrough for the workflow used on November 5, 2026.
Terraform provisions resources; GitHub Actions publishes packages; the dedicated
admin App Service runs Atlas.Admin against private SQL. Use App Service **SSH**
for schema and demo seeding. Azure Cloud Shell is useful for Azure CLI management,
but is not the private-network terminal where the successful schema commands ran.
Kudu is the deployment/WebJob service; its console is a different execution context.

## What was observed

| Step | Evidence and limit |
| --- | --- |
| Identity bootstrap | Succeeded after retry; managed-identity users and permissions verified |
| Live schema through SSH | `dotnet Atlas.Admin.dll --setup-database` succeeded on attempt 3; migrations and Full-Text initialization completed |
| Demo schema through SSH | Same command succeeded on attempt 2 |
| Website deployment | Operator confirmed live and demo were running |
| Demo seed | Use the guarded command below; capture its successful output during the rehearsal rather than assuming populated nodes prove the exact command used |
| Smoke checks | Deployment checks failed before the operator later observed available sites; timing is a hypothesis, not a confirmed root cause |

The private SQL administration workflow had multiple distinct failures: Kudu
HTTP 502 during package deployment, SQL login resets during bootstrap, and
managed-identity token acquisition cancellation during schema WebJob execution.
The SSH success does not explain or fix every WebJob failure. Keep the existing
workflow for publishing and bootstrap; use the successful SSH context for schema.
Detailed diagnostics remain in [DEPLOYMENT.md](DEPLOYMENT.md#deployment-preference-and-sql-connection-diagnostics).

## 1. Provision and record targets

Follow [the deployment runbook](DEPLOYMENT.md) for state storage, OIDC, GitHub
Environments, secrets, and the reviewed Terraform apply. For first-time identity
bootstrap, apply with `enable_sql_bootstrap=true`.

Record these outputs from the correct state: `resource_group`, `site_names`,
`admin_site_names`, `database_names`, and `site_urls`. Use output values instead
of reconstructing names. The original prefix was `atlas-7k4m9q2x`.

## 2. Publish Atlas.Admin and bootstrap each database

Run **Atlas manual private SQL administration** (`AtlasAdmin.yml`) from `main`:

| Variant | Operation | Confirmation | GitHub Environment |
| --- | --- | --- | --- |
| live | bootstrap | `bootstrap-live` | `atlas-ops-live` |
| demo | bootstrap | `bootstrap-demo` | `atlas-ops-demo` |

Each environment needs `AZURE_ADMIN_WEBAPP_NAME` and `AZURE_RESOURCE_GROUP` from
the recorded outputs and the Azure OIDC secrets documented in the runbook.
The workflow publishes all four job directories before triggering the selected
operation. Inspect packaging, deployment, invocation, and job completion separately.
An upload alone does not prove bootstrap completed.

After both bootstraps succeed, apply Terraform with `enable_sql_bootstrap=false`
to remove temporary administrator credentials. Do this before normal schema work.
Do not print app settings, tokens, passwords, or connection strings into evidence.

## 3. Apply schema from each admin App Service SSH terminal

Open **Azure Portal → App Services → the live admin app from `admin_site_names`
→ SSH**. Use the dedicated admin app, whose managed identity has schema privileges.
Check only the non-secret target settings:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/App_Data/jobs/triggered/atlas-schema
ls Atlas.Admin.dll
dotnet ./Atlas.Admin.dll --setup-database
```

Confirm the variant is `live` and the database matches the new live output before
running the command. Then repeat in the **demo admin app**, checking its demo target.
Wait for successful migration and supplemental Full-Text initialization output.
The C# host validates the variant, expected database, and managed identity itself.

If the DLL is missing, check the admin package deployment; do not run a different
copy or build in the cloud terminal. If a command fails, inspect the phase and
redacted error before retrying. Connection opening has bounded retries; a failure
later in migrations can leave completed work. Do not repeatedly replay commands
without reviewing their output.

## 4. Seed demo only

In the **demo admin app's SSH terminal**, verify the demo target again:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/App_Data/jobs/triggered/atlas-seed-demo
dotnet ./Atlas.Admin.dll --seed-demo
```

The demo admin app must already have its unique `ATLAS_DEMO_PASSWORD` configured
privately. The host rejects live and existing data. Never seed live or reset a
populated database to make a seed command pass.

## 5. Publish and verify each website

Run **Atlas manual Azure deployment** (`AtlasDeploy.yml`) for live, then demo.
It builds the Svelte static frontend and ASP.NET API into one deployed package.
The corresponding `atlas-live` / `atlas-demo` environment supplies
`AZURE_WEBAPP_NAME` and Azure OIDC secrets.

Record the deployment result separately from the smoke result. The current smoke
step checks `/health/live` and `/`, up to six attempts with ten-second pauses;
it does not test SQL readiness or loading nodes. If deployment succeeds and the
smoke step fails, inspect the app's startup logs and check those URLs again before
deciding to redeploy. A later successful page load does not retroactively pass CI.

Verify the initial feed, Full-Text search, persistent data, and sign-in/out at the
correct URL. Live may have no nodes; demo should show the intentionally seeded
content. Check a real API-backed action as well as the static page.
Database wake-up messaging is tracked separately in PTT-172.

## 6. Rehearse alongside the existing installation

A new `name_prefix` creates different names, but **a new prefix alone is not an
isolated installation**. A separate state key is essential. Changing the prefix
against the original state would plan replacements of the original installation.

The existing GitHub workflows hard-code their Environment names and have no
installation selector. Do not change their existing variables to rehearsal targets
while normal deployments can run. For the rehearsal, use a separate local Terraform
working directory and deploy the build artifacts explicitly to the new app names.
Before choosing an Actions-driven rehearsal, add distinct rehearsal Environments
and explicit target selection to the workflows; that change is not implemented by
this documentation update.

1. Create a fresh checkout at the commit being tested. Copy the example tfvars to
   an untracked file there and choose a fresh globally unique prefix, for example
   `atlas-rehearsal-<unique-suffix>`. Review name-length validation and Azure quotas.
2. Initialize from that checkout with the same protected state account/container
   if desired, but a **different blob key**, such as `atlas-rehearsal.tfstate`.
   Use the runbook's `terraform init` command with that new key. Do not copy the
   original `.terraform` directory or use `terraform init -migrate-state`.
3. Supply the rehearsal provisioning and demo passwords privately. Review the
   plan: it must create rehearsal resources and contain no updates, replacements,
   or deletes of the existing installation. Stop if the original names appear as
   change targets. Record the new state key and all output targets.
4. Apply, then follow steps 2–5 with explicitly isolated publishing targets.
   If using local Azure CLI artifact deployment, preserve the same package layout
   and trigger bootstrap only on the new admin hosts. Existing Actions currently
   target the original Environments; they cannot select this second installation.
5. Record each command, commit, run/artifact, target, completion result, and any
   retry. Mark unexecuted steps as unverified. A clean rehearsal means following
   the written sequence without undocumented fixes.

A second installation creates another B1 plan and private endpoint, with their
associated costs and SQL free-offer eligibility/quotas to check. Keep the original
installation running. Cleanup of the rehearsal is a separate deliberate operation;
SQL `prevent_destroy` intentionally blocks a casual full destroy.
