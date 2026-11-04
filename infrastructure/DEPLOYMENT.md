# Atlas capstone deployment runbook

**Status:** Terraform and GitHub Actions foundation for PTT-169. These files do **not** mean Azure has been provisioned, the site is running, or the school report has passed.

The goals are **production parity** with the tested local product behavior, minimum feasible recurring Azure costs with private SQL networking, a reproducible deployment, visible telemetry, and documented evidence for the school report.

## 1. Deployment architecture

- A single Linux Azure App Service plan runs an ASP.NET Core 10 API, with the compiled SvelteKit static SPA bundled into `wwwroot`. One HTTPS origin serves browser routes and all API requests; no Vite server is present in production.
- `/atlas-public-api/*` and `/atlas-session-api/*` are rewritten by the .NET host to `/api/*`, just as the local Vite development proxy rewrites them. Public requests do not carry session credentials; session requests retain credentials and require server-side authorization/CSRF checks.
- Azure SQL stores durable Identity and Atlas data. App Service HTTPS and Azure SQL **Private Link** provide a public website with a **non-public database**. Both web apps use a delegated VNet integration subnet; SQL has `public_network_access_enabled = false`. An NSG allows TCP 1433 to the SQL private endpoint subnet from the app subnet and denies other VNet sources. Private DNS resolves Azure SQL's normal FQDN inside the VNet. Least-privilege SQL runtime credentials are still a separate required setup step.
- Application Insights and a Log Analytics workspace provide cloud observability. Atlas.Api has a production OpenTelemetry exporter: verify real request/log/metric ingestion and audit delivery separately before reporting success.
- The default demo variant provisions another web app on the **same App Service plan** and a distinct SQL database. Demo can contain the known seed data; live must be separately initialized and protected. This is **two deployment configurations**, not a randomized statistically measured A/B experiment yet. See PTT-171.

The default now uses **B1 Linux App Service**, the least-capacity Basic plan supporting VNet integration, plus explicitly enabled **Azure SQL free-offer databases** (through AzAPI). The virtual network itself and App Service VNet integration add no usage charge, but B1 and the SQL Private Endpoint / private DNS zone are billable. SQL free-offer databases can pause when the monthly allowance is exhausted. This is a cost-conscious student deployment, not a free or high-availability production environment.\n\nThe original [legacy Terraform](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) already covered App Service, SQL, Blob, Application Insights and firewall rules. The rewritten configuration is modular; Blob Storage is omitted until there is an actual image-storage requirement (PTT-167).

## 2. Local prerequisites and Azure selection

Install Azure CLI, Terraform, .NET 10 SDK, Node 24 and optionally Visual Studio with ASP.NET/web publishing tooling.

```powershell
az login
az account list --output table
az account set --subscription "<subscription-id>"
az account show --query "{name:name,id:id,tenantId:tenantId}" --output json
terraform version
dotnet --version
node --version
```

Confirm subscription permissions and inspect Azure student credits, quotas and region/SKU availability for **B1, SQL free offer, private endpoints and private DNS**. Record subscription identifiers in **private configuration**, not source files. Validate Azure SQL Full-Text Search support in the chosen database tier before committing to that SKU.

## 3. Terraform state bootstrap (one-time)

Terraform state may contain SQL credentials: store it in a private, access-controlled Azure Storage account instead of committing it. The state account is intentionally bootstrapped outside the application Terraform root. In the examples below substitute globally unique resource names and adjust the Azure region.

```powershell
az group create --name atlas-tfstate-rg --location westus2
az storage account create --name <unique-state-account> --resource-group atlas-tfstate-rg --location westus2 --sku Standard_LRS --kind StorageV2 --allow-blob-public-access false --min-tls-version TLS1_2
az storage container create --name tfstate --account-name <unique-state-account> --auth-mode login
```

Use Entra authentication and the Storage Blob Data Contributor role only where required for state access. Review role propagation and container creation failures. Configure state recovery/retention, lock down state access, and avoid disclosing state contents in CI artifacts.

