# Azure and GitHub setup: where every value comes from

Use this reference before the [first-deployment walkthrough](FIRST-DEPLOYMENT.md).
It maps the current workflow inputs to their Azure source and GitHub destination.
The setup instructions, variable names, placeholder examples, and architecture
belong in this public repository. Actual passwords, state files, decrypted plans,
and access tokens do not. Subscription, tenant, client and resource IDs are
identifiers rather than passwords; the workflows still read the three Azure
sign-in IDs from GitHub Secrets, so configure them there as shown.

## Start with your own repository and Azure subscription

Fork Atlas or push a clone into a repository you control. A local clone alone
cannot hold GitHub Environments or run Actions in your own account. Enable Actions
in that repository, and ensure the workflow files exist on its default branch.
The apply workflow currently requires that branch to be named `main`.

Use your own Azure subscription, naming prefix, state backend, passwords and OIDC
identities. Replace `<owner>/<repo>` below with your repository, including its exact
capitalization. Do not use the upstream author's federated subjects or resource names.

Required tools are Azure CLI, Terraform, .NET 10 and Node 24; workflow-pinned
versions are in `.github/workflows/`. Your operator account needs permission to
create resources, set up Entra app registrations/service principals, and grant Azure
roles. Tenant policy may require an administrator to perform the identity steps.
B1 App Service, private endpoints, storage and telemetry can incur charges; review
[the budget notes](DEPLOYMENT.md#8-budget-and-deployment-verification) first.

## 1. Find subscription, tenant and region

In Azure Portal, open **Subscriptions → your subscription → Overview** for the
Subscription ID. Open **Microsoft Entra ID → Overview** for the Tenant ID.
Alternatively, in a signed-in local Azure CLI or Azure Cloud Shell Bash session:

```bash
az login
az account list --query '[].{name:name,subscription:id,tenant:tenantId}' -o table
az account set --subscription '<your-subscription-id>'
az account show --query '{subscription:id,tenant:tenantId,name:name}' -o json
az account list-locations --query '[].{name:name,displayName:displayName}' -o table
```

Use the location's machine `name`, such as `westus2`, for `AZURE_LOCATION`.
Confirm the region supports this deployment's SKUs and SQL free offer. Subscription
selection is separate from SQL administration and from the GitHub deployment identity.

## 2. Create state storage and record its four values

Follow [state bootstrap](DEPLOYMENT.md#3-terraform-state-bootstrap-one-time) to
create the private storage account and blob container before provisioning Atlas.
In Azure Portal, find **Storage accounts → your state account → Overview** for its
name and resource group, then **Data storage → Containers** for the container name.
The state key is a blob filename you choose, not an Azure-generated credential.

| GitHub Environment variable | Source | Example placeholder |
| --- | --- | --- |
| `AZURE_TFSTATE_RESOURCE_GROUP` | Resource group containing the state storage account | `atlas-tfstate-rg` |
| `AZURE_TFSTATE_STORAGE_ACCOUNT` | Your globally unique storage account name | `<your-state-account>` |
| `AZURE_TFSTATE_CONTAINER` | Private blob container created during bootstrap | `tfstate` |
| `AZURE_TFSTATE_KEY` | Chosen state blob filename for this installation | `atlas.tfstate` |

These values must match in `atlas-infra-review` and `atlas-infra-apply`. A second
installation needs another key and prefix; an existing installation must retain
its existing key. Do not rename its key just to match this example.

## 3. Create the GitHub-to-Azure OIDC identities

These are deployment identities, separate from the runtime and database-operations
managed identities that Terraform later attaches to the apps.

For each deployment identity, open **Microsoft Entra ID → App registrations → New
registration**, choose a recognizable name, and register it in your tenant. On the
registration's **Overview**, copy **Application (client) ID** and **Directory
(tenant) ID**. Use the Application ID for `AZURE_CLIENT_ID`, not the registration's
Object ID. The corresponding service principal appears under **Enterprise
applications**; Azure IAM role assignments target that service principal.
No client secret is needed for this OIDC workflow.

Open **Certificates & secrets → Federated credentials → Add credential**, select
the GitHub Actions scenario, enter your owner and repository, choose entity type
**Environment**, and enter the exact GitHub Environment name from this table.
The trust issuer is `https://token.actions.githubusercontent.com` and the audience
is `api://AzureADTokenExchange`.

| GitHub Environment | Federated subject | Identity purpose |
| --- | --- | --- |
| `atlas-infra-review` | `repo:<owner>/<repo>:environment:atlas-infra-review` | Plan and inspect resources |
| `atlas-infra-apply` | `repo:<owner>/<repo>:environment:atlas-infra-apply` | Apply the reviewed plan; use a separate identity from review |
| `atlas-ops-live` | `repo:<owner>/<repo>:environment:atlas-ops-live` | Publish tools/bootstrap identities on the live admin app |
| `atlas-ops-demo` | `repo:<owner>/<repo>:environment:atlas-ops-demo` | Publish tools/bootstrap identities on the demo admin app |
| `atlas-live` | `repo:<owner>/<repo>:environment:atlas-live` | Deploy the live website |
| `atlas-demo` | `repo:<owner>/<repo>:environment:atlas-demo` | Deploy the demo website |

Use distinct identities for the different privilege boundaries. Each Environment's
`AZURE_CLIENT_ID` must match the identity with that Environment's federated trust.
A repository or Environment rename requires updating the trust subject.

### Grant Azure permissions

In **the intended Azure scope → Access control (IAM) → Add role assignment**,
select the role and service principal. Authentication through OIDC and permission
through Azure RBAC are separate: a successful login alone grants no deployment access.

| Identity | Azure permissions and scope |
| --- | --- |
| Infra review | Reader over resources it plans, plus Storage Blob Data Contributor on the state container for reading state and acquiring its lock |
| Infra apply | Contributor over the deployment scope, plus Storage Blob Data Contributor on the state container |
| Website deployment | Website Contributor scoped to its matching public App Service |
| Admin deployment | Website Contributor scoped to its matching admin App Service; this identity can deploy privileged maintenance code, so restrict its use |
| Operator bootstrapping state | Resource-creation permission for state storage and Storage Blob Data Contributor for container/data access; ability to grant the listed role assignments |

The Terraform root creates the application resource group itself. Its first apply
therefore needs permission at a scope where that group can be created, normally the
subscription. A narrower resource-group provisioning scheme requires adapting that
bootstrap; assigning Contributor to a nonexistent group is not sufficient. Register
required Azure resource providers using an authorized operator if your scoped
identity cannot register them. Review/apply identities need visibility into the
backend resource group as required by the configured backend. Avoid granting
subscription Owner merely to bypass a permission error.

After provisioning, use the app's **Overview → JSON View** or Azure CLI to get its
resource ID for a narrowly scoped deployment role:

```bash
az webapp show --resource-group '<new-resource-group>' --name '<target-app-name>' --query id -o tsv
```

## 4. Find the SQL Entra administrator

Open **Microsoft Entra ID → Users → your chosen operator**, and copy the **User
principal name** and **Object ID**. For a security group, use its display name and
Object ID from **Groups → your group**. Prefer a group for shared operations.
For the currently signed-in user:

```bash
az ad signed-in-user show --query '{login:userPrincipalName,objectId:id}' -o json
```

Set `AZURE_SQL_ENTRA_ADMIN_LOGIN` to that login/group name and
`AZURE_SQL_ENTRA_ADMIN_OBJECT_ID` to its Object ID. This is the SQL administrator,
not a GitHub deployment app's client ID. Terraform's separate SQL provisioning
username defaults to `atlasprovisioner`; its password is described below.

## 5. Enter GitHub Environment secrets and variables

In **your GitHub repository → Settings → Environments**, create the six Environments
listed above. Open each Environment and add values under **Environment secrets**
or **Environment variables**, matching the tables. These workflows read
`secrets.NAME` and `vars.NAME` differently; putting a value in the other category
will not satisfy the workflow.

### Secrets

| Secret | Environments | Where to obtain the value |
| --- | --- | --- |
| `AZURE_CLIENT_ID` | All six | Application (client) ID of that Environment's deployment identity, from App registration Overview |
| `AZURE_TENANT_ID` | All six | Directory (tenant) ID, from Entra Overview or `az account show` |
| `AZURE_SUBSCRIPTION_ID` | All six | Subscription ID selected in step 1 |
| `TF_SQL_ADMIN_PASSWORD` | `atlas-infra-review` and `atlas-infra-apply` | Generate a strong unique SQL provisioning password privately; retain the same value for subsequent plans/applies unless intentionally rotating it |
| `ATLAS_TF_PLAN_PASSPHRASE` | `atlas-infra-review` and `atlas-infra-apply` | Generate a strong random value privately; use exactly the same value in both so apply can decrypt the reviewed plan |
| `ATLAS_DEMO_PASSWORD` | `atlas-infra-review` and `atlas-infra-apply` when seeding demo | Generate a unique demo account password privately; Terraform passes it to the demo admin app |

Passwords and the plan passphrase are values you create, not IDs to look up in
Azure. Use a password manager's generator. Do not reuse an Azure account password,
commit generated values, or echo them into workflow logs. The apply job uses the
saved encrypted plan, but mirroring its provisioning secrets keeps the setup
consistent if workflow responsibilities change. The admin workflow does not read
`ATLAS_DEMO_PASSWORD` from `atlas-ops-demo`; the demo admin App Service receives it
through Terraform.

### Variables configured before provisioning

| Variable | Environments | Source |
| --- | --- | --- |
| `ATLAS_NAME_PREFIX` | `atlas-infra-review` | Choose a globally unique prefix, 6–27 lowercase letters/digits/hyphens, starting with a letter and ending with a letter/digit; check derived Azure name limits |
| `AZURE_LOCATION` | `atlas-infra-review` | Location machine name from step 1 |
| `AZURE_SQL_ENTRA_ADMIN_LOGIN` | `atlas-infra-review` | SQL administrator login or group display name from step 4 |
| `AZURE_SQL_ENTRA_ADMIN_OBJECT_ID` | `atlas-infra-review` | The same SQL administrator's Object ID |
| `AZURE_TFSTATE_RESOURCE_GROUP` | Both infra Environments | State account's resource group from step 2 |
| `AZURE_TFSTATE_STORAGE_ACCOUNT` | Both infra Environments | State storage account name |
| `AZURE_TFSTATE_CONTAINER` | Both infra Environments | State container name |
| `AZURE_TFSTATE_KEY` | Both infra Environments | Chosen state key for this installation |

The apply job uses the reviewed saved plan; it does not reread prefix/location/SQL
administrator variables. Keep both backend configurations identical.

Configure `atlas-infra-apply` with required reviewers, restrict deployment to `main`,
and disable administrator bypass. If you approve your own runs, leave Prevent
self-review off. The workflow explicitly checks for a recorded approval; without
this protection it will fail. If your GitHub repository/plan cannot provide required
reviewers, use the documented local Terraform plan/apply procedure instead.

## 6. Populate targets after Terraform creates the apps

From `infrastructure/terraform`, initialized against **your installation's state**:

```bash
terraform output resource_group
terraform output site_names
terraform output admin_site_names
terraform output database_names
terraform output site_urls
```

| Variable | GitHub Environment | Exact source |
| --- | --- | --- |
| `AZURE_RESOURCE_GROUP` | `atlas-ops-live`, `atlas-ops-demo` | `resource_group` output |
| `AZURE_ADMIN_WEBAPP_NAME` | `atlas-ops-live` | `admin_site_names` output, `live` entry |
| `AZURE_ADMIN_WEBAPP_NAME` | `atlas-ops-demo` | `admin_site_names` output, `demo` entry |
| `AZURE_WEBAPP_NAME` | `atlas-live` | `site_names` output, `live` entry |
| `AZURE_WEBAPP_NAME` | `atlas-demo` | `site_names` output, `demo` entry |

You can also find these apps under **Azure Portal → Resource groups → your new
resource group → Resources**, then each **App Service → Overview**. Use the
App Service resource name, not its HTTPS URL, for the GitHub target variable.
Complete the per-app IAM grants from step 3 now that the app resources exist.

Terraform manages the apps' `ATLAS_SQL_CONNECTION_STRING`, managed identity IDs,
variant and expected database settings, plus temporary bootstrap credentials when
explicitly enabled. Do not copy your local SQL connection string or a provisioning
password into the public website. Azure Portal exposes these under the App
Service's **Settings → Environment variables**; inspect only what is needed and
avoid screenshots containing secret values.

## 7. Verify configuration before the walkthrough

Check that each Environment has its own correct client ID, exact OIDC subject,
and expected target. Confirm review and apply use the same backend/key and plan
passphrase. Run the manual plan first and inspect the intended prefix/resource
changes. Then follow [first deployment](FIRST-DEPLOYMENT.md) for apply, bootstrap,
credential removal, SSH schema setup, demo seeding and site deployment.

| Symptom | Check first |
| --- | --- |
| No matching federated identity / OIDC login failure | Owner/repository, Environment spelling/case, tenant, client ID, audience and subject |
| Login succeeds but Azure says authorization failed | Role assignment to the deployment service principal, scope and propagation |
| State blob access denied | Storage Blob Data Contributor data-plane grant, correct backend values and operator/identity |
| Missing workflow configuration | Environment secret versus variable placement and exact name |
| Apply rejected for missing approval | Required reviewer protection and a fresh run from `main` |
| Admin target mismatch | Live/demo admin output mapped to its corresponding ops Environment |
| App loads but nodes fail | Database bootstrap/schema/availability; static page success alone does not verify SQL |

## Official references

- [Azure Login using OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [App Service GitHub Actions deployment](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions?tabs=openid)
- [Terraform Azure backend authentication and roles](https://developer.hashicorp.com/terraform/language/backend/azurerm)
