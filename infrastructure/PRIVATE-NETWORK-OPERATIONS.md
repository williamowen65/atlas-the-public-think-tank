# Admin operations with a private Azure SQL database

**Decision:** Atlas's web applications stay publicly reachable over HTTPS, while Azure SQL has **no public network endpoint**. Database migrations, SQL Full-Text initialization, operator role changes and demo seeding must run from an explicitly authorized workload that can reach SQL over the Azure virtual network.

## Why our normal developer workflow changes

A VNet is similar to a private LAN. Terraform builds a VNet (`<prefix>-vnet`) with an application subnet (`snet-app`) and a SQL private-endpoint subnet (`snet-private-endpoints`). The app subnet uses App Service outbound VNet integration; the SQL logical server has a private endpoint with a private IP and private DNS under `privatelink.database.windows.net`.

- Browsers can reach **Atlas Live** and **Atlas Demo** through their normal public HTTPS addresses.
- These web apps can reach the **SQL private endpoint** by resolving the server's normal `*.database.windows.net` name inside the VNet.
- A standard GitHub-hosted runner or your home Windows machine **is not inside that VNet**. It cannot run SQL commands directly. We must not temporarily open SQL publicly just to run migrations.
- Terraform controls the network/subnets/NSG/DNS and Azure resources. An authorized administrative job executes application-maintained database schema and seed code **within the VNet**, not from a public CI machine.

## Preferred low-cost administrative execution design

Use **triggered Azure App Service WebJobs** on Linux, with a management-only web application sharing the existing B1 App Service plan. WebJobs are supported for Linux code apps and can be manually invoked through Azure's authenticated management-plane API; Azure CLI exposes:

```powershell
az webapp webjob triggered run --name "<admin-app-name>" --resource-group "<resource-group>" --webjob-name "atlas-database-setup"
```

The Terraform admin App Service module, dedicated [Atlas.Admin](../src/hosts/Atlas.Admin/README.md) executable, WebJob package and manually protected [AtlasAdmin.yml](../.github/workflows/AtlasAdmin.yml) workflow are now **implemented in PR #398**, but they have **not yet been deployed or tested against Azure**. Do not assume the sample command can run until the corresponding Azure resources and GitHub OIDC environment are configured.

The management app should **not** expose a public website or unauthenticated API route for executing SQL operations. Allow only a trusted deployment identity to trigger administrative operations. It can share the **existing paid App Service plan** to avoid an additional always-on compute plan, although it shares memory/CPU capacity and may need scheduling during low traffic.

Separate the jobs so permissions and consequences stay clear:

| Job | Database permissions | Invocation | Safety |
| --- | --- | --- | --- |
| Apply schema and Full-Text indexes | DDL/migration privileges in one selected database | Manually triggered before app release | Idempotent migrations and database-ready verification |
| Initialize demo | Demo-only account/content initialization | Explicit once on empty demo database | Refuses live DB; unique secret password required; no wipe-on-deploy |
| Inspect/search readiness | Read-only schema/index readiness | On demand or post-deploy verification | Safe status logs, no SQL secrets or user content |
| Grant operator/moderator roles | Narrow operator privilege | Separate privileged command with audit | Authenticated authorization and logged changes; never public API |

The existing [`Initialize-AtlasDatabase.ps1`](scripts/Initialize-AtlasDatabase.ps1) is a **Windows/development operator helper**, invoking the interactive Console's maintenance commands where SQL is already reachable. It does not provide Azure SQL Private Link connectivity from your home PC. Azure uses a separate, non-interactive [`Atlas.Admin`](../src/hosts/Atlas.Admin/README.md) .NET 10 executable with exactly three initial operations: SQL identity bootstrap, schema/Full-Text setup, and guarded demo seeding. GitHub publishes that executable into Linux triggered WebJob packages. The repository's [`run-webjob.sh`](webjobs/run-webjob.sh) is packaged as `run.sh` (Azure's WebJob entry-point filename) and launches one command only when Azure triggers that WebJob, **not** when the public Atlas app starts.

## Secure execution sequence (target design)

1. Apply reviewed Terraform manually. This creates web apps, VNet integration, private DNS, the Azure SQL Private Endpoint, and SQL databases with public networking disabled.
2. Use a secure one-time bootstrap to establish **per-variant SQL runtime and management identities**. Prefer Microsoft Entra/managed identity where feasible; do not grant the public application account schema-admin permissions.
3. Publish the versioned administrative executable/WebJob package to the management app. This should be wired into an **explicit GitHub Actions deployment workflow**, not invoked by a Terraform resource creation.
4. Choose `live` or `demo` and manually run the schema/Full-Text initializer through a GitHub Environment-protected workflow or authenticated Azure CLI command. Confirm the correct target and output.
5. For **demo only**, explicitly run one-time seed setup after schema initialization; never seed `live`, never reseed a populated database or expose a known default account password.
6. Validate SQL connectivity, private DNS, EF schema, Full-Text Search readiness, runtime SQL privileges, monitoring and response behavior.
7. Manually deploy application bits after successful database setup. Never assume a successful infrastructure plan proves the application is ready.

## Network topology and costs

The VNet and ordinary subnets themselves are free, and App Service outbound VNet integration has no additional fee. **B1 hosting is billable** and required instead of F1 for VNet integration. Azure SQL Private Endpoint / Private Link is a billed service and a private DNS zone can add charges. See pricing on your subscription/region; do not assume the full architecture is free.

The endpoint subnet's **Network Security Group (NSG)** provides a subnet-level, stateful firewall: the current rule allows port 1433 from the App Service integration subnet, denying other VNet sources. This is defense in depth; identity and least-privilege database roles remain essential. The current single VNet and shared SQL logical server do not by themselves ensure demo/live per-database permissions; verify these separately.

## Open work before calling admin automation complete

- Add the management-only App Service WebJob host/module and its dedicated narrowly scoped managed identity, configured from Terraform.
- Package and deploy schema/Full-Text and demo seed executables as triggered WebJobs. Validate that WebJob invocation runs inside the VNet integration context and uses the correct private DNS.
- Bootstrap the chosen database authentication model, grant least-privilege per-database runtime/ops access, and rotate/revoke provisioning credentials.
- Protect GitHub Environments/OIDC subjects, trigger privileges and job outputs; never log secrets, connection strings or user content.
- Add non-destructive integration tests for safe repeatable initialization, wrong-variant rejection and migration failures.\n- Design explicit GlobalModerator grant/revoke commands with operator approval, auditing, and session invalidation. The current interactive local Console workflow is **not** packaged into Azure. A private interactive administration portal would be a distinct feature, not a WebJob.
- Check Azure SQL free-offer eligibility and private endpoint billing; capture security evidence for PTT-169/170.

Microsoft documentation: [App Service VNet integration](https://learn.microsoft.com/azure/app-service/overview-vnet-integration), [WebJobs overview](https://learn.microsoft.com/azure/app-service/overview-webjobs), [run a triggered WebJob](https://learn.microsoft.com/cli/azure/webapp/webjob/triggered), [SQL Private Link](https://learn.microsoft.com/azure/azure-sql/database/private-endpoint-overview).
