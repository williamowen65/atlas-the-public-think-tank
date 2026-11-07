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