## 4. Terraform: initialize, plan and apply

From `infrastructure/terraform`:

```powershell
Copy-Item terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars locally. Set subscription_id, name_prefix, location and SKUs.
# Supply the provisioning password through a private environment variable:
$env:TF_VAR_sql_admin_password = "<strong-generated-password>"
terraform init -backend-config="resource_group_name=atlas-tfstate-rg" -backend-config="storage_account_name=<unique-state-account>" -backend-config="container_name=tfstate" -backend-config="key=atlas-capstone.tfstate" -backend-config="use_azuread_auth=true"
terraform fmt -recursive
terraform validate
terraform plan -out=capstone.tfplan
terraform apply capstone.tfplan
terraform output
```

Additional useful commands:

```powershell
terraform fmt -check -recursive
terraform state list
terraform output site_urls
terraform output virtual_network
terraform output sql_private_endpoint_id
terraform output sql_private_dns_zone
terraform plan
terraform plan -destroy
```

Review every `plan` before applying. A `-target` apply is **not** the default workflow. The SQL databases use `prevent_destroy` to avoid accidental data removal. Do not run `terraform destroy` against student deployment evidence or live data without a recovery plan.

### Optional: Azure-connected preview from GitHub Actions (no apply)

Once the **private Azure Blob Terraform state** and the Azure OIDC identity have been bootstrapped, run [Atlas Azure Terraform plan (manual, no apply)](../.github/workflows/AtlasAzurePlan.yml) from the **Actions** tab. This workflow is distinct from the offline `AtlasInfrastructure.yml` checks. It uses the real subscription and remote state to preview the proposed resource changes, but has **no `terraform apply` step** and does not save/upload plan artifacts. Its Azure identity needs read access to the planned resources plus **Storage Blob Data Contributor** for Terraform state locking; it does not need a blanket Owner grant.

Create a protected GitHub Environment named `atlas-infra-review` with a matching Azure federated credential subject `repo:williamowen65/atlas-the-public-think-tank:environment:atlas-infra-review`. Configure the following environment values:

