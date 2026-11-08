# First deployment

Use this walkthrough for a new installation, including a rehearsal alongside main.
Terraform runs in GitHub Actions; SQL commands run in the operations App Service’s
SSH terminal.

Complete [Azure setup](AZURE-SETUP.md) once before starting.
For the workflow diagram and control definitions, see [Deployment overview](DEPLOYMENT.md).

## 1. Choose your installation

For a rehearsal, use these values throughout the walkthrough:

| Setting | Example |
| --- | --- |
| Branch | `main`, or a trusted test branch allowed by the shared GitHub Environment |
| Installation | `rehearsal` |
| Rehearsal name | `trial02` — choose a new name for a separate installation |
| Applications to provision | `default` for live only, or explicitly `live`, `demo`, `both` |

For the main installation, use branch `main`, installation `main`, and no rehearsal
name. Its default application set is both.

Repeat the per-variant steps below only for the variants you provisioned.
A live-only rehearsal has no demo steps.

### Testing a branch before merge

Existing workflows can run their trusted branch versions through **Run workflow →
Branch**. Allow that branch in the shared Environment’s deployment branch rules.

A newly added workflow filename must first exist on the default branch before
ordinary manual dispatch is available. Access to shared credentials is only for
reviewed, trusted code.

## 2. Preview and provision

In GitHub Actions:

1. Run **Atlas Azure Terraform plan (manual, no apply)** with your chosen values.
2. Review the resolved resource names and proposed changes.
3. Run **Atlas Azure Terraform apply (manual)** with those same values.
4. Enable **SQL bootstrap credentials** for this initial apply.
5. Review its new plan and approve the apply job.

The apply Action creates its own saved plan; it does not reuse the preview run.

GitHub may call an Environment approval **Review deployments**, even for the
read-only preview. Approving access to the preview does not apply its plan.

### Record the outputs

Keep these outputs for the selected installation:

- `resource_group`
- `admin_site_names`
- `site_names` and `site_urls`
- `database_names`

Terraform creates and wires the application managed identities automatically.
Do not copy their generated client IDs into GitHub secrets. The shared
`AZURE_CLIENT_ID` belongs to the separate deployment identity.

## 3. Publish the admin tools

Run **Atlas admin tools (publish or bootstrap)** for each selected variant:

- Keep the same branch, installation and rehearsal name.
- Choose the provisioned `variant`.
- Set `operation` to **publish**.
- Leave confirmation blank.

This uploads the C# tools. It does not run SQL or deploy the public website.

## 4. Bootstrap database access through SSH

Open **Azure Portal → App Services → the matching operations app from
`admin_site_names` → SSH**.

Use **App Service SSH**, not Cloud Shell or the Kudu console.

