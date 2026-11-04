# Azure resources provisioned for Atlas

Status: **configuration committed, not deployed**. The authoritative definitions are in [`terraform/`](terraform/); check a `terraform plan` before each apply. This inventory describes the planned Azure infrastructure for school deployment (PTT-169), not application-level setup or an already-running system.

## Resource inventory

| Terraform address | Azure resource | Quantity | Why Atlas needs it |
| --- | --- | ---: | --- |
| `azurerm_resource_group.atlas` | Resource group | 1 | Groups all managed application resources for access, tracking and cleanup |
| `azurerm_service_plan.atlas` | Linux App Service plan (B1, billed) | 1 shared | VNet-capable hosting compute for the .NET API and bundled SvelteKit SPA |
| `module.sites[*].azurerm_linux_web_app.atlas` | Linux Web App | 1, or 2 with demo | Serves Atlas.Api and static frontend together at one HTTPS origin; `live` always exists |
| `module.sql.azurerm_mssql_server.atlas` | Azure SQL logical server | 1 | Hosts persistent Atlas + Identity databases |
| `module.sql.azapi_resource.variant[*]` | Azure SQL free-offer database (AzAPI ARM resource) | 1, or 2 with demo | Keeps live data and optional curated demo seed data separate without silently selecting paid Basic |
| `module.network.azurerm_virtual_network.atlas` | Private virtual network | 1 | Holds the App Service integration subnet and private SQL endpoint subnet |
| `module.network.azurerm_subnet.app` | Delegated app subnet | 1 | Connects outbound traffic from both web apps into the private network |
| `module.network.azurerm_subnet.private_endpoints` | Private endpoint subnet | 1 | Hosts SQL Private Link endpoint address |
| `module.network.azurerm_private_endpoint.sql` | SQL Private Endpoint (billed) | 1 | Permits SQL connectivity using private IP rather than public network |
| `module.network.azurerm_private_dns_zone.sql` | Private DNS zone (billed) | 1 | Resolves SQL's regular domain name to private endpoint IP |
| `module.network.azurerm_private_dns_zone_virtual_network_link.sql` | Private DNS VNet link | 1 | Enables private DNS resolution inside the VNet |
| `module.network.azurerm_network_security_group.sql` | Network security group (NSG) | 1 | Filters SQL subnet traffic to the app subnet/port 1433 |
| `module.network.azurerm_subnet_network_security_group_association.sql` | NSG subnet binding | 1 | Enforces the subnet firewall rules |
| `module.observability.azurerm_log_analytics_workspace.atlas` | Log Analytics workspace | 1 | Stores and queries selected operational telemetry; configured at 30 days retention |
| `module.observability.azurerm_application_insights.atlas` | Application Insights | 1 shared | Azure Monitor OpenTelemetry is configured in Atlas.Api, while real cloud ingestion must still be verified |

**Default:** `enable_demo = true`, creating separate demo and live apps and Azure SQL free-offer databases on the same VNet-capable B1 App Service plan. Set it to `false` for a live-only deployment. There is *no automatic A/B traffic assignment*, and demo must be seeded and secured independently. The free SQL offer has monthly limits; it is not an unrestricted no-charge production SLA.

Each Web App also receives an Azure **system-assigned managed identity**. Terraform currently does **not** grant it access to SQL, Key Vault, or other resources: assigning an identity is not the same as using it for authentication.

## Application and data flow

1. The user's browser connects to the Azure Web App over HTTPS.
2. The deployed ASP.NET Core host serves the SvelteKit static output from `wwwroot` and API requests from the same origin. Production rewrites the frontend's `/atlas-public-api/*` and `/atlas-session-api/*` prefixes to existing `/api/*` endpoints, mirroring local Vite proxy behavior.
3. Atlas.Api uses the environment-specific `ATLAS_SQL_CONNECTION_STRING` to access its own Azure SQL database.
4. App Service receives an Application Insights connection string; Atlas.Api configures the cloud OpenTelemetry SDK at startup. Log Analytics is associated with Application Insights. **Real request, metric, trace and audit ingestion still needs deployment verification.**

The SQL server now has **`public_network_access_enabled = false`**; there are **no public SQL firewall exceptions**. The web apps join a delegated VNet subnet, resolve SQL through private DNS, and connect to SQL through one Private Endpoint. A network security group permits SQL TCP/1433 from the app subnet and denies other VNet traffic to that endpoint subnet. Database credentials and permissions remain required even with this network restriction.

## Folder ownership

