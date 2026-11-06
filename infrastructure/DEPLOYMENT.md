# Atlas capstone deployment runbook

**Status (2026-11-06):** The operator confirmed live and demo are deployed. SQL identity bootstrap succeeded after retry; schema and Full-Text setup succeeded through App Service SSH. Follow the [first-deployment walkthrough](FIRST-DEPLOYMENT.md) for the working sequence and an isolated rehearsal. This records observed results, not a completed school acceptance report.

The goals are **production parity** with the tested local product behavior, minimum feasible recurring Azure costs with private SQL networking, a reproducible deployment, visible telemetry, and documented evidence for the school report.

## 1. Deployment architecture

- A single Linux Azure App Service plan runs an ASP.NET Core 10 API, with the compiled SvelteKit static SPA bundled into `wwwroot`. One HTTPS origin serves browser routes and all API requests; no Vite server is present in production.
- `/atlas-public-api/*` and `/atlas-session-api/*` are rewritten by the .NET host to `/api/*`, just as the local Vite development proxy rewrites them. Public requests do not carry session credentials; session requests retain credentials and require server-side authorization/CSRF checks.
- Azure SQL stores durable Identity and Atlas data. App Service HTTPS and Azure SQL **Private Link** provide a public website with a **non-public database**. Both web apps use a delegated VNet integration subnet; SQL has `public_network_access_enabled = false`. An NSG allows TCP 1433 to the SQL private endpoint subnet from the app subnet and denies other VNet sources. Private DNS resolves Azure SQL's normal FQDN inside the VNet. Least-privilege SQL runtime credentials are still a separate required setup step.
- Application Insights and a Log Analytics workspace provide cloud observability. Atlas.Api has a production OpenTelemetry exporter: verify real request/log/metric ingestion and audit delivery separately before reporting success.
- The default demo variant provisions another web app on the **same App Service plan** and a distinct SQL database. Demo can contain the known seed data; live must be separately initialized and protected. This is **two deployment configurations**, not a randomized statistically measured A/B experiment yet. See PTT-171.

The default now uses **B1 Linux App Service**, the least-capacity Basic plan supporting VNet integration, plus explicitly enabled **Azure SQL free-offer databases** (through AzAPI). The virtual network itself and App Service VNet integration add no usage charge, but B1 and the SQL Private Endpoint / private DNS zone are billable. SQL free-offer databases can pause when the monthly allowance is exhausted. This is a cost-conscious student deployment, not a free or high-availability production environment.

The original [legacy Terraform](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) already covered App Service, SQL, Blob, Application Insights and firewall rules. The rewritten configuration is modular; Blob Storage is omitted until there is an actual image-storage requirement (PTT-167).

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

### Provision or update Azure from GitHub Actions

Bootstrap the private Terraform state and Azure OIDC access first. Once this workflow is on `main`, open **Actions → Atlas Azure Terraform apply (manual)** and dispatch from `main`. Review **Plan Azure changes** in the run log, then approve the `atlas-infra-apply` job in that same run. It applies the exact plan reviewed above. If the plan reports no changes, the apply job is skipped.

- Configure `atlas-infra-review` with the values in the table below.
- Create `atlas-infra-apply`, restrict deployments to `main`, and configure **Required reviewers**. Disable administrator bypass. If you will start and approve your own runs, leave **Prevent self-review** off. The apply job requires a recorded approval and exits if none exists. If your GitHub plan does not offer required reviewers for this repository, use the local procedure below.
- Give the apply environment a separate Azure OIDC identity with the permissions to provision the declared resources and access the state blob. Use federated subject `repo:williamowen65/atlas-the-public-think-tank:environment:atlas-infra-apply`. The apply environment needs its own `AZURE_CLIENT_ID`, the same tenant and subscription, and the same four state backend variables as review.
- Set the same strong randomly generated `ATLAS_TF_PLAN_PASSPHRASE` secret in both environments. The workflow transfers an encrypted plan with one-day retention. Start a new run if the artifact expires or an operation fails.
- Leave **enable_sql_bootstrap** off for normal runs. For initial database setup, explicitly enable it in a run, complete both SQL identity bootstrap WebJobs, and run Terraform again with it off before applying database schema.

