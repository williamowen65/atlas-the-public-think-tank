# Atlas capstone deployment runbook

**Status:** Terraform and GitHub Actions foundation for PTT-169. These files do **not** mean Azure has been provisioned, the site is running, or the school report has passed.

The goals are **production parity** with the tested local product behavior, minimum recurring Azure costs, a reproducible deployment, visible telemetry, and documented evidence for the school report.

## 1. Deployment architecture

- A single Linux Azure App Service plan runs an ASP.NET Core 10 API, with the compiled SvelteKit static SPA bundled into `wwwroot`. One HTTPS origin serves browser routes and all API requests; no Vite server is present in production.
- `/atlas-public-api/*` and `/atlas-session-api/*` are rewritten by the .NET host to `/api/*`, just as the local Vite development proxy rewrites them. Public requests do not carry session credentials; session requests retain credentials and require server-side authorization/CSRF checks.
- Azure SQL stores durable Identity and Atlas data. App Service HTTPS, restricted SQL firewall rules and least-privilege runtime credentials protect the initial deployment. No public Azure-wide SQL firewall rule is enabled.
- Application Insights and a Log Analytics workspace provide cloud observability. The Azure agent wiring is a starting point: verify real request/log ingestion before reporting success.
- The default demo variant provisions another web app on the **same App Service plan** and a distinct SQL database. Demo can contain the known seed data; live must be separately initialized and protected. This is **two deployment configurations**, not a randomized statistically measured A/B experiment yet. See PTT-171.

The default is now **F1 Linux App Service plus the explicitly enabled Azure SQL free offer** (through the AzAPI provider). Microsoft describes F1 as a limited learning tier, not a production SLA. Free SQL offers can pause when the monthly allowance is exhausted; those pauses can make Atlas unavailable. These defaults are for a student proof-of-concept, not a high-availability public service.\n\nThe original [legacy Terraform](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) already covered App Service, SQL, Blob, Application Insights and firewall rules. The rewritten configuration is modular; Blob Storage is omitted until there is an actual image-storage requirement (PTT-167).

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

Confirm subscription permissions and inspect Azure student credits, quotas and region/SKU availability. Record subscription identifiers in **private configuration**, not source files. Validate Azure SQL Full-Text Search support in the chosen database tier before committing to that SKU.

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
terraform output possible_outbound_ipv4
terraform plan
terraform plan -destroy
```

Review every `plan` before applying. A `-target` apply is **not** the default workflow. The SQL databases use `prevent_destroy` to avoid accidental data removal. Do not run `terraform destroy` against student deployment evidence or live data without a recovery plan.

## 5. SQL reachability, credentials and migrations

The first Terraform apply creates **no Azure SQL firewall exceptions**. Read the App Service outbound IP list from `terraform output possible_outbound_ipv4`, add the needed IPs to `sql_allowed_ipv4` as a map of named exact IPv4 entries, and re-run `terraform plan` and `terraform apply`. For direct local migration access, temporarily allow only your own current public IP and remove it when done.

App Service egress IPs can be shared: IP allowlisting is only one control, **not** proof of app identity. Evaluate private endpoints/VNet integration later if costs permit; do not silently change the SQL firewall to `0.0.0.0`, which can allow connections from Azure generally.

Atlas.Api expects `ATLAS_SQL_CONNECTION_STRING`. Use **distinct least-privilege SQL users** for demo/live, not the Terraform provisioning administrator. This initial Terraform accepts sensitive per-variant connection strings through `TF_VAR_runtime_connection_strings` or a local ignored file; Terraform state still contains the values. A managed-identity/Key Vault path can reduce this risk in a follow-on. Protect credentials, SQL backups, and operator access.

Migrations, Identity schema creation, SQL Full-Text indexes and seed setup run through our version-controlled [database initializer](scripts/Initialize-AtlasDatabase.ps1), rather than Terraform resource side effects or application startup. They are automated **on operator invocation** using existing Console commands. Live must not be seeded; demo requires a unique `ATLAS_DEMO_PASSWORD` from a secure environment variable. A separate SQL principal/managed-identity bootstrap is still needed to replace admin access with least privilege. Follow [SQL instructions](../docs/PTT-87-SQL-Server.md) and [seed guide](../docs/workflows/local-demo-seed.md), verifying commands against the target database. Never seed the live database using demo-only credentials.

### Database initialization from versioned code

After SQL access and a least-privilege maintenance connection have been established, run the following from the repository root, with `ATLAS_SQL_CONNECTION_STRING` set privately for the **intended** database:

```powershell
./infrastructure/scripts/Initialize-AtlasDatabase.ps1 -Variant live -ExpectedDatabase "<prefix>-live"
```

For the demo database, set a **unique secret** `ATLAS_DEMO_PASSWORD` (do not use the known local password) and run the separate one-time seed:

```powershell
./infrastructure/scripts/Initialize-AtlasDatabase.ps1 -Variant demo -ExpectedDatabase "<prefix>-demo" -SeedDemo
```

The initializer checks the connection-string database against the explicit name and variant, applies checked-in migrations and Full-Text indexes, and refuses demo seeding against `live`. It refuses repeated seeding against an existing populated database. No console or SQL passwords should appear in committed config or CI logs. These are automated deployment operations, not Terraform-managed Azure resources.

Check Full-Text population readiness and Azure SQL free-tier serverless cold-resume/retry behavior before declaring the site healthy. The database initializer currently runs from an authorized operator environment with controlled SQL connectivity, not automatically from a generic GitHub runner (whose public egress IP is variable). An integrated cloud migration job is a follow-up once runner networking and credential handling are secure.

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

The sample now defaults to **F1** App Service and **Azure SQL General Purpose serverless with `useFreeLimit=true` and `AutoPause`**. Azure SQL free databases have monthly compute/storage limits; eligibility depends on subscription and region. The infrastructure does not silently replace an ineligible free database with a paid SKU. F1 has tight CPU/storage limits and no production SLA. Log Analytics usage above the included free allowance, Terraform state storage and other Azure services can still incur charges. Compare subscription eligibility, student credits and region restrictions before applying. Set a budget alert using a documented IaC-managed approach before enabling a paid upgrade.

For the WGU deployment report (PTT-170), capture the chosen architecture, Terraform `plan` summary (redacted), workflow runs/commit SHA, HTTPS URL, two variants if enabled, SQL persistence/FTS evidence, auth and security checks, and Application Insights screenshots with no sensitive identifiers. Target the first deployment evidence by **November 16**, first report submission by **November 19**, and confirmed pass before November 30.
