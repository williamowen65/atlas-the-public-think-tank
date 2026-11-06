# Atlas Azure infrastructure security

This guide describes checked-in controls and their limits. Successful deployment
and rehearsal runs do not constitute a production security audit. Read [DEPLOYMENT.md](DEPLOYMENT.md) for setup and [PROVISIONED-RESOURCES.md](PROVISIONED-RESOURCES.md) for resource inventory.

## Current security baseline

| Control | Code location | What it does | Limitation |
| --- | --- | --- | --- |
| Web HTTPS-only | `terraform/modules/webapp/main.tf` | `https_only = true` and minimum TLS 1.2 | Application authorization and session/CSRF checks must also pass |
| Legacy publishing closed | Web App module | Disables FTP/FTPS and FTP basic authentication | GitHub deployment OIDC/RBAC still requires Entra and GitHub setup |
| Private SQL only | `terraform/modules/sql/main.tf` | `public_network_access_enabled = false`; no public SQL firewall rules | The application still needs database authentication and authorization |
| Virtual network + Private Link | `terraform/modules/network/main.tf` | VNet with delegated app subnet, private SQL endpoint, private DNS zone and linked VNet | Private endpoint and DNS incur charges; VNet integration requires B1 or higher |
| Subnet firewall (NSG) | Network module | Permit TCP port 1433 from app subnet to SQL private endpoint subnet; deny other VNet sources | NSG rules are additional protection, not a replacement for SQL identity |
| Encrypted SQL transport | SQL module | Sets minimum SQL TLS 1.2 | Connection strings must also require encryption and validate server certificates |
| Data isolation | Terraform `live`/`demo` variants | Separate Azure SQL databases and Web Apps; one shared App Service plan | Runtime logins must have per-database access only; the UI must label and constrain demo behavior |
| Accidental data loss guard | SQL module | `prevent_destroy = true` protects each DB in ordinary Terraform changes | Does not protect against deletion outside Terraform, privileged SQL writes, or missing backups |
| Managed identities | Web App module | Separate user-assigned runtime and operations identities per variant; passwordless SQL connection strings | Bootstrap must establish contained users/grants; verify least privilege and cross-database rejection |
| SQL admin secret handling | Terraform variables and private backend | SQL administrator and demo seed passwords are sensitive; bootstrap settings are removed after use | Passwords can remain in protected state/history; runtime SQL sign-in uses managed identity |
| Deployment access | `.github/workflows/AtlasDeploy.yml` | Manual workflow, with shared-Environment short-lived GitHub OIDC credentials intended for Azure | Federated credentials, narrow Azure role grants and environment approval policy need to be provisioned before use |
| Safe telemetry | `Atlas.Api` and Azure observability module | Category-based app diagnostics and metrics via Azure Monitor; no raw SQL auto-instrumentation | Validate cloud ingestion, privacy and cost; durable audit outbox export remains incomplete |

**Public vs private:** The Atlas *website* needs a public HTTPS endpoint for visitors. The SQL *database* now has **no public network endpoint**. The .NET host connects through the Azure VNet and SQL Private Link over an internal IP address.

Ordinary GitHub-hosted runners and home machines cannot reach this private SQL endpoint: authorized database migration/administration jobs must run in the VNet. See [PRIVATE-NETWORK-OPERATIONS.md](PRIVATE-NETWORK-OPERATIONS.md).

**Important:** The NSG restricts network flows to SQL, but an authenticated and least-privilege SQL identity still grants actual database access. Database migrations and production runtime must use **separate privileges**, with no schema-admin credential on the public web app.

## Terraform CI security checks

[AtlasInfrastructure.yml](../.github/workflows/AtlasInfrastructure.yml) runs on pushes that change infrastructure files. It does **not** deploy to Azure and needs no Azure credentials.

| Check | Enforcement | Purpose |
| --- | --- | --- |
| `terraform fmt` / `init -backend=false` / `validate` | Blocking | Formatting and configuration validity |
| `tests/Atlas.Infrastructure.Tests/test_security_baseline.py` | **Blocking** | Parse Terraform HCL and verify HTTPS, FTP disabled, TLS minimum, user-assigned identities, SQL public access disabled, VNet integration, SQL Private Endpoint/private DNS/NSG rules, SQL free-offer safety defaults, and DB deletion protection |
| Trivy Terraform misconfiguration scan | **Advisory initially** | Report high/critical broader best-practice issues without making existing intentional tradeoffs silently disappear |
| Trivy infrastructure secret scan | **Blocking** | Detect high/critical exposed secrets accidentally committed to the Terraform directory |

The baseline tests use `python-hcl2==8.1.4` and do not access Azure. Trivy uses `aquasecurity/trivy-action@v0.36.0`, with readable findings in the GitHub Actions job output. A **passing baseline test** only proves the checked-in controls still have their expected values; it does **not** prove an Azure resource is secure or that no other Terraform settings can weaken it.

### Azure-connected predeployment preview (operator-invoked)

The separate [`AtlasAzurePlan.yml`](../.github/workflows/AtlasAzurePlan.yml) workflow authenticates to the selected Azure subscription using GitHub OIDC and consults the **real private Terraform state**. It runs `terraform init`, `validate` and `plan`, without `apply`.

It uses the same credential Environment as apply; the Action is read-only by behavior, not by a separate Azure identity. Only trusted, reviewed branches may use those credentials. It **never auto-runs on pull requests** and never uploads a Terraform plan artifact that could contain credentials.

The shared `atlas-infra-apply` credential Environment must be configured first (see [DEPLOYMENT.md](DEPLOYMENT.md)).

A real Terraform plan catches changes and some provider/state errors that the offline HCL tests cannot; **it is not identical to Azure ARM/Bicep What-If**, and it does not replace real deployment, quota and runtime verification.

### Follow-up security gates

1. Review Trivy's first findings. Confirm actual risks and Azure/free-tier constraints, and resolve them; then make the relevant HIGH/CRITICAL checks blocking. Never suppress a finding without a documented reason.
2. Exercise the **implemented manual Azure-connected `terraform plan` workflow** against the bootstrapped remote state, with the shared OIDC deployment identity. Review its proposed changes before any separate manual apply.
3. Verify **deployed configuration and drift** once Azure is running; static CI cannot see Portal-side changes or runtime permissions.
4. Verify deployed SQL grants, OIDC permissions and the protected operations host.
   Managed-identity sign-in and bootstrap are implemented. Test that runtime access
   cannot administer schema or cross into another database.
5. Add runtime security checks after deployment: HTTPS redirect, authentication/session/CSRF, API authorization, direct-link routing, audit-outbox delivery, SQL FTS readiness, and zero cross-database access.
6. Check backup/restore, telemetry retention/budget, and alerts. Monitor B1 hosting, private endpoint, private DNS and telemetry costs; do not silently replace the private network with public SQL access to save money.

## How to review locally

From the repository root, with Python and Terraform installed:

```powershell
python -m pip install "python-hcl2==8.1.4" "PyYAML==6.0.3"
python -m unittest discover -s tests/Atlas.Infrastructure.Tests -p "test_*.py" -v
Push-Location infrastructure/terraform
terraform fmt -check -recursive
terraform init -backend=false
terraform validate
Pop-Location
```

Trivy's full findings appear in the GitHub Actions workflow. Static scans and tests are safe to execute without provisioning resources.
