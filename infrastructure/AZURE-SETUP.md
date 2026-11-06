# Azure account setup and selectable deployment targets

The cloud Actions use **one shared GitHub Environment** for Azure credentials and
account settings. Public deployment profiles are committed in
[`config/deployment-targets.json`](config/deployment-targets.json). Choose the
installation in the Action dropdown instead of changing Environment variables.

The shared Environment retains the existing name **`atlas-infra-apply`** so the
current repository does not need another Environment or OIDC trust. Its historical
name does not limit it to Terraform: plan, apply, admin and website deployment all
use it. Live/demo are application variants, not GitHub Environments in this model.

| Setting | Where it lives | How it changes |
| --- | --- | --- |
| Azure credentials, subscription, region, SQL administrator and state storage account | Shared Environment | Configure once per Azure account/subscription |
| Main prefix/key and fallback rehearsal name | Tracked JSON profile | Reviewed configuration |
| Custom rehearsal name | Action text field | Enter the same name in plan, apply, admin and deployment |
| App and resource-group names | Derived from the selected profile, matching Terraform | No manual copying of output names into GitHub variables |
| Installation and applications to provision | Action dropdowns | Default: main both; rehearsal live only |
| Admin/deployment variant | Action dropdown | Select a variant that was provisioned |
| Website operation | Action dropdown | `deploy` publishes; `verify` only checks public HTTP readiness |
| Source branch | Run workflow branch selector / CLI `--ref` | Select the code to test |

Instructions and identifiers may be public. Passwords, tokens, Terraform state and
plaintext plans must remain private. A clone/fork does not include GitHub secrets.
An independent contributor sets up their own shared Environment, Azure subscription
and OIDC trust and edits the public profiles to name their own installation.

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
This is a CI deployment identity, separate from the four runtime/operations
managed identities Terraform creates for SQL access.

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
credential-bearing Environment. Main-target runs additionally require branch
`main` in the workflow. A separate subscription/identity remains an option if a
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

Open **your repository → Settings → Environments → atlas-infra-apply**. For a fork,
create that one Environment. For the existing repository, consolidate common values
from the older Environments here. Do not remove old configuration until the revised
workflow has been verified; repository changes do not automatically change GitHub Environment settings.

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

Retain required reviewers and disable bypass on the shared Environment. The apply workflow requires a recorded approval. Because the Environment is shared, approval protection also applies to plan/admin/deployment jobs using it; some runs may pause at more than one job.

Permit the trusted test branch in the Environment's branch rules before running the PR version. If reviewers are unavailable on your GitHub plan, use the local Terraform procedure instead of assuming apply will bypass approval.

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

Use that same name in the provisioning, admin and website Actions. A new name means separate resources
and state; the same name updates the same installation. The applications dropdown
(`deployment_shape`) offers `default`, `live`, `demo`, and `both`. Default resolves
to both for main and live only for rehearsal. It is independent of the branch.
Choose demo explicitly for demo-only fixtures, or both for a full rehearsal.

The state key deliberately does not depend on the application selection. Reducing
an existing installation's variants stops before planning with a clear reason.
Plans with any resource deletion or replacement also stop before apply. Use a new
name for a fresh single-variant test rather than treating an old installation as
empty. No delete Action or destructive override is included.

For an independent fork, change the main profile's prefix and set an explicit
new main state key before deploying. It must not refer to the upstream resources.
Profiles contain no credentials and are appropriate for public version control.
The dropdown options are intentionally fixed; adding more named profiles requires
updating the workflow choices and validation together.

No app-name variable setup is needed after Terraform provisioning. Review Terraform
outputs to confirm the derived names, then use the same installation dropdown for
admin tools and website deployment. Follow [first deployment](FIRST-DEPLOYMENT.md).

## Official references

- [Azure Login using OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [Terraform Azure backend authentication and roles](https://developer.hashicorp.com/terraform/language/backend/azurerm)
- [GitHub Environment approvals and branch restrictions](https://docs.github.com/en/actions/how-tos/deploy/configure-and-manage-deployments/manage-environments)
