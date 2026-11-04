# Azure resources provisioned for Atlas

Status: **configuration committed, not deployed**. The authoritative definitions are in [`terraform/`](terraform/); check a `terraform plan` before each apply. This inventory describes the planned Azure infrastructure for school deployment (PTT-169), not application-level setup or an already-running system.

## Resource inventory

| Terraform address | Azure resource | Quantity | Why Atlas needs it |
| --- | --- | ---: | --- |
| `azurerm_resource_group.atlas` | Resource group | 1 | Groups all managed application resources for access, tracking and cleanup |
| `azurerm_service_plan.atlas` | Linux App Service plan | 1 shared | Provides compute for the .NET API and bundled SvelteKit SPA |
| `module.sites[*].azurerm_linux_web_app.atlas` | Linux Web App | 1, or 2 with demo | Serves Atlas.Api and static frontend together at one HTTPS origin; `live` always exists |
| `module.sql.azurerm_mssql_server.atlas` | Azure SQL logical server | 1 | Hosts persistent Atlas + Identity databases |
| `module.sql.azurerm_mssql_database.variant[*]` | Azure SQL database | 1, or 2 with demo | Keeps live data and optional curated demo seed data separate |
| `module.sql.azurerm_mssql_firewall_rule.approved[*]` | Named SQL firewall rule | 0 initially; 1 per allowed IP entry | Explicitly limits which client IP addresses can reach SQL |
| `module.observability.azurerm_log_analytics_workspace.atlas` | Log Analytics workspace | 1 | Stores and queries selected operational telemetry; configured at 30 days retention |
| `module.observability.azurerm_application_insights.atlas` | Application Insights | 1 shared | Observes app request health/performance when production telemetry ingestion is verified |

**Default:** `enable_demo = false`, so there is one app and one SQL database. Setting `enable_demo = true` creates a second web app and database on the same App Service plan. There is *no automatic A/B traffic assignment*, and demo must be seeded and secured independently. The demo database incurs its own cost.

Each Web App also receives an Azure **system-assigned managed identity**. Terraform currently does **not** grant it access to SQL, Key Vault, or other resources: assigning an identity is not the same as using it for authentication.

## Application and data flow

1. The user's browser connects to the Azure Web App over HTTPS.
2. The deployed ASP.NET Core host serves the SvelteKit static output from `wwwroot` and API requests from the same origin. Production rewrites the frontend's `/atlas-public-api/*` and `/atlas-session-api/*` prefixes to existing `/api/*` endpoints, mirroring local Vite proxy behavior.
3. Atlas.Api uses the environment-specific `ATLAS_SQL_CONNECTION_STRING` to access its own Azure SQL database.
4. App Service is configured with an Application Insights connection string and agent settings. Log Analytics is associated with Application Insights. **Real request, metric, trace and audit ingestion still needs deployment verification.**

The SQL server has `public_network_access_enabled = true` but starts with **zero firewall exceptions**. This means public Internet SQL connections are not automatically allowed. The operator explicitly adds approved client IPs after checking App Service outbound IP addresses. These IPs may be shared by other Azure tenants; they are **not** an identity or a complete security boundary. For stronger network isolation, evaluate private endpoints/VNet integration and their cost.

## Folder ownership

- [`terraform/main.tf`](terraform/main.tf) composes the resource group, shared hosting plan and modules.
- [`terraform/modules/sql/main.tf`](terraform/modules/sql/main.tf) defines the Azure SQL server, databases and firewall allowlist.
- [`terraform/modules/webapp/main.tf`](terraform/modules/webapp/main.tf) defines each Linux Web App, HTTPS policy, managed identity, runtime version and app settings.
- [`terraform/modules/observability/main.tf`](terraform/modules/observability/main.tf) defines Application Insights and Log Analytics.
- [`terraform/variables.tf`](terraform/variables.tf) defines environment choices, SKUs, credentials and network inputs.
- [`terraform/outputs.tf`](terraform/outputs.tf) exposes app URLs, resource names and SQL outbound IP information.
- [`DEPLOYMENT.md`](DEPLOYMENT.md) documents Azure login, Terraform state setup, plan/apply commands, GitHub Actions deployment and acceptance checks.

## Security controls present in Terraform

- HTTPS-only Web Apps; minimum TLS 1.2; FTP publishing disabled.
- SQL firewall deny-by-default entries (the allowlist is empty initially), no blanket `0.0.0.0` rule.
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
| Database users, migrations, SQL Full-Text indexes or seed content | Explicit controlled database/application setup after provisioning |
| Runtime secret vault, managed-identity SQL sign-in or automated credential rotation | Not yet implemented; initial runtime connection strings are injected as sensitive Terraform inputs |
| Azure Blob Storage for user-uploaded images | Deferred until the image-storage design (PTT-167) |
| VNet/private endpoints, custom domain/DNS, automated backups beyond Azure service defaults, alert policies or cost budgets | Evaluate and add explicitly if needed for rubric, security or budget |
| A/B random assignment, experiment measurements, variant switching UI, or read-only demo policy | Product work under PTT-171; separate deployment variants are only the starting point |

## CI/CD and costs

**Continuous integration**: [`AtlasInfrastructure.yml`](../.github/workflows/AtlasInfrastructure.yml) checks `terraform fmt -check`, initializes providers without remote state, and runs `terraform validate`. It does **not** authenticate to Azure, create resources, check costs or evaluate comprehensive security policies.

**Deployment**: [`AtlasDeploy.yml`](../.github/workflows/AtlasDeploy.yml) is **manually triggered**, packages .NET + SvelteKit and deploys a chosen live/demo Web App via configured OIDC. It does not run `terraform apply`; Terraform provisioning is a separate manually approved step.

Defaults `app_service_sku = "B1"` and `sql_database_sku = "Basic"` are **billable**. SQL databases, compute, Log Analytics ingestion and other Azure operations can incur charges. Compare current region-specific prices, Azure student credits and free offers before applying anything. Terraform defines resources and can estimate *what* is created, but it does not enforce a spending cap.

For reproducible school-report evidence, capture an approved Terraform plan (with secrets removed), resource inventory, successful cloud deployment run, HTTPS/SQL/CSRF smoke tests, telemetry screenshots and a documented rollback or recovery procedure.