| Kind | Name | Meaning |
| --- | --- | --- |
| Secret | `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | Azure OIDC identity and target subscription (GUIDs, not passwords) |
| Secret | `TF_SQL_ADMIN_PASSWORD` | Required existing SQL server administrator password, passed to Terraform as a sensitive variable; keep the same value across repeated plans |
| Secret (demo only) | `ATLAS_DEMO_PASSWORD` | Optional unique password used by the demo-only seed job; never print |
| Variable | `ATLAS_NAME_PREFIX`, `AZURE_LOCATION` | Atlas resource prefix and Azure region |
| Variable | `AZURE_SQL_ENTRA_ADMIN_LOGIN`, `AZURE_SQL_ENTRA_ADMIN_OBJECT_ID` | Intended Azure SQL Entra group/user for SQL server administration |
| Variable | `AZURE_TFSTATE_RESOURCE_GROUP`, `AZURE_TFSTATE_STORAGE_ACCOUNT`, `AZURE_TFSTATE_CONTAINER`, `AZURE_TFSTATE_KEY` | Existing private state backend parameters created during bootstrap |

This workflow deliberately assumes **B1**, both `live` and `demo`, and `enable_sql_bootstrap=false` (the normal safe setting). If you change those choices in your actual infrastructure, synchronize the workflow before relying on the plan. The workflow must exist on the repository's default branch before GitHub displays it as a normal manually runnable workflow. For the *first* deployment, you can execute `terraform plan` locally as documented above. An Azure-connected plan reads Azure metadata and state, but it does not guarantee quota availability, a successful apply, private DNS function, SQL connectivity or running WebJobs. These still need deployment verification.

## 5. SQL reachability, credentials and migrations

The initial Terraform apply now configures **no public SQL endpoint**: `public_network_access_enabled = false`. The [network module](terraform/modules/network/main.tf) creates the VNet, delegated application subnet, private-endpoint subnet, private DNS zone/link, SQL Private Endpoint and an NSG with a narrowly scoped SQL rule. There are no SQL public firewall exceptions to add. Follow [security design](SECURITY.md) for the network boundaries.

**Public site, private data:** the web applications remain available on public HTTPS, but outgoing database connections travel over VNet integration to the private SQL endpoint. VNet DNS resolves `<server>.database.windows.net` to a private address. Never put the private IP directly in the SQL connection string.

Your laptop and GitHub-hosted runners **cannot directly open a connection** to that SQL endpoint. Do not temporarily enable public SQL access to apply migrations. Run database initialization from a controlled process already integrated into the VNet. The proposed least-cost pattern is an on-demand **triggered App Service WebJob** using a separately secured, management-only identity and a published self-contained Console or migration executable. Microsoft supports WebJobs on Linux code-based App Service and Azure CLI provides `az webapp webjob triggered run`. This management job is **not yet provisioned or wired into the deploy pipeline**; `Initialize-AtlasDatabase.ps1` is only for an environment with existing private SQL connectivity. See [private network operations](PRIVATE-NETWORK-OPERATIONS.md).

Atlas.Api expects `ATLAS_SQL_CONNECTION_STRING`. Use **distinct least-privilege SQL users or Entra-managed identities** for demo/live, never the Terraform provisioning administrator in the public web host. The initial Terraform can take sensitive variant-specific connection strings, but Terraform state still contains them; move to Entra/Key Vault as part of credential hardening. Protect credentials, SQL backups, and operator access.

Migrations, Identity schema creation, SQL Full-Text indexes and seed setup run through our version-controlled [database initializer](scripts/Initialize-AtlasDatabase.ps1), rather than Terraform resource side effects or application startup. They are automated **on operator invocation** using existing Console commands. Live must not be seeded; demo requires a unique `ATLAS_DEMO_PASSWORD` from a secure environment variable. A separate SQL principal/managed-identity bootstrap is still needed to replace admin access with least privilege. Follow [SQL instructions](../docs/PTT-87-SQL-Server.md) and [seed guide](../docs/workflows/local-demo-seed.md), verifying commands against the target database. Never seed the live database using demo-only credentials.

### Database initialization from versioned code

Only from an **authorized VNet-connected administrative execution environment** with SQL migration permissions, run the following from the repository root, with `ATLAS_SQL_CONNECTION_STRING` set privately for the **intended** database:

```powershell
./infrastructure/scripts/Initialize-AtlasDatabase.ps1 -Variant live -ExpectedDatabase "<prefix>-live"
```

For the demo database, set a **unique secret** `ATLAS_DEMO_PASSWORD` (do not use the known local password) and run the separate one-time seed:

```powershell
./infrastructure/scripts/Initialize-AtlasDatabase.ps1 -Variant demo -ExpectedDatabase "<prefix>-demo" -SeedDemo
```

The initializer checks the connection-string database against the explicit name and variant, applies checked-in migrations and Full-Text indexes, and refuses demo seeding against `live`. It refuses repeated seeding against an existing populated database. No console or SQL passwords should appear in committed config or CI logs. These are automated deployment operations, not Terraform-managed Azure resources.

Check Full-Text population readiness and Azure SQL free-tier serverless cold-resume/retry behavior before declaring the site healthy. The script cannot reach SQL from your home PC or a standard GitHub-hosted runner. Its eventual cloud execution will use a management-only triggered WebJob or an equivalent VNet-attached runner, with separate migration privileges and no public SQL access. This integration is not yet implemented.

See [telemetry wiring](TELEMETRY.md) for OpenTelemetry and the separate durable cloud-audit follow-up.

## 6. GitHub Actions cloud deployment

There are two new workflows:

- [AtlasInfrastructure.yml](../.github/workflows/AtlasInfrastructure.yml): on pushes touching IaC, verify Terraform formatting, initialization and structural validity. No cloud credentials or resource creation.
- [AtlasDeploy.yml](../.github/workflows/AtlasDeploy.yml): manual deploy of one chosen variant (`live` or `demo`). Build the frontend with Node, publish `Atlas.Api` with .NET, copy frontend output into the API package, then deploy it to the chosen Azure App Service using OIDC.

Create GitHub Environments named `atlas-live` and, when provisioned, `atlas-demo`. Each needs environment variable `AZURE_WEBAPP_NAME` from Terraform outputs and secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`. Configure an Entra deployment identity with a federated OIDC subject matching `repo:williamowen65/atlas-the-public-think-tank:environment:atlas-live` (and an equivalent subject for demo). Grant only the role needed to deploy to the target App Service. Configure GitHub environment approval rules for live when available.

