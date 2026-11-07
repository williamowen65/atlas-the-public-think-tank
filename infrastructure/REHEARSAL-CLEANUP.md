# Delete a rehearsal installation

Use **Atlas rehearsal cleanup (plan or delete)** in GitHub Actions.
It removes one rehearsal’s tracked resources, including SQL data, and updates its
Terraform state. Main is not an available target.

## Before starting

- Confirm the exact rehearsal name.
- Finish or cancel deployment and administration work using it.
- Preserve any data you need before deletion.

Cleanup reuses the shared `atlas-infra-apply` Environment and Azure identity.
It shares a concurrency group with rehearsal apply, but does not cancel admin jobs.

## 1. Preview deletion

Open **Actions → Atlas rehearsal cleanup (plan or delete) → Run workflow**.

| Field | Example |
| --- | --- |
| Branch | `main` |
| Rehearsal name | `trial02`, or `atlas-rehearsal-trial02` |
| Operation | `plan` |
| Confirmation | Blank |

Approve Environment access if prompted. Plan mode does not delete resources.

Check the resolved target:

- Resource group: `atlas-rehearsal-trial02-rg`
- State key: `rehearsal/atlas-rehearsal-trial02.tfstate`

Review every resource in the destroy list. Missing state is an error; an already
empty tracked state is a no-op.

No application-count choice is needed. Cleanup uses the resources tracked for that
installation, whether live-only, demo-only or both.

## 2. Request deletion

Start a **new run** with the same name:

| Field | Value |
| --- | --- |
| Branch | `main` |
| Operation | `delete` |
| Confirmation | Exact full resource-group name, such as `atlas-rehearsal-trial02-rg` |

The workflow creates a new destroy plan. Review it before approving
**Delete the approved rehearsal installation**.

The delete job applies that exact saved plan. For the scope checks, SQL protection
override and encrypted-plan handling, see [Security](SECURITY.md#rehearsal-deletion-protections).

## What remains

- Shared state storage account and container.
- The rehearsal state blob, updated to record the resources as gone.
- Any retained state versions under the storage retention policy.

State remains private, even after deletion. Do not edit or delete main’s state.

Reusing the rehearsal name in provisioning can recreate the installation.
A new name selects a separate installation.

## If cleanup stops

### Partial deletion

Inspect the failure and start a fresh cleanup run. Old job retries are rejected.

Untracked resources, Azure deletion locks or missing permissions can block cleanup.
The resource group must be empty before Terraform removes it.

### Resources already deleted in Azure Portal

A Terraform refresh can detect missing resources. Review a cleanup plan to reconcile
the rehearsal state.

Do not run provisioning unless you intend to recreate the installation.