### Find your SQL Entra administrator values

Microsoft Entra ID is the directory behind your Azure sign-in. If you want your own Azure account to be the SQL Entra administrator, open **Azure Portal → Microsoft Entra ID → Users → your user** in the subscription's directory. Alternatively, run this in Azure Cloud Shell while signed in to the correct directory:

```bash
az ad signed-in-user show --query '{login:userPrincipalName, objectId:id}' -o table
```

| GitHub Environment variable | Value |
| --- | --- |
| `AZURE_SQL_ENTRA_ADMIN_LOGIN` | Your **User principal name** (CLI `login`). For an Entra group, use the group's display name. |
| `AZURE_SQL_ENTRA_ADMIN_OBJECT_ID` | The same user or group's **Object ID** (CLI `objectId`). |

These identify the Azure SQL administrator. The Terraform workflow signs in using its separate OIDC client ID.

### Local Terraform procedure

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

This workflow deliberately assumes **B1**, both `live` and `demo`, and `enable_sql_bootstrap=false` (the normal safe setting). If you change those choices in your actual infrastructure, synchronize the workflow before relying on the plan. The workflow must exist on the repository's default branch before GitHub displays it as a normal manually runnable workflow. You can also use the local procedure above. An Azure-connected plan reads Azure metadata and state, but it does not guarantee quota availability, a successful apply, private DNS function, SQL connectivity or running WebJobs. These still need deployment verification.

## 5. SQL reachability, credentials and migrations

The initial Terraform apply now configures **no public SQL endpoint**: `public_network_access_enabled = false`. The [network module](terraform/modules/network/main.tf) creates the VNet, delegated application subnet, private-endpoint subnet, private DNS zone/link, SQL Private Endpoint and an NSG with a narrowly scoped SQL rule. There are no SQL public firewall exceptions to add. Follow [security design](SECURITY.md) for the network boundaries.

**Public site, private data:** the web applications remain available on public HTTPS, but outgoing database connections travel over VNet integration to the private SQL endpoint. VNet DNS resolves `<server>.database.windows.net` to a private address. Never put the private IP directly in the SQL connection string.

Your laptop and GitHub-hosted runners **cannot directly open a connection** to that SQL endpoint. Keep SQL private. Publish the dedicated Atlas.Admin package with [AtlasAdmin.yml](../.github/workflows/AtlasAdmin.yml), use its bootstrap operation to create SQL identities, and use **Azure Portal → the matching admin App Service → SSH** for schema and demo seeding. The successful SSH commands and exact package directories are in the [walkthrough](FIRST-DEPLOYMENT.md). The WebJob execution path remains available but encountered failures described below.

Atlas.Api expects `ATLAS_SQL_CONNECTION_STRING`. Use **distinct least-privilege SQL users or Entra-managed identities** for demo/live, never the Terraform provisioning administrator in the public web host. Terraform now configures **separate user-assigned managed identities** and per-variant passwordless SQL connection strings for the public site and operations host. SQL database principal permissions still require the manual, temporary provisioning bootstrap, and Terraform state still contains the SQL server provisioning password; protect the state and use the bootstrap credentials only when needed. Protect credentials, SQL backups, and operator access.

Migrations, Identity schema creation, SQL Full-Text indexes and seed setup run through explicit [Atlas.Admin operations](../src/hosts/Atlas.Admin/README.md). They are automated **on operator invocation** using dedicated non-interactive `Atlas.Admin` commands in Azure; the separate interactive Console remains a local development tool. Live must not be seeded; demo requires a unique `ATLAS_DEMO_PASSWORD` from a secure environment variable. An explicit SQL contained-user bootstrap command is implemented but must be run and verified in Azure before passwordless access will work. Follow [SQL instructions](../docs/PTT-87-SQL-Server.md) and [seed guide](../docs/workflows/local-demo-seed.md), verifying commands against the target database. Never seed the live database using demo-only credentials.