Run **Actions → Atlas manual Azure deployment → Run workflow**, select the variant, then inspect the build and deploy results. The pipeline does **not** apply Terraform; infrastructure changes remain a separate reviewed action. The workflow is untested against Azure until the credentials/environments are configured.

**Visual Studio alternative:** right-click `Atlas.Api` → Publish → Azure → Azure App Service. Choose the existing provisioned app. Publishing only `Atlas.Api` will not include the SvelteKit build automatically; package the frontend first or use the GitHub pipeline as the reproducible release mechanism. Verify credentials and environment settings rather than relying on local `appsettings.Development.json`.

## 7. Production parity checklist

| Concern | Local experience | Azure acceptance |
| --- | --- | --- |
| Web UI | Vite SvelteKit SPA | Static `dist` packaged under ASP.NET `wwwroot` with direct-link fallback |
| APIs | Vite proxies `/atlas-public-api` and `/atlas-session-api` | Same paths translated by ASP.NET; no dev-only proxy or insecure TLS |
| Authentication | Local Identity cookie/CSRF | Host-only HTTPS cookies; sign in/out, owner rules, refresh and CSRF tested at public URL |
| Data | SQL Server with Full-Text | Azure SQL migrations, Full-Text readiness, and persistent queries tested |
| Seed/live | Local developer seed | Separate demo/live databases, intentional seed policy, no cross-environment writes |
| Security | API rate/body limits and authorization | HTTPS, firewall, secrets, session protections, trusted proxy behavior and 404 routing verified |
| Telemetry | Local structured files, metrics and audit outbox | Cloud ingestion, restricted access, retention and durable audit behavior demonstrated |
| Release | Local development and CI | Versioned Terraform and an OIDC-triggered GitHub Actions deployment |

A demo environment must be clearly labeled and should restrict writes to seeded content through application policy. Terraform cannot enforce that business rule. A full A/B experiment additionally needs assignment, exposure tracking and meaningful outcomes.

## 8. Budget and school evidence

The sample now defaults to a **paid B1 App Service plan** for private VNet integration and Azure SQL **General Purpose serverless free offer** with `useFreeLimit=true` and `AutoPause`. Azure SQL free databases have monthly compute/storage limits; eligibility depends on subscription and region. The infrastructure does not silently replace an ineligible free database with a paid SKU. F1 cannot provide App Service VNet integration. B1 is billable even when SQL is within its free allowance. The SQL Private Endpoint has a recurring charge, and the private DNS zone may have usage charges. Log Analytics above its included allowance, Terraform state storage and other Azure services can also incur charges. Compare subscription eligibility, student credits and region restrictions before applying. Set a budget alert using a documented IaC-managed approach before enabling a paid upgrade.

For the WGU deployment report (PTT-170), capture the chosen architecture, Terraform `plan` summary (redacted), workflow runs/commit SHA, HTTPS URL, two variants if enabled, SQL persistence/FTS evidence, auth and security checks, and Application Insights screenshots with no sensitive identifiers. Target the first deployment evidence by **November 16**, first report submission by **November 19**, and confirmed pass before November 30.
