# First deployment and isolated rehearsal

This walkthrough follows the successful November 5, 2026 sequence, with the
admin Action and package layout simplified afterward. Rehearse the updated
publication path before treating it as verified in Azure.
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
The SSH success does not explain or fix every WebJob failure. Keep the
workflow for publishing and bootstrap; use the successful SSH context for schema.
Detailed diagnostics remain in [DEPLOYMENT.md](DEPLOYMENT.md#deployment-preference-and-sql-connection-diagnostics).

Before starting, use [Azure and GitHub setup](AZURE-SETUP.md) to locate every
Azure value and configure your own repository's Environments, OIDC and secrets.

For the existing installation, select **installation: main** in every cloud
Action and run from branch `main`. For a rehearsal, follow section 6 with
**installation: rehearsal** and the code branch under test.

## 1. Provision and record targets

Follow [the deployment runbook](DEPLOYMENT.md) for state storage, OIDC, GitHub
Environments, secrets, and the reviewed Terraform apply. For first-time identity
bootstrap, apply with `enable_sql_bootstrap=true`.

Record these outputs from the correct state: `resource_group`, `site_names`,
`admin_site_names`, `database_names`, and `site_urls`. Use output values instead
of reconstructing names. The original prefix was `atlas-7k4m9q2x`.

## 2. Publish Atlas.Admin and bootstrap each database

Run **Atlas admin tools (publish or bootstrap)** (`AtlasAdmin.yml`) from `main`:

| Variant | Operation | Confirmation |
| --- | --- | --- |
| live | bootstrap | `bootstrap-live` |
| demo | bootstrap | `bootstrap-demo` |

The shared credential Environment is `atlas-infra-apply` for every cloud Action.
App/resource-group targets are derived from the selected tracked profile and
checked before deployment; no per-target GitHub variables are required.
The Action has two operations:

| Operation | Result |
| --- | --- |
| `publish` | Upload the SSH tools and bootstrap job; no SQL command runs |
| `bootstrap` | Upload the same package, run identity bootstrap, and verify job completion; exact confirmation required |

Tools are published under `/home/site/wwwroot/tools/atlas-admin`; the only triggered
WebJob is `atlas-bootstrap`. Clean publication removes obsolete WebJob directories
from the dedicated admin host. It does not deploy the public website. Inspect
packaging, deployment, invocation, and bootstrap completion separately; an upload
alone does not prove bootstrap completed.

After bootstrap succeeds for every provisioned variant, apply Terraform with `enable_sql_bootstrap=false`
to remove temporary administrator credentials. Do this before normal schema work.
Do not print app settings, tokens, passwords, or connection strings into evidence.

## 3. Apply schema from each admin App Service SSH terminal

Open **Azure Portal → App Services → the live admin app from `admin_site_names`
→ SSH**. Use the dedicated admin app, whose managed identity has schema privileges.
Check only the non-secret target settings:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/tools/atlas-admin
ls Atlas.Admin.dll
dotnet ./Atlas.Admin.dll --setup-database
```

Confirm the variant is `live` and the database matches the new live output before
running the command. For a full installation, repeat in the **demo admin app**, checking its demo target.
For a single-variant installation, run only against the selected variant.
Wait for successful migration and supplemental Full-Text initialization output.
The C# host validates the variant, expected database, and managed identity itself.

For later schema maintenance, select operation `publish` with confirmation blank
to refresh the tools without repeating bootstrap. If the DLL is missing, check
the admin package deployment; do not run a different
copy or build in the cloud terminal. If a command fails, inspect the phase and
redacted error before retrying. Connection opening has bounded retries; a failure
later in migrations can leave completed work. Do not repeatedly replay commands
without reviewing their output.

## 4. Seed demo only

In the **demo admin app's SSH terminal**, verify the demo target again:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/tools/atlas-admin
dotnet ./Atlas.Admin.dll --seed-demo
```

The demo admin app must already have its unique `ATLAS_DEMO_PASSWORD` configured
privately. The host rejects live and existing data. Never seed live or reset a
populated database to make a seed command pass.

## 5. Publish and verify each website

Run **Atlas manual Azure deployment** (`AtlasDeploy.yml`) for each provisioned
variant. Use the same installation and rehearsal name used during provisioning.
It builds the Svelte static frontend and ASP.NET API into one deployed package.
The shared `atlas-infra-apply` Environment supplies Azure OIDC secrets; the
selected tracked profile supplies the target app name.

Choose operation `deploy` to publish. The separate readiness job makes one short
request each to Atlas liveness and the root page. If Azure has not finished startup,
the summary says **package publication succeeded; verification pending**. It does
not keep the runner waiting for minutes. Later select operation `verify`, with the
same installation/name/variant, to check the site without rebuilding or redeploying.
Verify-only fails if the site is still unavailable. Inspect startup logs if it
persists. These checks do not query SQL or confirm the running commit.

Verify the initial feed, Full-Text search, persistent data, and sign-in/out at the
correct URL. Live may have no nodes; demo should show the intentionally seeded
content. Check a real API-backed action as well as the static page.
Database wake-up messaging is tracked separately in PTT-172.

## 6. Rehearse alongside the existing installation

Each cloud Action now asks for **installation: rehearsal or main**. This is
independent of **variant: live or demo** and the code branch chosen for the run.
The default installation is rehearsal. Main still uses the existing profile
and resource URLs; the label does not rename existing Azure resources.

| Choice | Code branch | Target configuration |
| --- | --- | --- |
| `main` | Must be `main` | Existing main profile and state/resources; shared credentials |
| `rehearsal` | May be a feature branch or `main` | Separate rehearsal profile, state and resources; shared credentials |

### One-time shared setup

Consolidate Azure credentials/account settings in the existing `atlas-infra-apply`
Environment using [Azure setup](AZURE-SETUP.md). No new rehearsal Environments or
OIDC subjects are needed. Keep its reviewer protection and allow only trusted
branches, including this PR branch while testing. Both installations deliberately
use that identity's permissions.

The Action has a `rehearsal_name` field. Enter a unique suffix such as `trial02`;
all four Actions then use prefix `atlas-rehearsal-trial02` and state key
`rehearsal/atlas-rehearsal-trial02.tfstate`. Keep the name identical through the
sequence. Blank uses the tracked example `atlas-rehearsal-a1b2c3`; it is not a
branch-derived or automatically unique value. Main retains `atlas-7k4m9q2x` and
its confirmed `atlas-capstone.tfstate` key.

For plan/apply, choose the applications: `default` means main both and rehearsal
live only; `live`, `demo`, and `both` explicitly choose the resource set. The steps
below demonstrate **both** to rehearse the full workflow. A live-only rehearsal
omits all demo bootstrap/schema/seed/deploy steps; a demo-only rehearsal omits live.

### Walk through a rehearsal from this PR branch

1. Run **Atlas Azure Terraform plan**, choose installation `rehearsal`, and select
   branch `docs/deployment-ssh-walkthrough`. Enter rehearsal name `trial02` and
   applications `both`. Review the proposed resources.
2. Run **Atlas Azure Terraform apply** from that same branch with installation
   `rehearsal`, the same name `trial02`, applications `both`, and
   `enable_sql_bootstrap=true`. Review the plan and approve the
   shared Environment job. Expect new rehearsal resources, with no main
   resources selected for changes.
3. Verify the new Terraform outputs match the selected prefix. Target app names
   are derived automatically; there is no post-provisioning GitHub variable copy.
   Ensure the shared deployment identity has the required Azure role scope.
4. Run **Atlas admin tools** with installation `rehearsal` and name `trial02`, separately for live
   and demo, operation `bootstrap`, with the matching confirmation. Then apply
   Terraform again against `rehearsal`, name `trial02`, applications `both`, with bootstrap disabled.
5. Use the new admin apps' SSH terminals for schema and demo seeding as above.
   Run **Atlas manual Azure deployment** with installation `rehearsal`, name `trial02`, and each
   variant, selecting the branch you want to test.
6. Verify the new URLs and record the code commit, selected installation, state
   key, operation results and any retries. Do not treat an unexecuted rehearsal
   as evidence that the new workflow has succeeded in Azure.

The workflow files already exist on the repository's default branch. You can
therefore dispatch their branch versions before merging this PR. In Actions,
use the **Run workflow → Branch** selector. If the UI still shows the older
input form, use GitHub CLI with an explicit ref and inputs:

```bash
gh workflow run AtlasAzurePlan.yml --repo <owner>/<repo> --ref docs/deployment-ssh-walkthrough -f installation=rehearsal -f rehearsal_name=trial02 -f deployment_shape=both
gh workflow run AtlasAzureApply.yml --repo <owner>/<repo> --ref docs/deployment-ssh-walkthrough -f installation=rehearsal -f rehearsal_name=trial02 -f deployment_shape=both -f enable_sql_bootstrap=true
gh workflow run AtlasAdmin.yml --repo <owner>/<repo> --ref docs/deployment-ssh-walkthrough -f installation=rehearsal -f rehearsal_name=trial02 -f variant=demo -f operation=bootstrap -f confirmation=bootstrap-demo
gh workflow run AtlasDeploy.yml --repo <owner>/<repo> --ref docs/deployment-ssh-walkthrough -f installation=rehearsal -f rehearsal_name=trial02 -f variant=demo
```

These are separate commands for their respective steps; complete each prerequisite
before running the next. A newly added workflow filename must first exist on the
default branch for ordinary manual dispatch. That does not require merging every
subsequent code or workflow change before testing it on a branch.

After shared setup, repeat runs by selecting `rehearsal`; no Environment-variable
swapping is needed. Reusing the prefix/state updates that same rehearsal
installation. To create another independent installation, enter a new rehearsal name, retaining
the old state for deliberate cleanup. Each run selects one rehearsal name;
the text field chooses a resource/state name, not a GitHub Environment.

A rehearsal creates another B1 plan and private endpoint, with associated costs
and SQL eligibility/quotas to check. Cleanup is separate; SQL `prevent_destroy`
blocks a casual full destroy. Profile and naming checks reduce mistakes but do not isolate Azure permissions.
Only run reviewed, trusted branch code with the shared deployment credentials.

GitHub references: [manual workflow dispatch](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow)
and [deployment Environment protection](https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments).

## Deliberate cleanup

Use the separate [rehearsal cleanup workflow](REHEARSAL-CLEANUP.md) for an explicit
reviewed destroy plan. It has no main target and updates the rehearsal state while
preserving shared backend storage. Normal deployment still stops variant removal,
deletion and replacement. Manual Azure resource-group deletion does not update
Terraform state immediately and can lead to recreation by the provisioning Action.

## Rehearsal observations (2026-11-06)

For `atlas-rehearsal-1234134`, live-only provisioning and SSH schema setup completed.
Admin package publication succeeded, but the Azure CLI bootstrap trigger returned
HTTP 404 before completion polling began. The published C# bootstrap command then
succeeded through SSH on connection attempt 3. The trigger's root cause remains
unconfirmed; do not repeatedly invoke bootstrap without checking its result.

If publication succeeded but triggering failed, the operator can explicitly run
`dotnet ./Atlas.Admin.dll --bootstrap-cloud-identities` from the admin SSH tools
directory while temporary bootstrap credentials are still enabled. Confirm the
success output before disabling those credentials through Terraform.

Website package publication also succeeded, while the old six-attempt smoke check
received HTTP 404. The operator later confirmed the Atlas site became available.
The new deploy/verify interface records that distinction without treating delayed
readiness as a failed package publication. Account/feed acceptance remains a
separate verification step.