### Database initialization from versioned code

**Azure:** Follow [first deployment](FIRST-DEPLOYMENT.md): provision with temporary bootstrap credentials, publish/run bootstrap for each variant, disable the credentials through Terraform, then run schema through the matching admin host's SSH terminal. Seed demo only after schema succeeds. Publish the website separately with AtlasDeploy.yml.

**Local:** Run `dotnet run --project src/hosts/Atlas.Admin -- --initialize-local-database` from the repository root. Append `--seed-demo` for a fresh, disposable local database.


Check Full-Text readiness and a real node request before declaring the site healthy. Schema execution through SSH was observed to succeed for live on attempt 3 and demo on attempt 2. This does not prove the WebJob execution path works for every operation or establish the cause of earlier failures.

See [telemetry wiring](TELEMETRY.md) for OpenTelemetry and the separate durable cloud-audit follow-up.

## 6. GitHub Actions cloud deployment

The primary workflows are:

- [AtlasInfrastructure.yml](../.github/workflows/AtlasInfrastructure.yml): on pushes touching IaC, verify Terraform formatting, initialization and structural validity. No cloud credentials or resource creation.
- [AtlasAdmin.yml](../.github/workflows/AtlasAdmin.yml): manually publish the minimal **Atlas.Admin** executable as protected triggered WebJobs; explicitly invoke bootstrap, schema or demo-only seed and check execution history. It does **not** deploy the interactive Atlas.Console.
- [AtlasDeploy.yml](../.github/workflows/AtlasDeploy.yml): manual deploy of one chosen variant (`live` or `demo`). Build the frontend with Node, publish `Atlas.Api` with .NET, copy frontend output into the API package, then deploy it to the chosen Azure App Service using OIDC.