- [`terraform/main.tf`](terraform/main.tf) composes the resource group, shared hosting plan and modules.
- [`terraform/modules/sql/main.tf`](terraform/modules/sql/main.tf) defines the Azure SQL server, databases and firewall allowlist.
- [`terraform/modules/webapp/main.tf`](terraform/modules/webapp/main.tf) defines each Linux Web App, HTTPS policy, managed identity, runtime version and app settings.
- [`terraform/modules/observability/main.tf`](terraform/modules/observability/main.tf) defines Application Insights and Log Analytics.
- [`terraform/modules/network/main.tf`](terraform/modules/network/main.tf) defines the VNet, subnets, private SQL endpoint and DNS, plus the NSG firewall.
- [`terraform/variables.tf`](terraform/variables.tf) defines environment choices, SKUs, credentials and network inputs.
- [`terraform/outputs.tf`](terraform/outputs.tf) exposes app URLs, resource names and SQL outbound IP information.
- [`DEPLOYMENT.md`](DEPLOYMENT.md) documents Azure login, Terraform state setup, plan/apply commands, GitHub Actions deployment and acceptance checks.

## Security controls present in Terraform

- HTTPS-only Web Apps; minimum TLS 1.2; FTP publishing disabled.
- SQL **public network access disabled**; database reachable over VNet integration/Private Link only. NSG limits private endpoint access to the delegated app subnet.
- Separate SQL database per variant, and a `prevent_destroy` lifecycle guard on each SQL database.
- A provisioned system-assigned identity for each app, ready for future RBAC integration.
- Sensitive variable declarations for SQL administrator credentials and runtime connection strings; **these values can still reside in Terraform state** and require a private state backend.
- Project/purpose and deployment-variant resource tags to aid inventory and cost review.

These settings alone do not prove production security: validate authentication/CSRF, privileged operations, SQL permissions, trusted proxy headers, and cloud telemetry after deployment. Managed identity/database RBAC, private SQL networking, rate-limit behavior across multiple instances, and application-secrets rotation are future verification or implementation items.

## What Terraform does *not* currently provision

| Item | Current handling |
| --- | --- |
| Terraform backend Storage Account and private state container | One-time Azure CLI bootstrap from the [deployment runbook](DEPLOYMENT.md) |
| GitHub Actions OIDC federation, environment protection and deployment RBAC | Configure manually in Entra ID/GitHub before the manual deploy workflow can authenticate |
| Database users, migrations, SQL Full-Text indexes or seed content | The [Atlas.Admin workflow](../.github/workflows/AtlasAdmin.yml) triggers WebJobs for SQL identity bootstrap, migrations/Full-Text setup and demo seeding |
| Runtime secret vault, managed-identity SQL sign-in or automated credential rotation | Not yet implemented; initial runtime connection strings are injected as sensitive Terraform inputs |
| Azure Blob Storage for user-uploaded images | Deferred until the image-storage design (PTT-167) |
| Custom public domain/DNS, automated backups beyond Azure service defaults, alert policies or cost budgets | Evaluate and add explicitly if needed for rubric, security or budget |
| A/B random assignment, experiment measurements, variant switching UI, or read-only demo policy | Product work under PTT-171; separate deployment variants are only the starting point |

## CI/CD and costs

**Continuous integration**: [`AtlasInfrastructure.yml`](../.github/workflows/AtlasInfrastructure.yml) checks `terraform fmt -check`, initializes providers without remote state, and runs `terraform validate`. It does **not** authenticate to Azure, create resources, check costs or evaluate comprehensive security policies.

**Deployment**: [`AtlasDeploy.yml`](../.github/workflows/AtlasDeploy.yml) is **manually triggered**, packages .NET + SvelteKit and deploys a chosen live/demo Web App via configured OIDC. It does not run `terraform apply`; Terraform provisioning is a separate manually approved step.

Defaults now select a **billable B1** App Service plan (the VNet integration baseline) and an explicitly flagged **Azure SQL General Purpose serverless free offer** using AzAPI. If your subscription or region is ineligible, provisioning should fail rather than silently upgrade to a paid SQL tier. The free SQL allowance has monthly compute, data and backup limits; The VNet itself and App Service VNet integration have no extra fee, but a SQL Private Endpoint and private DNS zone are billable. Terraform state storage, additional Log Analytics ingestion and other Azure services can still incur charges. Terraform defines resources and can estimate *what* is created, but it does not enforce a spending cap.

For reproducible school-report evidence, capture an approved Terraform plan (with secrets removed), resource inventory, successful cloud deployment run, HTTPS/SQL/CSRF smoke tests, telemetry screenshots and a documented rollback or recovery procedure.

## Code-driven data initialization

Run `bootstrap` and then `schema` for each Azure database through the [Atlas.Admin workflow](../.github/workflows/AtlasAdmin.yml). The workflow invokes a WebJob inside the VNet and checks the database name and environment. Run `seed-demo` only for the demo database after schema setup. It requires a unique `ATLAS_DEMO_PASSWORD` secret and refuses a populated database. See the [deployment runbook](DEPLOYMENT.md) for the provisioning and authorization sequence.

See [production telemetry](TELEMETRY.md) for what is exported to Application Insights and which durable cloud-audit pieces are still outstanding.
