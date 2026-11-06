# Delete a rehearsal installation with Terraform

Use **Atlas rehearsal cleanup (plan or delete)**, from
[`AtlasAzureCleanup.yml`](../.github/workflows/AtlasAzureCleanup.yml). It is a
separate path from normal deployment, which continues to reject deletion and
replacement. It removes the managed resources for one rehearsal installation,
including its SQL data, apps, network and resource group, and updates its Terraform
state. It does not offer the main installation as a target.

## Before using it

The workflow is available on the default branch. It deliberately
requires a fresh manual run from branch `main`; no extra branch needs access to
GitHub Environment secrets. It reuses the existing **`atlas-infra-apply`** shared
Environment, Azure OIDC identity, state-account configuration and required reviewers.
No new Environment, password or GitHub variable is needed.

Confirm the rehearsal's exact name and finish/cancel any deployment or maintenance
work using it. Cleanup shares Terraform apply's rehearsal concurrency group, so
those two workflows cannot modify rehearsal state concurrently. It does not cancel
an unrelated admin job automatically. Deletion removes the rehearsal's database
contents; preserve anything needed before approving it.

## 1. Preview the destroy plan

In GitHub **Actions → Atlas rehearsal cleanup (plan or delete) → Run workflow**:

| Field | Value for the existing example |
| --- | --- |
| Branch | `main` |
| Rehearsal name | `1234134` (or full prefix `atlas-rehearsal-1234134`) |
| Operation | `plan` |
| Confirmation | Leave blank for plan-only |

Approve access to the shared Environment if prompted. This mode does not apply
anything. It verifies the existing state blob before initializing the backend;
missing state is an error rather than a guessed deletion target.

Check the resolved target is **`atlas-rehearsal-1234134-rg`**, with state key **`rehearsal/atlas-rehearsal-1234134.tfstate`**. Review the destroy list. Every managed resource must be inside that exact resource group and configured subscription.

The workflow rejects another group, main resource IDs, creation, updates and replacements. An already-empty tracked state is a harmless no-op. It does not blindly delete an Azure resource group based only on its name.

No application-count choice is needed: destroy mode removes the resources actually
tracked for this installation, whether it was live-only, demo-only or both.

## 2. Explicitly request deletion

Start a **new run** with the same name, operation **`delete`**, and confirmation:

```text
atlas-rehearsal-1234134-rg
```

The confirmation must match the full resolved resource-group name exactly. The
workflow produces a new destroy plan; it does not reuse a previous plan-only run.
Review that plan before approving **Delete the approved rehearsal installation**.
A recorded shared-Environment approval is required. Old job retries are rejected;
start a new run if a plan expires or deletion partially fails.

The delete job verifies/decrypts the saved plan, checks the rehearsal scope again,
and applies that exact plan. It does not run a new unreviewed `terraform destroy`.
Plans are encrypted for transfer with one-day artifact retention; plaintext plans
and temporary overrides are removed from the runner. Raw state/plan JSON is never
printed or uploaded.

## SQL protection and resource-group handling

The checked-in SQL module keeps **`prevent_destroy = true`**. After rehearsal state
scope is checked, only the cleanup checkout generates a temporary Terraform JSON
override setting it to false. The delete job recreates that same override for the
reviewed plan. Normal apply/deployment does not generate it or relax its guards.

A second temporary provider override explicitly requires the resource group to be
empty before the provider removes it. If you manually added unrelated/untracked
resources to the rehearsal group, final group deletion can fail instead of silently
deleting those resources. Inspect and handle them deliberately before starting a
fresh cleanup run. Azure deletion locks or permissions can also cause partial failure.

This workflow is scoped to resources tracked by this Terraform root. It is not a
universal Azure resource-group deletion tool and is not a cleanup path for main.

## What stays afterward

The shared Terraform storage account/container and the rehearsal state blob are
retained. On successful deletion, Terraform records the managed resources as gone.
The blob remains a private state record; it can still contain metadata and outputs,
so it is not a public artifact. State versioning/retention policy continues to apply.

Reusing the same rehearsal name with the normal provisioning Action can recreate
that installation. A different name selects different resources/state. Do not delete
or edit main's `atlas-capstone.tfstate` as part of this procedure.

If you deleted the group manually in Azure already, Terraform's next refresh can
notice the missing resources. Review a cleanup plan to reconcile the selected
rehearsal state; do not use the provisioning Action unless recreation is intended.

## Observed cleanup and verification limits

The operator confirmed successful plan/delete cleanup of the live-only
`atlas-rehearsal-1234134` installation on November 6, 2026. This is separate from
the offline checks and does not prove cleanup of every supported deployment shape.

Offline tests cover scope rejection, confirmations, plan actions, temporary overrides,
secret-safe messages and workflow wiring. A local Terraform built-in-resource fixture
also verifies lifecycle override behavior without contacting Azure. Azure cleanup remains an explicit operator action. Merging code does not delete
resources.