Create GitHub Environments named `atlas-live` and, when provisioned, `atlas-demo`. Each needs environment variable `AZURE_WEBAPP_NAME` from Terraform outputs and secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`. Configure an Entra deployment identity with a federated OIDC subject matching `repo:williamowen65/atlas-the-public-think-tank:environment:atlas-live` (and an equivalent subject for demo). Grant only the role needed to deploy to the target App Service. Configure GitHub environment approval rules for live when available.

Run **Actions → Atlas manual Azure deployment → Run workflow**, select the variant, then inspect the build and deploy results. The pipeline does **not** apply Terraform; infrastructure changes remain a separate reviewed action. The operator confirmed both sites deployed. A smoke-step failure can occur after the deployment step succeeds; inspect those results separately before redeploying.

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


## Deployment preference and SQL connection diagnostics

Use GitHub Actions as the deployment path for the API and bundled Svelte frontend,
as well as the separate Atlas.Admin host. Website publishing from Visual Studio
is optional; it is not a prerequisite for SQL identity bootstrap.

### Managed-identity token diagnostic (current `diagnose` operation)

Run **Atlas manual private SQL administration** from **main**, with variant
**live** (or **demo**), operation **diagnose**, and confirmation blank. Open the
`atlas-diagnose` WebJob output after package deployment. Bootstrap credentials
are not required; leave `enable_sql_bootstrap=false`.

The diagnostic runs inside the WebJob process and reports only whether
`IDENTITY_ENDPOINT` and `IDENTITY_HEADER` are present. It requests an Azure SQL
token from the platform endpoint for `ATLAS_ADMIN_MI_CLIENT_ID`, using the
[App Service REST contract](https://learn.microsoft.com/azure/app-service/overview-managed-identity#rest-endpoint-reference).
A raw TCP probe first attempts the endpoint host/port with a five-second timeout,
without sending credentials. Its result includes elapsed time and structured
socket codes only. The diagnostic then attempts HTTP even if that probe fails.
HTTP response headers have a separate 30-second budget; a successful response
body has a ten-second budget. Logs distinguish TCP failure, waiting for HTTP
headers, and reading the token body, with HTTP status and token presence validation. Endpoint URLs, identity headers, tokens, and response bodies
are never logged. Redirects and proxy forwarding are disabled for this request.

- Missing endpoint settings: the WebJob did not receive the platform identity environment.
- Non-success HTTP status: the endpoint responded but rejected the request; the response body is deliberately withheld.
- TCP failure: raw endpoint connectivity failed at that moment; compare with the subsequent HTTP result.
- TCP success followed by an HTTP headers timeout: the TCP probe connected, but the separate token request received no headers within budget; this does not prove which service or network component stalled.
- Body timeout: HTTP headers arrived, but the token body could not be read within budget.
- Transport failure: token service access failed within the job; this alone does not identify the underlying cause.
- Success: the platform issued a nonempty token. This does not verify SQL network connectivity, SQL permissions, or SqlClient's own token acquisition path.

No SQL connection is opened and no SQL commands, schema changes, grants or seed
operations run. Share the complete diagnostic output before retrying schema.
This replaces the earlier five-attempt SQL-password diagnostic, which required
temporary bootstrap credentials and ran `SELECT 1`.

This check investigates the 2026-11-05 schema failure during managed-identity
token acquisition (operation canceled), after the attached identity and configured
Client ID were checked and matched. It is a diagnostic, not another workaround.
The original first-login SQL reset and earlier Kudu deployment 502s remain
separate unresolved issues. Bootstrap and schema retain their documented bounded
connection retry workaround; this token probe does not silently fix either issue.

### Bootstrap connection retry workaround

Bootstrap attempts to open its SQL connection at most three times, waiting ten
seconds between retryable failures. Retries cover the observed socket connection
reset and SQL timeout, database-unavailable, service-transient, and throttling
error codes. Other failures, including explicit login rejection, fail immediately.
Retries stop once the connection opens; user creation and grants are never replayed
by this policy. This is a workaround for the observed first-login reset, whose
root cause remains unresolved. It does not establish that serverless resume caused
the failure. The current token diagnostic does not use this retry policy.


### Schema connection retry workaround (2026-11-05)

Bootstrap diagnostics observed a first SQL login failing with a connection reset,
followed by a successful login with identical settings. Bootstrap subsequently
succeeded on its second attempt. The root cause remains unresolved; serverless
resume has not been established as the cause.

The live schema WebJob later started but failed with SQL number 0, state 0,
class 20. Its original log omitted the provider message, so that failure is **not
confirmed to have the same cause**. Schema now uses the same bounded connection
retry workaround as bootstrap: at most three attempts, ten seconds apart, for
recognized transient SQL errors or a nested socket connection reset. Login
rejection and other unrecognized errors are not retried.

The schema command explicitly opens and retains its EF context connection before
calling migrations and supplemental SQL initialization. Only connection opening
is retried. Migrations, Full-Text setup and other SQL changes are never replayed
by this policy. A later failure may leave completed migrations in place; inspect
the migration history and the failure phase before deciding to rerun.

Failure logs now identify the phase and include redacted exception messages,
inner exceptions, SQL codes, client connection IDs and socket error codes. Use
these details to distinguish managed-identity authentication, transport and
migration failures. This is resilience plus diagnostic visibility, not a verified
root-cause fix. Schema execution subsequently succeeded through App Service SSH; the schema WebJob path still needs separate validation. Demo seeding has not
been given this connection retry policy.

The earlier Kudu HTTP 502 failures occurred during package deployment, before
schema started. High host CPU overlapped those failures, but causation was not
proved. SQL connection retries do not address that separate deployment issue.
