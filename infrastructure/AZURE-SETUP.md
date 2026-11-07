# Azure account setup and selectable deployment targets

The cloud Actions use **one shared GitHub Environment** for Azure credentials and
account settings. Public deployment profiles are committed in
[`config/deployment-targets.json`](config/deployment-targets.json). Choose the
installation in the Action dropdown instead of changing Environment variables.

The shared Environment is **`atlas-infra-apply`**. It holds common credentials and
account settings, not a separate configuration for each live/demo variant.

For Action controls and defaults, use [Deployment overview](DEPLOYMENT.md#choose-the-target).

Instructions and identifiers may be public. Passwords, tokens, Terraform state and
plaintext plans must remain private. Cloning a repository does not copy its secrets.

## 1. Find your Azure account values

Azure Portal → **Subscriptions → your subscription → Overview** gives the
Subscription ID. **Microsoft Entra ID → Overview** gives the Directory (tenant) ID.
In a local Azure CLI or Azure Cloud Shell Bash session:

```bash
az login
az account list --query '[].{name:name,subscription:id,tenant:tenantId}' -o table
az account set --subscription '<your-subscription-id>'
az account show --query '{subscription:id,tenant:tenantId,name:name}' -o json
az account list-locations --query '[].{name:name,displayName:displayName}' -o table
```

Use a location's machine name, such as `westus2`, for `AZURE_LOCATION`. Check region,
SKU and SQL free-offer eligibility. B1 and private networking have recurring costs;
see [deployment costs](DEPLOYMENT.md#costs-and-verification).

## 2. Bootstrap private Terraform state storage

Create the state account outside the application Terraform root. In a local
Azure CLI/Cloud Shell session, substitute your own unique account name:

```bash
az group create --name atlas-tfstate-rg --location westus2
az storage account create --name <unique-state-account> --resource-group atlas-tfstate-rg --location westus2 --sku Standard_LRS --kind StorageV2 --allow-blob-public-access false --min-tls-version TLS1_2
az storage container create --name tfstate --account-name <unique-state-account> --auth-mode login
```

The operator needs the appropriate Azure resource permissions and Storage Blob
Data Contributor data access. Configure storage retention/recovery and restrict
access: state contains provisioning secrets.

Find the account name and resource group in **Storage accounts → your state
account → Overview**; find the container under **Data storage → Containers**.
Use the same account/container for multiple installations if desired, but a
separate state blob key for each installation. Never migrate an existing state
to a rehearsal merely to change target names.

## 3. Configure one GitHub-to-Azure OIDC identity

For a new repository, create an Entra application under **Microsoft Entra ID → App
registrations → New registration**. Copy **Application (client) ID** and **Directory
(tenant) ID** from its Overview. The application Object ID is not the client ID.
Azure role assignments target its service principal under **Enterprise applications**.
No client secret is required for OIDC.

Under **Certificates & secrets → Federated credentials → Add credential**, choose
the GitHub Actions scenario. Enter your repository owner/name, entity type
**Environment**, and Environment name **`atlas-infra-apply`**. The resulting trust is:

- Issuer: `https://token.actions.githubusercontent.com`
- Audience: `api://AzureADTokenExchange`
- Subject: `repo:<owner>/<repo>:environment:atlas-infra-apply`

Use your own owner/repository in a fork. The current repository can retain its
existing apply identity/trust; verify it has the permissions for all credential-bearing cloud Actions.
This is the CI deployment identity, separate from the runtime and operations
managed identities Terraform creates for each selected variant.

### Azure permissions

Use **the Azure scope → Access control (IAM) → Add role assignment**. OIDC sign-in
and Azure permissions are separate. The shared identity needs:

| Capability | Role/scope to consider |
| --- | --- |
| Provision the declared application resources | Contributor over the intended deployment scope |
| Read/write/lock Terraform state | Storage Blob Data Contributor on the state container |
| Publish websites/admin tools and trigger bootstrap | Website Contributor on each target App Service, unless already covered by Contributor at a containing scope |

The Terraform root creates resource groups itself, so initial provisioning normally
requires subscription-level resource-creation permission. A narrower resource-group
scope requires a different bootstrap/import arrangement. Register required resource
providers with an authorized operator if the scoped CI identity cannot do so.
Do not grant Owner just to bypass an error.

Both installations now deliberately share that identity's permissions. Profile
validation prevents targeting mistakes; it does not isolate an untrusted workflow
from the Azure account. Allow and approve only reviewed, trusted branches for this
credential-bearing Environment. Main live, infrastructure and admin runs require branch
`main`. Demo website deployments may use a reviewed feature branch. A separate subscription/identity remains an option if a
stronger authorization boundary is needed later.

## 4. Find the SQL administrator

Open **Microsoft Entra ID → Users → chosen operator** for the User principal name
and Object ID. For a group, use its display name and Object ID under **Groups**.
For the signed-in user:

```bash
az ad signed-in-user show --query '{login:userPrincipalName,objectId:id}' -o json
```

These populate the SQL Entra administrator variables below. They identify an
operator/group, not the GitHub deployment application. Terraform's separate SQL
provisioning username defaults to `atlasprovisioner`.

## 5. Configure the shared GitHub Environment

Open **your repository → Settings → Environments** and create or select
**`atlas-infra-apply`**.

### Environment secrets

| Name | Where the value comes from |
| --- | --- |
| `AZURE_CLIENT_ID` | Application (client) ID of the shared deployment identity |
| `AZURE_TENANT_ID` | Directory (tenant) ID from Entra Overview / step 1 |
| `AZURE_SUBSCRIPTION_ID` | The selected subscription ID from step 1 |
| `TF_SQL_ADMIN_PASSWORD` | Privately generated strong SQL provisioning password; preserve the existing value for an existing server unless deliberately rotating it |
| `ATLAS_TF_PLAN_PASSPHRASE` | Privately generated strong random value used to encrypt/decrypt the reviewed Terraform plan |
| `ATLAS_DEMO_PASSWORD` | Privately generated unique demo-account password, passed by Terraform to demo admin hosts |

The three Azure IDs are identifiers rather than passwords; the workflows read
them from Secrets to match the existing configuration. Generate passwords with a
password manager, not an example copied from this document. No values belong in
workflow logs or version control.

### Shared Environment variables

| Name | Where the value comes from |
| --- | --- |
| `AZURE_LOCATION` | Azure location machine name from step 1 |
| `AZURE_SQL_ENTRA_ADMIN_LOGIN` | Chosen SQL administrator's login or group display name from step 4 |
| `AZURE_SQL_ENTRA_ADMIN_OBJECT_ID` | Object ID of that same SQL administrator |
| `AZURE_TFSTATE_RESOURCE_GROUP` | State storage account's resource group from step 2 |
| `AZURE_TFSTATE_STORAGE_ACCOUNT` | State storage account name |
| `AZURE_TFSTATE_CONTAINER` | Private state container name, such as `tfstate` |
| `AZURE_TFSTATE_KEY` | Existing main installation's state key; used only while main's tracked `state_key` is null |

Secrets and Variables are different sections: workflows use `secrets.NAME` and
`vars.NAME` respectively. The deployment no longer reads `ATLAS_NAME_PREFIX`,
`AZURE_WEBAPP_NAME`, `AZURE_ADMIN_WEBAPP_NAME` or `AZURE_RESOURCE_GROUP` from GitHub
Environment variables. Target names now come from the selected tracked profile.

### Reviewers and trusted branches

- Require reviewers and disable approval bypass. Apply requires a recorded approval.
- Allow only trusted deployment branches, including a reviewed test branch when needed.
- Check that your GitHub plan supports the required Environment protection.

Environment approval can pause preview, apply, admin and deployment jobs.
If the required protection is unavailable, resolve that before using apply; it
does not bypass the approval check.

## 6. Review the public target profiles

The installation dropdown is `main` or `rehearsal`. The JSON contains their prefixes
and state keys. All other target names are derived exactly as Terraform defines them:

| Target | Naming rule |
| --- | --- |
| Resource group | `<prefix>-rg` |
| Public apps / databases | `<prefix>-live`, `<prefix>-demo` |
| Admin apps | `<prefix>-live-ops`, `<prefix>-demo-ops` |
| SQL logical server | `<prefix>-sql` |

The committed main prefix is the existing installation's `atlas-7k4m9q2x`.
Its state key is `atlas-capstone.tfstate`, confirmed by the operator from the
existing backend. This preserves the current installation's state; it does not
create or migrate a state blob.

The tracked rehearsal fallback is `atlas-rehearsal-a1b2c3`, an example chosen in this configuration, unrelated to any branch or commit. Instead of editing it, enter `rehearsal_name` in the Action, for example `trial02`.

The resolved prefix becomes `atlas-rehearsal-trial02` and the state key becomes `rehearsal/atlas-rehearsal-trial02.tfstate`. Full prefixes starting `atlas-rehearsal-` are accepted too. Use lowercase letters/digits/hyphens and a full prefix of at most 27 characters.

Leave the field blank only when intentionally using the tracked fallback. Main does not accept a rehearsal-name override.

Keep the same name across provisioning, admin tools and website deployment.
The [deployment overview](DEPLOYMENT.md#choose-the-target) defines application
choices and defaults; [Security](SECURITY.md#plan-and-apply-protections) explains
why reducing an existing installation’s resource set stops.

### Deploying your own fork

1. Create your own shared Environment, Azure identity and OIDC trust.
2. Change the main profile’s prefix and give it a new explicit state key.
3. Confirm neither target points to upstream resources or state.

Profiles contain no credentials and can be public. The workflow dropdown choices
are fixed; adding a new profile also requires updating workflow choices and validation.

### Continue to deployment

Use [First deployment](FIRST-DEPLOYMENT.md). Terraform outputs confirm the app names;
no post-provisioning GitHub app-name variables are needed.

## Official references

- [Azure Login using OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [Terraform Azure backend authentication and roles](https://developer.hashicorp.com/terraform/language/backend/azurerm)
- [GitHub Environment approvals and branch restrictions](https://docs.github.com/en/actions/how-tos/deploy/configure-and-manage-deployments/manage-environments)