Check the non-secret target, then run bootstrap:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/tools/atlas-admin
dotnet ./Atlas.Admin.dll --bootstrap-cloud-identities
```

Confirm the variant and database match the selected installation.
Expected success for live:

```text
Connected to Azure SQL. Configuring managed-identity database users.
Azure SQL contained managed-identity users verified for live.
Remove ATLAS_BOOTSTRAP_SQL_* settings by disabling enable_sql_bootstrap and applying Terraform.
```

Demo reports `demo` instead. Repeat in its operations app if you provisioned it.

Bootstrap establishes database users and permissions. It does not create the
application schema or seed data. Wait for success before continuing.

## 5. Remove temporary credentials

Run **Terraform apply** again:

- Use the same installation, rehearsal name and application set.
- Disable SQL bootstrap credentials.
- Review and approve the change.

This removes temporary administrator settings from the operations apps.
Terraform reports an in-place change, even if additions and deletions are both zero.

It does not erase the SQL administrator password from protected state/history.

## 6. Apply the schema

In each selected operations app’s SSH terminal, check the target again and run:

```bash
printf 'Variant: %s\nDatabase: %s\n' "$ATLAS_DEPLOYMENT_VARIANT" "$ATLAS_EXPECTED_DATABASE"
cd /home/site/wwwroot/tools/atlas-admin
dotnet ./Atlas.Admin.dll --setup-database
```

Expected success:

```text
Schema: connection opened; applying migrations and supplemental SQL objects.
Schema and SQL Full-Text initialization completed; background index population may continue.
```

This applies migrations and SQL Full-Text setup using the operations app’s managed
identity. Wait for completion before deploying the website.

## 7. Seed an empty demo database, if needed

Skip this step for live.

In the **demo operations app’s SSH terminal**, confirm the demo target and run:

```bash
cd /home/site/wwwroot/tools/atlas-admin
dotnet ./Atlas.Admin.dll --seed-demo
```

Terraform supplies `ATLAS_DEMO_PASSWORD` from the shared GitHub secret.
The command refuses live and existing data; do not reset a populated database to
make seeding pass.

## 8. Deploy and verify the website

Run **Atlas manual Azure deployment** for each provisioned variant:

- Use the same installation and rehearsal name.
- Select the code branch to deploy.
- Choose operation **deploy**.

The Action builds and publishes the frontend/API package, then makes a short
readiness check. If startup is still in progress, it reports **verification pending**
after successful publication.

Later, run operation **verify** with the same target. It checks the public page
and Atlas liveness response without rebuilding or redeploying. It fails if the
site is still unavailable.

### Check the application

HTTP readiness alone does not check SQL, authentication or the running commit.
At the URL from `site_urls`, check:

- Feed loading and a real database-backed request.
- Sign-in/out and persistent data.
- Full-Text search after background indexing.
- Intended demo content, if seeded. An empty live feed can be valid.

## If a step fails

### SQL connection retries

Bootstrap and schema retry recognized transient connection-opening failures up to
three times, ten seconds apart. Database wake-up has not been confirmed as their cause.

Retries do not replay migrations or grants. If SSH disconnects or a later phase
fails, inspect the result before rerunning; completed database changes may remain.

### Optional bootstrap Action returns HTTP 404

The admin Action also offers `bootstrap`, with confirmation `bootstrap-live` or
`bootstrap-demo`. Its WebJob trigger has failed after successful publication.

Use the published tools through SSH as in step 4 while bootstrap credentials remain
enabled. Upload success alone is not bootstrap success.

### Website verification is still pending

Run `verify` again after startup. For persistent failure, inspect the public app’s
Log stream in Azure rather than repeatedly redeploying.

## Clean up a rehearsal

Follow [Rehearsal cleanup](REHEARSAL-CLEANUP.md) to delete its tracked resources
and update its state. Keep the same rehearsal name; main is not a cleanup target.

## Grant a global moderator after registration

First test on demo. Run **Atlas admin tools (publish or bootstrap)** from the
PR branch with installation **main**, variant **demo**, and operation **publish**.
This updates the tools without bootstrapping identities, applying migrations, or
seeding data. Older installations with only WebJob folders need this publication
to create the current SSH tool directory.

In the **demo Ops App Service → SSH**, run:

```bash
cd /home/site/wwwroot/tools/atlas-admin
dotnet ./Atlas.Admin.dll --grant-global-moderator "account@example.com" grant-demo
```

Use an existing demo account with confirmed email, no active lockout, and an
active participant profile. The command does not register accounts or confirm
email. Repeating a successful grant makes no further changes. Sign out and
sign back in, then open `/moderation` to verify access.

After demo verification and merging the PR, publish the tools from **main** to
variant **live**. In **live Ops → SSH**, use the same directory and run:

```bash
dotnet ./Atlas.Admin.dll --grant-global-moderator "account@example.com" grant-live
```

The confirmation must match the Ops host's configured variant. Existing managed
identity and expected-database validation also applies. Role assignment and
security-stamp update run in one transaction. No website deployment or Terraform
change is required for this command.

### If the moderator command waits

The command prints timestamped progress for SQL connection opening, account checks,
role assignment, and commit. SQL connection opening retries at most three times
within a 90-second cancellation budget; each SQL command has a 30-second timeout.
Retries apply only before account checks or changes, never to the role transaction.
The connection used for the grant is the same connection opened by the readiness
step. Copy the last progress message and any following error from SSH when diagnosing
an issue; SSH command output may not appear in App Service Log stream.

### Managed identity tokens and connection troubleshooting

**No manual token request is required before running Atlas.Admin.** Setup, demo
seeding, and moderator assignment use the configured managed-identity SQL
connection string. SqlClient acquires an Azure SQL access token automatically
(and may reuse a cached token). The token proves which identity is connecting;
the contained SQL user and grants determine what that identity may do.

| Command | SQL authentication |
| --- | --- |
| `--bootstrap-cloud-identities` | Temporary SQL administrator username/password; creates the identity users and grants |
| `--setup-database` | Operations user-assigned managed identity, selected by connection-string `User Id` |
| `--seed-demo` | Operations user-assigned managed identity; demo only |
| `--grant-global-moderator` | Operations user-assigned managed identity |

The successful setup and seed commands did not need a separate token check:
their SQL driver handled authentication automatically. The moderator command
uses that same authentication mechanism, with a bounded connection-opening
budget. Token acquisition, SQL network access, database wake-up, and SQL
permissions are separate things to investigate when a connection fails.

#### If token acquisition is canceled

An error containing `Failed to acquire access token for
ActiveDirectoryManagedIdentity` and `The operation was canceled` identifies a
failure during token acquisition. It does not establish a missing SQL grant or
a database wake-up problem. A cancellation alone does not establish the root
cause.

In the affected **demo or live Ops App Service**:

1. Check **Settings → Identity → User assigned** for its matching Ops identity.
   System-assigned identity can remain off for this configuration.
2. Check that `ATLAS_ADMIN_MI_CLIENT_ID` and the SQL connection string's `User Id`
   both match that identity's **Client ID**, not its Object/Principal ID.
3. In that Ops host's **SSH** session, check the endpoint variable names without
   printing their values:

   ```bash
   printenv | cut -d= -f1 | sort | grep -E '^IDENTITY_(ENDPOINT|HEADER)$'
   ```

   Both `IDENTITY_ENDPOINT` and `IDENTITY_HEADER` should appear. Their presence
   alone does not prove the endpoint can issue a token.
4. If needed, request a SQL token directly using the host's configured client ID:

   ```bash
   curl --silent --show-error --max-time 30 \
     --output /dev/null \
     --write-out 'HTTP status: %{http_code}\n' \
     --get "$IDENTITY_ENDPOINT" \
     --header "X-IDENTITY-HEADER: $IDENTITY_HEADER" \
     --data-urlencode "api-version=2019-08-01" \
     --data-urlencode "resource=https://database.windows.net/" \
     --data-urlencode "client_id=$ATLAS_ADMIN_MI_CLIENT_ID"
   ```

   Run without shell tracing (`set -x`); do not print the identity header or token.
   HTTP 200 means this request obtained a token. The response body is discarded.
   It does **not** connect to SQL, wake the database, verify SQL grants, or pass
   that token to Atlas.Admin. A non-200 response or timeout needs further
   investigation before assuming a SQL permissions problem.
5. Retry the intended Atlas.Admin command. If it still fails, retain the last
   progress message and error, and inspect its actual connection path and timeout
   handling. Do not add GitHub variables or enable another identity solely
   because one token request was canceled.

#### Observed demo recovery

During a demo Ops moderator grant, the first reported connection attempt failed
with token acquisition canceled under the moderator connection budget. The
operator verified the configured client IDs, received HTTP 200 from the direct
token check, and reran the moderator command successfully through account checks
and role assignment. This confirms the tested identity and required SQL access
worked on the successful run. A transient token-acquisition delay is plausible;
the exact cause remains unconfirmed. There is no evidence that the manual request
was necessary for recovery or that it supplied Atlas.Admin's token.

For live, run the normal command in **live Ops SSH** with the live confirmation.
Use the checks above only if troubleshooting is needed. An optional curl check
before a demonstration verifies token acquisition only; database readiness needs
an actual SQL connection. Demo success does not verify the separate live identity
and database grants.

References: [SqlClient Microsoft Entra authentication](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication)
and [App Service managed identities](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity).
